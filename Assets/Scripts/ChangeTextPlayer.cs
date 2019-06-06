using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Change player state message.
/// For the debug UI in top left corner.
/// </summary>
[RequireComponent(typeof(Text))]
public class ChangeTextPlayer : MonoBehaviour {

	string _state;

	Player _player;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start() {
		_state = gameObject.name;
		_player = FindObjectOfType<Player>();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update() {
		GetComponent<Text>().text = _state + ": " + _player.States.GetState(_state);
	}
}
