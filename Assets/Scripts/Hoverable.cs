using UnityEngine;

/// <summary>
/// Hoverable.
/// Object will change color if mouse is over it.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class Hoverable : MonoBehaviour {

	protected bool hover;

	protected Color originalColor;
	protected Color altColor;
	protected Material mat;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () {
		mat = GetComponent<Renderer>().material;
		originalColor = mat.color;
		altColor = originalColor + new Color(0.2f, 0.2f, 0.2f);
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {

		hover = Raycaster.pointingAtObject(gameObject);
		if (hover)
			mat.color = altColor;
		else
		{
			if (mat.color == altColor)
				mat.color = originalColor;
		}
	}
}
