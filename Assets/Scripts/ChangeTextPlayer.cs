using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Change player state message.
/// For the debug UI in top left corner.
/// </summary>
[RequireComponent(typeof(Text))]
public class ChangeTextPlayer : MonoBehaviour {

	string state;

	Player player;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () {
		state = gameObject.name;
		player = FindObjectOfType<Player>();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {
		GetComponent<Text>().text = state + ": " + player.states.getState(state);
	}
}
