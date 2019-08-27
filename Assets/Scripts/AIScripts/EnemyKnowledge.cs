using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gathers world knowledge.
/// </summary>
[RequireComponent(typeof(StrategyManager))]
[RequireComponent(typeof(DebugComponent))]
public class EnemyKnowledge : MonoBehaviour {

	int team;

	const int neutral = -1;

	// World info
	public HashSet<GameObject> neutralCrabSet { get; set; }			// All neutral crabs
	public HashSet<GameObject> aiCrabSet { get; set; }			// All allied crabs
	public HashSet<GameObject> unarmedCrabSet { get; set; }			// All unarmed allied crabs
	public HashSet<GameObject> armedCrabSet { get; set; }			// All armed allied crabs
	public HashSet<GameObject> dangerZoneCrabSet { get; set; }		// All enemy crabs in the danger zone
    public HashSet<GameObject> warningZoneCrabSet { get; set; }     // All enemy crabs in the warning zone
	public HashSet<GameObject> enemyCrabSet { get; set; }			// All enemy crabs
	public List<GameObject> neutralCrabList { get; set; }
	public List<GameObject> aiCrabList { get; set; }
	public List<GameObject> unarmedCrabList { get; set; }
	public List<GameObject> armedCrabList { get; set; }
    public List<GameObject> dangerZoneCrabList { get; set; }
    public List<GameObject> warningZoneCrabList { get; set; }
	public List<GameObject> enemyCrabList { get; set; }

	public Dictionary<GameObject, bool> isBusyDict { get; set; }	// The busyness of allied crabs

	public HashSet<GameObject> aiNestSet { get; set; }			// All allied nests
	public HashSet<GameObject> aiWorkshopSet { get; set; }		// All allied workshops
	public HashSet<GameObject> aiArmourySet { get; set; }		// All allied armouries
	public HashSet<GameObject> aiTowerSet { get; set; }			// All allied towers
	public HashSet<GameObject> aiCastleSet { get; set; }		// All allied castles
	public HashSet<GameObject> aiSiegeSet { get; set; }			// All allied siege weapons
	public HashSet<GameObject> aiBlockSet { get; set; }			// All allied walls
	

    public HashSet<GameObject> enemyNestSet { get; set; }           // All enemy nests
    public HashSet<GameObject> enemyWorkshopSet { get; set; }       // All enemy workshops
    public HashSet<GameObject> enemyArmourySet { get; set; }        // All enemy armouries
    public HashSet<GameObject> enemyTowerSet { get; set; }          // All enemy towers
    public HashSet<GameObject> enemyCastleSet { get; set; }         // All enemy castles
    public HashSet<GameObject> enemySiegeSet { get; set; }          // All enemy siege weapons
    public HashSet<GameObject> enemyBlockSet { get; set; }          // All enemy walls

    public HashSet<GameObject> resourceSet { get; set; }            // All resources
    public List<GameObject> resourceList { get; set; }

    public List<GameObject> aiNestList { get; set; }
    public List<GameObject> aiWorkshopList { get; set; }
    public List<GameObject> aiArmouryList { get; set; }
	public List<GameObject> aiTowerList { get; set; }
	public List<GameObject> aiCastleList { get; set; }
	public List<GameObject> aiSiegeList { get; set; }
	public List<GameObject> aiBlockList { get; set; }

    public List<GameObject> enemyNestList { get; set; }
    public List<GameObject> enemyWorkshopList { get; set; }
    public List<GameObject> enemyArmouryList { get; set; }
    public List<GameObject> enemyTowerList { get; set; }
    public List<GameObject> enemyCastleList { get; set; }
    public List<GameObject> enemySiegeList { get; set; }
    public List<GameObject> enemyBlockList { get; set; }

	// Defending State Info
	public float distanceToClosestEnemyCrab { get; set; }
	//private HashSet<GameObject> emptyEnemyCastles;

	// Attacking and Defending State Info
	public float troopRatio { get; set; } // player to enemy crab ratio

	// Collecting State Info
	public int totalWood { get; set; }
	public int totalStone { get; set; }

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () {
		Debug.Log("Started knowledge.");

		team = GetComponent<Team>().team;

		neutralCrabSet = new HashSet<GameObject>();
		aiCrabSet = new HashSet<GameObject>();
		unarmedCrabSet = new HashSet<GameObject>();
		armedCrabSet = new HashSet<GameObject>();
		dangerZoneCrabSet = new HashSet<GameObject>();
        warningZoneCrabSet = new HashSet<GameObject>();
		enemyCrabSet = new HashSet<GameObject>();

		neutralCrabList = new List<GameObject>();
		aiCrabList = new List<GameObject>();
		unarmedCrabList = new List<GameObject>();
		armedCrabList = new List<GameObject>();
		dangerZoneCrabList = new List<GameObject>();
        warningZoneCrabList = new List<GameObject>();
		enemyCrabList = new List<GameObject>();

		isBusyDict = new Dictionary<GameObject, bool>();

		aiNestSet = new HashSet<GameObject>();
		aiArmourySet = new HashSet<GameObject>();
		aiWorkshopSet = new HashSet<GameObject>();
		aiTowerSet = new HashSet<GameObject>();
		aiCastleSet = new HashSet<GameObject>();
		aiSiegeSet = new HashSet<GameObject>();
		aiBlockSet = new HashSet<GameObject>();
		
        enemyNestSet = new HashSet<GameObject>();
        enemyArmourySet = new HashSet<GameObject>();
        enemyWorkshopSet = new HashSet<GameObject>();
        enemyTowerSet = new HashSet<GameObject>();
        enemyCastleSet = new HashSet<GameObject>();
        enemySiegeSet = new HashSet<GameObject>();
        enemyBlockSet = new HashSet<GameObject>();

		aiNestList = new List<GameObject>();
		aiArmouryList = new List<GameObject>();
		aiWorkshopList = new List<GameObject>();
		aiTowerList = new List<GameObject>();
		aiCastleList = new List<GameObject>();
		aiSiegeList = new List<GameObject>();
		aiBlockList = new List<GameObject>();

        enemyNestList = new List<GameObject>();
        enemyArmouryList = new List<GameObject>();
        enemyWorkshopList = new List<GameObject>();
        enemyTowerList = new List<GameObject>();
        enemyCastleList = new List<GameObject>();
        enemySiegeList = new List<GameObject>();
        enemyBlockList = new List<GameObject>();

        resourceSet = new HashSet<GameObject>();
        resourceList = new List<GameObject>();
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () 
	{
		totalWood = 0;
		totalStone = 0;

		gatherData();
	}

	/// <summary>
	/// Gathers data.
	/// </summary>
	void gatherData() 
	{
		GameObject[] everything = FindObjectsOfType<GameObject>();
		for(int i = 0; i < everything.Length; i++) 
		{
			switch (everything[i].tag) {
			case Tags.Crab:
				processCrab(everything[i]);
				break;
			case Tags.Castle:
                totalWood += everything[i].GetComponent<CastleController>().getWoodPieces();
                totalStone += everything[i].GetComponent<CastleController>().getStonePieces();
				if(everything[i].GetComponent<Team>().team == team)
					processStructure(everything[i], aiCastleList, aiCastleSet);
				else
					processStructure(everything[i], enemyCastleList, enemyCastleSet);
				break;
			case Tags.Nest:
				if(everything[i].GetComponent<Team>().team == team)
					processStructure(everything[i], aiNestList, aiNestSet);
				else
					processStructure(everything[i], enemyNestList, enemyNestSet);
				break;
			case Tags.Armoury:
				if(everything[i].GetComponent<Team>().team == team)
					processStructure(everything[i], aiArmouryList, aiArmourySet);
				else
					processStructure(everything[i], enemyArmouryList, enemyArmourySet);
				break;
			case Tags.Workshop:
				if(everything[i].GetComponent<Team>().team == team)
					processStructure(everything[i], aiWorkshopList, aiWorkshopSet);
				else
					processStructure(everything[i], enemyWorkshopList, enemyWorkshopSet);
				break;
			case Tags.Block:
				if(everything[i].GetComponent<Team>().team == team)
					processStructure(everything[i], aiBlockList, aiBlockSet);
				else
					processStructure(everything[i], enemyBlockList, enemyBlockSet);
				break;
			case Tags.Catapult:
			case Tags.Ballista:
				if(everything[i].GetComponent<Team>().team == team)
					processStructure(everything[i], aiSiegeList, aiSiegeSet);
				else
					processStructure(everything[i], enemySiegeList, enemySiegeSet);
				break;
			case Tags.Stone:
			case Tags.Wood:
				processResource(everything[i]);
				break;
			}
		}
	}

	/// <summary>
	/// Evaluates crab and places it in the correct collection.
	/// </summary>
	/// <param name="crab">Crab object.</param>
	void processCrab (GameObject crab)
	{
		int crabTeam = crab.GetComponent<Team>().team;

		if (crabTeam == team && !isBusyDict.ContainsKey(crab))
			isBusyDict.Add(crab, crab.GetComponent<CrabController>().isBusy());
		else if (crabTeam == team)
			isBusyDict[crab] = crab.GetComponent<CrabController>().isBusy();

		if (crabTeam == team) 
			processFriendlyCrab(crab);
		else if ((crabTeam != team && crabTeam != neutral))
			processEnemyCrab(crab);
		else if (crabTeam == neutral)
		{
			if (!neutralCrabSet.Contains(crab))
			{
				neutralCrabSet.Add(crab);
				neutralCrabList.Add(crab);
			}
		}
		else if (GetComponent<DebugComponent>().debug)
			Debug.Log("Crab has no valid team.");
	}

	/// <summary>
	/// Processes the friendly crab.
	/// </summary>
	/// <param name="crab">Friendly Crab.</param>
	void processFriendlyCrab (GameObject crab)
	{
		if (!aiCrabSet.Contains(crab)) 
		{
			aiCrabSet.Add(crab);
			aiCrabList.Add(crab);
			if (crab.GetComponent<CrabController>().inventory.isArmed())
			{
				if(!armedCrabSet.Contains(crab))
				{
					armedCrabSet.Add(crab);
					armedCrabList.Add(crab);
				}
			}
			else
			{
				if(!unarmedCrabSet.Contains(crab))
				{
					unarmedCrabSet.Add(crab);
					unarmedCrabList.Add(crab);
				}
			}
		}
		else
		{
			if (crab.GetComponent<CrabController>().inventory.isArmed())
			{
				if(unarmedCrabSet.Contains(crab))
				{
					unarmedCrabSet.Remove(crab);
					unarmedCrabList.Remove(crab);
				}
			}
			else 
			{
				if(armedCrabSet.Contains(crab))
				{
					armedCrabSet.Remove(crab);
					armedCrabList.Remove(crab);
				}
			}
		}
	}

	/// <summary>
	/// Processes the enemy crab.
	/// </summary>
	/// <param name="crab">Enemy Crab.</param>
	void processEnemyCrab (GameObject crab)
	{
		GameObject closestCastle = InfoTool.closestObject(crab, aiCastleList);
		float dist = float.MaxValue;

		if(closestCastle)
			dist = Vector3.Distance(crab.transform.position, closestCastle.transform.position);

		if (!enemyCrabSet.Contains(crab)) 
		{
			enemyCrabSet.Add(crab);
			enemyCrabList.Add(crab);
		}

		if(closestCastle)
        {
            if (dist < GetComponent<StrategyManager>().dangerDistance)
            {
                if (!dangerZoneCrabSet.Contains(crab))
                {
                    dangerZoneCrabSet.Add(crab);
                    dangerZoneCrabList.Add(crab);
                }
            }
            else if (dist > GetComponent<StrategyManager>().dangerDistance)
            {
                if (dangerZoneCrabSet.Contains(crab))
                {
                    dangerZoneCrabSet.Remove(crab);
                    dangerZoneCrabList.Remove(crab);
                }
            }
            else if (dist < GetComponent<StrategyManager>().warningDistance)
			{
				if (!warningZoneCrabSet.Contains(crab))
				{
					warningZoneCrabSet.Add(crab);
					warningZoneCrabList.Add(crab);
				}
			}
			else if (dist > GetComponent<StrategyManager>().warningDistance)
			{
				if (warningZoneCrabSet.Contains(crab))
				{
					warningZoneCrabSet.Remove(crab);
					warningZoneCrabList.Remove(crab);
				}
			}
		}
	}

	/// <summary>
	/// Evaluates the nest and places it in the correct collection.
	/// </summary>
	/// <param name="building">Building object.</param>
	/// <param name="list">List to add to.</param>
	/// <param name="hashSet">Set to add to.</param>
	void processStructure (GameObject building, List<GameObject> list, HashSet<GameObject> hashSet)
	{
		if (!hashSet.Contains(building))
		{
			hashSet.Add(building);
			list.Add(building);
		}
	}

	/// <summary>
	/// Evaluates the resource and places it in the correct collection.
	/// </summary>
	/// <param name="resource">Resource.</param>
	void processResource (GameObject resource)
	{
		if (!resourceSet.Contains(resource)) 
		{
			resourceSet.Add(resource);
			resourceList.Add(resource);
		}
	}
}
