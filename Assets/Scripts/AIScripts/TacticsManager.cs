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

    public int smallUnitSize;

    public CastleController mainCastle { get; set; }

    List<Unit> smallUnits;

    EnemyKnowledge knowledge;
    StrategyManager strategyManager;
    IncomeManager incomeManager;

    int crabCount = 0;

    bool debug;

    void Awake()
    {
        smallUnits = new List<Unit>();
        knowledge = GetComponent<EnemyKnowledge>();
        strategyManager = GetComponent<StrategyManager>();
        incomeManager = GetComponent<IncomeManager>();
        debug = GetComponent<DebugComponent>().debug;
    }

    // Use this for initialization
    void Start ()
    {   
        
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (knowledge.aiCastleList.Count > 0 && mainCastle == null)
            mainCastle = knowledge.aiCastleList[0].GetComponent<CastleController>();

        if (crabCount == 0 && knowledge.aiCrabList.Count != crabCount)
        {
            sortCrabsIntoUnits();
        }
        else if (knowledge.aiCrabList.Count < crabCount)
        {
            // crabs have been destroyed
            // Clean out units
            for (int i = 0; i < smallUnits.Count; i++) {
                Unit currentUnit = smallUnits[i];
                int killedUnits = currentUnit.countKilledUnits();
                if (killedUnits > 0)
                {
                    currentUnit.cleanUnit();
                }
            }
        }
        else if (knowledge.aiCrabList.Count > crabCount)
        {
            // new crabs have been created.
            smallUnits.Clear();
            sortCrabsIntoUnits();
        }
    }

    public void launchAttack(int numberOfUnits, GameObject target)
    {
        if (numberOfUnits > smallUnits.Count)
        {
            foreach (Unit unit in smallUnits)
            {
                unit.attack(target);
            }
        }
        else
        {
            for (int i = 0; i < numberOfUnits; i++)
            {
                smallUnits[i].attack(target);
            }
        }
    }

    public void equipUnit(int unitNum, string weapon, GameObject armoury)
    {
        if (armoury.GetComponent<HoldsWeapons>() == null)
        {
            return;
        }

        if (unitNum < smallUnits.Count && unitNum >= 0)
        {
            smallUnits[unitNum].equip(weapon, armoury);
        }
    }
    
    void sortCrabsIntoUnits() {
        int unitCount = knowledge.aiCrabList.Count / smallUnitSize;

        int crabPos = 0;
        for (int i = 0; i < unitCount; i++) 
        {
            Unit newUnit = new Unit(smallUnitSize);
            for (int j = 0; j < smallUnitSize; j++)
            {
                newUnit.AddCrab(knowledge.aiCrabList[crabPos]);
                crabPos++;
            }
            smallUnits.Add(newUnit);
        }
    }
}
