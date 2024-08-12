using UnityEngine;

/// <summary>
/// Junction controller.
/// </summary>
public class JunctionController : MonoBehaviour {

	const int MaxHealth = 10;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start() {
		GetComponent<Attackable>().SetHealth(MaxHealth);
	}

	/// <summary>
	/// Placeholder so SendMessage doesn't return null
	/// </summary>
	public void UpdateUI() {}

	/// <summary>
	/// Converts to tower.
	/// </summary>
	public void ConvertToTower()
	{
		CrabController crab = InfoTool.FindIdleCrab(GetComponent<Team>().team);
		crab.StartRebuild(gameObject, Tags.Tower);
	}
}
