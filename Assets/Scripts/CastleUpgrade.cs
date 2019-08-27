using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleUpgrade : Upgradable {

	[Tooltip("The max upgrade of the object")]
	public int maxLevel;
	[Tooltip("The amount of resources to upgrade")]
	public int upgradeCost;

	int level;				// current level
	int nextLevel;			// next level to upgrade to. Used to check for ability to upgrade.

	// Use this for initialization
	void Start () {
		level = 1;
		nextLevel = 0;
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
	/// Starts the castle upgrade.
	/// </summary>
	public void startCastleUpgrade (int woodAmount, int stoneAmount)
	{
		if (canUpgrade(level + 1, woodAmount, stoneAmount))
		{
			nextLevel = level + 1;
			upgrading = true;
			timeConsumed = 0.0f;
		}
		else if (debug)
			Debug.Log("Couldn't upgrade castle!");
	}

	/// <summary>
	/// Upgrades castle when time is up.
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

			level = nextLevel;
			timeConsumed = 0.0f;
			upgrading = false;
			otherCrabs = 1;
			gameObject.SendMessage("upgradeFinished", nextLevel, SendMessageOptions.DontRequireReceiver);
		}
	}

	/// <summary>
	/// Does castle have enough resources or has it reached the max level?
	/// </summary>
	/// <returns><c>true</c>, if level less than or equal to maxLevel, <c>false</c> otherwise.</returns>
	/// <param name="testLevel">The next level.</param>
	/// <param name="woodAmount">Amount of wood</param>
	/// <param name="stoneAmount">Amount of stone</param>
	bool canUpgrade (int testLevel, int woodAmount, int stoneAmount)
	{
		return testLevel <= maxLevel && (woodAmount >= upgradeCost && stoneAmount >= upgradeCost);
	}

	/// <summary>
	/// Gets the current level.
	/// </summary>
	/// <returns>The current level.</returns>
	public int getLevel ()
	{
		return level;
	}
}
