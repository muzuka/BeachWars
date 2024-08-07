using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Change crab state message.
/// For the debug UI in top left corner.
/// </summary>
[RequireComponent(typeof(Text))]
public class ChangeTextCrab : MonoBehaviour {

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

		if (_player.HasSelected)
		{
            if (_player.Selected != null)
            {
                if (_player.Selected.tag == Tags.Crab)
                {
                    GetComponent<Text>().text = _state + ": " + _player.Selected.GetComponent<CrabController>().ActionStates.GetState(_state);
                }
            }
		}

	}
}
