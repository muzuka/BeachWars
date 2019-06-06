using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Production manager.
/// 	Assigns crabs to build buildings and walls.
/// 	Asks IncomeManager for crabs to use.
/// 	Receives building schedule from StrategyManager.
/// 	Builds castle designs.
/// </summary>
public class ProductionManager : MonoBehaviour {

    public float WeaponCreationTime;
    public float BuildingDistance;

	StrategyManager _strategyManager;
	IncomeManager _incomeManager;

	// building location
	List<Vector3> _locations;                                // The eight locations around a castle
	List<Dictionary<int, bool>> _buildLocationOccupiedDict;  // list of dictionaries storing locations around each castle

	BuildingGoal _currentGoal;
    GameObject _currentGhostBuilding;
    bool _isBuilding;

    Timer _weaponTimer;

    public GameObject Workshop { get; set; }
    public GameObject Armoury { get; set; }

	/// <summary>
    /// Start this instance
    /// </summary>
	void Start() {
		_strategyManager = GetComponent<StrategyManager>();
		_incomeManager = GetComponent<IncomeManager>();

		_locations = new List<Vector3> ();
		_locations.Add (new Vector3(-BuildingDistance, 0.0f, -BuildingDistance));
		_locations.Add (new Vector3(0.0f, 0.0f, -BuildingDistance));
		_locations.Add (new Vector3(BuildingDistance, 0.0f, -BuildingDistance));
		_locations.Add (new Vector3(BuildingDistance, 0.0f, 0.0f));
		_locations.Add (new Vector3(BuildingDistance, 0.0f, BuildingDistance));
		_locations.Add (new Vector3(0.0f, 0.0f, BuildingDistance));
		_locations.Add (new Vector3(-BuildingDistance, 0.0f, BuildingDistance));
		_locations.Add (new Vector3(-BuildingDistance, 0.0f, 0.0f));

		_buildLocationOccupiedDict = new List<Dictionary<int, bool>> ();
		InitBuildLocationList ();

		_currentGoal = new BuildingGoal(_strategyManager.Strategy.BuildingQueue.Dequeue());
        _isBuilding = false;

        Workshop = null;

        _weaponTimer = new Timer(WeaponCreationTime);
	}

    /// <summary>
    /// Update this instance
    /// </summary>
    void Update() {
        if (!_isBuilding)
        {
            if (BuildingCost.CanBuild(_currentGoal.buildingType, _strategyManager.Knowledge.TotalWood, _strategyManager.Knowledge.TotalStone))
            {
                BuildNewBuilding();
            }
        }

        if (_isBuilding && _currentGhostBuilding == null)
        {
            HandleNewBuilding();
        }

        if (Armoury != null)
        {
            _weaponTimer.update(BuildNewWeapon);
        }

        if (Workshop != null) 
        {
            // if there is a workshop then check for the correct amount of resources and start building a catapult or ballista.
            // should build a catapult if enemy has a castle, otherwise build a ballista.

            int catapultWoodCost = Workshop.GetComponent<WorkshopController>().CatapultWoodCost;
            int catapultStoneCost = Workshop.GetComponent<WorkshopController>().CatapultStoneCost;
            int ballistaWoodCost = Workshop.GetComponent<WorkshopController>().BallistaWoodCost;
            int ballistaStoneCost = Workshop.GetComponent<WorkshopController>().BallistaStoneCost;

            int currentWood = _strategyManager.MainCastle.GetWoodPieces();
            int currentStone = _strategyManager.MainCastle.GetStonePieces();

            if (currentWood > ballistaWoodCost && currentStone > ballistaStoneCost && Workshop.GetComponent<WorkshopController>().Building) {
                _strategyManager.StartBuildingSiegeWeapon(Workshop, Tags.Ballista);
            }
        }
	}

    /// <summary>
    /// Creates a new ghost building
    /// </summary>
    void BuildNewBuilding()
    {
        GameObject spareCrab = _incomeManager.GetSpareCrab();
        Vector3 openLocation = GetOpenLocation();
        if (spareCrab != null)
        {
            if (_currentGoal.buildingType != "None")
            {
                _currentGhostBuilding = Instantiate(Resources.Load<GameObject>("Prefabs/PreviewStructures/Ghost" + _currentGoal.buildingType), openLocation, Quaternion.identity);
                _currentGhostBuilding.GetComponent<Team>().team = GetComponent<Team>().team;
                _strategyManager.StartBuildFromGhost(spareCrab, _currentGhostBuilding);
                _isBuilding = true;

                if (GetComponent<DebugComponent>().Debug)
                    Debug.Log("Created new ghost!");
            }
        }
        else
        {
            if (GetComponent<DebugComponent>().Debug)
                Debug.Log("Cannot find spare crab.");
        }
    }
    
     /// <summary>
     /// Register new building with the production manager
     /// </summary>
    void HandleNewBuilding()
    {
        if (_currentGoal.buildingType == Tags.Nest)
        {
            GameObject nest = InfoTool.ClosestObjectWithTag(_strategyManager.Knowledge.AICastleList[0], Tags.Nest);
            nest.GetComponent<Team>().team = GetComponent<Team>().team;
            GameObject crab = _incomeManager.GetSpareCrab();
            _incomeManager.CrabEnteredBuilding(crab);

            if (crab != null || nest != null)
            {
                _strategyManager.EnterBuilding(crab, nest);
            }
        }
        else if (_currentGoal.buildingType == Tags.Workshop)
        {
            Workshop = InfoTool.ClosestObjectWithTag(_strategyManager.Knowledge.AICastleList[0], Tags.Workshop);
        }
        else if (_currentGoal.buildingType == Tags.Armoury)
        {
            Armoury = InfoTool.ClosestObjectWithTag(_strategyManager.Knowledge.AICastleList[0], Tags.Armoury);
        }

        _isBuilding = false;
        if (_strategyManager.Strategy.BuildingQueue.Count > 0)
        {
            _currentGoal = new BuildingGoal(_strategyManager.Strategy.BuildingQueue.Dequeue());
        }
        else
        {
            _currentGoal = new BuildingGoal("None");
        }
        EventManager.TriggerEvent("FinishedBuilding");
        if (GetComponent<DebugComponent>().Debug)
            Debug.Log("New goal: " + _currentGoal.buildingType);
    }

    /// <summary>
    /// Builds a new weapon at an armoury
    /// </summary>
    void BuildNewWeapon()
    {
        if (Armoury == null)
        {
            return;
        }
        string nextWeapon = NextWeaponToBuild();
        if (nextWeapon != null)
        {
            Armoury.GetComponent<HoldsWeapons>().BuildWeapon(nextWeapon);
        }
    }

    /// <summary>
    /// Figures out next weapon to build
    /// </summary>
    /// <returns>Tag of the next weapon</returns>
    string NextWeaponToBuild()
    {
        // Which weapon do I build
        // percentages: spears -> 25%, hammers -> 25%, bows -> 25%, shield -> 25%
        if (Armoury == null)
        {
            return null;
        }
        if (Armoury.GetComponent<HoldsWeapons>() == null)
        {
            return null;
        }
        HoldsWeapons weaponInfo = Armoury.GetComponent<HoldsWeapons>();

        if (weaponInfo.Weapons[Tags.Spear] < HoldsWeapons.Capacity)
        {
            return Tags.Spear;
        }
        else if (weaponInfo.Weapons[Tags.Hammer] < HoldsWeapons.Capacity)
        {
            return Tags.Hammer;
        }
        else if (weaponInfo.Weapons[Tags.Bow] < HoldsWeapons.Capacity)
        {
            return Tags.Bow;
        }
        else if (weaponInfo.Weapons[Tags.Shield] < HoldsWeapons.Capacity)
        {
            return Tags.Shield;
        }
        else
        {
            return null;
        }
    }

	/// <summary>
	/// Gets an open location to build building.
	/// </summary>
	/// <returns>The open location.</returns>
	Vector3 GetOpenLocation()
	{
        Vector3 castleLocation = _strategyManager.Knowledge.AICastleList[0].transform.position;

        for (int i = 0; i < _locations.Count; i++) {
			if (!_buildLocationOccupiedDict [0] [i])
            {
                _buildLocationOccupiedDict[0][i] = true;
                return castleLocation + _locations[i];
            }
		}

		return new Vector3();
	}

	/// <summary>
	/// Inits the build location list.
	/// </summary>
	void InitBuildLocationList()
	{
		CastleController [] castles = FindObjectsOfType<CastleController> ();

		for (int i = 0; i < castles.Length; i++) {
			_buildLocationOccupiedDict.Add (new Dictionary<int, bool> ());
			for (int j = 0; j < _locations.Count; j++)
				_buildLocationOccupiedDict [_buildLocationOccupiedDict.Count - 1].Add (j, false);
		}
	}
}
