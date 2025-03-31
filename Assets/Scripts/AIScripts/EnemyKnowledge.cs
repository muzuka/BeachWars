using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gathers world knowledge.
/// </summary>
[RequireComponent(typeof(StrategyManager))]
[RequireComponent(typeof(DebugComponent))]
public class EnemyKnowledge : MonoBehaviour {

	int _team;

	const int _neutral = -1;

	// World info
	public HashSet<GameObject> NeutralCrabSet { get; set; }			// All neutral crabs
	public HashSet<GameObject> AICrabSet { get; set; }			// All allied crabs
	public HashSet<GameObject> UnarmedCrabSet { get; set; }			// All unarmed allied crabs
	public HashSet<GameObject> ArmedCrabSet { get; set; }			// All armed allied crabs
	public HashSet<GameObject> DangerZoneCrabSet { get; set; }		// All enemy crabs in the danger zone
    public HashSet<GameObject> WarningZoneCrabSet { get; set; }     // All enemy crabs in the warning zone
	public HashSet<GameObject> EnemyCrabSet { get; set; }			// All enemy crabs
	public List<GameObject> NeutralCrabList { get; set; }
	public List<GameObject> AICrabList { get; set; }
	public List<GameObject> UnarmedCrabList { get; set; }
	public List<GameObject> ArmedCrabList { get; set; }
    public List<GameObject> DangerZoneCrabList { get; set; }
    public List<GameObject> WarningZoneCrabList { get; set; }
	public List<GameObject> EnemyCrabList { get; set; }

	public Dictionary<GameObject, bool> IsBusyDict { get; set; }	// The busyness of allied crabs

	public HashSet<GameObject> AINestSet { get; set; }			// All allied nests
	public HashSet<GameObject> AIWorkshopSet { get; set; }		// All allied workshops
	public HashSet<GameObject> AIArmourySet { get; set; }		// All allied armouries
	public HashSet<GameObject> AITowerSet { get; set; }			// All allied towers
	public HashSet<GameObject> AICastleSet { get; set; }		// All allied castles
	public HashSet<GameObject> AISiegeSet { get; set; }			// All allied siege weapons
	public HashSet<GameObject> AIBlockSet { get; set; }			// All allied walls

    public HashSet<GameObject> EnemyNestSet { get; set; }           // All enemy nests
    public HashSet<GameObject> EnemyWorkshopSet { get; set; }       // All enemy workshops
    public HashSet<GameObject> EnemyArmourySet { get; set; }        // All enemy armouries
    public HashSet<GameObject> EnemyTowerSet { get; set; }          // All enemy towers
    public HashSet<GameObject> EnemyCastleSet { get; set; }         // All enemy castles
    public HashSet<GameObject> EnemySiegeSet { get; set; }          // All enemy siege weapons
    public HashSet<GameObject> EnemyBlockSet { get; set; }          // All enemy walls

    public HashSet<GameObject> ResourceSet { get; set; }            // All resources
    public List<GameObject> ResourceList { get; set; }

    public List<GameObject> AINestList { get; set; }
    public List<GameObject> AIWorkshopList { get; set; }
    public List<GameObject> AIArmouryList { get; set; }
	public List<GameObject> AITowerList { get; set; }
	public List<GameObject> AICastleList { get; set; }
	public List<GameObject> AISiegeList { get; set; }
	public List<GameObject> AIBlockList { get; set; }

    public List<GameObject> EnemyNestList { get; set; }
    public List<GameObject> EnemyWorkshopList { get; set; }
    public List<GameObject> enemyArmouryList { get; set; }
    public List<GameObject> EnemyTowerList { get; set; }
    public List<GameObject> EnemyCastleList { get; set; }
    public List<GameObject> EnemySiegeList { get; set; }
    public List<GameObject> EnemyBlockList { get; set; }

	// Defending State Info
	public float DistanceToClosestEnemyCrab { get; set; }
	//private HashSet<GameObject> emptyEnemyCastles;

	// Attacking and Defending State Info
	public float TroopRatio { get; set; } // player to enemy crab ratio

	// Collecting State Info
	public int TotalWood { get; set; }
	public int TotalStone { get; set; }

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start() {
		Debug.Log("Started knowledge.");

		_team = GetComponent<Team>().team;

		NeutralCrabSet = new HashSet<GameObject>();
		AICrabSet = new HashSet<GameObject>();
		UnarmedCrabSet = new HashSet<GameObject>();
		ArmedCrabSet = new HashSet<GameObject>();
		DangerZoneCrabSet = new HashSet<GameObject>();
        WarningZoneCrabSet = new HashSet<GameObject>();
		EnemyCrabSet = new HashSet<GameObject>();

		NeutralCrabList = new List<GameObject>();
		AICrabList = new List<GameObject>();
		UnarmedCrabList = new List<GameObject>();
		ArmedCrabList = new List<GameObject>();
		DangerZoneCrabList = new List<GameObject>();
        WarningZoneCrabList = new List<GameObject>();
		EnemyCrabList = new List<GameObject>();

		IsBusyDict = new Dictionary<GameObject, bool>();

		AINestSet = new HashSet<GameObject>();
		AIArmourySet = new HashSet<GameObject>();
		AIWorkshopSet = new HashSet<GameObject>();
		AITowerSet = new HashSet<GameObject>();
		AICastleSet = new HashSet<GameObject>();
		AISiegeSet = new HashSet<GameObject>();
		AIBlockSet = new HashSet<GameObject>();
		
        EnemyNestSet = new HashSet<GameObject>();
        EnemyArmourySet = new HashSet<GameObject>();
        EnemyWorkshopSet = new HashSet<GameObject>();
        EnemyTowerSet = new HashSet<GameObject>();
        EnemyCastleSet = new HashSet<GameObject>();
        EnemySiegeSet = new HashSet<GameObject>();
        EnemyBlockSet = new HashSet<GameObject>();

		AINestList = new List<GameObject>();
		AIArmouryList = new List<GameObject>();
		AIWorkshopList = new List<GameObject>();
		AITowerList = new List<GameObject>();
		AICastleList = new List<GameObject>();
		AISiegeList = new List<GameObject>();
		AIBlockList = new List<GameObject>();

        EnemyNestList = new List<GameObject>();
        enemyArmouryList = new List<GameObject>();
        EnemyWorkshopList = new List<GameObject>();
        EnemyTowerList = new List<GameObject>();
        EnemyCastleList = new List<GameObject>();
        EnemySiegeList = new List<GameObject>();
        EnemyBlockList = new List<GameObject>();

        ResourceSet = new HashSet<GameObject>();
        ResourceList = new List<GameObject>();
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update() 
	{
		TotalWood = 0;
		TotalStone = 0;

		GatherData();
	}

	/// <summary>
	/// Gathers data.
	/// </summary>
	void GatherData() 
	{
		GameObject[] everything = FindObjectsOfType<GameObject>();
		for (int i = 0; i < everything.Length; i++) 
		{
			switch (everything[i].tag) {
			case Tags.Crab:
				ProcessCrab(everything[i]);
				break;
			case Tags.Castle:
                TotalWood += everything[i].GetComponent<CastleController>().GetWoodPieces();
                TotalStone += everything[i].GetComponent<CastleController>().GetStonePieces();
				if (everything[i].GetComponent<Team>().team == _team)
					ProcessStructure(everything[i], AICastleList, AICastleSet);
				else
					ProcessStructure(everything[i], EnemyCastleList, EnemyCastleSet);
				break;
			case Tags.Nest:
				if (everything[i].GetComponent<Team>().team == _team)
					ProcessStructure(everything[i], AINestList, AINestSet);
				else
					ProcessStructure(everything[i], EnemyNestList, EnemyNestSet);
				break;
			case Tags.Armoury:
				if (everything[i].GetComponent<Team>().team == _team)
					ProcessStructure(everything[i], AIArmouryList, AIArmourySet);
				else
					ProcessStructure(everything[i], enemyArmouryList, EnemyArmourySet);
				break;
			case Tags.Workshop:
				if (everything[i].GetComponent<Team>().team == _team)
					ProcessStructure(everything[i], AIWorkshopList, AIWorkshopSet);
				else
					ProcessStructure(everything[i], EnemyWorkshopList, EnemyWorkshopSet);
				break;
			case Tags.Block:
				if (everything[i].GetComponent<Team>().team == _team)
					ProcessStructure(everything[i], AIBlockList, AIBlockSet);
				else
					ProcessStructure(everything[i], EnemyBlockList, EnemyBlockSet);
				break;
			case Tags.Catapult:
			case Tags.Ballista:
				if (everything[i].GetComponent<Team>().team == _team)
					ProcessStructure(everything[i], AISiegeList, AISiegeSet);
				else
					ProcessStructure(everything[i], EnemySiegeList, EnemySiegeSet);
				break;
			case Tags.Stone:
			case Tags.Wood:
				ProcessResource(everything[i]);
				break;
			}
		}
	}

	/// <summary>
	/// Evaluates crab and places it in the correct collection.
	/// </summary>
	/// <param name="crab">Crab object.</param>
	void ProcessCrab(GameObject crab)
	{
		int crabTeam = crab.GetComponent<Team>().team;

		if (crabTeam == _team && !IsBusyDict.ContainsKey(crab))
        {
            IsBusyDict.Add(crab, crab.GetComponent<CrabController>().IsBusy()); 
        }
		else if (crabTeam == _team)
        {
            IsBusyDict[crab] = crab.GetComponent<CrabController>().IsBusy(); 
        }

		if (crabTeam == _team)
        {
            ProcessFriendlyCrab(crab); 
        }
		else if ((crabTeam != _team && crabTeam != _neutral))
        {
            ProcessEnemyCrab(crab); 
        }
		else if (crabTeam == _neutral)
		{
			if (!NeutralCrabSet.Contains(crab))
			{
				NeutralCrabSet.Add(crab);
				NeutralCrabList.Add(crab);
			}
		}
		else if (GetComponent<DebugComponent>().IsDebugModeEnabled)
			Debug.Log("Crab has no valid team.");
	}

	/// <summary>
	/// Processes the friendly crab.
	/// </summary>
	/// <param name="crab">Friendly Crab.</param>
	void ProcessFriendlyCrab(GameObject crab)
	{
		if (!AICrabSet.Contains(crab)) 
		{
			AICrabSet.Add(crab);
			AICrabList.Add(crab);
			if (crab.GetComponent<CrabController>().Inventory.IsArmed())
			{
				if (!ArmedCrabSet.Contains(crab))
				{
					ArmedCrabSet.Add(crab);
					ArmedCrabList.Add(crab);
				}
			}
			else
			{
				if (!UnarmedCrabSet.Contains(crab))
				{
					UnarmedCrabSet.Add(crab);
					UnarmedCrabList.Add(crab);
				}
			}
		}
		else
		{
			if (crab.GetComponent<CrabController>().Inventory.IsArmed())
			{
				if (UnarmedCrabSet.Contains(crab))
				{
					UnarmedCrabSet.Remove(crab);
					UnarmedCrabList.Remove(crab);
				}
			}
			else 
			{
				if (ArmedCrabSet.Contains(crab))
				{
					ArmedCrabSet.Remove(crab);
					ArmedCrabList.Remove(crab);
				}
			}
		}
	}

	/// <summary>
	/// Processes the enemy crab.
	/// </summary>
	/// <param name="crab">Enemy Crab.</param>
	void ProcessEnemyCrab(GameObject crab)
	{
		GameObject closestCastle = InfoTool.ClosestObject(crab.transform.position, AICastleList);
		float dist = float.MaxValue;

		if (closestCastle)
        {
            dist = Vector3.Distance(crab.transform.position, closestCastle.transform.position); 
        }

		if (!EnemyCrabSet.Contains(crab)) 
		{
			EnemyCrabSet.Add(crab);
			EnemyCrabList.Add(crab);
		}

		if (closestCastle)
        {
            if (dist < GetComponent<StrategyManager>().DangerDistance)
            {
                if (!DangerZoneCrabSet.Contains(crab))
                {
                    DangerZoneCrabSet.Add(crab);
                    DangerZoneCrabList.Add(crab);
                }
            }
            else if (dist > GetComponent<StrategyManager>().DangerDistance)
            {
                if (DangerZoneCrabSet.Contains(crab))
                {
                    DangerZoneCrabSet.Remove(crab);
                    DangerZoneCrabList.Remove(crab);
                }
            }
            else if (dist < GetComponent<StrategyManager>().WarningDistance)
			{
				if (!WarningZoneCrabSet.Contains(crab))
				{
					WarningZoneCrabSet.Add(crab);
					WarningZoneCrabList.Add(crab);
				}
			}
			else if (dist > GetComponent<StrategyManager>().WarningDistance)
			{
				if (WarningZoneCrabSet.Contains(crab))
				{
					WarningZoneCrabSet.Remove(crab);
					WarningZoneCrabList.Remove(crab);
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
	void ProcessStructure(GameObject building, List<GameObject> list, HashSet<GameObject> hashSet)
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
	void ProcessResource(GameObject resource)
	{
		if (!ResourceSet.Contains(resource)) 
		{
			ResourceSet.Add(resource);
			ResourceList.Add(resource);
		}
	}
}
