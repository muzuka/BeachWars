using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum wallUpgradeType {NORMAL, WOOD, STONE}

public class WallUpgrade : Upgradable {

	public wallUpgradeType currentLevel { get; set; }
	wallUpgradeType targetLevel;

	// Use this for initialization
	void Start () {
		targetLevel = wallUpgradeType.NORMAL;
		currentLevel = wallUpgradeType.NORMAL;
		upgrading = false;
		timeConsumed = 0.0f;
		otherCrabs = 1;
		debug = GetComponent<DebugComponent>().debug;
	}
	
	// Update is called once per frame
	void Update () {
		if (upgrading)
			upgrade();
	}

	/// <summary>
	/// Starts the wall upgrade.
	/// </summary>
	/// <param name="targetLevel">Target level.</param>
	public void startWallUpgrade (wallUpgradeType targetLevel, Inventory crabInventory)
	{
		if (canUpgrade(targetLevel, crabInventory))
		{
			upgrading = true;
			timeConsumed = 0.0f;
			this.targetLevel = targetLevel;
		}
		else if(debug)
			Debug.Log("Couldn't upgrade wall!");
	}

	/// <summary>
	/// Upgrades wall when time is up.
	/// </summary>
	protected override void upgrade ()
	{
		if (debug)
			Debug.Log("Upgrading " + gameObject.tag + ": " + timeConsumed + " / " + (timeToUpgrade / otherCrabs));

		timeConsumed += Time.deltaTime;
		if (timeConsumed > (timeToUpgrade / otherCrabs))
		{
			if (debug)
				Debug.Log("Upgrade complete!");

			timeConsumed = 0.0f;
			upgrading = false;
			otherCrabs = 1;
			gameObject.SendMessage("upgradeFinished", targetLevel, SendMessageOptions.DontRequireReceiver);
			currentLevel = targetLevel;
		}
	}

	/// <summary>
	/// Does the crab have the proper resources to upgrade this wall?
	/// </summary>
	bool canUpgrade (wallUpgradeType targetType, Inventory crabInventory)
	{
		if (crabInventory.full()) 
		{
			if (targetType == wallUpgradeType.STONE)
				return crabInventory.isAllOneType(Tags.Stone);
			else if (targetType == wallUpgradeType.WOOD)
				return crabInventory.isAllOneType(Tags.Wood);
		}
		return false;
	}
}
