using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public enum CrabSpecies {ROCK, FIDDLER, TGIANT, SPIDER, COCONUT, HORSESHOE, SEAWEED, CALICO, TRILOBITE, KAKOOTA};

/// <summary>
/// Crab controller.
/// Handles all crab actions that can be commanded.
/// </summary>
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Team))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Attackable))]
[RequireComponent(typeof(DebugComponent))]
public class CrabController : MonoBehaviour {

	#region public variables

	// public variables
	[Tooltip("The species of crab")]
	public CrabSpecies type;				// type of crab
	[Tooltip("The range of the crab")]
	public float attackRange;				// distance to hit

    [Header("Sound Clips:")]
	[Tooltip("Sound to play on death")]
	public AudioClip deathClip;
	[Tooltip("Sound to play on hit")]
	public AudioClip hitClip;

	[Header("Time variables:")]
	[Tooltip("Time to build things")]
	public float timeToBuild;				// time to build object
	[Tooltip("Time to upgrade things")]
	public float timeToUpgrade;				// time to upgrade block
    [Tooltip("Time to rebuild")]
    public float timeToRebuild;
    [Tooltip("Time to repair")]
    public float timeToRepair;

    //weapon values
    [Header("Weapon Values:")]
    const float spearAttack = 1.0f;
    const float spearDistance = 1.5f;
    const float shieldDefense = 0.5f;
    const float hammerAttack = 1.0f;
    const float bowAttack = 1.5f;
    const float bowRange = 10.0f;

    [Header("Level stat increases:")]
    public float damageIncrease;
    public float healthIncrease;

    [Header("Other variables:")]
    [Tooltip("Amount that is restored to a building being rebuilt")]
    public float rebuildAmount;
    [Tooltip("Experience needed to reach next level")]
    public int nextLevelExperience;
    [Tooltip("Experience gained by kill")]
    public int levelGain;
    [Tooltip("Distance that a crab can see.")]
    public int awarenessMultiplier;

    #endregion

    #region private variables

    // stats decided by data in crabStats.xml
    float maxHealth;				// maximum health of crab
	float maxSpeed;					// speed of crab used by agent
	float attackDamage;				// base damage of crab
	float attackSpeed;				// time between attacks

	Team team;                      // team of crab

	// action states
	public StateController actionStates { get; set; }
	string[] actionStateList = {"Moving", "Attacking", "Building", "Collecting", "Unloading", "Entering", "Capturing", "Recruiting", "Upgrading", "Repairing", "Taking", "Dismantling", "Rebuilding"};

    //weapon states
    StateController weaponStates;
	string[] weaponStateList = {Tags.Spear, Tags.Shield, Tags.Hammer, Tags.Bow};

	// building variables
	bool rebuilding;				            // Is the crab rebuilding a wall or junction?
	GameObject currentGhostBuilding;            // the block that owns the clicked ghost block
    GhostBuilder ghostBuilderReference;         // reference to ghost buildings builder script   
    string buildingType;			            // the selected type of building

	wallUpgradeType upgradeType;				// type of upgrade for a block
    
	// interacting knowledge
	public Vector3 destination { get; set; }	// current destination
	public GameObject target { get; set; }		// current target object to attack, collect from, or capture
	GameObject secondTarget; 					// A secondary target that may be used after another action finishes.
	Player controller;							// reference to player object, null when not selected.

	bool selected;								// is the crab selected?

	CastleController mainCastle;				// The closest castle to the crab.

	// recruitment variables
	string recruitmentUIName;					// name of the prefab file for the respective recruitment UI
	GameObject recruitmentUI;					// Canvas of ui that appears when player approaches
	bool playerIsNear;							// is player nearby?
	bool uiOpen;								// is the UI open

	// TODO find good container
	public Inventory inventory { get; set; }	// inventory holds objects

	NavMeshAgent crabAgent;                     // reference to navmeshagent

    // time variables
    float timeToCapture;
    float timeConsumed;

	int neutralTeamCode = -1;

    Timer attackTimer;
    Timer repairTimer;
    Timer rebuildTimer;
    Timer dismantleTimer;

	// level variables
	int level;
	int experience;

	int selectedCrabs;					// the amount of crabs selected

	Material crabMaterial;				// material of the crab
		
	int recruitmentSacrifices;			// number of sacrifices required to hire

	string currentCraft;

	string weaponToTake;
	string resourceToTake;

	bool debug;

	GameObject destinationMarker;

	#endregion

	#region Monobehaviour functions

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		inventory = new Inventory();
		inventory.emptyInventory();

		crabMaterial = GetComponent<Renderer>().material;

		team = GetComponent<Team>();
		if (team.team == -1)
		{
			setNeutralComponent(type);
		}

		loadStats();
		crabAgent = GetComponent<NavMeshAgent>();
		crabAgent.speed = maxSpeed;
        GetComponent<Attackable>().setHealth(maxHealth);

        timeConsumed = 0.0f;
		actionStates = new StateController(actionStateList);
		weaponStates = new StateController(weaponStateList);
		target = null;
		secondTarget = null;
		destination = new Vector3();
		upgradeType = 0;
        mainCastle = null;
        destinationMarker = null;
        ghostBuilderReference = null;
        selected = false;
		selectedCrabs = 1;
		level = 1;
		experience = 0;

		recruitmentSacrifices = 0;

		currentCraft = "none";
		weaponToTake = "none";
		resourceToTake = "none";

        debug = GetComponent<DebugComponent>().debug;
		inventory.debug = debug;
		actionStates.debug = debug;
		weaponStates.debug = debug;

        attackTimer = new Timer(attackSpeed);
        repairTimer = new Timer(timeToRepair);
        rebuildTimer = new Timer(timeToRebuild);
        dismantleTimer = new Timer(attackSpeed);
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update ()
	{
		if (mainCastle == null) 
		{
			CastleController[] castles = FindObjectsOfType<CastleController>();
			for (int i = 0; i < castles.Length; i++) {
				if (castles[i].gameObject.GetComponent<Team>().team == GetComponent<Team>().team)
				{
					mainCastle = castles[i];
					break;
				}
			}
		}

		// updates UI
		currentCraft = getCraftableType();

		//level up
		if (experience >= nextLevelExperience)
			levelUp();

		// update what weapons the crab is carrying
		inventory.setWeaponFlags(weaponStates);

		if (target == null)
			actionStates.setState("Attacking", false);

		if (Input.GetKeyDown(KeyCode.Space) && selected)
			actionStates.printStates();

		if (!actionStates.isIdle())
		{
			checkForInteractions();
		} 
		else 
		{
			// The code below is very slow
			// findEnemyCrabs is a major performance issue MUST FIX!!!!
            /*
			GameObject enemyCrab = null;

			//// if not neutral
			if (team.team != neutralTeamCode)
				enemyCrab = findEnemyCrabs ();

			if (enemyCrab != null)
				startAttack (enemyCrab);*/
		}
			
	}

	/// <summary>
	/// Call before destroying the crab.
	/// </summary>
	public void destroyed () 
	{
		if (debug)
			Debug.Log("Crab died!");
		inventory.dropInventory(gameObject);
		AudioSource.PlayClipAtPoint(deathClip, transform.position);

		if (target)
			target.SendMessage("enemyDied", SendMessageOptions.DontRequireReceiver);
		if (controller)
			controller.GetComponent<Player>().deselect(gameObject);
	}

    public void OnDisable()
    {
        Destroy(destinationMarker);
    }

    #endregion

    #region checkForInteractions

    /// <summary>
    /// Acts based upon state
    /// </summary>
    void checkForInteractions ()
	{
		if (actionStates.getState("Attacking"))
		{
			if (canInteract())
				updateAttack();
			else
				startAttack(target);
		}

		if (isInBuildRange())
			actionStates.setState("Moving", false);

		if (actionStates.getState("Taking") && canInteract())
		{
			if (actionStates.getState("Building"))
				takeResource();
			else
				takeWeapon(weaponToTake);
		}

        // Building state is active
		if (actionStates.getState("Building"))
		{
            // if in range than transfer materials or build
			if (isInBuildRange())
			{
				updateGhostBuild();
			}
            // otherwise do something to help build it.
			else {
                // only do something if there is a ghost building
				if (currentGhostBuilding)
				{
					if (ghostBuilderReference == null) 
					{
						ghostBuilderReference = currentGhostBuilding.GetComponent<GhostBuilder> ();
					}
					
                    // if ghost building requires more material
					if (!ghostBuilderReference.canBuild())
					{

                        // check that inventory is still required
                        // if not, unload and start collecting next type
                        bool needsBoth = ghostBuilderReference.needsWood() && ghostBuilderReference.needsStone();

                        if (!actionStates.getState("Unloading"))
                        {
                            if (!needsBoth && ghostBuilderReference.needsWood() && inventory.contains(Tags.Stone))
                            {
                                startUnloading(mainCastle.gameObject);
                            }
                            else if (!needsBoth && ghostBuilderReference.needsStone() && inventory.contains(Tags.Wood))
                            {
                                startUnloading(mainCastle.gameObject);
                            }
                        }
						

                        // if idle get more resources
                        if (!actionStates.getState("Unloading") && !actionStates.getState("Collecting"))
                        {
                            if (inventory.empty())
                            {
                                if (ghostBuilderReference.needsWood())
                                {
                                    getMoreResource(Tags.Wood);
                                }
                                else if (ghostBuilderReference.needsStone())
                                {
                                    getMoreResource(Tags.Stone);
                                }
                            }
                        }
					}
				}
                // If ghost is gone then cancel
				else 
				{
					actionStates.clearStates();

					// if inventory is not empty, unload resources and go idle
					// else, go idle
					if (!inventory.empty())
					{
						startUnloading(mainCastle.gameObject);
					}
					else
					{
						crabAgent.isStopped = true;
					}
				}
			}
		}

        if (actionStates.getState("Rebuilding") && isInBuildRange())
            rebuildTimer.update(updateRebuild);

		if (actionStates.getState("Collecting") && canInteract())
			updateCollect();

		if (actionStates.getState("Unloading") && canInteract())
			updateUnload();

		if (actionStates.getState("Entering") && canInteract())
			updateEnter();

        if (actionStates.getState("Capturing") && canInteract())
        {
            if (!target.GetComponent<Enterable>().occupied())
                capture();
        }

		if (actionStates.getState("Recruiting") && canInteract())
			recruit();

		if (actionStates.getState("Upgrading") && canInteract())
			updateUpgrade();

        if (actionStates.getState("Repairing") && canInteract())
            repairTimer.update(repair);
        
        if (actionStates.getState("Dismantling") && canInteract())
            dismantleTimer.update(attack);

        if (getDistanceToPosition(destination) < attackRange)
        {
            Destroy(destinationMarker);
        }
	}

	#endregion

	#region Move functions
	// ================================================================================================
	// Move Code
	/// <summary>
	/// Starts to move crab.
	/// </summary>
	/// <param name="dest">Destination.</param>
	public void startMove (Vector3 dest) 
	{
		if (debug)
			Debug.Log("Crab moving to " + dest);

		crabAgent.isStopped = false;
		crabAgent.ResetPath();
		crabAgent.destination = dest;

		destination = dest;

        if (!destinationMarker)
        {
            destinationMarker = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Etc/DestinationMarker"), destination, Quaternion.identity);
        }
        else
        {
            destinationMarker.transform.position = destination;
        }

        actionStates.setState("Moving", true);

		//TODO: play walking animation
	}

    public void startNewMove (Vector3 dest)
    {
        startMove(dest);
        actionStates.clearStates();
        actionStates.setState("Moving", true);
    }

	/// <summary>
	/// Stops crab from moving.
	/// </summary>
	public void stopMove ()
	{
		crabAgent.isStopped = true;
		crabAgent.ResetPath();
		actionStates.setState("Moving", false);
        Destroy(destinationMarker);
	}

	#endregion

	#region Attack functions

	// ================================================================================================
	// Attack Code
	/// <summary>
	/// Starts crab attack.
	/// </summary>
	/// <param name="attackTarget">Attack target.</param>
	public void startAttack (GameObject attackTarget) 
	{
		if (debug)
		{
			Debug.Assert(attackTarget.GetComponent<Attackable>());
			Debug.Log("Started attack!");
		}

		target = attackTarget;

		if ((!validTarget()) || !targetIsEnemy()) 
		{
			if (debug)
				Debug.Log("Invalid Target");
			target = null;
			return;
		}
			
		if (IdUtility.isMoveable(target.tag))
			target.SendMessage("setAttacker", gameObject, SendMessageOptions.DontRequireReceiver);

		startMove(target.transform.position);
		
		actionStates.setState("Attacking", true);
	}

	/// <summary>
	/// Attacks target when time has passed.
	/// </summary>
	void attack () 
	{
		if (debug)
			Debug.Log("Crab attack");

		float finalAttack = attackDamage;

		if (weaponStates.getState(Tags.Spear))
			finalAttack += spearAttack;
		else if (weaponStates.getState(Tags.Hammer))
			finalAttack += hammerAttack;
		else if (weaponStates.getState(Tags.Bow))
			finalAttack = bowAttack;
		
		if (target.tag != Tags.Crab)
			target.GetComponent<Attackable>().attacked(finalAttack);
		else
			target.GetComponent<CrabController>().attacked(finalAttack);
		
		AudioSource.PlayClipAtPoint(hitClip, transform.position);
		timeConsumed = 0.0f;
		crabAgent.isStopped = false;
	}

	/// <summary>
	/// Calls attack function.
	/// </summary>
	void updateAttack ()
	{
		if (debug)
			Debug.Log("Attacking");
		crabAgent.isStopped = true;
        attackTimer.update(attack);
	}

	/// <summary>
	/// Is target object valid?
	/// </summary>
	/// <returns><c>true</c>, if target is valid, <c>false</c> otherwise.</returns>
	bool validTarget () 
	{
		return (target.tag == Tags.Block || target.tag == Tags.Castle ||
				IdUtility.isMoveable(target.tag) || IdUtility.isBuilding(target.tag));
	}

	/// <summary>
	/// Is target an enemy?
	/// </summary>
	/// <returns><c>true</c>, if target is enemy, <c>false</c> otherwise.</returns>
	bool targetIsEnemy () 
	{
		int tempTeam;

		if (IdUtility.isResource(target.tag))
			return true;
		else
			tempTeam = target.GetComponent<Team>().team;

		return (tempTeam != team.team);
	}

	/// <summary>
	/// Finds the first enemy crab in range.
	/// </summary>
	/// <returns>The enemy crab.</returns>
	GameObject findEnemyCrabs ()
	{
		CrabController[] crabs = FindObjectsOfType<CrabController>();

		for(int i = 0; i < crabs.Length; i++)
		{
			int crabTeam = crabs[i].GetComponent<Team>().team;
			if (crabTeam != team.team && crabTeam != neutralTeamCode)
			{
				float dist = getDistanceToObject(crabs[i].gameObject);

				if (dist <= (attackRange * 3))
                {
					return crabs[i].gameObject;
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Decrease health by amount.
	/// </summary>
	/// <param name="damage">Damage amount.</param>
	public void attacked (float damage) 
	{
		cancelCapture();

		if (weaponStates.getState(Tags.Shield))
			damage *= shieldDefense;
		
		GetComponent<Attackable>().attacked(damage);
		//TODO: play damaged animation
		startAttack(target);
	}

	/// <summary>
	/// Sets the attacker object.
	/// </summary>
	/// <param name="enemy">Enemy object.</param>
	public void setAttacker (GameObject enemy) 
	{
		target = enemy;
	}

	/// <summary>
	/// Called by enemy crab upon death
	/// </summary>
	public void enemyDied () 
	{
		experience += levelGain;
		actionStates.clearStates();
		target = null;
	}

    #endregion

    #region Dismantle functions

    public void startDismantling (GameObject structure)
    {
        if (debug)
            Debug.Log("Started Dismantling");

        target = structure;
        dismantleTimer = new Timer(attackSpeed);

        startMove(target.transform.position);

        actionStates.setState("Dismantling", true);
    }

    #endregion

    #region Unload functions

    // ===================================================================================================
    // Unload code
    /// <summary>
    /// Starts unloading of resources into a castle.
    /// </summary>
    /// <param name="castle">Castle object.</param>
    public void startUnloading (GameObject unloadTarget) 
	{
		if (debug)
			Debug.Assert(unloadTarget.tag == Tags.Castle || unloadTarget.tag == Tags.Ghost);

		target = unloadTarget;

		startMove(target.transform.position);

		actionStates.setState("Unloading", true);
		if (secondTarget)
			actionStates.setState("Collecting", true);
	}

	/// <summary>
	/// Unloads resources.
	/// </summary>
	void unload () 
	{
		for (int i = 0; i < inventory.inventory.Length; i++) 
		{
			if (inventory.inventory[i] != null && IdUtility.isResource(inventory.inventory[i])) 
			{
				target.GetComponent<CastleController>().give(inventory.inventory[i]);
				inventory.inventory[i] = null;
			}
		}

		actionStates.setState("Unloading", false);
		crabAgent.isStopped = true;
	}

	/// <summary>
	/// Unloads resources and commands crab to collect more resources.
	/// </summary>
	void updateUnload ()
	{
		if (debug)
			Debug.Log("Unloading");

		unload();
		inventory.sortInventory();

		if (actionStates.getState("Collecting"))
		{
			if (actionStates.getState("Building"))
			{
				buildFromGhost(currentGhostBuilding);
			}
			else
			{
                if (secondTarget != null)
                {
                    GameObject newSecond = target;
                    startCollecting(secondTarget);
                    secondTarget = newSecond;
                }
			}
		}

		if (selected)
			controller.GetComponent<Player>().getInventory(inventory.inventory);
	}

	#endregion

	#region Build functions

	// ===================================================================================================
	// Build code
	/// <summary>
	/// Starts crab building
	/// </summary>
	/// <param name="type">A valid building type.</param>
	/// <param name="pos">Position.</param>
	public void startBuild (string type, Vector3 pos) 
	{
		if (debug)
			Debug.Log("Building " + type + ".");

		buildingType = type;
		timeConsumed = 0.0f;

        startMove(pos);

		actionStates.setState("Building", true);
	}

	/// <summary>
	/// Builds from ghost.
	/// </summary>
	/// <param name="ghostObject">Ghost object.</param>
	public void buildFromGhost (GameObject ghostObject) 
	{
		buildingType = ghostObject.name.Remove(0, 5);
		currentGhostBuilding = ghostObject;
		timeConsumed = 0.0f;
		ghostBuilderReference = ghostObject.GetComponent<GhostBuilder>();

		startMove(ghostObject.transform.position);
        
		actionStates.setState("Building", true);
	}

	/// <summary>
	/// Builds from block.
	/// </summary>
	/// <param name="obj">Block object.</param>
	/// <param name="newType">Tower or gate.</param>
	public void startRebuild (GameObject obj, string newType)
	{
		buildingType = newType;
        rebuildTimer = new Timer(timeToRebuild);
        actionStates.setState("Rebuilding", true);
		currentGhostBuilding = obj;
		timeConsumed = 0.0f;

		startMove(obj.transform.position);
	}

	/// <summary>
	/// Check ability to build.
	/// </summary>
	void updateGhostBuild () 
	{
		if (debug)
			Debug.Log("Building from ghost");

		GhostBuilder builder = currentGhostBuilding.GetComponent<GhostBuilder>();

		if (builder.canBuild()) 
		{
			build();
		}
		else
		{
			if (builder.woodAmount < builder.woodRequirement)
			{
				// get wood

				// Check inventory
				// If enough put into building ghost
				if (inventory.contains(Tags.Wood))
				{
					int amount = inventory.countInventory(Tags.Wood);

					builder.woodAmount += amount;
					inventory.emptyInventory(Tags.Wood);

					if (builder.canBuild())
					{
						build();
						return;
					}
				}

				// Check castle
				// if enough get wood
				getMoreResource(Tags.Wood);
			}
			else if (builder.stoneAmount < builder.stoneRequirement)
			{
				// get stone

				// Check inventory
				// If enough put into building ghost
				if (inventory.contains(Tags.Stone))
				{
					int amount = inventory.countInventory(Tags.Stone);

					builder.stoneAmount += amount;
					inventory.emptyInventory(Tags.Stone);

					if (builder.canBuild()) {
						build();
						return;
					}
				}

				// Check castle
				// if enough get stone
				getMoreResource(Tags.Stone);
			}
		}
	}

	/// <summary>
	/// Check ability to rebuild.
	/// </summary>
	void updateRebuild()
	{
		if (debug)
			Debug.Log("Building from wall segment!");

		actionStates.clearStates();
		timeConsumed = 0.0f;

		Transform blockTransform = currentGhostBuilding.transform;
		Destroy(currentGhostBuilding);
		Instantiate(Resources.Load("Prefabs/Structures/" + buildingType), blockTransform.position, blockTransform.rotation);
	}

	/// <summary>
	/// Sets the number of crabs selected.
	/// </summary>
	/// <param name="crabs">Number of crabs.</param>
	public void setCrabs (int crabs) 
	{
		selectedCrabs = crabs;
	}

	void build() 
	{
		actionStates.clearStates ();
		timeConsumed = 0.0f;

		ghostBuilderReference.build ();
	}

	void getMoreResource(string resource) 
	{
		if (mainCastle.canTake (resource)) {
			startTake (mainCastle.gameObject, resource);
		}
        else
        {
			startCollecting (InfoTool.closestObjectWithTag (gameObject, resource));
		}
	}

	#endregion

	#region Upgrade functions

	// ===================================================================================================
	// Upgrade code
	/// <summary>
	/// Starts crab upgrading.
	/// </summary>
	/// <param name="obj">Castle or block object.</param>
	public void startUpgrading (GameObject obj)
	{
		if (debug)
			Debug.Log("Started upgrading");

		target = obj;

		if (inventory.isAllOneType(Tags.Wood))
			upgradeType = wallUpgradeType.WOOD;
		else if (inventory.isAllOneType(Tags.Stone))
			upgradeType = wallUpgradeType.STONE;
		else
			upgradeType = wallUpgradeType.NORMAL;

		startMove(obj.transform.position);

		actionStates.setState("Upgrading", true);
	}

	/// <summary>
	/// Check ability to upgrade.
	/// </summary>
	void updateUpgrade ()
	{
		if (debug)
			Debug.Log("Upgrading");

		target.GetComponent<WallUpgrade>().startWallUpgrade(upgradeType, inventory);
		target.GetComponent<WallUpgrade>().setCrabs(selectedCrabs);
		inventory.emptyInventory();

		if (selected)
			controller.GetComponent<Player>().getInventory(inventory.inventory);
		
		actionStates.clearStates();
	}

	#endregion

	#region Collect functions

	// ===================================================================================================
	// Collect code
	/// <summary>
	/// Starts crab collecting.
	/// </summary>
	/// <param name="obj">Wood, stone, or weapon object.</param>
	public void startCollecting (GameObject obj)
	{
		if (debug)
			Debug.Log("Started collecting");

        if (mainCastle != null && inventory.full())
        {
            secondTarget = obj;
            startUnloading(mainCastle.gameObject);
        }
        else
        {
            target = obj;
            startMove(obj.transform.position);
            actionStates.setState("Collecting", true);
        }
	}

	/// <summary>
	/// Collects until inventory is full.
	/// </summary>
	IEnumerator collect ()
	{
		while (canInteract() && !inventory.full() && actionStates.getState("Collecting")) 
		{
			if (debug)
				Debug.Log("Taking " + target.tag + ".");

			inventory.addToInventory(target.tag);

			target.GetComponent<ResourceController>().take();

			yield return null;
		}
	}

	/// <summary>
	/// Collects the weapon.
	/// </summary>
	void collectWeapon () 
	{
		if (debug)
			Debug.Log("Taking weapon.");

		inventory.addToInventory(target.tag);
		weaponStates.setState(target.tag, true);
		Destroy(target);

		target = null;
	}

	/// <summary>
	/// Checks ability to collect.
	/// </summary>
	void updateCollect ()
	{
		if (debug)
			Debug.Log("Collecting");

		if (target == null)
		{
			goIdle();
			return;
		}

		string targetTag = target.transform.tag;

		if (IdUtility.isResource(targetTag))
		{
			StartCoroutine(collect());
			if (inventory.full())
			{
				secondTarget = target;

				if (actionStates.getState("Building"))
				{
					buildFromGhost(currentGhostBuilding);
				}
				else
				{
					startUnloading(mainCastle.gameObject);
				}
			}
		}
		else if (IdUtility.isWeapon(targetTag))
		{
			actionStates.clearStates();
			collectWeapon();
		}
		inventory.sortInventory();

		if (selected)
			controller.GetComponent<Player>().getInventory(inventory.inventory);
	}

	#endregion

	#region Take functions

	// ===================================================================================================
	// Taking weapon code
	/// <summary>
	/// Starts crab taking a weapon.
	/// </summary>
	/// <param name="weapon">Weapon tag.</param>
	public void startTakeWeapon (string weapon)
	{
		weaponToTake = weapon;
		actionStates.setState("Taking", true);

		target = controller.targetedArmoury;

		startMove(target.transform.position);
	}

    public void startTakeWeapon (string weapon, GameObject armoury)
    {
        weaponToTake = weapon;
        actionStates.setState("Taking", true);

        target = armoury;

        startMove(target.transform.position);
    }

	/// <summary>
	/// Takes the weapon.
	/// </summary>
	/// <param name="weapon">Weapon tag.</param>
	void takeWeapon (string weapon)
	{
		if (controller.targetedArmoury.GetComponent<HoldsWeapons>().requestWeapon(gameObject, weapon))
		{
			inventory.addToInventory(weapon);
			weaponToTake = "none";
			actionStates.setState("Taking", false);
		}
		else
		{
			if (debug)
				Debug.Log("Not enough Resources!");
		}
	}

	/// <summary>
	/// Starts crab taking resources from castle.
	/// </summary>
	/// <param name="castle">Castle object.</param>
	/// <param name="takeTag">Wood or stone.</param>
	void startTake (GameObject castle, string takeTag)
	{
		resourceToTake = takeTag;
		actionStates.setState("Taking", true);

		target = castle;

		startMove(target.transform.position);
	}

	/// <summary>
	/// Takes the resource from castle.
	/// </summary>
	void takeResource ()
	{
		while (!inventory.full())
		{
			if (mainCastle.canTake(resourceToTake))
			{
				mainCastle.take(resourceToTake);
				inventory.addToInventory(resourceToTake);
			}
			else
				break;
		}

		if (actionStates.getState ("Building")) {
            secondTarget = target;
            startMove(currentGhostBuilding.transform.position);
            actionStates.setState("Taking", false);
        }
        else
        {
            resourceToTake = "none";
            actionStates.setState("Taking", false);
        }
	}

	#endregion

	#region Enter functions

	// ===================================================================================================
	// Entering castle code
	/// <summary>
	/// Starts crab entering a tower or castle.
	/// </summary>
	/// <param name="enterable">Enterable script.</param>
	public void startEnter (GameObject enterable) 
	{
		if (debug)
		{
			Debug.Assert(enterable.GetComponent<Enterable>());
			Debug.Log("Started entering");
		}

		target = enterable;

		startMove(target.transform.position);

		actionStates.setState("Entering", true);
	}

	/// <summary>
	/// Requests entry to enterable.
	/// </summary>
	void enter () 
	{
		target.GetComponent<Enterable>().requestEntry(gameObject);
	}

	/// <summary>
	/// Checks ability to enter.
	/// </summary>
	void updateEnter ()
	{
		if (debug)
			Debug.Log("Entering");

		enter();
		actionStates.clearStates();
	}

	#endregion

	#region Capture functions

	// ==========================================================================================================
	// Capturing castle code
	/// <summary>
	/// Starts crab capturing the castle.
	/// </summary>
	/// <param name="castle">Castle object.</param>
	public void captureCastle (GameObject castle)
	{
		if (debug)
			Debug.Assert(castle.tag == Tags.Castle);

		target = castle;
		timeToCapture = target.GetComponent<CastleController>().getResistanceTime();

		startMove(target.transform.position);

		actionStates.setState("Capturing", true);
	}

	/// <summary>
	/// Captures castle when time is up.
	/// </summary>
	void capture () 
	{
		if (debug)
		{
			Debug.Log("Capturing");
			Debug.Log(timeConsumed + " / " + (timeToCapture / selectedCrabs));
		}

		timeConsumed += Time.deltaTime;
		if (timeConsumed >= (timeToCapture / selectedCrabs) && !target.GetComponent<Enterable>().occupied())
		{
			if (debug)
				Debug.Log("Captured castle");

			target.GetComponent<Team>().team = team.team;

			cancelCapture();
			target.GetComponent<Enterable>().requestEntry(gameObject);
		}
		else if (isCapturing() && !canInteract() || target.gameObject.tag != Tags.Castle)
			cancelCapture();
	}

	/// <summary>
	/// Cancels the capture.
	/// </summary>
	void cancelCapture () 
	{
		timeConsumed = 0.0f;
		actionStates.clearStates();
	}

	/// <summary>
	/// Is crab capturing castle?
	/// </summary>
	/// <returns><c>true</c>, if capture is in progress, <c>false</c> otherwise.</returns>
	bool isCapturing () 
	{
		return ((timeConsumed > 0.0f) && (timeConsumed < (timeToCapture / selectedCrabs)));
	}

	#endregion

	#region Recruit functions

	// ===================================================================================================
	// Recruiting code
	/// <summary>
	/// Starts crab recruiting.
	/// </summary>
	/// <param name="crab">Crab object.</param>
	public void startRecruiting (GameObject crab) 
	{
		if (debug)
			Debug.Assert(crab.tag == Tags.Crab);

		target = crab;

		startMove(crab.transform.position);

		actionStates.setState("Recruiting", true);
	}

	/// <summary>
	/// Recruits crab.
	/// </summary>
	void recruit () 
	{
		// check requirements
		// if good then
			// run dialog
			// change crab team
		if (debug)
			Debug.Log("Recruiting");

		if (canRecruit()) 
		{	
			if (debug)
				Debug.Log("Recruitment successful!");
			target.GetComponent<CrabController>().team = team;
		}
		else
		{
			if (debug)
				Debug.Log("Recruitment unsuccessful!");

			if (target.GetComponent<SacrificeNeutralCrab>() != null)
			{
				target.GetComponent<SacrificeNeutralCrab>().startSacrifice(gameObject);
			}
		}
		actionStates.clearStates();
	}

	/// <summary>
	/// Checks crab type requirements.
	/// </summary>
	/// <returns><c>true</c>, if crab is recruitable, <c>false</c> otherwise.</returns>
	bool canRecruit () 
	{
		switch (target.GetComponent<CrabController>().type) {
		case CrabSpecies.ROCK:
		case CrabSpecies.KAKOOTA:
		case CrabSpecies.CALICO:
		case CrabSpecies.SEAWEED:
			return true;
		case CrabSpecies.FIDDLER:
			return hasCastles(1);
		case CrabSpecies.SPIDER:
			return hasCastles(2);
		case CrabSpecies.TGIANT:
		case CrabSpecies.COCONUT:
			return target.GetComponent<SacrificeNeutralCrab>().canRecruit();
		case CrabSpecies.HORSESHOE:
			return hasVariedSpecies(2);
		default:
			if (debug)
				Debug.Log("Impossible crab type: " + target.GetComponent<CrabController>().type);
			return false;
		}
	}
		
	/// <summary>
	/// Are there enough friendly castles?
	/// </summary>
	/// <returns><c>true</c>, if there are a correct number of friendly castles, <c>false</c> otherwise.</returns>
	/// <param name="numOfCastles">Number of castles.</param>
	bool hasCastles (int numOfCastles) 
	{
		CastleController[] castles = FindObjectsOfType<CastleController>();
		int friendlyCastles = 0;

		for (int i = 0; i < castles.Length; i++) 
		{
			if (castles[i].GetComponent<Team>().team == team.team)
				friendlyCastles++;
			
			if (friendlyCastles == numOfCastles)
				return true;
		}
		return false;
	}
		
	/// <summary>
	/// Are there enough kinds of crabs?
	/// </summary>
	/// <returns><c>true</c>, if number of different crab types are correct, <c>false</c> otherwise.</returns>
	/// <param name="numOfSpecies">Number of species.</param>
	bool hasVariedSpecies (int numOfSpecies) 
	{
		CrabController[] crabs = FindObjectsOfType<CrabController>();
		var crabSpecies = new HashSet<CrabSpecies>();

		for (int i = 0; i < crabs.Length; i++) 
		{
			if (crabs[i].GetComponent<Team>().team == team.team) 
			{
				if (!crabSpecies.Contains(crabs[i].GetComponent<CrabController>().type))
					crabSpecies.Add(crabs[i].GetComponent<CrabController>().type);
			}
		}

		return (crabSpecies.Count == numOfSpecies);
	}

	#endregion

	#region Repair Functions

	// ===================================================================================================
	// Repairing code
	/// <summary>
	/// Starts crab repairing object.
	/// </summary>
	/// <param name="repairTarget">Repair target.</param>
	public void startRepair (GameObject repairTarget) 
	{
		target = repairTarget;
        repairTimer = new Timer(timeToRepair);

		startMove(repairTarget.transform.position);

		actionStates.setState("Repairing", true);
	}

	/// <summary>
	/// Repairs object when time is up.
	/// </summary>
	void repair () 
	{
		if (debug)
			Debug.Log("Repairing");
        
		target.SendMessage("repair", rebuildAmount, SendMessageOptions.DontRequireReceiver);
	}

	#endregion

	#region Distance functions

	// ===================================================================================================
	// checks distance to object
	/// <summary>
	/// Is crab close enough to the target?
	/// </summary>
	/// <returns><c>true</c>, if crab can interact, <c>false</c> otherwise.</returns>
	bool canInteract () 
	{
		float tempRange = attackRange;

		if (actionStates.getState("Attacking")) 
		{
			if (weaponStates.getState(Tags.Bow))
				tempRange += bowRange;
			else if (weaponStates.getState(Tags.Spear))
				tempRange += spearDistance;
		}

		return (getDistanceToObject (target) < tempRange);
	}

	/// <summary>
	/// Like canInteract but without the range modifiers.
	/// </summary>
	/// <returns><c>true</c>, if in build range, <c>false</c> otherwise.</returns>
	bool isInBuildRange ()
	{
		return (getDistanceToPosition(destination) < attackRange);
	}

	/// <summary>
	/// Gets the distance to position from crab.
	/// </summary>
	/// <returns>The distance to position.</returns>
	/// <param name="pos">Position.</param>
	float getDistanceToPosition (Vector3 pos)
	{
		return Vector3.Distance(transform.position, pos);
	}

	/// <summary>
	/// Gets the distance to object from crab.
	/// </summary>
	/// <returns>The distance to object.</returns>
	/// <param name="obj">Object.</param>
	public float getDistanceToObject (GameObject obj) 
	{
		if(obj == null)
		{
			goIdle();
			return 0.0f;
		}

		Vector3 crabPosition = transform.position;
		Vector3 objectPosition = obj.transform.position;
		RaycastHit[] hits = Physics.RaycastAll(new Ray(crabPosition, objectPosition - crabPosition));

		for (int i = 0; i < hits.Length; i++) 
		{
			if (hits[i].collider.gameObject == obj)
				return hits[i].distance;
		}

		return float.MaxValue; // Most cases should find object so return value that will fail
	}

	#endregion

	#region Crafting functions

	// ===================================================================================================
	// Crafting functions

	/// <summary>
	/// Parses inventory and returns object that is buildable.
	/// </summary>
	/// <returns>Hammer, Spear, Shield, or Bow.</returns>
	public string getCraftableType () 
	{
		int stones = 0, sticks = 0;

		for (int i = 0; i < inventory.inventory.Length; i++) 
		{
			if (inventory.inventory[i] == null)
				continue;
			
			if (inventory.inventory[i] == Tags.Stone)
				stones++;
			else if (inventory.inventory[i] == Tags.Wood)
				sticks++;
		}

		if (stones == 2 && sticks == 1)
			return Tags.Hammer;
		else if (stones == 1 && sticks == 2)
			return Tags.Spear;
		else if (sticks == 3)
			return Tags.Shield;
		else if (sticks == 2)
			return Tags.Bow;
		else
			return "None";
	}

	/// <summary>
	/// Craft the specified type.
	/// </summary>
	/// <param name="type">Hammer, Spear, Shield, or Bow.</param>
	public void craft (string type) 
	{
		switch (type) {
		case Tags.Hammer:
			inventory.removeItem(Tags.Stone);
			inventory.removeItem(Tags.Stone);
			inventory.removeItem(Tags.Wood);
			break;
		case Tags.Spear:
			inventory.removeItem(Tags.Stone);
			inventory.removeItem(Tags.Wood);
			inventory.removeItem(Tags.Wood);
			break;
		case Tags.Shield:
			inventory.removeItem(Tags.Wood);
			inventory.removeItem(Tags.Wood);
			inventory.removeItem(Tags.Wood);
			break;
		case Tags.Bow:
			inventory.removeItem(Tags.Wood);
			inventory.removeItem(Tags.Wood);
			break;
		}

		inventory.addToInventory(type);
	}

    #endregion

    /// <summary>
    /// Is a player crab near?
    /// </summary>
    /// <returns><c>true</c>, if player crab is near, <c>false</c> otherwise.</returns>
    public bool isCrabNear ()
	{
		CrabController[] crabs = FindObjectsOfType<CrabController>();

		for (int i = 0; i < crabs.Length; i++)
		{
			float dist = getDistanceToObject(crabs[i].gameObject);

			if (dist <= (attackRange * awarenessMultiplier))
			{
				if (debug)
					Debug.Log("Distance to " + crabs[i].name + " is " + dist);
				
				if (crabs[i].GetComponent<Team>().team == 0)
				{
					if (debug)
						Debug.Log("Found Crab!");
					
					return true;
				}
			}
		}

		return false;
	}

	#region UI functions

	// ===================================================================================================
	// Internal functions
	/// <summary>
	/// Updates the UI.
	/// </summary>
	/// <param name="gui">GUI script.</param>
	public void updateUI (GUIController gui)
	{
		gui.craftButton.gameObject.SetActive(currentCraft != "None");
		gui.subCraftButton.gameObject.SetActive(currentCraft == Tags.Shield);
		GetComponent<Attackable>().setHealth(gui.healthSlider);
		gui.gameObject.GetComponent<Player>().getInventory(inventory.inventory);

		gui.crabLevelText.text = "Level: " + level;

		gui.expSlider.value = experience;
		gui.expSlider.minValue = 0;
		gui.expSlider.maxValue = nextLevelExperience;

		if (currentCraft == Tags.Shield)
			gui.craftButton.GetComponentInChildren<Text>().text = "Craft Shield";
		else
			gui.craftButton.GetComponentInChildren<Text>().text = "Craft";
	}

	/// <summary>
	/// Gets the inventory as a string array.
	/// </summary>
	/// <returns>The inventory array.</returns>
	public string[] getInventory () 
	{
		inventory.sortInventory();
		return inventory.inventory;
	}

	/// <summary>
	/// Toggles the selected.
	/// </summary>
	public void toggleSelected () 
	{
		selected = !selected;
	}

	/// <summary>
	/// Sets the controller.
	/// </summary>
	/// <param name="player">Player script.</param>
	public void setController (Player player) 
	{
		controller = player;
		selected = true;
	}

	#endregion

	#region Internal functions

	public void goIdle ()
	{
		stopMove();
		actionStates.clearStates();
	}

	/// <summary>
	/// Levels up crab.
	/// </summary>
	void levelUp () 
	{
		level++;
		experience = 0;
		nextLevelExperience = level * nextLevelExperience;

		attackDamage += damageIncrease;
		maxHealth += healthIncrease;
	}

	#endregion

	#region Public info functions

	/// <summary>
	/// Is the crab busy?
	/// </summary>
	/// <returns><c>true</c>, if crab is busy, <c>false</c> otherwise.</returns>
	public bool isBusy () 
	{
		return !actionStates.isIdle();
	}

	/// <summary>
	/// Gets the type as a string.
	/// </summary>
	/// <returns>The crab type.</returns>
	public string getType ()
	{
		switch (type) {
		case CrabSpecies.ROCK:
			return "Rock Crab";
		case CrabSpecies.FIDDLER:
			return "Fiddler Crab";
		case CrabSpecies.TGIANT:
			return "Tasmanian Crab";
		case CrabSpecies.SPIDER:
			return "Spider Crab";
		case CrabSpecies.COCONUT:
			return "Coconut Crab";
		case CrabSpecies.HORSESHOE:
			return "Horseshoe Crab";
		case CrabSpecies.SEAWEED:
			return "Seaweed Crab";
		case CrabSpecies.CALICO:
			return "Calico Crab";
		case CrabSpecies.KAKOOTA:
			return "Kakoota Crab";
		case CrabSpecies.TRILOBITE:
			return "Trilobite";
		default:
			return "null";
		}
	}

	#endregion

	#region Stat init functions

	/// <summary>
	/// Sets the stats.
	/// </summary>
	/// <param name="mHealth">Maxhealth.</param>
	/// <param name="mSpeed">Maxspeed.</param>
	/// <param name="aDamage">Attackdamage.</param>
	/// <param name="aSpeed">Attackspeed.</param>
	void setStats (float mHealth, float mSpeed, float aDamage, float aSpeed) 
	{
		maxHealth = mHealth;
		maxSpeed = mSpeed;
		attackDamage = aDamage;
		attackSpeed = aSpeed;
	}

	/// <summary>
	/// Loads the stats.
	/// </summary>
	void loadStats () 
	{
        CrabStats stats = CrabStats.load(Path.Combine(Application.dataPath, "GameData/crabStats.xml"));

        int pos = (int)type;

        float health = float.Parse(stats.Health[pos].Text);
        float movementSpeed = float.Parse(stats.MovementSpeed[pos].Text);
        float attackDamage = float.Parse(stats.AttackDamage[pos].Text);
        float attackSpeed = float.Parse(stats.AttackSpeed[pos].Text);

        float scaleX = float.Parse(stats.ScaleFactor[pos].x);
        float scaleY = float.Parse(stats.ScaleFactor[pos].y);
        float scaleZ = float.Parse(stats.ScaleFactor[pos].z);

        gameObject.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        setStats(health, movementSpeed, attackDamage, attackSpeed);

        if (type == CrabSpecies.CALICO)
        {
            crabMaterial = Resources.Load<Material>("Materials/Crabs/Calico");
        }
        else if (type == CrabSpecies.KAKOOTA)
        {
            if (crabMaterial != null)
                crabMaterial.color = Color.red;
            else if (debug)
                Debug.Log("Crab material is null.");
        }
	}

	void setNeutralComponent(CrabSpecies species)
	{
		switch (species)
		{
		case CrabSpecies.TGIANT:
			gameObject.AddComponent<SacrificeNeutralCrab>();
			GetComponent<SacrificeNeutralCrab>().sacrificesRequired = 5;
			break;
		case CrabSpecies.COCONUT:
			gameObject.AddComponent<SacrificeNeutralCrab>();
			GetComponent<SacrificeNeutralCrab>().sacrificesRequired = 2;
			break;
		}
	}
}

#endregion