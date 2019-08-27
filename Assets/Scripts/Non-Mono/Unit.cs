using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit
{
    public int targetSize { get; set; }
    public List<GameObject> members { get; set; }
    StateController unitStateController;
    string[] unitStates = { "Moving", "Attacking", "Capturing"};

    public Unit ()
    {
        targetSize = 3;
        members = new List<GameObject>();
        unitStateController = new StateController(unitStates);
    }

    public Unit (int targetSize)
    {
        this.targetSize = targetSize;
        members = new List<GameObject>();
        unitStateController = new StateController(unitStates);
    }

    public Unit (int targetSize, List<GameObject> units) 
    {
        this.targetSize = targetSize;
        this.members = units;
        unitStateController = new StateController(unitStates);
    }

    public void AddCrab (GameObject crab)
    {
        if (members.Count + 1 <= targetSize)
        {
            members.Add(crab);
        }
    }

    public void attack (GameObject target)
    {
        foreach (GameObject member in members)
        {
            member.SendMessage("startAttack", target);
        }
    }

    public void equip (string weapon, GameObject armoury)
    {
        foreach (GameObject member in members)
        {
            member.GetComponent<CrabController>().startTakeWeapon(weapon, armoury);
        }
    }

    // Cleans unit of killed members.
    public void cleanUnit () {
        for (int i = 0; i < members.Count; i++)
        {
            if (members[i] == null)
            {
                members.RemoveAt(i);
                i--;
            }
        }
    }

    public int countKilledUnits ()
    {
        int killedUnitCount = 0;
        for (int i = 0; i < members.Count; i++)
        {
            if (members[i] == null)
            {
                killedUnitCount++;
            }
        }
        return killedUnitCount;
    }

    public bool needsMoreCrabs ()
    {
        return members.Count < targetSize;
    }
}
