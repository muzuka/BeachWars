using UnityEngine;
using System.Collections.Generic;

public enum Stage {START, MID, END};

/// <summary>
/// Strategy manager.
/// 	Decides on goals and delegates action to other managers.
/// 	Divides strategy into tiers.
/// 	Decides what to build.
/// 	Decides how much to collect.
/// 	Decides what units to produce.
/// 	Decides what weapons to produce.
/// </summary>
[RequireComponent(typeof(EnemyKnowledge))]
[RequireComponent(typeof(DebugComponent))]
[RequireComponent(typeof(IncomeManager))]
[RequireComponent(typeof(ProductionManager))]
[RequireComponent(typeof(ReconManager))]
[RequireComponent(typeof(TacticsManager))]
public class StrategyManager : MonoBehaviour {

    [Tooltip("Distance from castle to raise caution")]
    public float warningDistance;
    [Tooltip("Distance from castle to raise alert")]
    public float dangerDistance;

    [Tooltip("Stage of game to start on")]
	public Stage startMode;

	[Header("Activity flags:")]
	[Tooltip("Should the enemy gather resources?")]
	public bool gathering;
	[Tooltip("Should the enemy build buildings?")]
	public bool building;
	[Tooltip("Should the enemy make more crabs?")]
	public bool makingCrabs;
	[Tooltip("Should the enemy attack the player?")]
	public bool harassing;
	[Tooltip("Should the enemy retaliate against attack?")]
	public bool defending;

	[Header("Gathering variables:")]
	public int lowResourceCount;
	public int maxGatheringCrabs;

	[Header("Building variables:")]
	public int maxBuildingCrabs;

    [Header("Build order:")]
    public int initialWoodGoal;
    public int initialStoneGoal;
    public List<string> buildingQueue;
    public float woodToStoneRatio;

	// The main queue of commands to execute
	public Queue<Command> commands { get; set; }

	public EnemyKnowledge knowledge { get; set; }
    public TacticsManager tacticsManager { get; set; }
    public ProductionManager productionManager { get; set; }

    public CastleController mainCastle { get; set; }

	public Strategy strategy;

    Timer circleTimer;
    int circleLifeTime = 10;

    const int neutral = -1;

	Stage currentMode;

	bool debug;

	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake () 
	{
        generateBuildOrder();

		currentMode = startMode;

        commands = new Queue<Command>();
        knowledge = GetComponent<EnemyKnowledge>();
        tacticsManager = GetComponent<TacticsManager>();
        productionManager = GetComponent<ProductionManager>();
		debug = GetComponent<DebugComponent>().debug;
	}

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
        if (debug) {
            Debug.Log("Started Thinker.");
        }

        circleTimer = new Timer(circleLifeTime);
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () 
	{
        if (knowledge.aiCastleList.Count > 0 && mainCastle == null)
            mainCastle = knowledge.aiCastleList[0].GetComponent<CastleController>();

        if (debug)
        {
            circleTimer.update(drawCircles);
        }

        if (beingAttacked())
        {
            tacticsManager.launchAttack(1, knowledge.dangerZoneCrabList[0]);
        }
    }

    // build standard build order
    void generateBuildOrder ()
    {
        // initial list: {Nest, Nest, Armoury, Workshop}
        strategy = new Strategy(initialWoodGoal, initialStoneGoal);
        foreach (string building in buildingQueue)
        {
            strategy.buildingQueue.Enqueue(building);
        }
        strategy.woodToStoneRatio = woodToStoneRatio;
    }

	/// <summary>
	/// Is base being attacked?
	/// </summary>
	/// <returns><c>true</c>, if base is attacked, <c>false</c> otherwise.</returns>
	bool beingAttacked ()
	{
        return knowledge.dangerZoneCrabSet.Count > 0;
	}

    #region Crab Commands

    /// <summary>
    /// Starts building.
    /// </summary>
    /// <param name="crab">Crab.</param>
    /// <param name="buildingType">Building type.</param>
    /// <param name="location">Location.</param>
    public void startBuild (GameObject crab, string buildingType, Vector3 location)
	{
        BuildCommand command = new BuildCommand();
		command.crab = crab;
		command.buildingType = buildingType;
		command.location = location;
		commands.Enqueue(command);
	}

    /// <summary>
    /// Starts building from ghost.
    /// </summary>
    /// <param name="crab">Crab.</param>
    /// <param name="ghost">Ghost.</param>
    public void startBuildFromGhost(GameObject crab, GameObject ghost)
    {
        BuildFromGhostCommand command = new BuildFromGhostCommand();
        command.crab = crab;
        command.ghostBuilding = ghost;
        commands.Enqueue(command);
    }

    /// <summary>
    /// Starts collecting
    /// </summary>
    /// <param name="crab">Crab.</param>
    /// <param name="resource">Resource.</param>
	public void startCollect (GameObject crab, GameObject resource) 
	{
        CollectCommand command = new CollectCommand();
		command.crab = crab;
		command.resource = resource;
		commands.Enqueue(command);
	}

    /// <summary>
    /// Stops all activity of the crab.
    /// </summary>
    /// <param name="crab">Crab.</param>
    public void stopCrab (GameObject crab)
    {
        StopCommand command = new StopCommand();
        command.crab = crab;
        commands.Enqueue(command);
    }

    /// <summary>
    /// Starts entering.
    /// </summary>
    /// <param name="crab">Crab.</param>
    /// <param name="building">Building.</param>
    public void enterBuilding (GameObject crab, GameObject building)
    {
        EnterCommand command = new EnterCommand();
        command.crab = crab;
        command.building = building;
        commands.Enqueue(command);
    }

    /// <summary>
    /// Starts building a siege weapon
    /// </summary>
    /// <param name="workshop"></param>
    /// <param name="type"></param>
    public void startBuildingSiegeWeapon(GameObject workshop, string type) 
    {
        SiegeCommand command = new SiegeCommand();
        command.siegeWorkshop = workshop;
        command.buildingType = type;
        commands.Enqueue(command);
    }

    /// <summary>
    /// Tells a crab to get a weapon from an armoury
    /// </summary>
    /// <param name="crab"></param>
    /// <param name="armoury"></param>
    /// <param name="weapon"></param>
    public void startTakeWeapon(GameObject crab, GameObject armoury, string weapon)
    {
        TakeWeaponCommand command = new TakeWeaponCommand();
        command.crab = crab;
        command.armoury = armoury;
        command.weapon = weapon;
        commands.Enqueue(command);
    }

    #endregion

    #region command functions



    #endregion

    void drawCircles()
    {
        if (knowledge.aiCastleList.Count > 0)
        {
            DebugTools.DrawCircle(knowledge.aiCastleList[0].transform.position, dangerDistance, circleLifeTime);
            DebugTools.DrawCircle(knowledge.aiCastleList[0].transform.position, warningDistance, circleLifeTime);
        }
    }

    /// <summary>
    /// Gets the ratio of AI to player crabs.
    /// </summary>
    /// <param name="playerCrabs">Player crabs.</param>
    /// <param name="enemyCrabs">Enemy crabs.</param>
    void getRatio (int playerCrabs, int enemyCrabs)
	{
		knowledge.troopRatio = (float)playerCrabs/(float)enemyCrabs;
	}
}
