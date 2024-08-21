using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

/// <summary>
/// Camera mover.
/// Always attached to main camera object.
/// </summary>
public class CameraMover : MonoBehaviour {

	[Tooltip("The speed of the camera")]
	public float CameraSpeed;

	public BoxCollider CameraBounds;

	public BaseControls _cameraControls;

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

	void Awake()
	{
		_cameraControls = new BaseControls();
	}

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
	
	void OnEnable()
	{
		_cameraControls.Enable();
	}

	void OnDisable()
	{
		_cameraControls.Disable();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update() 
	{
		_mousePos = Mouse.current.position.ReadValue();

		// Set direction flags
		_right = (_mousePos.x > Screen.width - 5.0f || _cameraControls.Camera.Right.IsPressed());
		_left = (_mousePos.x < 5.0f || _cameraControls.Camera.Left.IsPressed());
		_up = (_mousePos.y > Screen.height - 5.0f || _cameraControls.Camera.Forward.IsPressed());
		_down = (_mousePos.y < 5.0f || _cameraControls.Camera.Back.IsPressed());

		// Override if out of bounds
		_right = _right && transform.position.x < CameraBounds.bounds.max.x;
		_left = _left && transform.position.x > CameraBounds.bounds.min.x;
		_up = _up && transform.position.z < CameraBounds.bounds.max.z;
		_down = _down && transform.position.z > CameraBounds.bounds.min.z;
		
		// Check for mouse wheel movement
		if (_cameraControls.Camera.ZoomOut.ReadValue<float>() > 0 && transform.position.y < _maxHeight)
			_cameraTransform.Translate(-_forwardDirection.direction);
		else if (_cameraControls.Camera.ZoomIn.ReadValue<float>() > 0 && transform.position.y > _minHeight)
			_cameraTransform.Translate(_forwardDirection.direction);
		
		if (_up)
			_cameraTransform.Translate(_upDirection * CameraSpeed);
		if (_down)
			_cameraTransform.Translate(_upDirection * -CameraSpeed);
		if (_left)
			_cameraTransform.Translate(new Vector3(-CameraSpeed, 0.0f, 0.0f));
		if (_right)
			_cameraTransform.Translate(new Vector3(CameraSpeed, 0.0f, 0.0f));
	}
}
