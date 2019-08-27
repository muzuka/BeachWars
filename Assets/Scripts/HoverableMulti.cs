using UnityEngine;

/// <summary>
/// Hoverable multi.
/// For hovering over armoury
/// </summary>
public class HoverableMulti : Hoverable {

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () {
		originalColor = GetComponentInChildren<Renderer>().material.color;
		altColor = originalColor + new Color(0.2f, 0.2f, 0.2f);
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {
		hover = Raycaster.pointingAtObject(gameObject);
		if (hover)
			changeColors(altColor);
		else
		{
			changeColors(originalColor);
		}
	}

	/// <summary>
	/// Changes the color.
	/// </summary>
	/// <param name="newColor">New color.</param>
	void changeColors(Color newColor)
	{
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		for(int i = 0; i < renderers.Length; i++)
		{
			renderers[i].material.color = newColor;
		}
	}
}
