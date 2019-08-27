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
	public CrabSpecies Type;				// type of crab
	[Tooltip("The range of the crab")]
	public float AttackRange;				// distance to hit

    [Header("Sound Clips:")]
	[Tooltip("Sound to play on death")]
	public AudioClip DeathClip;
	[Tooltip("Sound to play on hit")]
	public AudioClip HitClip;

	[Header("Time variables:")]
	[Tooltip("Time to build things")]
	public float TimeToBuild;				// time to build object
	[Tooltip("Time to upgrade things")]
	public float TimeToUpgrade;				// time to upgrade block
    [Tooltip("Time to rebuild")]
    public float TimeToRebuild;
    [Tooltip("Time to repair")]
    public float TimeToRepair;

    //weapon values
    [Header("Weapon Values:")]
    const float _spearAttack = 1.0f;
    const float _spearDistance = 1.5f;
    const float _shieldDefense = 0.5f;
    const float _hammerAttack = 1.0f;
    const float _bowAttack = 1.5f;
    const float _bowRange = 10.0f;

    [Header("Level stat increases:")]
    public float DamageIncrease;
    public float HealthIncrease;

    [Header("Other variables:")]
    [Tooltip("Amount that is restored to a building being rebuilt")]
    public float RebuildAmount;
    [Tooltip("Experience needed to reach next level")]
    public int NextLevelExperience;
    [Tooltip("Experience gained by kill")]
    public int LevelGain;
    [Tooltip("Distance that a crab can see.")]
    public int AwarenessMultiplier;

    #endregion

    #region private variables

    // stats decided by data in crabStats.xml
    float _maxHealth;				// maximum health of crab
	float _maxSpeed;					// speed of crab used by agent
	float _attackDamage;				// base damage of crab
	float _attackSpeed;				// time between attacks

	Team _team;                      // team of crab

	// action states
	public StateController ActionStates { get; set; }
	string[] _actionStateList = {"Moving", "Attacking", "Building", "Collecting", "Unloading", "Entering", "Capturing", "Recruiting", "Upgrading", "Repairing", "Taking", "Dismantling", "Rebuilding"};

    //weapon states
    StateController _weaponStates;
	string[] _weaponStateList = {Tags.Spear, Tags.Shield, Tags.Hammer, Tags.Bow};

	// building variables
	bool _rebuilding;				            // Is the crab rebuilding a wall or junction?
    GhostBuilder _ghostBuilderReference;         // reference to ghost buildings builder script   
    string _buildingType;			            // the selected type of building

	wallUpgradeType _upgradeType;				// type of upgrade for a block
    
	// interacting knowledge
	public Vector3 Destination { get; set; }	// current destination
	public GameObject Target { get; set; }		// current target object to attack, collect from, or capture
	GameObject _secondTarget; 					// A secondary target that may be used after another action finishes.
	Player _controller;							// reference to player object, null when not selected.

	bool _selected;								// is the crab selected?

	CastleController _mainCastle;				// The closest castle to the crab.

	// recruitment variables
	string _recruitmentUIName;					// name of the prefab file for the respective recruitment UI
	GameObject _recruitmentUI;					// Canvas of ui that appears when player approaches
	bool _playerIsNear;							// is player nearby?
	bool _uiOpen;								// is the UI open

	// TODO find good container
	public Inventory Inventory { get; set; }	// inventory holds objects

	NavMeshAgent _crabAgent;                     // reference to navmeshagent

    // time variables
    float _timeToCapture;
    float _timeConsumed;

	const int _neutralTeamCode = -1;

    Timer _attackTimer;
    Timer _repairTimer;
    Timer _rebuildTimer;
    Timer _dismantleTimer;

	// level variables
	int _level;
	int _experience;

	int _selectedCrabs;					// the amount of crabs selected

	Material _crabMaterial;				// material of the crab
		
	int _recruitmentSacrifices;			// number of sacrifices required to hire

	string _currentCraft;

	string _weaponToTake;
	string _resourceToTake;

	bool _debug;

	GameObject _destinationMarker;

	#endregion

	#region Monobehaviour functions

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		Inventory = new Inventory();
		Inventory.EmptyInventory();

		_crabMaterial = GetComponent<Renderer>().material;

		_team = GetComponent<Team>();
		if (_team.team == -1)
		{
			SetNeutralComponent(Type);
		}

		LoadStats();
		_crabAgent = GetComponent<NavMeshAgent>();
		_crabAgent.speed = _maxSpeed;
        GetComponent<Attackable>().SetHealth(_maxHealth);

        _timeConsumed = 0.0f;
		ActionStates = new StateController(_actionStateList);
		_weaponStates = new StateController(_weaponStateList);
		Target = null;
		_secondTarget = null;
		Destination = new Vector3();
		_upgradeType = 0;
        _mainCastle = null;
        _destinationMarker = null;
        _ghostBuilderReference = null;
        _selected = false;
		_selectedCrabs = 1;
		_level = 1;
		_experience = 0;

		_recruitmentSacrifices = 0;

		_currentCraft = "none";
		_weaponToTake = "none";
		_resourceToTake = "none";

        _debug = GetComponent<DebugComponent>().Debug;
		Inventory.Debug = _debug;
		ActionStates.Debug = _debug;
		_weaponStates.Debug = _debug;

        _attackTimer = new Timer(_attackSpeed);
        _repairTimer = new Timer(TimeToRepair);
        _rebuildTimer = new Timer(TimeToRebuild);
        _dismantleTimer = new Timer(_attackSpeed);
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update()
	{
		if (_mainCastle == null) 
		{
			CastleController[] castles = FindObjectsOfType<CastleController>();
			for (int i = 0; i < castles.Length; i++) {
				if (castles[i].gameObject.GetComponent<Team>().team == GetComponent<Team>().team)
				{
					_mainCastle = castles[i];
					break;
				}
			}
		}

		// updates UI
		_currentCraft = GetCraftableType();

		//level up
		if (_experience >= NextLevelExperience)
        {
            LevelUp();
        }

		// update what weapons the crab is carrying
		Inventory.SetWeaponFlags(_weaponStates);

		if (Target == null)
        {
            ActionStates.SetState("Attacking", false); 
        }

		if (Input.GetKeyDown(KeyCode.Space) && _selected)
        {
            ActionStates.printStates(); 
        }

		if (!ActionStates.IsIdle())
		{
			CheckForInteractions();
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
	public void Destroyed() 
	{
		if (_debug)
			Debug.Log("Crab died!");
		Inventory.DropInventory(gameObject);
		AudioSource.PlayClipAtPoint(DeathClip, transform.position);

		if (Target)
        {
            Target.SendMessage("enemyDied", SendMessageOptions.DontRequireReceiver); 
        }

		if (_controller)
        {
            _controller.Deselect(gameObject); 
        }
	}

    public void OnDisable()
    {
        Destroy(_destinationMarker);
    }

    #endregion

    #region checkForInteractions

    /// <summary>
    /// Acts based upon state
    /// </summary>
    void CheckForInteractions()
	{
		if (ActionStates.GetState("Attacking"))
		{
			if (CanInteract())
            {
                UpdateAttack(); 
            }
			else
            {
                StartAttack(Target); 
            }
		}

		if (IsInBuildRange())
			ActionStates.SetState("Moving", false);

		if (ActionStates.GetState("Taking") && CanInteract())
		{
			if (ActionStates.GetState("Building"))
            {
                TakeResource(); 
            }
			else
            {
                TakeWeapon(_weaponToTake); 
            }
		}

        // Building state is active
		if (ActionStates.GetState("Building"))
		{
            // if in range than transfer materials or build
			if (IsInBuildRange())
			{
				UpdateGhostBuild();
			}
            // otherwise do something to help build it.
			else {
                // only do something if there is a ghost building
				if (_ghostBuilderReference)
				{
                    // if ghost building requires more material
					if (!_ghostBuilderReference.CanBuild())
					{

                        // check that inventory is still required
                        // if not, unload and start collecting next type
                        bool needsBoth = _ghostBuilderReference.NeedsWood() && _ghostBuilderReference.NeedsStone();

                        if (!ActionStates.GetState("Unloading"))
                        {
                            if (!needsBoth && _ghostBuilderReference.NeedsWood() && Inventory.Contains(Tags.Stone))
                            {
                                StartUnloading(_mainCastle.gameObject);
                            }
                            else if (!needsBoth && _ghostBuilderReference.NeedsStone() && Inventory.Contains(Tags.Wood))
                            {
                                StartUnloading(_mainCastle.gameObject);
                            }
                        }
						

                        // if idle get more resources
                        if (!ActionStates.GetState("Unloading") && !ActionStates.GetState("Collecting"))
                        {
                            if (Inventory.Empty())
                            {
                                if (_ghostBuilderReference.NeedsWood())
                                {
                                    GetMoreResource(Tags.Wood);
                                }
                                else if (_ghostBuilderReference.NeedsStone())
                                {
                                    GetMoreResource(Tags.Stone);
                                }
                            }
                        }
					}
				}
                // If ghost is gone then cancel
				else 
				{
					ActionStates.ClearStates();

					// if inventory is not empty, unload resources and go idle
					// else, go idle
					if (!Inventory.Empty())
					{
						StartUnloading(_mainCastle.gameObject);
					}
					else
					{
						_crabAgent.isStopped = true;
					}
				}
			}
		}

        if (ActionStates.GetState("Rebuilding") && IsInBuildRange())
            _rebuildTimer.update(UpdateRebuild);

		if (ActionStates.GetState("Collecting") && CanInteract())
			UpdateCollect();

		if (ActionStates.GetState("Unloading") && CanInteract())
			UpdateUnload();

		if (ActionStates.GetState("Entering") && CanInteract())
			UpdateEnter();

        if (ActionStates.GetState("Capturing") && CanInteract())
        {
            if (!Target.GetComponent<Enterable>().Occupied())
            {
                Capture(); 
            }
        }

		if (ActionStates.GetState("Recruiting") && CanInteract())
			Recruit();

		if (ActionStates.GetState("Upgrading") && CanInteract())
			UpdateUpgrade();

        if (ActionStates.GetState("Repairing") && CanInteract())
            _repairTimer.update(Repair);
        
        if (ActionStates.GetState("Dismantling") && CanInteract())
            _dismantleTimer.update(Attack);

        if (GetDistanceToPosition(Destination) < AttackRange)
        {
            Destroy(_destinationMarker);
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
	public void StartMove(Vector3 dest) 
	{
		if (_debug)
			Debug.Log("Crab moving to " + dest);

		_crabAgent.isStopped = false;
		_crabAgent.ResetPath();
		_crabAgent.destination = dest;

		Destination = dest;

        if (!_destinationMarker)
        {
            _destinationMarker = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/Etc/DestinationMarker"), Destination, Quaternion.identity);
        }
        else
        {
            _destinationMarker.transform.position = Destination;
        }

        ActionStates.SetState("Moving", true);

		//TODO: play walking animation
	}

    public void StartNewMove(Vector3 dest)
    {
        StartMove(dest);
        ActionStates.ClearStates();
        ActionStates.SetState("Moving", true);
    }

	/// <summary>
	/// Stops crab from moving.
	/// </summary>
	public void StopMove()
	{
		_crabAgent.isStopped = true;
		_crabAgent.ResetPath();
		ActionStates.SetState("Moving", false);
        Destroy(_destinationMarker);
	}

	#endregion

	#region Attack functions

	// ================================================================================================
	// Attack Code
	/// <summary>
	/// Starts crab attack.
	/// </summary>
	/// <param name="attackTarget">Attack target.</param>
	public void StartAttack(GameObject attackTarget) 
	{
		if (_debug)
		{
			Debug.Assert(attackTarget.GetComponent<Attackable>());
			Debug.Log("Started attack!");
		}

		Target = attackTarget;

		if ((!ValidTarget()) || !TargetIsEnemy()) 
		{
			if (_debug)
				Debug.Log("Invalid Target");
			Target = null;
			return;
		}
			
		if (IdUtility.IsMoveable(Target.tag))
        {
            Target.SendMessage("SetAttacker", gameObject, SendMessageOptions.DontRequireReceiver); 
        }

		StartMove(Target.transform.position);
		
		ActionStates.SetState("Attacking", true);
	}

	/// <summary>
	/// Attacks target when time has passed.
	/// </summary>
	void Attack() 
	{
		if (_debug)
			Debug.Log("Crab attack");

		float finalAttack = _attackDamage;

		if (_weaponStates.GetState(Tags.Spear))
        {
            finalAttack += _spearAttack; 
        }
		else if (_weaponStates.GetState(Tags.Hammer))
        {
            finalAttack += _hammerAttack; 
        }
		else if (_weaponStates.GetState(Tags.Bow))
        {
            finalAttack = _bowAttack; 
        }
		
		if (Target.tag != Tags.Crab)
        {
            Target.GetComponent<Attackable>().Attacked(finalAttack); 
        }
		else
        {
            Target.GetComponent<CrabController>().Attacked(finalAttack); 
        }
		
		AudioSource.PlayClipAtPoint(HitClip, transform.position);
		_timeConsumed = 0.0f;
		_crabAgent.isStopped = false;
	}

	/// <summary>
	/// Calls attack function.
	/// </summary>
	void UpdateAttack()
	{
		if (_debug)
			Debug.Log("Attacking");
		_crabAgent.isStopped = true;
        _attackTimer.update(Attack);
	}

	/// <summary>
	/// Is target object valid?
	/// </summary>
	/// <returns><c>true</c>, if target is valid, <c>false</c> otherwise.</returns>
	bool ValidTarget() 
	{
		return (Target.tag == Tags.Block || Target.tag == Tags.Castle ||
				IdUtility.IsMoveable(Target.tag) || IdUtility.IsBuilding(Target.tag));
	}

	/// <summary>
	/// Is target an enemy?
	/// </summary>
	/// <returns><c>true</c>, if target is enemy, <c>false</c> otherwise.</returns>
	bool TargetIsEnemy() 
	{
		int tempTeam;

		if (IdUtility.IsResource(Target.tag))
        {
            return true; 
        }
		else
        {
            tempTeam = Target.GetComponent<Team>().team; 
        }

		return (tempTeam != _team.team);
	}

	/// <summary>
	/// Finds the first enemy crab in range.
	/// </summary>
	/// <returns>The enemy crab.</returns>
	GameObject FindEnemyCrabs()
	{
		CrabController[] crabs = FindObjectsOfType<CrabController>();

		for (int i = 0; i < crabs.Length; i++)
		{
			int crabTeam = crabs[i].GetComponent<Team>().team;
			if (crabTeam != _team.team && crabTeam != _neutralTeamCode)
			{
				float dist = GetDistanceToObject(crabs[i].gameObject);

				if (dist <= (AttackRange * 3))
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
	public void Attacked(float damage) 
	{
		CancelCapture();

		if (_weaponStates.GetState(Tags.Shield))
        {
            damage *= _shieldDefense; 
        }
		
		GetComponent<Attackable>().Attacked(damage);
		//TODO: play damaged animation
		StartAttack(Target);
	}

	/// <summary>
	/// Sets the attacker object.
	/// </summary>
	/// <param name="enemy">Enemy object.</param>
	public void SetAttacker(GameObject enemy) 
	{
		Target = enemy;
	}

	/// <summary>
	/// Called by enemy crab upon death
	/// </summary>
	public void EnemyDied() 
	{
		_experience += LevelGain;
		ActionStates.ClearStates();
		Target = null;
	}

    #endregion

    #region Dismantle functions

    public void StartDismantling(GameObject structure)
    {
        if (_debug)
            Debug.Log("Started Dismantling");

        Target = structure;
        _dismantleTimer = new Timer(_attackSpeed);

        StartMove(Target.transform.position);

        ActionStates.SetState("Dismantling", true);
    }

    #endregion

    #region Unload functions

    // ===================================================================================================
    // Unload code
    /// <summary>
    /// Starts unloading of resources into a castle.
    /// </summary>
    /// <param name="castle">Castle object.</param>
    public void StartUnloading(GameObject unloadTarget) 
	{
		Target = unloadTarget;

		StartMove(Target.transform.position);

		ActionStates.SetState("Unloading", true);
		if (_secondTarget)
        {
            ActionStates.SetState("Collecting", true); 
        }
	}

	/// <summary>
	/// Unloads resources.
	/// </summary>
	void Unload() 
	{
        if (Target.GetComponent<CastleController>() == null)
        {
            Debug.LogWarning("Target is not a castle.");
            return;
        }

		for (int i = 0; i < Inventory.Items.Length; i++) 
		{
			if (Inventory.Items[i] != null && IdUtility.IsResource(Inventory.Items[i])) 
			{
				Target.GetComponent<CastleController>().Give(Inventory.Items[i]);
				Inventory.Items[i] = null;
			}
		}

		ActionStates.SetState("Unloading", false);
		_crabAgent.isStopped = true;
	}

	/// <summary>
	/// Unloads resources and commands crab to collect more resources.
	/// </summary>
	void UpdateUnload()
	{
		if (_debug)
			Debug.Log("Unloading");

		Unload();
		Inventory.SortInventory();

		if (ActionStates.GetState("Collecting"))
		{
			if (ActionStates.GetState("Building"))
			{
				BuildFromGhost(Target);
			}
			else
			{
                if (_secondTarget != null)
                {
                    GameObject newSecond = Target;
                    StartCollecting(_secondTarget);
                    _secondTarget = newSecond;
                }
			}
		}

		if (_selected)
        {
            _controller.GetInventory(Inventory.Items); 
        }
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
	public void StartBuild(string type, Vector3 pos) 
	{
		if (_debug)
			Debug.Log("Building " + type + ".");

		_buildingType = type;
		_timeConsumed = 0.0f;

        StartMove(pos);

		ActionStates.SetState("Building", true);
	}

	/// <summary>
	/// Builds from ghost.
	/// </summary>
	/// <param name="ghostObject">Ghost object.</param>
	public void BuildFromGhost(GameObject ghostObject) 
	{
		_buildingType = ghostObject.name.Remove(0, 5);
		_timeConsumed = 0.0f;
		_ghostBuilderReference = ghostObject.GetComponent<GhostBuilder>();

		StartMove(ghostObject.transform.position);
        
		ActionStates.SetState("Building", true);
	}

	/// <summary>
	/// Builds from block.
	/// </summary>
	/// <param name="obj">Block object.</param>
	/// <param name="newType">Tower or gate.</param>
	public void StartRebuild(GameObject obj, string newType)
	{
		_buildingType = newType;
        _rebuildTimer = new Timer(TimeToRebuild);
        ActionStates.SetState("Rebuilding", true);
        Target = obj;
		_timeConsumed = 0.0f;

		StartMove(obj.transform.position);
	}

	/// <summary>
	/// Check ability to build.
	/// </summary>
	void UpdateGhostBuild() 
	{
		if (_debug)
			Debug.Log("Building from ghost");

		if (_ghostBuilderReference.CanBuild()) 
		{
			Build();
		}
		else
		{
			if (_ghostBuilderReference.WoodAmount < _ghostBuilderReference.WoodRequirement)
			{
				// get wood

				// Check inventory
				// If enough put into building ghost
				if (Inventory.Contains(Tags.Wood))
				{
					int amount = Inventory.CountInventory(Tags.Wood);

					_ghostBuilderReference.WoodAmount += amount;
					Inventory.EmptyInventory(Tags.Wood);

					if (_ghostBuilderReference.CanBuild())
					{
						Build();
						return;
					}
				}

				// Check castle
				// if enough get wood
				GetMoreResource(Tags.Wood);
			}
			else if (_ghostBuilderReference.StoneAmount < _ghostBuilderReference.StoneRequirement)
			{
				// get stone

				// Check inventory
				// If enough put into building ghost
				if (Inventory.Contains(Tags.Stone))
				{
					int amount = Inventory.CountInventory(Tags.Stone);

					_ghostBuilderReference.StoneAmount += amount;
					Inventory.EmptyInventory(Tags.Stone);

					if (_ghostBuilderReference.CanBuild()) {
						Build();
						return;
					}
				}

				// Check castle
				// if enough get stone
				GetMoreResource(Tags.Stone);
			}
		}
	}

	/// <summary>
	/// Check ability to rebuild.
	/// </summary>
	void UpdateRebuild()
	{
		if (_debug)
			Debug.Log("Building from wall segment!");

		ActionStates.ClearStates();
		_timeConsumed = 0.0f;

		Transform blockTransform = Target.transform;
		Destroy(Target);
		Instantiate(Resources.Load("Prefabs/Structures/" + _buildingType), blockTransform.position, blockTransform.rotation);
	}

	/// <summary>
	/// Sets the number of crabs selected.
	/// </summary>
	/// <param name="crabs">Number of crabs.</param>
	public void SetCrabs(int crabs) 
	{
		_selectedCrabs = crabs;
	}

	void Build() 
	{
		ActionStates.ClearStates ();
		_timeConsumed = 0.0f;

		_ghostBuilderReference.Build ();
	}

	void GetMoreResource(string resource) 
	{
		if (_mainCastle.CanTake (resource))
        {
			StartTake (_mainCastle.gameObject, resource);
		}
        else
        {
			StartCollecting (InfoTool.ClosestObjectWithTag (gameObject, resource));
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
	public void StartUpgrading(GameObject obj)
	{
		if (_debug)
			Debug.Log("Started upgrading");

		Target = obj;

		if (Inventory.IsAllOneType(Tags.Wood))
			_upgradeType = wallUpgradeType.WOOD;
		else if (Inventory.IsAllOneType(Tags.Stone))
			_upgradeType = wallUpgradeType.STONE;
		else
			_upgradeType = wallUpgradeType.NORMAL;

		StartMove(obj.transform.position);

		ActionStates.SetState("Upgrading", true);
	}

	/// <summary>
	/// Check ability to upgrade.
	/// </summary>
	void UpdateUpgrade()
	{
		if (_debug)
			Debug.Log("Upgrading");

		Target.GetComponent<WallUpgrade>().StartWallUpgrade(_upgradeType, Inventory);
		Target.GetComponent<WallUpgrade>().SetCrabs(_selectedCrabs);
		Inventory.EmptyInventory();

		if (_selected)
        {
            _controller.GetInventory(Inventory.Items); 
        }
		
		ActionStates.ClearStates();
	}

	#endregion

	#region Collect functions

	// ===================================================================================================
	// Collect code
	/// <summary>
	/// Starts crab collecting.
	/// </summary>
	/// <param name="obj">Wood, stone, or weapon object.</param>
	public void StartCollecting(GameObject obj)
	{
		if (_debug)
			Debug.Log("Started collecting");

        if (_mainCastle != null && Inventory.Full())
        {
            _secondTarget = obj;
            StartUnloading(_mainCastle.gameObject);
        }
        else
        {
            Target = obj;
            StartMove(obj.transform.position);
            ActionStates.SetState("Collecting", true);
        }
	}

	/// <summary>
	/// Collects until inventory is full.
	/// </summary>
	IEnumerator Collect()
	{
        if (Target.GetComponent<ResourceController>() == null)
        {
            Debug.LogWarning("Collecting from a non-resource.");
            yield break;
        }

		while (CanInteract() && !Inventory.Full() && ActionStates.GetState("Collecting")) 
		{
			if (_debug)
				Debug.Log("Taking " + Target.tag + ".");

			Inventory.AddToInventory(Target.tag);

			Target.GetComponent<ResourceController>().Take();

			yield return null;
		}
	}

	/// <summary>
	/// Collects the weapon.
	/// </summary>
	void CollectWeapon() 
	{
		if (_debug)
			Debug.Log("Taking weapon.");

		Inventory.AddToInventory(Target.tag);
		_weaponStates.SetState(Target.tag, true);
		Destroy(Target);

		Target = null;
	}

	/// <summary>
	/// Checks ability to collect.
	/// </summary>
	void UpdateCollect()
	{
		if (_debug)
			Debug.Log("Collecting");

		if (Target == null)
		{
			GoIdle();
			return;
		}

		string targetTag = Target.transform.tag;

		if (IdUtility.IsResource(targetTag))
		{
			StartCoroutine(Collect());
			if (Inventory.Full())
			{
				_secondTarget = Target;

				if (ActionStates.GetState("Building"))
				{
					BuildFromGhost(_ghostBuilderReference.gameObject);
				}
				else
				{
					StartUnloading(_mainCastle.gameObject);
				}
			}
		}
		else if (IdUtility.IsWeapon(targetTag))
		{
			ActionStates.ClearStates();
			CollectWeapon();
		}
		Inventory.SortInventory();

		if (_selected)
			_controller.GetInventory(Inventory.Items);
	}

	#endregion

	#region Take functions

	// ===================================================================================================
	// Taking weapon code
	/// <summary>
	/// Starts crab taking a weapon.
	/// </summary>
	/// <param name="weapon">Weapon tag.</param>
	public void StartTakeWeapon(string weapon)
	{
		_weaponToTake = weapon;
		ActionStates.SetState("Taking", true);

		Target = _controller.TargetedArmoury.gameObject;

		StartMove(Target.transform.position);
	}

    public void StartTakeWeapon(string weapon, GameObject armoury)
    {
        _weaponToTake = weapon;
        ActionStates.SetState("Taking", true);

        Target = armoury;

        StartMove(Target.transform.position);
    }

	/// <summary>
	/// Takes the weapon.
	/// </summary>
	/// <param name="weapon">Weapon tag.</param>
	void TakeWeapon(string weapon)
	{
		if (_controller.TargetedArmoury.RequestWeapon(gameObject, weapon))
		{
			Inventory.AddToInventory(weapon);
			_weaponToTake = "none";
			ActionStates.SetState("Taking", false);
		}
		else
		{
			if (_debug)
				Debug.Log("Not enough Resources!");
		}
	}

	/// <summary>
	/// Starts crab taking resources from castle.
	/// </summary>
	/// <param name="castle">Castle object.</param>
	/// <param name="takeTag">Wood or stone.</param>
	void StartTake(GameObject castle, string takeTag)
	{
		_resourceToTake = takeTag;
		ActionStates.SetState("Taking", true);

		Target = castle;

		StartMove(Target.transform.position);
	}

	/// <summary>
	/// Takes the resource from castle.
	/// </summary>
	void TakeResource()
	{
		while (!Inventory.Full())
		{
			if (_mainCastle.CanTake(_resourceToTake))
			{
				_mainCastle.Take(_resourceToTake);
				Inventory.AddToInventory(_resourceToTake);
			}
			else
				break;
		}

		if (ActionStates.GetState ("Building")) {
            _secondTarget = Target;
            StartMove(_ghostBuilderReference.transform.position);
            ActionStates.SetState("Taking", false);
        }
        else
        {
            _resourceToTake = "none";
            ActionStates.SetState("Taking", false);
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
	public void StartEnter(GameObject enterable) 
	{
		if (_debug)
		{
			Debug.Assert(enterable.GetComponent<Enterable>());
			Debug.Log("Started entering");
		}

		Target = enterable;

		StartMove(Target.transform.position);

		ActionStates.SetState("Entering", true);
	}

	/// <summary>
	/// Requests entry to enterable.
	/// </summary>
	void Enter() 
	{
        if (Target.GetComponent<Enterable>())
        {
            Target.GetComponent<Enterable>().RequestEntry(gameObject); 
        }
	}

	/// <summary>
	/// Checks ability to enter.
	/// </summary>
	void UpdateEnter()
	{
		if (_debug)
			Debug.Log("Entering");

		Enter();
		ActionStates.ClearStates();
	}

	#endregion

	#region Capture functions

	// ==========================================================================================================
	// Capturing castle code
	/// <summary>
	/// Starts crab capturing the castle.
	/// </summary>
	/// <param name="castle">Castle object.</param>
	public void CaptureCastle(GameObject castle)
	{
		if (_debug)
        {
            Debug.Assert(castle.GetComponent<CastleController>());
            Debug.Log("Capturing castle.");
        }
			
		Target = castle;
		_timeToCapture = Target.GetComponent<CastleController>().GetResistanceTime();

		StartMove(Target.transform.position);

		ActionStates.SetState("Capturing", true);
	}

	/// <summary>
	/// Captures castle when time is up.
	/// </summary>
	void Capture() 
	{
		if (_debug)
		{
			Debug.Log("Capturing");
			Debug.Log(_timeConsumed + " / " + (_timeToCapture / _selectedCrabs));
		}

        if (Target.GetComponent<Enterable>() == null || Target.GetComponent<Team>() == null)
        {
            Debug.LogWarning("Target isn't enterable nor does it have a team.");
            return;
        }

		_timeConsumed += Time.deltaTime;
		if (_timeConsumed >= (_timeToCapture / _selectedCrabs) && !Target.GetComponent<Enterable>().Occupied())
		{
			if (_debug)
				Debug.Log("Captured castle");

			Target.GetComponent<Team>().team = _team.team;

			CancelCapture();
			Target.GetComponent<Enterable>().RequestEntry(gameObject);
		}
		else if (IsCapturing() && !CanInteract() || Target.gameObject.tag != Tags.Castle)
        {
            CancelCapture(); 
        }
	}

	/// <summary>
	/// Cancels the capture.
	/// </summary>
	void CancelCapture() 
	{
		_timeConsumed = 0.0f;
		ActionStates.ClearStates();
	}

	/// <summary>
	/// Is crab capturing castle?
	/// </summary>
	/// <returns><c>true</c>, if capture is in progress, <c>false</c> otherwise.</returns>
	bool IsCapturing() 
	{
		return ((_timeConsumed > 0.0f) && (_timeConsumed < (_timeToCapture / _selectedCrabs)));
	}

	#endregion

	#region Recruit functions

	// ===================================================================================================
	// Recruiting code
	/// <summary>
	/// Starts crab recruiting.
	/// </summary>
	/// <param name="crab">Crab object.</param>
	public void StartRecruiting(GameObject crab) 
	{
		if (_debug)
			Debug.Assert(crab.tag == Tags.Crab);

		Target = crab;

		StartMove(crab.transform.position);

		ActionStates.SetState("Recruiting", true);
	}

	/// <summary>
	/// Recruits crab.
	/// </summary>
	void Recruit() 
	{
		// check requirements
		// if good then
			// run dialog
			// change crab team
		if (_debug)
			Debug.Log("Recruiting");

        if (Target.GetComponent<CrabController>() == null)
        {
            Debug.LogWarning("Target is not a crab.");
            return;
        }

		if (CanRecruit()) 
		{	
			if (_debug)
				Debug.Log("Recruitment successful!");
			Target.GetComponent<CrabController>()._team = _team;
		}
		else
		{
			if (_debug)
				Debug.Log("Recruitment unsuccessful!");

			if (Target.GetComponent<SacrificeNeutralCrab>() != null)
			{
				Target.GetComponent<SacrificeNeutralCrab>().StartSacrifice(gameObject);
			}
		}
		ActionStates.ClearStates();
	}

	/// <summary>
	/// Checks crab type requirements.
	/// </summary>
	/// <returns><c>true</c>, if crab is recruitable, <c>false</c> otherwise.</returns>
	bool CanRecruit() 
	{
        if (Target.GetComponent<CrabController>() == null)
        {
            Debug.LogWarning("Target is not a crab.");
            return false;
        }

		switch (Target.GetComponent<CrabController>().Type) {
		case CrabSpecies.ROCK:
		case CrabSpecies.KAKOOTA:
		case CrabSpecies.CALICO:
		case CrabSpecies.SEAWEED:
			return true;
		case CrabSpecies.FIDDLER:
			return HasCastles(1);
		case CrabSpecies.SPIDER:
			return HasCastles(2);
		case CrabSpecies.TGIANT:
		case CrabSpecies.COCONUT:
			return Target.GetComponent<SacrificeNeutralCrab>().CanRecruit();
		case CrabSpecies.HORSESHOE:
			return HasVariedSpecies(2);
		default:
			if (_debug)
				Debug.Log("Impossible crab type: " + Target.GetComponent<CrabController>().Type);
			return false;
		}
	}
		
	/// <summary>
	/// Are there enough friendly castles?
	/// </summary>
	/// <returns><c>true</c>, if there are a correct number of friendly castles, <c>false</c> otherwise.</returns>
	/// <param name="numOfCastles">Number of castles.</param>
	bool HasCastles(int numOfCastles) 
	{
		CastleController[] castles = FindObjectsOfType<CastleController>();
		int friendlyCastles = 0;

		for (int i = 0; i < castles.Length; i++) 
		{
			if (castles[i].GetComponent<Team>().team == _team.team)
            {
                friendlyCastles++; 
            }
			
			if (friendlyCastles == numOfCastles)
            {
                return true; 
            }
		}
		return false;
	}
		
	/// <summary>
	/// Are there enough kinds of crabs?
	/// </summary>
	/// <returns><c>true</c>, if number of different crab types are correct, <c>false</c> otherwise.</returns>
	/// <param name="numOfSpecies">Number of species.</param>
	bool HasVariedSpecies(int numOfSpecies) 
	{
		CrabController[] crabs = FindObjectsOfType<CrabController>();
		var crabSpecies = new HashSet<CrabSpecies>();

		for (int i = 0; i < crabs.Length; i++) 
		{
			if (crabs[i].GetComponent<Team>().team == _team.team) 
			{
				if (!crabSpecies.Contains(crabs[i].GetComponent<CrabController>().Type))
                {
                    crabSpecies.Add(crabs[i].GetComponent<CrabController>().Type); 
                }
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
	public void StartRepair(GameObject repairTarget) 
	{
		Target = repairTarget;
        _repairTimer = new Timer(TimeToRepair);

		StartMove(repairTarget.transform.position);

		ActionStates.SetState("Repairing", true);
	}

	/// <summary>
	/// Repairs object when time is up.
	/// </summary>
	void Repair() 
	{
		if (_debug)
			Debug.Log("Repairing");
        
		Target.SendMessage("Repair", RebuildAmount, SendMessageOptions.DontRequireReceiver);
	}

	#endregion

	#region Distance functions

	// ===================================================================================================
	// checks distance to object
	/// <summary>
	/// Is crab close enough to the target?
	/// </summary>
	/// <returns><c>true</c>, if crab can interact, <c>false</c> otherwise.</returns>
	bool CanInteract() 
	{
		float tempRange = AttackRange;

		if (ActionStates.GetState("Attacking")) 
		{
			if (_weaponStates.GetState(Tags.Bow))
            {
                tempRange += _bowRange; 
            }
			else if (_weaponStates.GetState(Tags.Spear))
            {
                tempRange += _spearDistance; 
            }
		}

		return (GetDistanceToObject (Target) < tempRange);
	}

	/// <summary>
	/// Like canInteract but without the range modifiers.
	/// </summary>
	/// <returns><c>true</c>, if in build range, <c>false</c> otherwise.</returns>
	bool IsInBuildRange()
	{
		return (GetDistanceToPosition(Destination) < AttackRange);
	}

	/// <summary>
	/// Gets the distance to position from crab.
	/// </summary>
	/// <returns>The distance to position.</returns>
	/// <param name="pos">Position.</param>
	float GetDistanceToPosition(Vector3 pos)
	{
		return Vector3.Distance(transform.position, pos);
	}

	/// <summary>
	/// Gets the distance to object from crab.
	/// </summary>
	/// <returns>The distance to object.</returns>
	/// <param name="obj">Object.</param>
	public float GetDistanceToObject(GameObject obj) 
	{
		if (obj == null)
		{
			GoIdle();
			return 0.0f;
		}

		Vector3 crabPosition = transform.position;
		Vector3 objectPosition = obj.transform.position;
		RaycastHit[] hits = Physics.RaycastAll(new Ray(crabPosition, objectPosition - crabPosition));

		for (int i = 0; i < hits.Length; i++) 
		{
			if (hits[i].collider.gameObject == obj)
            {
                return hits[i].distance; 
            }
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
	public string GetCraftableType() 
	{
		int stones = 0, sticks = 0;

		for (int i = 0; i < Inventory.Items.Length; i++) 
		{
			if (Inventory.Items[i] == null)
				continue;
			
			if (Inventory.Items[i] == Tags.Stone)
				stones++;
			else if (Inventory.Items[i] == Tags.Wood)
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
	public void Craft(string type) 
	{
		switch (type) {
		case Tags.Hammer:
			Inventory.RemoveItem(Tags.Stone);
			Inventory.RemoveItem(Tags.Stone);
			Inventory.RemoveItem(Tags.Wood);
			break;
		case Tags.Spear:
			Inventory.RemoveItem(Tags.Stone);
			Inventory.RemoveItem(Tags.Wood);
			Inventory.RemoveItem(Tags.Wood);
			break;
		case Tags.Shield:
			Inventory.RemoveItem(Tags.Wood);
			Inventory.RemoveItem(Tags.Wood);
			Inventory.RemoveItem(Tags.Wood);
			break;
		case Tags.Bow:
			Inventory.RemoveItem(Tags.Wood);
			Inventory.RemoveItem(Tags.Wood);
			break;
		}

		Inventory.AddToInventory(type);
	}

    #endregion

    /// <summary>
    /// Is a player crab near?
    /// </summary>
    /// <returns><c>true</c>, if player crab is near, <c>false</c> otherwise.</returns>
    public bool IsCrabNear()
	{
		CrabController[] crabs = FindObjectsOfType<CrabController>();

		for (int i = 0; i < crabs.Length; i++)
		{
			float dist = GetDistanceToObject(crabs[i].gameObject);

			if (dist <= (AttackRange * AwarenessMultiplier))
			{
				if (_debug)
					Debug.Log("Distance to " + crabs[i].name + " is " + dist);
				
				if (crabs[i].GetComponent<Team>().team == 0)
				{
					if (_debug)
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
	public void UpdateUI(GUIController gui)
	{
		gui.CraftButton.gameObject.SetActive(_currentCraft != "None");
		gui.SubCraftButton.gameObject.SetActive(_currentCraft == Tags.Shield);
		GetComponent<Attackable>().SetHealth(gui.HealthSlider);
		_controller.GetInventory(Inventory.Items);

		gui.CrabLevelText.text = "Level: " + _level;

		gui.ExpSlider.value = _experience;
		gui.ExpSlider.minValue = 0;
		gui.ExpSlider.maxValue = NextLevelExperience;

		if (_currentCraft == Tags.Shield)
        {
            gui.CraftButton.GetComponentInChildren<Text>().text = "Craft Shield"; 
        }
		else
        {
            gui.CraftButton.GetComponentInChildren<Text>().text = "Craft"; 
        }
	}

	/// <summary>
	/// Gets the inventory as a string array.
	/// </summary>
	/// <returns>The inventory array.</returns>
	public string[] GetInventory() 
	{
		Inventory.SortInventory();
		return Inventory.Items;
	}

	/// <summary>
	/// Toggles the selected.
	/// </summary>
	public void ToggleSelected() 
	{
		_selected = !_selected;
	}

	/// <summary>
	/// Sets the controller.
	/// </summary>
	/// <param name="player">Player script.</param>
	public void SetController(Player player) 
	{
		_controller = player;
		_selected = true;
	}

	#endregion

	#region Internal functions

	public void GoIdle()
	{
		StopMove();
		ActionStates.ClearStates();
	}

	/// <summary>
	/// Levels up crab.
	/// </summary>
	void LevelUp() 
	{
		_level++;
		_experience = 0;
		NextLevelExperience = _level * NextLevelExperience;

		_attackDamage += DamageIncrease;
		_maxHealth += HealthIncrease;
	}

	#endregion

	#region Public info functions

	/// <summary>
	/// Is the crab busy?
	/// </summary>
	/// <returns><c>true</c>, if crab is busy, <c>false</c> otherwise.</returns>
	public bool IsBusy() 
	{
		return !ActionStates.IsIdle();
	}

	/// <summary>
	/// Gets the type as a string.
	/// </summary>
	/// <returns>The crab type.</returns>
	public string GetType()
	{
		switch (Type) {
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
	void SetStats(float mHealth, float mSpeed, float aDamage, float aSpeed) 
	{
		_maxHealth = mHealth;
		_maxSpeed = mSpeed;
		_attackDamage = aDamage;
		_attackSpeed = aSpeed;
	}

	/// <summary>
	/// Loads the stats.
	/// </summary>
	void LoadStats() 
	{
        CrabStats stats = CrabStats.load(Path.Combine(Application.dataPath, "GameData/crabStats.xml"));

        int pos = (int)Type;

        float health = float.Parse(stats.Health[pos].Text);
        float movementSpeed = float.Parse(stats.MovementSpeed[pos].Text);
        float attackDamage = float.Parse(stats.AttackDamage[pos].Text);
        float attackSpeed = float.Parse(stats.AttackSpeed[pos].Text);

        float scaleX = float.Parse(stats.ScaleFactor[pos].x);
        float scaleY = float.Parse(stats.ScaleFactor[pos].y);
        float scaleZ = float.Parse(stats.ScaleFactor[pos].z);

        gameObject.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        SetStats(health, movementSpeed, attackDamage, attackSpeed);

        if (Type == CrabSpecies.CALICO)
        {
            _crabMaterial = Resources.Load<Material>("Materials/Crabs/Calico");
        }
        else if (Type == CrabSpecies.KAKOOTA)
        {
            if (_debug)
                Debug.Log("Crab material is null.");

            if (_crabMaterial != null)
            {
                _crabMaterial.color = Color.red; 
            }
        }
	}

	void SetNeutralComponent(CrabSpecies species)
	{
		switch (species)
		{
		case CrabSpecies.TGIANT:
			gameObject.AddComponent<SacrificeNeutralCrab>();
			GetComponent<SacrificeNeutralCrab>().SacrificesRequired = 5;
			break;
		case CrabSpecies.COCONUT:
			gameObject.AddComponent<SacrificeNeutralCrab>();
			GetComponent<SacrificeNeutralCrab>().SacrificesRequired = 2;
			break;
		}
	}
}

#endregion