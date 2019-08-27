using UnityEngine;

/// <summary>
/// Camera mover.
/// Always attached to main camera object.
/// </summary>
public class CameraMover : MonoBehaviour {

	[Tooltip("The speed of the camera")]
	public float cameraSpeed;

	Vector3 mousePos;
	Transform cameraTransform;
	Vector3 upDirection;
	Vector3 result;

	bool right;
	bool left;
	bool up;
	bool down;

	//pre calculated points
	Vector3 upperLeft = new Vector3(-1f, 0.0f, -1f);
	Vector3 upperRight = new Vector3(1f, 0.0f, -1f);
	Vector3 lowerLeft = new Vector3(-1f, 0.0f, 1f);
	Vector3 lowerRight = new Vector3(1f, 0.0f, -1f);

	const int minHeight = 20;
	const int maxHeight = 50;

	Ray forwardDirection;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () 
	{
		cameraTransform = gameObject.transform;

		upDirection = cameraTransform.worldToLocalMatrix * (new Vector3(0.0f, 0.0f, 0.5f));
		upDirection.Normalize();

		forwardDirection = Camera.main.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, 0.0f));

		upperLeft = cameraTransform.worldToLocalMatrix * upperLeft;
		upperRight = cameraTransform.worldToLocalMatrix * upperRight;
		lowerLeft = cameraTransform.worldToLocalMatrix * lowerLeft;
		lowerRight = cameraTransform.worldToLocalMatrix * lowerRight;
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () 
	{
		mousePos = Input.mousePosition;

		// define direction flags
		right = (mousePos.x > Screen.width - 5.0f || Input.GetKey(KeyCode.RightArrow));
		left = (mousePos.x < 5.0f || Input.GetKey(KeyCode.LeftArrow));
		up = (mousePos.y < 5.0f || Input.GetKey(KeyCode.DownArrow));
		down = (mousePos.y > Screen.height - 5.0f || Input.GetKey(KeyCode.UpArrow));

		// Check for mouse wheel movement
		if (Input.mouseScrollDelta.y < 0 && transform.position.y < maxHeight)
			cameraTransform.Translate(-forwardDirection.direction);
		else if (Input.mouseScrollDelta.y > 0 && transform.position.y > minHeight)
			cameraTransform.Translate(forwardDirection.direction);

		// Check directions and move accordingly
		if (up && right) 
		{
			result = upperRight - lowerLeft;
			result.Normalize();
			cameraTransform.Translate(result * cameraSpeed);
		}
		else if (up && left) 
		{
			result = upperLeft - lowerRight;
			result.Normalize();
			cameraTransform.Translate(result * cameraSpeed);
		}
		else if (down && right) 
		{
			result = lowerRight - upperLeft;
			result.Normalize();
			cameraTransform.Translate(result * cameraSpeed);
		}
		else if (down && left) 
		{
			result = lowerLeft - upperRight;
			result.Normalize();
			cameraTransform.Translate(result * cameraSpeed);
		}
		else if (up)
			cameraTransform.Translate(upDirection * -cameraSpeed);
		else if (down)
			cameraTransform.Translate(upDirection * cameraSpeed);
		else if (left)
			cameraTransform.Translate(new Vector3(-cameraSpeed, 0.0f, 0.0f));
		else if (right)
			cameraTransform.Translate(new Vector3(cameraSpeed, 0.0f, 0.0f));
	}
}
