using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Resource controller.
/// Handles amount of resources and life of object.
/// </summary>
public class ResourceController : MonoBehaviour {

	[Tooltip("Amount of resource")]
	public int resourceCount;			// amount of resource available(set by level design)

	List<GameObject> crabs;		// list of crabs collecting from resource

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		crabs = new List<GameObject>();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update ()
	{
		if (resourceCount <= 0)
		{
			for(int i = 0; i < crabs.Count; i++)
			{
				crabs[i].SendMessage("stopCollecting", SendMessageOptions.DontRequireReceiver);
			}
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// Placeholder so SendMessage doesn't return null.
	/// </summary>
	public void setController () {}

	/// <summary>
	/// Placeholder so SendMessage doesn't return null.
	/// </summary>
	public void updateUI () {}

	/// <summary>
	/// Sets the crabs that are taking resources.
	/// </summary>
	/// <param name="crabs">Crab list.</param>
	public void setCrabs (List<GameObject> crabs)
	{
		this.crabs = crabs;
	}

	/// <summary>
	/// Crab takes a resource.
	/// </summary>
	public void take ()
	{
		resourceCount--;
	}
}
