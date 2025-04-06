using UnityEngine;

/// <summary>
/// Junction controller.
/// </summary>
public class JunctionController : MonoBehaviour, IUnit {

	const int MaxHealth = 10;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start() {
		GetComponent<Attackable>().SetHealth(MaxHealth);
	}

	/// <summary>
	/// Converts to tower.
	/// </summary>
	public void ConvertToTower()
	{
		CrabController crab = InfoTool.FindIdleCrab(GetComponent<Team>().team);
		crab.StartRebuild(gameObject, Tags.Tower);
	}
	
	public void SetController(Player player) {}
	public void SetAttacker(GameObject enemy) {}
	public void UpdateUI(InfoViewController gui) {}
	public void Deselect() {}
	public void ToggleSelected() {}
	public void Destroyed() {}
	public void EnemyDied() {}
}
