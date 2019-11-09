using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Resource controller.
/// Handles amount of resources and life of object.
/// </summary>
public class ResourceController : MonoBehaviour {

	[Tooltip("Amount of resource")]
	public int ResourceCount;			// amount of resource available(set by level design)

	List<GameObject> _crabs;		// list of crabs collecting from resource

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		_crabs = new List<GameObject>();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update()
	{
		if (ResourceCount <= 0)
		{
			for (int i = 0; i < _crabs.Count; i++)
			{
				_crabs[i].SendMessage("stopCollecting", SendMessageOptions.DontRequireReceiver);
			}
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// Placeholder so SendMessage doesn't return null.
	/// </summary>
	public void SetController() {}

	/// <summary>
	/// Placeholder so SendMessage doesn't return null.
	/// </summary>
	public void UpdateUI() {}

	/// <summary>
	/// Sets the crabs that are taking resources.
	/// </summary>
	/// <param name="crabs">Crab list.</param>
	public void SetCrabs(List<GameObject> crabs)
	{
		this._crabs = crabs;
	}

	/// <summary>
	/// Crab takes a resource.
	/// </summary>
	public void Take()
	{
		ResourceCount--;
	}
}
