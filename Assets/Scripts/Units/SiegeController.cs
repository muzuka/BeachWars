﻿using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Siege controller.
/// Controls siege weapons(catapults and ballistas)
/// </summary>
public class SiegeController : MonoBehaviour, IUnit {

    public GameObject Projectile;

	// public unit stats
	[Tooltip("How far it can attack")]
	public float AttackRange;
	[Tooltip("The damage it can deal")]
	public float AttackDamage;
	[Tooltip("The frequency of attack")]
	public float AttackSpeed;
	[Tooltip("The starting health")]
	public float MaxHealth;

	bool _attacking;				// attacking?
	bool _entering;				// entering?

	float _timeConsumed;			// time consumed

	Team _team;					// team of siege weapon

	NavMeshAgent _siegeAgent;	// NavMeshAgent of siege weapon

	bool _selected;				// is it selected?
	Vector3 _destination;		// current destination to attack or move
	GameObject _target;			// object to attack
	Player _controller;			// player object
    Animator _animator;          // for accessing the animator
            
	GameObject _boulder;
	
	DebugComponent _debug;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		_attacking = false;
		_entering = false;

		_timeConsumed = 0.0f;

		_selected = false;
		_destination = new Vector3();
		_target = null;
		_controller = null;
		_siegeAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

		GetComponent<Attackable>().SetHealth(MaxHealth);

		_team = GetComponent<Team>();
		_debug = GetComponent<DebugComponent>();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update()
	{
		if (!GetComponent<Enterable>().Occupied())
        {
            _siegeAgent.isStopped = true; 
        }

		if (_attacking && CanAttack())
		{
			_siegeAgent.isStopped = true;
			Attack();
		}

		if (_entering && CanInteract())
		{
			Enter();
			_entering = false;
		}

	}

	/// <summary>
	/// Destroyed this instance.
	/// </summary>
	public void Destroyed()
	{
		if (_debug)
			Debug.Log(tag + " was destroyed.");

		if (_target)
        {
            _target.GetComponent<IUnit>().EnemyDied(); 
        }
		if (_controller)
        {
            _controller.GetComponent<Player>().Deselect(gameObject); 
        }
	}

	/// <summary>
	/// Starts siege weapon moving.
	/// </summary>
	/// <param name="dest">Destination.</param>
	public void StartMove(Vector3 dest)
	{
		if (GetComponent<Enterable>().Occupied())
		{
			_debug.LogMessage("Crab moving to " + dest);
			_siegeAgent.isStopped = false;
			_siegeAgent.ResetPath();
			_destination = dest;
			_siegeAgent.destination = _destination;
			// play walking animation
		}
	}

    /// <summary>
    /// Stops siege weapon from 
    /// </summary>
    public void StopMove()
    {
        _siegeAgent.isStopped = true;
        _siegeAgent.ResetPath();
    }

	/// <summary>
	/// Starts siege weapon attacking.
	/// </summary>
	/// <param name="attackTarget">Target object.</param>
	public void StartAttack(GameObject attackTarget)
	{
		_debug.Assert(attackTarget.GetComponent<Attackable>() != null);
		_debug.LogMessage("Started attack!");

		_target = attackTarget;

		if ((!ValidTarget()) || !TargetIsEnemy())
		{
			_debug.LogMessage("Invalid Target");
			
			_target = null;
			return;
		}

		if (IdUtility.IsMoveable(_target))
        {
            _target.GetComponent<IUnit>().SetAttacker(gameObject); 
        }

		StartMove(_target.transform.position);
		_attacking = true;
	}

	/// <summary>
	/// Attacks when time is up.
	/// </summary>
	void Attack()
	{
		_timeConsumed += Time.deltaTime;
		if (_timeConsumed >= AttackSpeed)
		{
			_debug.LogMessage(gameObject.tag + " launched attack!");

            _animator.SetTrigger("Fire");
			_boulder = Instantiate(Projectile, transform.position, transform.rotation);
			_boulder.GetComponent<ProjectileController>().Destination = _target.transform.position;
			_boulder.GetComponent<ProjectileController>().Moving = true;
			_target.GetComponent<Attackable>().Attacked(AttackDamage);
			_timeConsumed = 0.0f;
			_siegeAgent.isStopped = false;
		}
	}

	/// <summary>
	/// Sets the attacker.
	/// </summary>
	/// <param name="enemy">Enemy object.</param>
	public void SetAttacker(GameObject enemy)
	{
		_target = enemy;
	}

	/// <summary>
	/// Starts siege weapon to enter a tower.
	/// </summary>
	/// <param name="enterTarget">Target object.</param>
	public void StartEnter(GameObject enterTarget)
	{
		_debug.LogMessage("Started entering");

		_target = enterTarget;

		if (!_target.CompareTag(Tags.Tower) || !_target.GetComponent<Team>().OnTeam(_team.team))
		{
			_debug.LogMessage("Cannot Enter");
			
			_target = null;
			return;
		}

		StartMove(_target.transform.position);
		_entering = true;
	}

	/// <summary>
	/// Enters the tower.
	/// </summary>
	void Enter()
	{
		_target.GetComponent<Enterable>().RequestEntry(gameObject);
		if (_controller)
        {
            _controller.GetComponent<Player>().Deselect(); 
        }
	}

	/// <summary>
	/// Called by enemy to inform object to stop attacking.
	/// </summary>
	public void EnemyDied()
	{
		_attacking = false;
		_target = null;
	}

	/// <summary>
	/// Is target object valid?
	/// </summary>
	/// <returns><c>true</c>, if target is valid, <c>false</c> otherwise.</returns>
	bool ValidTarget()
	{
		return (IdUtility.IsCrab(_target) || 
		        _target.CompareTag(Tags.Block) || 
		        _target.CompareTag(Tags.Castle) || 
		        IdUtility.IsSiegeWeapon(_target));
	}

	/// <summary>
	/// Is target an enemy?
	/// </summary>
	/// <returns><c>true</c>, if target is enemy, <c>false</c> otherwise.</returns>
	bool TargetIsEnemy()
	{
		if (IdUtility.IsResource(_target))
        {
            return true; 
        }

		return !_target.GetComponent<Team>().OnTeam(_team.team);
	}

	/// <summary>
	/// Is siege weapon close enough to the target?
	/// For entering.
	/// </summary>
	/// <returns><c>true</c>, if siege weapon can interact, <c>false</c> otherwise.</returns>
	bool CanInteract()
	{
		return (GetDistance(_destination) < AttackRange / 2);
	}

	/// <summary>
	/// Is crab close enough to the target?
	/// For attacking.
	/// </summary>
	/// <returns><c>true</c>, if crab can interact, <c>false</c> otherwise.</returns>
	bool CanAttack()
	{
		return (GetDistance(_destination) < AttackRange);
	}

	/// <summary>
	/// Gets the distance from siege weapon.
	/// </summary>
	/// <returns>The distance.</returns>
	/// <param name="pos">Position.</param>
	float GetDistance(Vector3 pos)
	{
		return Vector3.Distance(gameObject.transform.position, pos);
	}

	/// <summary>
	/// Is siege weapon busy?
	/// </summary>
	/// <returns><c>true</c>, if siege weapon is busy, <c>false</c> otherwise.</returns>
	public bool IsBusy()
	{
		return _attacking;
	}

	/// <summary>
	/// Toggles the selected.
	/// </summary>
	public void ToggleSelected()
	{
		_selected = !_selected;
	}
	
	public void Deselect()
	{
		Debug.Log("deselected");
		_controller = null;
		_selected = false;
	}
	
	/// <summary>
	/// Sets the controller.
	/// </summary>
	/// <param name="obj">Player script.</param>
	public void SetController(Player obj)
	{
		_controller = obj;
		_selected = true;
	}

	/// <summary>
	/// Updates the UI.
	/// </summary>
	/// <param name="gui">GUI script.</param>
	public void UpdateUI(InfoViewController gui)
	{
		GetComponent<Attackable>().SetHealth(gui.HealthSlider);
	}
}
