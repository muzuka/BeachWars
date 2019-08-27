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

    public float weaponCreationTime;
    public float buildingDistance;

	StrategyManager strategyManager;
	IncomeManager incomeManager;

	// building location
	List<Vector3> locations;                                // The eight locations around a castle
	List<Dictionary<int, bool>> buildLocationOccupiedDict;  // list of dictionaries storing locations around each castle

	BuildingGoal currentGoal;
    GameObject currentGhostBuilding;
    bool isBuilding;

    Timer weaponTimer;

    public GameObject workshop { get; set; }
    public GameObject armoury { get; set; }

	// Use this for initialization
	void Start () {
		strategyManager = GetComponent<StrategyManager>();
		incomeManager = GetComponent<IncomeManager>();

		locations = new List<Vector3> ();
		locations.Add (new Vector3 (-buildingDistance, 0.0f, -buildingDistance));
		locations.Add (new Vector3 (0.0f, 0.0f, -buildingDistance));
		locations.Add (new Vector3 (buildingDistance, 0.0f, -buildingDistance));
		locations.Add (new Vector3 (buildingDistance, 0.0f, 0.0f));
		locations.Add (new Vector3 (buildingDistance, 0.0f, buildingDistance));
		locations.Add (new Vector3 (0.0f, 0.0f, buildingDistance));
		locations.Add (new Vector3 (-buildingDistance, 0.0f, buildingDistance));
		locations.Add (new Vector3 (-buildingDistance, 0.0f, 0.0f));

		buildLocationOccupiedDict = new List<Dictionary<int, bool>> ();
		initBuildLocationList ();

		currentGoal = new BuildingGoal(strategyManager.strategy.buildingQueue.Dequeue());
        isBuilding = false;

        workshop = null;

        weaponTimer = new Timer(weaponCreationTime);
	}

    // Update is called once per frame
    void Update () {
        if (!isBuilding)
        {
            if (BuildingCost.canBuild(currentGoal.buildingType, strategyManager.knowledge.totalWood, strategyManager.knowledge.totalStone))
            {
                buildNewBuilding();
            }
        }

        if (isBuilding && currentGhostBuilding == null)
        {
            handleNewBuilding();
        }

        if (armoury != null)
        {
            weaponTimer.update(buildNewWeapon);
        }

        if (workshop != null) 
        {
            // if there is a workshop then check for the correct amount of resources and start building a catapult or ballista.
            // should build a catapult if enemy has a castle, otherwise build a ballista.

            int catapultWoodCost = workshop.GetComponent<WorkshopController>().catapultWoodCost;
            int catapultStoneCost = workshop.GetComponent<WorkshopController>().catapultStoneCost;
            int ballistaWoodCost = workshop.GetComponent<WorkshopController>().ballistaWoodCost;
            int ballistaStoneCost = workshop.GetComponent<WorkshopController>().ballistaStoneCost;

            int currentWood = strategyManager.mainCastle.getWoodPieces();
            int currentStone = strategyManager.mainCastle.getStonePieces();

            if (currentWood > ballistaWoodCost && currentStone > ballistaStoneCost && workshop.GetComponent<WorkshopController>().building) {
                strategyManager.startBuildingSiegeWeapon(workshop, Tags.Ballista);
            }
        }
	}

    void buildNewBuilding()
    {
        GameObject spareCrab = incomeManager.getSpareCrab();
        Vector3 openLocation = getOpenLocation();
        if (spareCrab != null)
        {
            if (currentGoal.buildingType != "None")
            {
                currentGhostBuilding = Instantiate(Resources.Load<GameObject>("Prefabs/PreviewStructures/Ghost" + currentGoal.buildingType), openLocation, Quaternion.identity);
                currentGhostBuilding.GetComponent<Team>().team = GetComponent<Team>().team;
                strategyManager.startBuildFromGhost(spareCrab, currentGhostBuilding);
                isBuilding = true;

                if (GetComponent<DebugComponent>().debug)
                    Debug.Log("Created new ghost!");
            }
        }
        else
        {
            if (GetComponent<DebugComponent>().debug)
                Debug.Log("Cannot find spare crab.");
        }
    }

    void handleNewBuilding()
    {
        if (currentGoal.buildingType == Tags.Nest)
        {
            GameObject nest = InfoTool.closestObjectWithTag(strategyManager.knowledge.aiCastleList[0], Tags.Nest);
            nest.GetComponent<Team>().team = GetComponent<Team>().team;
            GameObject crab = incomeManager.getSpareCrab();
            incomeManager.crabEnteredBuilding(crab);

            if (crab != null || nest != null)
            {
                strategyManager.enterBuilding(crab, nest);
            }
        }
        else if (currentGoal.buildingType == Tags.Workshop)
        {
            workshop = InfoTool.closestObjectWithTag(strategyManager.knowledge.aiCastleList[0], Tags.Workshop);
        }
        else if (currentGoal.buildingType == Tags.Armoury)
        {
            armoury = InfoTool.closestObjectWithTag(strategyManager.knowledge.aiCastleList[0], Tags.Armoury);
        }

        isBuilding = false;
        if (strategyManager.strategy.buildingQueue.Count > 0)
        {
            currentGoal = new BuildingGoal(strategyManager.strategy.buildingQueue.Dequeue());
        }
        else
        {
            currentGoal = new BuildingGoal("None");
        }
        EventManager.TriggerEvent("FinishedBuilding");
        if (GetComponent<DebugComponent>().debug)
            Debug.Log("New goal: " + currentGoal.buildingType);
    }

    void buildNewWeapon ()
    {
        if (armoury == null)
        {
            return;
        }
        string nextWeapon = nextWeaponToBuild();
        if (nextWeapon != null)
        {
            armoury.GetComponent<HoldsWeapons>().buildWeapon(nextWeapon);
        }
    }

    string nextWeaponToBuild ()
    {
        // Which weapon do I build
        // percentages: spears -> 25%, hammers -> 25%, bows -> 25%, shield -> 25%
        if (armoury == null)
        {
            return null;
        }
        if (armoury.GetComponent<HoldsWeapons>() == null)
        {
            return null;
        }
        HoldsWeapons weaponInfo = armoury.GetComponent<HoldsWeapons>();

        if (weaponInfo.weapons[Tags.Spear] < HoldsWeapons.capacity)
        {
            return Tags.Spear;
        }
        else if (weaponInfo.weapons[Tags.Hammer] < HoldsWeapons.capacity)
        {
            return Tags.Hammer;
        }
        else if (weaponInfo.weapons[Tags.Bow] < HoldsWeapons.capacity)
        {
            return Tags.Bow;
        }
        else if (weaponInfo.weapons[Tags.Shield] < HoldsWeapons.capacity)
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
	Vector3 getOpenLocation ()
	{
        Vector3 castleLocation = strategyManager.knowledge.aiCastleList[0].transform.position;

        for (int i = 0; i < locations.Count; i++) {
			if (!buildLocationOccupiedDict [0] [i])
            {
                buildLocationOccupiedDict[0][i] = true;
                return castleLocation + locations[i];
            }
		}

		return new Vector3 ();
	}

	/// <summary>
	/// Inits the build location list.
	/// </summary>
	void initBuildLocationList ()
	{
		CastleController [] castles = FindObjectsOfType<CastleController> ();

		for (int i = 0; i < castles.Length; i++) {
			buildLocationOccupiedDict.Add (new Dictionary<int, bool> ());
			for (int j = 0; j < locations.Count; j++)
				buildLocationOccupiedDict [buildLocationOccupiedDict.Count - 1].Add (j, false);
		}
	}
}
