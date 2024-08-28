using UnityEngine;

/// <summary>
/// Changes color of object.
/// </summary>
public class GhostColorChanger : MonoBehaviour
{
	public Material Invalid;
	public Material Valid;
	
	MeshRenderer _meshRenderer;

	public bool CanBuild;

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
		_meshRenderer.material = Invalid;
		CanBuild = false;
	}

	/// <summary>
	/// Raises the trigger exit event.
	/// </summary>
	void OnTriggerExit()
	{
		_meshRenderer.material = Valid;
		CanBuild = true;
	}
}
