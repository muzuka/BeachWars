using UnityEngine;

/// <summary>
/// Junction controller.
/// </summary>
[RequireComponent(typeof(Attackable))]
[RequireComponent(typeof(Team))]
public class JunctionController : MonoBehaviour {

	const int JUNCTIONMAXHEALTH = 10;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () {
		GetComponent<Attackable>().setHealth(JUNCTIONMAXHEALTH);
	}

	/// <summary>
	/// Placeholder so SendMessage doesn't return null
	/// </summary>
	public void updateUI () {}

	/// <summary>
	/// Converts to tower.
	/// </summary>
	public void convertToTower ()
	{
		CrabController crab = InfoTool.findIdleCrab(GetComponent<Team>().team);
		crab.startRebuild(gameObject, Tags.Tower);
	}
}
