using UnityEngine;

/// <summary>
/// Camera mover.
/// Always attached to main camera object.
/// </summary>
public class CameraMover : MonoBehaviour {

	[Tooltip("The speed of the camera")]
	public float CameraSpeed;

	Vector3 _mousePos;
	Transform _cameraTransform;
	Vector3 _upDirection;
	Vector3 _result;

	bool _right;
	bool _left;
	bool _up;
	bool _down;

	//pre calculated points
	Vector3 _upperLeft = new Vector3(-1f, 0.0f, -1f);
	Vector3 _upperRight = new Vector3(1f, 0.0f, -1f);
	Vector3 _lowerLeft = new Vector3(-1f, 0.0f, 1f);
	Vector3 _lowerRight = new Vector3(1f, 0.0f, -1f);

	const int _minHeight = 20;
	const int _maxHeight = 50;

	Ray _forwardDirection;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start() 
	{
		_cameraTransform = gameObject.transform;

		_upDirection = _cameraTransform.worldToLocalMatrix * (new Vector3(0.0f, 0.0f, 0.5f));
		_upDirection.Normalize();

		_forwardDirection = Camera.main.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, 0.0f));

		_upperLeft = _cameraTransform.worldToLocalMatrix * _upperLeft;
		_upperRight = _cameraTransform.worldToLocalMatrix * _upperRight;
		_lowerLeft = _cameraTransform.worldToLocalMatrix * _lowerLeft;
		_lowerRight = _cameraTransform.worldToLocalMatrix * _lowerRight;
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update() 
	{
		_mousePos = Input.mousePosition;

		// define direction flags
		_right = (_mousePos.x > Screen.width - 5.0f || Input.GetKey(KeyCode.RightArrow));
		_left = (_mousePos.x < 5.0f || Input.GetKey(KeyCode.LeftArrow));
		_up = (_mousePos.y < 5.0f || Input.GetKey(KeyCode.DownArrow));
		_down = (_mousePos.y > Screen.height - 5.0f || Input.GetKey(KeyCode.UpArrow));

		// Check for mouse wheel movement
		if (Input.mouseScrollDelta.y < 0 && transform.position.y < _maxHeight)
			_cameraTransform.Translate(-_forwardDirection.direction);
		else if (Input.mouseScrollDelta.y > 0 && transform.position.y > _minHeight)
			_cameraTransform.Translate(_forwardDirection.direction);

		// Check directions and move accordingly
		if (_up && _right) 
		{
			_result = _upperRight - _lowerLeft;
			_result.Normalize();
			_cameraTransform.Translate(_result * CameraSpeed);
		}
		else if (_up && _left) 
		{
			_result = _upperLeft - _lowerRight;
			_result.Normalize();
			_cameraTransform.Translate(_result * CameraSpeed);
		}
		else if (_down && _right) 
		{
			_result = _lowerRight - _upperLeft;
			_result.Normalize();
			_cameraTransform.Translate(_result * CameraSpeed);
		}
		else if (_down && _left) 
		{
			_result = _lowerLeft - _upperRight;
			_result.Normalize();
			_cameraTransform.Translate(_result * CameraSpeed);
		}
		else if (_up)
			_cameraTransform.Translate(_upDirection * -CameraSpeed);
		else if (_down)
			_cameraTransform.Translate(_upDirection * CameraSpeed);
		else if (_left)
			_cameraTransform.Translate(new Vector3(-CameraSpeed, 0.0f, 0.0f));
		else if (_right)
			_cameraTransform.Translate(new Vector3(CameraSpeed, 0.0f, 0.0f));
	}
}
