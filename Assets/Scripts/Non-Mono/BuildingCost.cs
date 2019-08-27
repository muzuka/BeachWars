
using System.IO;
using UnityEngine;

public static class BuildingCost
{
	public static bool canBuild (string type, int woodAmount, int stoneAmount)
	{
		BuildingCosts buildingCosts = BuildingCosts.load(Path.Combine(Application.dataPath, "GameData/buildingCosts.xml"));

		int woodCost = 0;
		int stoneCost = 0;

		for (int i = 0; i < buildingCosts.WoodCost.Count; i++)
		{
			if (buildingCosts.WoodCost[i].Name == type) 
			{
				woodCost = int.Parse(buildingCosts.WoodCost[i].Text);
				stoneCost = int.Parse(buildingCosts.StoneCost[i].Text);
			}
		}

		if(woodAmount >= woodCost && stoneAmount >= stoneCost)
			return true;
		else
			return false;
	}

	public static int getWoodRequirement(string type) {
		BuildingCosts buildingCosts = BuildingCosts.load(Path.Combine(Application.dataPath, "GameData/buildingCosts.xml"));

		for (int i = 0; i < buildingCosts.WoodCost.Count; i++) {
			if (buildingCosts.WoodCost[i].Name == type) {
				int woodCost = int.Parse(buildingCosts.WoodCost [i].Text);
				return woodCost;
			}
		}

		return -1;
	}

	public static int getStoneRequirement(string type) {
		BuildingCosts buildingCosts = BuildingCosts.load(Path.Combine(Application.dataPath, "GameData/buildingCosts.xml"));

		for (int i = 0; i < buildingCosts.WoodCost.Count; i++) {
			if (buildingCosts.StoneCost[i].Name == type) {
				int stoneCost = int.Parse (buildingCosts.WoodCost [i].Text);
				return stoneCost;
			}
		}

		return -1;
	}
}

