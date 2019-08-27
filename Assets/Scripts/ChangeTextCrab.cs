using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Change crab state message.
/// For the debug UI in top left corner.
/// </summary>
[RequireComponent(typeof(Text))]
public class ChangeTextCrab : MonoBehaviour {

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

		if (player.hasSelected)
		{
            if (player.selected != null)
            {
                if (player.selected.tag == Tags.Crab)
                {
                    GetComponent<Text>().text = state + ": " + player.selected.GetComponent<CrabController>().actionStates.getState(state);
                }
            }
		}

	}
}
