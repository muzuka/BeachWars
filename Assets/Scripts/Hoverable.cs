using UnityEngine;

/// <summary>
/// Hoverable.
/// Object will change color if mouse is over it.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class Hoverable : MonoBehaviour {

	protected bool Hover;

	protected Color OriginalColor;
	protected Color AltColor;
	protected Material Mat;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start() {
		Mat = GetComponent<Renderer>().material;
		OriginalColor = Mat.color;
		AltColor = OriginalColor + new Color(0.2f, 0.2f, 0.2f);
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update() {

		Hover = Raycaster.PointingAtObject(gameObject);
		if (Hover)
			Mat.color = AltColor;
		else
		{
			if (Mat.color == AltColor)
            {
                Mat.color = OriginalColor; 
            }
		}
	}
}
