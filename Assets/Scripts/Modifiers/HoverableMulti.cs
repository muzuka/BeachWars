using UnityEngine;

/// <summary>
/// Hoverable multi.
/// For hovering over armoury
/// </summary>
public class HoverableMulti : Hoverable {

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start() {
		OriginalColor = GetComponentInChildren<Renderer>().material.color;
		AltColor = OriginalColor + new Color(0.2f, 0.2f, 0.2f);
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update() {
		Hover = Raycaster.PointingAtObject(gameObject);
		if (Hover)
        {
            ChangeColors(AltColor); 
        }
		else
		{
			ChangeColors(OriginalColor);
		}
	}

	/// <summary>
	/// Changes the color.
	/// </summary>
	/// <param name="newColor">New color.</param>
	void ChangeColors(Color newColor)
	{
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].material.color = newColor;
		}
	}
}
