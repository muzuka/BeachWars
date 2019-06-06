using System.Collections.Generic;
using UnityEngine;
public class BuildingGoal : Goal
{
	public string buildingType { get; set; }

	public BuildingGoal(string building)
	{
		buildingType = building;
	}

	public bool isFinished(List<GameObject> buildings)
	{
		foreach (GameObject building in buildings) {
			if (building.tag == buildingType)
				return true;
		}
		return false;
	}
}