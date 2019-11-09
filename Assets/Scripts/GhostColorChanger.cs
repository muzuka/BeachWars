using UnityEngine;

/// <summary>
/// Changes color of object.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class GhostColorChanger : MonoBehaviour {

	MeshRenderer _meshRenderer;

	public bool CanBuild { get; set; }

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		CanBuild = true;
		_meshRenderer = GetComponent<MeshRenderer>();
	}

	/// <summary>
	/// Raises the trigger enter event.
	/// </summary>
	void OnTriggerEnter()
	{
		_meshRenderer.material = Resources.Load<Material>("Materials/Denial");
		CanBuild = false;
	}

	/// <summary>
	/// Raises the trigger exit event.
	/// </summary>
	void OnTriggerExit()
	{
		_meshRenderer.material = Resources.Load<Material>("Materials/GhostBlock");
		CanBuild = true;
	}
}
