using System.IO;
using UnityEngine;

public static class BuildingCost
{
    /// <summary>
    /// Determines if the resources for a building are enough.
    /// </summary>
    /// <param name="type">Building type</param>
    /// <param name="woodAmount">Amount of wood</param>
    /// <param name="stoneAmount">Amount of stone</param>
    /// <returns></returns>
	public static bool CanBuild(string type, int woodAmount, int stoneAmount)
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

		if (woodAmount >= woodCost && stoneAmount >= stoneCost)
        {
            return true; 
        }
		else
        {
            return false; 
        }
	}

    /// <summary>
    /// Gets wood requirement
    /// </summary>
    /// <param name="type">Building type</param>
    /// <returns>Amount of wood required</returns>
	public static int GetWoodRequirement(string type)
    {
		BuildingCosts buildingCosts = BuildingCosts.load(Path.Combine(Application.dataPath, "GameData/buildingCosts.xml"));

		for (int i = 0; i < buildingCosts.WoodCost.Count; i++) {
			if (buildingCosts.WoodCost[i].Name == type) {
				int woodCost = int.Parse(buildingCosts.WoodCost [i].Text);
				return woodCost;
			}
		}

		return -1;
	}

    /// <summary>
    /// Gets stone requirement
    /// </summary>
    /// <param name="type">Building type</param>
    /// <returns>Amount of stone required</returns>
	public static int GetStoneRequirement(string type)
    {
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

