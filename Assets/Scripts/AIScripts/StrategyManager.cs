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
    public float WarningDistance;
    [Tooltip("Distance from castle to raise alert")]
    public float DangerDistance;

    [Tooltip("Stage of game to start on")]
	public Stage StartMode;

	[Header("Activity flags:")]
	[Tooltip("Should the enemy gather resources?")]
	public bool Gathering;
	[Tooltip("Should the enemy build buildings?")]
	public bool Building;
	[Tooltip("Should the enemy make more crabs?")]
	public bool MakingCrabs;
	[Tooltip("Should the enemy attack the player?")]
	public bool Harassing;
	[Tooltip("Should the enemy retaliate against attack?")]
	public bool Defending;

	[Header("Gathering variables:")]
	public int LowResourceCount;
	public int MaxGatheringCrabs;

	[Header("Building variables:")]
	public int MaxBuildingCrabs;

    [Header("Build order:")]
    public int InitialWoodGoal;
    public int InitialStoneGoal;
    public List<string> BuildingQueue;
    public float WoodToStoneRatio;

	// The main queue of commands to execute
	public Queue<Command> Commands { get; set; }

	public EnemyKnowledge Knowledge { get; set; }
    public TacticsManager TacticsManager { get; set; }
    public ProductionManager ProductionManager { get; set; }

    public CastleController MainCastle { get; set; }

	public Strategy Strategy;

    Timer _circleTimer;
    int _circleLifeTime = 10;

    const int _neutral = -1;

	Stage _currentMode;

	bool _debug;

	/// <summary>
	/// Wake this instance.
	/// </summary>
	void Awake() 
	{
        GenerateBuildOrder();

		_currentMode = StartMode;

        Commands = new Queue<Command>();
        Knowledge = GetComponent<EnemyKnowledge>();
        TacticsManager = GetComponent<TacticsManager>();
        ProductionManager = GetComponent<ProductionManager>();
		_debug = GetComponent<DebugComponent>().Debug;
	}

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
        if (_debug) {
            Debug.Log("Started Thinker.");
        }

        _circleTimer = new Timer(_circleLifeTime);
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update() 
	{
        if (Knowledge.AICastleList.Count > 0 && MainCastle == null)
            MainCastle = Knowledge.AICastleList[0].GetComponent<CastleController>();

        if (_debug)
        {
            _circleTimer.update(DrawCircles);
        }

        if (BeingAttacked())
        {
            TacticsManager.LaunchAttack(1, Knowledge.DangerZoneCrabList[0]);
        }
    }

    /// <summary>
    /// Generates the build order
    /// </summary>
    void GenerateBuildOrder()
    {
        // initial list: {Nest, Nest, Armoury, Workshop}
        Strategy = new Strategy(InitialWoodGoal, InitialStoneGoal);
        foreach (string building in BuildingQueue)
        {
            Strategy.BuildingQueue.Enqueue(building);
        }
        Strategy.WoodToStoneRatio = WoodToStoneRatio;
    }

	/// <summary>
	/// Is base being attacked?
	/// </summary>
	/// <returns><c>true</c>, if base is attacked, <c>false</c> otherwise.</returns>
	bool BeingAttacked()
	{
        return Knowledge.DangerZoneCrabSet.Count > 0;
	}

    #region Crab Commands

    /// <summary>
    /// Starts building.
    /// </summary>
    /// <param name="crab">Crab.</param>
    /// <param name="buildingType">Building type.</param>
    /// <param name="location">Location.</param>
    public void StartBuild(GameObject crab, string buildingType, Vector3 location)
	{
        BuildCommand command = new BuildCommand();
		command.Crab = crab;
		command.BuildingType = buildingType;
		command.Location = location;
		Commands.Enqueue(command);
	}

    /// <summary>
    /// Starts building from ghost.
    /// </summary>
    /// <param name="crab">Crab.</param>
    /// <param name="ghost">Ghost.</param>
    public void StartBuildFromGhost(GameObject crab, GameObject ghost)
    {
        BuildFromGhostCommand command = new BuildFromGhostCommand();
        command.Crab = crab;
        command.GhostBuilding = ghost;
        Commands.Enqueue(command);
    }

    /// <summary>
    /// Starts collecting
    /// </summary>
    /// <param name="crab">Crab.</param>
    /// <param name="resource">Resource.</param>
	public void StartCollect(GameObject crab, GameObject resource) 
	{
        CollectCommand command = new CollectCommand();
		command.Crab = crab;
		command.Resource = resource;
		Commands.Enqueue(command);
	}

    /// <summary>
    /// Stops all activity of the crab.
    /// </summary>
    /// <param name="crab">Crab.</param>
    public void StopCrab(GameObject crab)
    {
        StopCommand command = new StopCommand();
        command.Crab = crab;
        Commands.Enqueue(command);
    }

    /// <summary>
    /// Starts entering.
    /// </summary>
    /// <param name="crab">Crab.</param>
    /// <param name="building">Building.</param>
    public void EnterBuilding(GameObject crab, GameObject building)
    {
        EnterCommand command = new EnterCommand();
        command.Crab = crab;
        command.Building = building;
        Commands.Enqueue(command);
    }

    /// <summary>
    /// Starts building a siege weapon
    /// </summary>
    /// <param name="workshop"></param>
    /// <param name="type"></param>
    public void StartBuildingSiegeWeapon(GameObject workshop, string type) 
    {
        SiegeCommand command = new SiegeCommand();
        command.SiegeWorkshop = workshop;
        command.BuildingType = type;
        Commands.Enqueue(command);
    }

    /// <summary>
    /// Tells a crab to get a weapon from an armoury
    /// </summary>
    /// <param name="crab"></param>
    /// <param name="armoury"></param>
    /// <param name="weapon"></param>
    public void StartTakeWeapon(GameObject crab, GameObject armoury, string weapon)
    {
        TakeWeaponCommand command = new TakeWeaponCommand();
        command.Crab = crab;
        command.Armoury = armoury;
        command.Weapon = weapon;
        Commands.Enqueue(command);
    }

    #endregion

    /// <summary>
    /// Draws debug circles of the various distances the AI watches.
    /// </summary>
    void DrawCircles()
    {
        if (Knowledge.AICastleList.Count > 0)
        {
            DebugTools.DrawCircle(Knowledge.AICastleList[0].transform.position, DangerDistance, _circleLifeTime);
            DebugTools.DrawCircle(Knowledge.AICastleList[0].transform.position, WarningDistance, _circleLifeTime);
        }
    }

    /// <summary>
    /// Gets the ratio of AI to player crabs.
    /// </summary>
    /// <param name="playerCrabs">Player crabs.</param>
    /// <param name="enemyCrabs">Enemy crabs.</param>
    void GetRatio(int playerCrabs, int enemyCrabs)
	{
		Knowledge.TroopRatio = (float)playerCrabs/(float)enemyCrabs;
	}
}
