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
	public int WoodGoal;
    [Tooltip("Starting stone goal amount.")]
    public int StoneGoal;

    [Tooltip("Ratio of wood to stone to follow.")]
    public float WoodToStoneRatio;

	StrategyManager _strategyManager;
    EnemyKnowledge _knowledge;

	List<GameObject> _crabs;
	List<GameObject> _woodCrabs;
	List<GameObject> _stoneCrabs;
	List<GameObject> _idleCrabs;

    GameObject _currentWoodResource;
    GameObject _currentStoneResource;

    int _currentCrabCount;

    bool _initialSetupFinished = false;

	/// <summary>
    /// Start this instance.
    /// </summary>
	void Start()
    {
		_crabs = new List<GameObject>();
		_woodCrabs = new List<GameObject>();
		_stoneCrabs = new List<GameObject>();

		_strategyManager = GetComponent<StrategyManager>();
        _knowledge = GetComponent<EnemyKnowledge>();

		WoodGoal = _strategyManager.Strategy.WoodGoal;
		StoneGoal = _strategyManager.Strategy.StoneGoal;
        WoodToStoneRatio = _strategyManager.Strategy.WoodToStoneRatio;
	}

    private void OnEnable()
    {
        EventManager.StartListening("FinishedBuilding", ReassignCrabs);
    }

    private void OnDisable()
    {
        EventManager.StopListening("FinishedBuilding", ReassignCrabs);
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        if (_currentWoodResource == null)
        {
            if (_strategyManager.Knowledge.AICastleList.Count > 0 && _strategyManager.Knowledge.ResourceList.Count > 0)
            {
                _currentWoodResource = InfoTool.ClosestObject(_knowledge.AICastleList[0].transform.position, _knowledge.ResourceList.FindAll(x => (x != null && x.tag == Tags.Wood)));
                List<GameObject> tempList = new List<GameObject>(_woodCrabs);
                foreach (GameObject crab in tempList)
                {
                    CrabController crabController = crab.GetComponent<CrabController>();
                    crabController.GoIdle();
                    PlaceCrab(crabController);
                }
            }
        }
        if (_currentStoneResource == null)
        {
            if (_strategyManager.Knowledge.AICastleList.Count > 0 && _strategyManager.Knowledge.ResourceList.Count > 0)
            {
                _currentStoneResource = InfoTool.ClosestObject(_knowledge.AICastleList[0].transform.position, _knowledge.ResourceList.FindAll(x => (x != null && x.tag == Tags.Stone)));
                List<GameObject> tempList = new List<GameObject>(_stoneCrabs);
                foreach (GameObject crab in tempList)
                {
                    CrabController crabController = crab.GetComponent<CrabController>();
                    crabController.GoIdle();
                    PlaceCrab(crabController);
                }
            }
        }

        if (!_initialSetupFinished)
        {
            InitializeIncomeManager();
            _currentCrabCount = _knowledge.AICrabList.Count;
        }

        if (_currentCrabCount != _knowledge.AICrabList.Count)
        {
            HashSet<GameObject> newCrabs = new HashSet<GameObject>(_crabs);
            newCrabs.IntersectWith(_knowledge.AICrabSet);
            _crabs.AddRange(newCrabs);
            foreach (GameObject crab in newCrabs)
            {
                PlaceCrab(crab.GetComponent<CrabController>());
            }
            _currentCrabCount = _knowledge.AICrabSet.Count;
        }
	}

    /// <summary>
    /// Finds resources and assigns crabs to it.
    /// </summary>
    void InitializeIncomeManager()
    {
        if (_strategyManager.Knowledge.AICrabList.Count > 0)
        {
            _crabs = _strategyManager.Knowledge.AICrabList;
            _woodCrabs.Clear();
            _stoneCrabs.Clear();
            _currentWoodResource = InfoTool.ClosestObject(_knowledge.AICastleList[0].transform.position, _knowledge.ResourceList.FindAll(x => (x != null && x.tag == Tags.Wood)));
            _currentStoneResource = InfoTool.ClosestObject(_knowledge.AICastleList[0].transform.position, _knowledge.ResourceList.FindAll(x => (x != null && x.tag == Tags.Stone)));

            foreach (GameObject crab in _crabs)
            {
                CrabController crabController = crab.GetComponent<CrabController>();
                PlaceCrab(crabController);
                _initialSetupFinished = true;
            }
        }
    }

    /// <summary>
    /// Remove crab from control group when it enters a building.
    /// </summary>
    /// <param name="crab">Crab</param>
    public void CrabEnteredBuilding(GameObject crab)
    {
        if (_crabs.Contains(crab))
        {
            _crabs.Remove(crab);
        }
        else if (_woodCrabs.Contains(crab))
        {
            _woodCrabs.Remove(crab);
        }
        else if (_stoneCrabs.Contains(crab))
        {
            _stoneCrabs.Remove(crab);
        }
    }

    /// <summary>
    /// Returns a crab if either stone or wood gathering crabs have more than one.
    /// </summary>
    /// <returns>The spare crab.</returns>
	public GameObject GetSpareCrab()
	{
        GameObject crab = null;

        if (_stoneCrabs.Count > 1)
        {
            crab = _stoneCrabs[0];
            _stoneCrabs.RemoveAt(0);
            return crab;
        }

        if (_woodCrabs.Count > 1)
        {
            crab = _woodCrabs[0];
            _woodCrabs.RemoveAt(0);
            return crab;
        }

		return crab;
	}

    /// <summary>
    /// For a given idle crab, decides which resource to gather.
    /// </summary>
    /// <param name="crab">Crab.</param>
    void PlaceCrab(CrabController crab)
    {
        if (crab.ActionStates.IsIdle())
        {
            if (_woodCrabs.Count > 0 && _stoneCrabs.Count > 0)
            {
                if ((float)_woodCrabs.Count / (float)_stoneCrabs.Count > WoodToStoneRatio)
                {
                    _stoneCrabs.Add(crab.gameObject);
                    _strategyManager.StartCollect(crab.gameObject, _currentStoneResource);
                }
                else if ((float)_woodCrabs.Count / (float)_stoneCrabs.Count < WoodToStoneRatio)
                {
                    _woodCrabs.Add(crab.gameObject);
                    _strategyManager.StartCollect(crab.gameObject, _currentWoodResource);
                }
                else
                {
                    _stoneCrabs.Add(crab.gameObject);
                    _strategyManager.StartCollect(crab.gameObject, _currentStoneResource);
                }
            }
            else
            {
                if (_woodCrabs.Count == 0)
                {
                    _woodCrabs.Add(crab.gameObject);
                    _strategyManager.StartCollect(crab.gameObject, _currentWoodResource);
                }
                else if (_stoneCrabs.Count == 0)
                {
                    _stoneCrabs.Add(crab.gameObject);
                    _strategyManager.StartCollect(crab.gameObject, _currentStoneResource);
                }
            }
        }
    }

    /// <summary>
    /// When done building, find idle crabs and make them start collecting.
    /// </summary>
    void ReassignCrabs()
    {
        foreach (GameObject crab in _crabs)
        {
            CrabController crabController = crab.GetComponent<CrabController>();
            PlaceCrab(crabController);
        }
    }
}
