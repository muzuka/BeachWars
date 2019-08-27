using UnityEngine;

/// <summary>
/// Changes color of object.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class GhostColorChanger : MonoBehaviour {

	MeshRenderer meshRenderer;

	public bool canBuild { get; set; }

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		canBuild = true;
		meshRenderer = GetComponent<MeshRenderer>();
	}

	/// <summary>
	/// Raises the trigger enter event.
	/// </summary>
	void OnTriggerEnter ()
	{
		meshRenderer.material = Resources.Load<Material>("Materials/Denial");
		canBuild = false;
	}

	/// <summary>
	/// Raises the trigger exit event.
	/// </summary>
	void OnTriggerExit ()
	{
		meshRenderer.material = Resources.Load<Material>("Materials/GhostBlock");
		canBuild = true;
	}
}
