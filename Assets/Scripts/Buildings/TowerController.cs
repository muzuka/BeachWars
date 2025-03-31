using UnityEngine;

/// <summary>
/// Tower controller.
/// Handles attacking.
/// </summary>
public class TowerController : MonoBehaviour {

	[Tooltip("Base distance it attacks from")]
	public float AttackRange;
    [Tooltip("Starting health")]
    public int MaxHealth;
    [Tooltip("Time between attacks")]
    public float TimeToAttack;

	float _attackDamage;

    float _finalTimeToAttack;
	float _timeConsumed;

	GameObject _target;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		_target = null;

        _finalTimeToAttack = TimeToAttack;
		_timeConsumed = 0.0f;

		GetComponent<Attackable>().SetHealth(MaxHealth);
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update()
	{
		GameObject occupant = GetComponent<Enterable>().Occupant;

		if (GetComponent<Enterable>().Occupied())
		{
			switch (occupant.tag) {
			case Tags.Crab:
				_attackDamage = 1.5f;
				_finalTimeToAttack = TimeToAttack;
				break;
			case Tags.Catapult:
			case Tags.Ballista:
				_attackDamage = occupant.GetComponent<SiegeController>().AttackDamage;
				_finalTimeToAttack = occupant.GetComponent<SiegeController>().AttackSpeed;
				break;
			}
		}

		if (!_target)
        {
            GetTarget(); 
        }
	
		if (_target && Vector3.Distance(gameObject.transform.position, _target.transform.position) <= AttackRange && GetComponent<Enterable>().Occupied())
		{
			_timeConsumed += Time.deltaTime;
			if (_timeConsumed >= _finalTimeToAttack)
            {
                _target.GetComponent<Attackable>().Attacked(_attackDamage); 
            }
		}

	}

	/// <summary>
	/// Call when tower is destroyed.
	/// </summary>
	public void Destroyed()
	{
		if (GetComponent<DebugComponent>().IsDebugModeEnabled)
			Debug.Log("Tower was destroyed.");
	}

	/// <summary>
	/// Gets a nearby target to attack.
	/// </summary>
	void GetTarget()
	{
		_target = null;
		CrabController[] crabs = FindObjectsOfType<CrabController>();
		Team team = GetComponent<Team>();
		for (int i = 0; i < crabs.Length; i++)
		{
			if (!team.OnTeam(crabs[i].gameObject.GetComponent<Team>().team))
			{
				if (Vector3.Distance(crabs[i].gameObject.transform.position, gameObject.transform.position) <= AttackRange)
				{
					_target = crabs[i].gameObject;
					break;
				}
			}
		}
	}
}
