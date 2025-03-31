using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tactics manager.
/// 	Handles combat strategy.
/// 	Tracks proximity of enemy crabs to friendly units and buildings.
/// 	Executes individual actions to defeat enemies.
/// 	Can use all crabs indiscriminately.
/// 	Can release crabs from combat back to other managers.
/// 	Can organize crabs into groups.
/// </summary>
public class TacticsManager : MonoBehaviour {

    public int SmallUnitSize;

    public CastleController MainCastle { get; set; }

    List<Unit> _smallUnits;

    EnemyKnowledge _knowledge;
    StrategyManager _strategyManager;
    IncomeManager _incomeManager;

    int _crabCount = 0;

    DebugComponent _debug;

    /// <summary>
    /// Wake this instance
    /// </summary>
    void Awake()
    {
        _smallUnits = new List<Unit>();
        _knowledge = GetComponent<EnemyKnowledge>();
        _strategyManager = GetComponent<StrategyManager>();
        _incomeManager = GetComponent<IncomeManager>();
        _debug = GetComponent<DebugComponent>();
    }
	
	/// <summary>
    /// Update this instance
    /// </summary>
	void Update()
    {
        if (_knowledge.AICastleList.Count > 0 && MainCastle == null)
            MainCastle = _knowledge.AICastleList[0].GetComponent<CastleController>();

        if (_knowledge.AICrabList.Count < _crabCount)
        {
            // crabs have been destroyed
            // Clean out units
            for (int i = 0; i < _smallUnits.Count; i++) {
                Unit currentUnit = _smallUnits[i];
                int killedUnits = currentUnit.CountKilledUnits();
                if (killedUnits > 0)
                {
                    currentUnit.CleanUnit();
                }
            }
        }
        else if (_knowledge.AICrabList.Count > _crabCount)
        {
            // new crabs have been created.
            _smallUnits.Clear();
            SortCrabsIntoUnits();
        }
    }

    /// <summary>
    /// Launches attack on a target
    /// </summary>
    /// <param name="numberOfUnits">Number of small units to attack</param>
    /// <param name="target">Target</param>
    public void LaunchAttack(int numberOfUnits, GameObject target)
    {
        if (numberOfUnits > _smallUnits.Count)
        {
            foreach (Unit unit in _smallUnits)
            {
                unit.Attack(target);
            }
        }
        else
        {
            for (int i = 0; i < numberOfUnits; i++)
            {
                _smallUnits[i].Attack(target);
            }
        }
    }

    /// <summary>
    /// Commands crabs to grab a weapon
    /// </summary>
    /// <param name="unitNum">Number of crabs to command</param>
    /// <param name="weapon">The kind of weapon to equip</param>
    /// <param name="armoury">The armoury with the weapons</param>
    public void EquipUnit(int unitNum, string weapon, GameObject armoury)
    {
        if (armoury.GetComponent<HoldsWeapons>() == null)
        {
            return;
        }

        if (unitNum < _smallUnits.Count && unitNum >= 0)
        {
            _smallUnits[unitNum].Equip(weapon, armoury);
        }
    }
    
    /// <summary>
    /// Divides the crabs into units.
    /// </summary>
    void SortCrabsIntoUnits() {
        int unitCount = _knowledge.AICrabList.Count / SmallUnitSize;

        int crabPos = 0;
        for (int i = 0; i < unitCount; i++) 
        {
            Unit newUnit = new Unit(SmallUnitSize);
            for (int j = 0; j < SmallUnitSize; j++)
            {
                newUnit.AddCrab(_knowledge.AICrabList[crabPos]);
                crabPos++;
            }
            _smallUnits.Add(newUnit);
        }
    }
}
