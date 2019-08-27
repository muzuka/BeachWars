using UnityEngine;

/// <summary>
/// Provides ray casting utilities.
/// </summary>
public static class Raycaster {

	/// <summary>
	/// Returns ray from mouse position.
	/// </summary>
	/// <returns>The ray.</returns>
	public static Ray getRay () 
	{
		Vector3 mPos = Input.mousePosition;
		return Camera.main.ScreenPointToRay(mPos);
	}

	/// <summary>
	/// Casts a ray from mouse position.
	/// </summary>
	/// <returns>The RaycastHit object.</returns>
	public static RaycastHit shootMouseRay ()
	{
		RaycastHit hit;
		Physics.Raycast(getRay(), out hit);
		return hit;
	}

	/// <summary>
	/// Gets the object at mouse position.
	/// </summary>
	/// <returns>The object.</returns>
	public static GameObject getObjectAtRay ()
	{
		return shootMouseRay().transform.gameObject;
	}

	/// <summary>
	/// Is mouse pointing at object with tag?
	/// </summary>
	/// <returns>true, if mouse is pointing and false, otherwise.</returns>
	/// <param name="tag">Object tag.</param>
	public static bool isPointingAt (string tag)
	{
		return shootMouseRay().transform.gameObject.tag == tag;
	}

	/// <summary>
	/// Is mouse pointing at specific object?
	/// </summary>
	/// <returns><c>true</c>, if object is pointed at, <c>false</c> otherwise.</returns>
	/// <param name="query">Object.</param>
	public static bool pointingAtObject(GameObject query)
	{
		if (!query)
			return false;

		RaycastHit hit = shootMouseRay();

		if (!hit.transform)
			return false;
		else
			return hit.transform.gameObject == query;
	}
}
