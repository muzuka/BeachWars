using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum wallUpgradeType {NORMAL, WOOD, STONE}

public class WallUpgrade : Upgradable {

	public wallUpgradeType CurrentLevel { get; set; }
	wallUpgradeType _targetLevel;

	/// <summary>
    /// Start this instance
    /// </summary>
	void Start() {
		_targetLevel = wallUpgradeType.NORMAL;
		CurrentLevel = wallUpgradeType.NORMAL;
		Upgrading = false;
		TimeConsumed = 0.0f;
		OtherCrabs = 1;
		Debug = GetComponent<DebugComponent>().Debug;
	}
	
	/// <summary>
    /// Update this instance
    /// </summary>
	void Update() {
		if (Upgrading)
        {
            Upgrade(); 
        }
	}

	/// <summary>
	/// Starts the wall upgrade.
	/// </summary>
	/// <param name="targetLevel">Target level.</param>
	public void StartWallUpgrade(wallUpgradeType targetLevel, Inventory crabInventory)
	{
		if (CanUpgrade(targetLevel, crabInventory))
		{
			Upgrading = true;
			TimeConsumed = 0.0f;
			this._targetLevel = targetLevel;
		}
		else if (Debug)
            UnityEngine.Debug.Log("Couldn't upgrade wall!");
	}

	/// <summary>
	/// Upgrades wall when time is up.
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

			TimeConsumed = 0.0f;
			Upgrading = false;
			OtherCrabs = 1;
			gameObject.SendMessage("UpgradeFinished", _targetLevel, SendMessageOptions.DontRequireReceiver);
			CurrentLevel = _targetLevel;
		}
	}

	/// <summary>
	/// Does the crab have the proper resources to upgrade this wall?
	/// </summary>
	bool CanUpgrade(wallUpgradeType targetType, Inventory crabInventory)
	{
		if (crabInventory.Full()) 
		{
			if (targetType == wallUpgradeType.STONE)
            {
                return crabInventory.IsAllOneType(Tags.Stone); 
            }
			else if (targetType == wallUpgradeType.WOOD)
            {
                return crabInventory.IsAllOneType(Tags.Wood); 
            }
		}
		return false;
	}
}
