using UnityEngine;

/// <summary>
/// Upgradable.
/// Handles upgrading objects.
/// </summary>
[RequireComponent(typeof(DebugComponent))]
[RequireComponent(typeof(CastleController))]
public class Upgradable : MonoBehaviour {

	[Tooltip("The amount of time to upgrade object")]
	public float TimeToUpgrade;		// time required to upgrade object

	protected int OtherCrabs;			// amount of crabs selected

	protected bool Upgrading;			// is object being upgraded?
	protected float TimeConsumed;		// time that has gone by

	protected bool Debug;

	/// <summary>
	/// Upgrades object when time is up.
	/// </summary>
	protected virtual void Upgrade()
	{
	}

	/// <summary>
	/// Sets the number of crabs.
	/// </summary>
	/// <param name="crabs">Crabs.</param>
	public void SetCrabs(int crabs)
	{
		OtherCrabs = crabs;
	}
}
