using UnityEngine;

// Code from Ben D'Angelo's RTS selection solution
public static class SelectUtils {

	// Input: The camera script.
	// Input: The click position.
	// Returns the object at location.
	public static GameObject FindEntityAt(Camera camera, Vector3 position)
	{
		RaycastHit hit;

		if (Physics.Raycast(camera.ScreenPointToRay(position), out hit, 100))
		{
			GameObject entity = hit.transform.gameObject;
			if (!entity)
				return entity;
		}

		return null;
	}

	// Input: The camera script.
	// Input: The starting position of the box.
	// Input: The ending position of the box.
	// Returns the bounds contained in the box.
	public static Bounds GetViewportBounds(Camera camera, Vector3 anchor, Vector3 outer)
	{
		Vector3 anchorView = camera.ScreenToViewportPoint(anchor);
		Vector3 outerView = camera.ScreenToViewportPoint(outer);
		Vector3 min = Vector3.Min(anchorView, outerView);
		Vector3 max = Vector3.Max(anchorView, outerView);

		min.z = camera.nearClipPlane;
		max.z = camera.farClipPlane;

		var bounds = new Bounds();
		bounds.SetMinMax(min, max);
		return bounds;
	}

	// Input: The camera script.
	// Input: A bound.
	// Input: Position to query.
	// Returns whether position is within bounds.
	public static bool IsWithinBounds(Camera camera, Bounds viewportBounds, Vector3 position)
	{
		Vector3 viewportPoint = camera.WorldToViewportPoint(position);

		return viewportBounds.Contains(viewportPoint);
	}

}
