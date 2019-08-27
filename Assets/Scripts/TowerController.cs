using UnityEngine;

/// <summary>
/// Tower controller.
/// Handles attacking.
/// </summary>
[RequireComponent(typeof(Attackable))]
[RequireComponent(typeof(Enterable))]
[RequireComponent(typeof(DebugComponent))]
public class TowerController : MonoBehaviour {

	[Tooltip("Base distance it attacks from")]
	public float attackRange;
    [Tooltip("Starting health")]
    public int maxHealth;
    [Tooltip("Time between attacks")]
    public float timeToAttack;

	float attackDamage;

    float finalTimeToAttack;
	float timeConsumed;

	GameObject target;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		target = null;

        finalTimeToAttack = timeToAttack;
		timeConsumed = 0.0f;

		GetComponent<Attackable>().setHealth(maxHealth);
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update()
	{
		GameObject occupant = GetComponent<Enterable>().occupant;

		if (GetComponent<Enterable>().occupied())
		{
			switch (occupant.tag) {
			case Tags.Crab:
				attackDamage = 1.5f;
				finalTimeToAttack = timeToAttack;
				break;
			case Tags.Catapult:
			case Tags.Ballista:
				attackDamage = occupant.GetComponent<SiegeController>().attackDamage;
				finalTimeToAttack = occupant.GetComponent<SiegeController>().attackSpeed;
				break;
			}
		}

		if (!target)
			getTarget();
	
		if (target && Vector3.Distance(gameObject.transform.position, target.transform.position) <= attackRange && GetComponent<Enterable>().occupied())
		{
			timeConsumed += Time.deltaTime;
			if (timeConsumed >= finalTimeToAttack)
				target.GetComponent<Attackable>().attacked(attackDamage);
		}

	}

	/// <summary>
	/// Call when tower is destroyed.
	/// </summary>
	public void destroyed ()
	{
		if (GetComponent<DebugComponent>().debug)
			Debug.Log("Tower was destroyed.");
	}

	/// <summary>
	/// Gets a nearby target to attack.
	/// </summary>
	void getTarget ()
	{
		target = null;
		CrabController[] crabs = FindObjectsOfType<CrabController>();
		for(int i = 0; i < crabs.Length; i++)
		{
			if(crabs[i].gameObject.GetComponent<Team>().team != GetComponent<Team>().team)
			{
				if (Vector3.Distance(crabs[i].gameObject.transform.position, gameObject.transform.position) <= attackRange)
				{
					target = crabs[i].gameObject;
					break;
				}
			}
		}
	}
}
