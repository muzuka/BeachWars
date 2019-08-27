using UnityEngine;

/// <summary>
/// Upgradable.
/// Handles upgrading objects.
/// </summary>
[RequireComponent(typeof(DebugComponent))]
[RequireComponent(typeof(CastleController))]
public class Upgradable : MonoBehaviour {

	[Tooltip("The amount of time to upgrade object")]
	public float timeToUpgrade;		// time required to upgrade object

	protected int otherCrabs;			// amount of crabs selected

	protected bool upgrading;			// is object being upgraded?
	protected float timeConsumed;		// time that has gone by

	protected bool debug;

	/// <summary>
	/// Upgrades object when time is up.
	/// </summary>
	protected virtual void upgrade ()
	{
	}

	/// <summary>
	/// Sets the number of crabs.
	/// </summary>
	/// <param name="crabs">Crabs.</param>
	public void setCrabs (int crabs)
	{
		otherCrabs = crabs;
	}
}
