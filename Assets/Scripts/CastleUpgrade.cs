using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleUpgrade : Upgradable {

	[Tooltip("The max upgrade of the object")]
	public int MaxLevel;
	[Tooltip("The amount of resources to upgrade")]
	public int UpgradeCost;

	int _level;				// current level
	int _nextLevel;			// next level to upgrade to. Used to check for ability to upgrade.

	// Use this for initialization
	void Start() {
		_level = 1;
		_nextLevel = 0;
		Upgrading = false;
		TimeConsumed = 0.0f;
		OtherCrabs = 1;
		Debug = GetComponent<DebugComponent>().Debug;
	}
	
	// Update is called once per frame
	void Update() {
		if (Upgrading)
			Upgrade();
	}

	/// <summary>
	/// Starts the castle upgrade.
	/// </summary>
	public void StartCastleUpgrade(int woodAmount, int stoneAmount)
	{
		if (CanUpgrade(_level + 1, woodAmount, stoneAmount))
		{
			_nextLevel = _level + 1;
			Upgrading = true;
			TimeConsumed = 0.0f;
		}
		else if (Debug)
            UnityEngine.Debug.Log("Couldn't upgrade castle!");
	}

	/// <summary>
	/// Upgrades castle when time is up.
	/// </summary>
	protected override void Upgrade() 
	{
		if (Debug)
            UnityEngine.Debug.Log("Upgrading " + gameObject.tag + ": " + TimeConsumed + " / " + (TimeToUpgrade / OtherCrabs));

		TimeConsumed += Time.deltaTime;
		if (TimeConsumed > (TimeToUpgrade / OtherCrabs))
		{
			if (Debug)
                UnityEngine.Debug.Log("Upgrade complete!");

			_level = _nextLevel;
			TimeConsumed = 0.0f;
			Upgrading = false;
			OtherCrabs = 1;
			gameObject.SendMessage("UpgradeFinished", _nextLevel, SendMessageOptions.DontRequireReceiver);
		}
	}

	/// <summary>
	/// Does castle have enough resources or has it reached the max level?
	/// </summary>
	/// <returns><c>true</c>, if level less than or equal to maxLevel, <c>false</c> otherwise.</returns>
	/// <param name="testLevel">The next level.</param>
	/// <param name="woodAmount">Amount of wood</param>
	/// <param name="stoneAmount">Amount of stone</param>
	bool CanUpgrade(int testLevel, int woodAmount, int stoneAmount)
	{
		return testLevel <= MaxLevel && (woodAmount >= UpgradeCost && stoneAmount >= UpgradeCost);
	}

	/// <summary>
	/// Gets the current level.
	/// </summary>
	/// <returns>The current level.</returns>
	public int GetLevel()
	{
		return _level;
	}
}
