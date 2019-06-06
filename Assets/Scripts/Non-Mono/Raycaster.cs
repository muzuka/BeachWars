using UnityEngine;

/// <summary>
/// Provides ray casting utilities.
/// </summary>
public static class Raycaster {

	/// <summary>
	/// Returns ray from mouse position.
	/// </summary>
	/// <returns>The ray.</returns>
	public static Ray GetRay() 
	{
		Vector3 mPos = Input.mousePosition;
		return Camera.main.ScreenPointToRay(mPos);
	}

	/// <summary>
	/// Casts a ray from mouse position.
	/// </summary>
	/// <returns>The RaycastHit object.</returns>
	public static RaycastHit ShootMouseRay()
	{
		RaycastHit hit;
		Physics.Raycast(GetRay(), out hit);
		return hit;
	}

	/// <summary>
	/// Gets the object at mouse position.
	/// </summary>
	/// <returns>The object.</returns>
	public static GameObject GetObjectAtRay()
	{
		return ShootMouseRay().transform.gameObject;
	}

	/// <summary>
	/// Is mouse pointing at object with tag?
	/// </summary>
	/// <returns>true, if mouse is pointing and false, otherwise.</returns>
	/// <param name="tag">Object tag.</param>
	public static bool IsPointingAt(string tag)
	{
		return ShootMouseRay().transform.gameObject.tag == tag;
	}

	/// <summary>
	/// Is mouse pointing at specific object?
	/// </summary>
	/// <returns><c>true</c>, if object is pointed at, <c>false</c> otherwise.</returns>
	/// <param name="query">Object.</param>
	public static bool PointingAtObject(GameObject query)
	{
		if (!query)
			return false;

		RaycastHit hit = ShootMouseRay();

		if (!hit.transform)
        {
            return false; 
        }
		else
        {
            return hit.transform.gameObject == query; 
        }
	}
}
