using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Income manager.
/// 	Assigns crabs to collect resources.
/// 	Attempts to reach goal set by StrategyManager.
/// 	Lets crabs go once goal is reached.
/// </summary>
public class IncomeManager : MonoBehaviour {

    [Tooltip("Starting wood goal amount.")]
	public int woodGoal;
    [Tooltip("Starting stone goal amount.")]
    public int stoneGoal;

    [Tooltip("Ratio of wood to stone to follow.")]
    public float woodToStoneRatio;

	StrategyManager strategyManager;
    EnemyKnowledge knowledge;

	List<GameObject> crabs;
	List<GameObject> woodCrabs;
	List<GameObject> stoneCrabs;
	List<GameObject> idleCrabs;

    GameObject currentWoodResource;
    GameObject currentStoneResource;

    int currentCrabCount;

    bool initialSetupFinished = false;

	// Use this for initialization
	void Start ()
    {
		crabs = new List<GameObject>();
		woodCrabs = new List<GameObject>();
		stoneCrabs = new List<GameObject>();

		strategyManager = GetComponent<StrategyManager>();
        knowledge = GetComponent<EnemyKnowledge>();

		woodGoal = strategyManager.strategy.woodGoal;
		stoneGoal = strategyManager.strategy.stoneGoal;
        woodToStoneRatio = strategyManager.strategy.woodToStoneRatio;
	}

    private void OnEnable()
    {
        EventManager.StartListening("FinishedBuilding", reassignCrabs);
    }

    private void OnDisable()
    {
        EventManager.StopListening("FinishedBuilding", reassignCrabs);
    }

    // Update is called once per frame
    void Update ()
    {
        if (currentWoodResource == null)
        {
            if (strategyManager.knowledge.aiCastleList.Count > 0 && strategyManager.knowledge.resourceList.Count > 0)
            {
                currentWoodResource = InfoTool.closestObjectWithTag(strategyManager.knowledge.aiCastleList[0], Tags.Wood, strategyManager.knowledge.resourceList);
                List<GameObject> tempList = new List<GameObject>(woodCrabs);
                foreach (GameObject crab in tempList)
                {
                    CrabController crabController = crab.GetComponent<CrabController>();
                    crabController.goIdle();
                    placeCrab(crabController);
                }
            }
        }
        if (currentStoneResource == null)
        {
            if (strategyManager.knowledge.aiCastleList.Count > 0 && strategyManager.knowledge.resourceList.Count > 0)
            {
                currentStoneResource = InfoTool.closestObjectWithTag(strategyManager.knowledge.aiCastleList[0], Tags.Stone, strategyManager.knowledge.resourceList);
                List<GameObject> tempList = new List<GameObject>(stoneCrabs);
                foreach (GameObject crab in tempList)
                {
                    CrabController crabController = crab.GetComponent<CrabController>();
                    crabController.goIdle();
                    placeCrab(crabController);
                }
            }
        }

        if (!initialSetupFinished)
        {
            initializeIncomeManager();
            currentCrabCount = knowledge.aiCrabList.Count;
        }

        if (currentCrabCount != knowledge.aiCrabList.Count)
        {
            HashSet<GameObject> newCrabs = new HashSet<GameObject>(crabs);
            newCrabs.IntersectWith(knowledge.aiCrabSet);
            crabs.AddRange(newCrabs);
            foreach (GameObject crab in newCrabs)
            {
                placeCrab(crab.GetComponent<CrabController>());
            }
            currentCrabCount = knowledge.aiCrabSet.Count;
        }
	}

    void initializeIncomeManager ()
    {
        if (strategyManager.knowledge.aiCrabList.Count > 0)
        {
            crabs = strategyManager.knowledge.aiCrabList;
            woodCrabs.Clear();
            stoneCrabs.Clear();
            currentWoodResource = InfoTool.closestObjectWithTag(strategyManager.knowledge.aiCastleList[0], Tags.Wood, strategyManager.knowledge.resourceList);
            currentStoneResource = InfoTool.closestObjectWithTag(strategyManager.knowledge.aiCastleList[0], Tags.Stone, strategyManager.knowledge.resourceList);

            foreach (GameObject crab in crabs)
            {
                CrabController crabController = crab.GetComponent<CrabController>();
                placeCrab(crabController);
                initialSetupFinished = true;
            }
        }
    }

    public void crabEnteredBuilding(GameObject crab)
    {
        if (crabs.Contains(crab))
        {
            crabs.Remove(crab);
        } else if (woodCrabs.Contains(crab))
        {
            woodCrabs.Remove(crab);
        } else if (stoneCrabs.Contains(crab))
        {
            stoneCrabs.Remove(crab);
        }
    }

    /// <summary>
    /// Returns a crab if either stone or wood gathering crabs have more than one.
    /// </summary>
    /// <returns>The spare crab.</returns>
	public GameObject getSpareCrab ()
	{
        GameObject crab = null;

        if (stoneCrabs.Count > 1)
        {
            crab = stoneCrabs[0];
            stoneCrabs.RemoveAt(0);
            return crab;
        }

        if (woodCrabs.Count > 1)
        {
            crab = woodCrabs[0];
            woodCrabs.RemoveAt(0);
            return crab;
        }

		return crab;
	}

    /// <summary>
    /// For a given idle crab, decides which resource to gather.
    /// </summary>
    /// <param name="crab">Crab.</param>
    void placeCrab(CrabController crab)
    {
        if (crab.actionStates.isIdle())
        {
            if (woodCrabs.Count > 0 && stoneCrabs.Count > 0)
            {
                if ((float)woodCrabs.Count / (float)stoneCrabs.Count > woodToStoneRatio)
                {
                    stoneCrabs.Add(crab.gameObject);
                    strategyManager.startCollect(crab.gameObject, currentStoneResource);
                }
                else if ((float)woodCrabs.Count / (float)stoneCrabs.Count < woodToStoneRatio)
                {
                    woodCrabs.Add(crab.gameObject);
                    strategyManager.startCollect(crab.gameObject, currentWoodResource);
                }
                else
                {
                    stoneCrabs.Add(crab.gameObject);
                    strategyManager.startCollect(crab.gameObject, currentStoneResource);
                }
            }
            else
            {
                if (woodCrabs.Count == 0)
                {
                    woodCrabs.Add(crab.gameObject);
                    strategyManager.startCollect(crab.gameObject, currentWoodResource);
                }
                else if (stoneCrabs.Count == 0)
                {
                    stoneCrabs.Add(crab.gameObject);
                    strategyManager.startCollect(crab.gameObject, currentStoneResource);
                }
            }
        }
    }

    /// <summary>
    /// When done building, find idle crabs and make them start collecting.
    /// </summary>
    void reassignCrabs()
    {
        foreach (GameObject crab in crabs)
        {
            CrabController crabController = crab.GetComponent<CrabController>();
            placeCrab(crabController);
        }
    }
}
