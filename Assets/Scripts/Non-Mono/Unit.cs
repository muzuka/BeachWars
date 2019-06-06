using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit
{
    public int TargetSize { get; set; }
    public List<GameObject> Members { get; set; }
    StateController _unitStateController;
    string[] _unitStates = { "Moving", "Attacking", "Capturing"};

    public Unit()
    {
        TargetSize = 3;
        Members = new List<GameObject>();
        _unitStateController = new StateController(_unitStates);
    }

    public Unit(int targetSize)
    {
        this.TargetSize = targetSize;
        Members = new List<GameObject>();
        _unitStateController = new StateController(_unitStates);
    }

    public Unit(int targetSize, List<GameObject> units) 
    {
        this.TargetSize = targetSize;
        this.Members = units;
        _unitStateController = new StateController(_unitStates);
    }

    public void AddCrab(GameObject crab)
    {
        if (Members.Count + 1 <= TargetSize)
        {
            Members.Add(crab);
        }
    }

    public void Attack(GameObject target)
    {
        foreach (GameObject member in Members)
        {
            member.SendMessage("startAttack", target);
        }
    }

    public void Equip(string weapon, GameObject armoury)
    {
        foreach (GameObject member in Members)
        {
            member.GetComponent<CrabController>().StartTakeWeapon(weapon, armoury);
        }
    }

    // Cleans unit of killed members.
    public void CleanUnit() {
        for (int i = 0; i < Members.Count; i++)
        {
            if (Members[i] == null)
            {
                Members.RemoveAt(i);
                i--;
            }
        }
    }

    public int CountKilledUnits()
    {
        int killedUnitCount = 0;
        for (int i = 0; i < Members.Count; i++)
        {
            if (Members[i] == null)
            {
                killedUnitCount++;
            }
        }
        return killedUnitCount;
    }

    public bool NeedsMoreCrabs()
    {
        return Members.Count < TargetSize;
    }
}
