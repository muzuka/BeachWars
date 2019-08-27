using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Siege controller.
/// Controls siege weapons (catapults and ballistas)
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Attackable))]
[RequireComponent(typeof(Team))]
[RequireComponent(typeof(DebugComponent))]
[RequireComponent(typeof(Enterable))]
public class SiegeController : MonoBehaviour {

	// public unit stats
	[Tooltip("How far it can attack")]
	public float attackRange;
	[Tooltip("The damage it can deal")]
	public float attackDamage;
	[Tooltip("The frequency of attack")]
	public float attackSpeed;
	[Tooltip("The starting health")]
	public float maxHealth;

	bool attacking;				// attacking?
	bool entering;				// entering?

	float timeConsumed;			// time consumed

	Team team;					// team of siege weapon

	NavMeshAgent siegeAgent;	// NavMeshAgent of siege weapon

	bool selected;				// is it selected?
	Vector3 destination;		// current destination to attack or move
	GameObject target;			// object to attack
	Player controller;			// player object
    Animator animator;          // for accessing the animator

	GameObject boulder;

	bool debug;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		attacking = false;
		entering = false;

		timeConsumed = 0.0f;

		selected = false;
		destination = new Vector3();
		target = null;
		controller = null;
		siegeAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

		GetComponent<Attackable>().setHealth(maxHealth);

		team = GetComponent<Team>();

		debug = GetComponent<DebugComponent>().debug;
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update ()
	{
		if (!GetComponent<Enterable>().occupied())
			siegeAgent.isStopped = true;

		if (attacking && canAttack())
		{
			siegeAgent.isStopped = true;
			attack();
		}

		if (entering && canInteract())
		{
			enter();
			entering = false;
		}

	}

	/// <summary>
	/// Destroyed this instance.
	/// </summary>
	public void destroyed ()
	{
		if (debug)
			Debug.Log(tag + " was destroyed.");

		if (target)
			target.SendMessage("enemyDied", SendMessageOptions.DontRequireReceiver);
		if (controller)
			controller.GetComponent<Player>().deselect(gameObject);
	}

	/// <summary>
	/// Starts siege weapon moving.
	/// </summary>
	/// <param name="dest">Destination.</param>
	public void startMove (Vector3 dest)
	{
		if (GetComponent<Enterable>().occupied())
		{
			if (debug)
				Debug.Log("Crab moving to " + dest);

			siegeAgent.isStopped = false;
			siegeAgent.ResetPath();
			destination = dest;
			siegeAgent.destination = destination;
			// play walking animation
		}
	}

    /// <summary>
    /// Stops siege weapon from 
    /// </summary>
    public void stopMove()
    {
        siegeAgent.isStopped = true;
        siegeAgent.ResetPath();
    }

	/// <summary>
	/// Starts siege weapon attacking.
	/// </summary>
	/// <param name="attackTarget">Target object.</param>
	public void startAttack (GameObject attackTarget)
	{
		if (debug)
		{
			Debug.Assert(attackTarget.GetComponent<Attackable>());
			Debug.Log("Started attack!");
		}

		target = attackTarget;

		if ((!validTarget()) || !targetIsEnemy())
		{
			if (debug)
				Debug.Log("Invalid Target");
			
			target = null;
			return;
		}

		if(IdUtility.isMoveable(target.tag))
			target.SendMessage("setAttacker", gameObject, SendMessageOptions.DontRequireReceiver);

		startMove(target.transform.position);
		attacking = true;
	}

	/// <summary>
	/// Attacks when time is up.
	/// </summary>
	void attack ()
	{
		timeConsumed += Time.deltaTime;
		if (timeConsumed >= attackSpeed)
		{
			if (debug)
				Debug.Log(gameObject.tag + " launched attack!");

            animator.SetTrigger("Fire");
			boulder = (GameObject)Instantiate(Resources.Load("Prefabs/Projectiles/Boulder"), transform.position, transform.rotation);
			boulder.GetComponent<ProjectileController>().destination = target.transform.position;
			boulder.GetComponent<ProjectileController>().moving = true;
			target.GetComponent<Attackable>().attacked(attackDamage);
			timeConsumed = 0.0f;
			siegeAgent.isStopped = false;
		}
	}

	/// <summary>
	/// Sets the attacker.
	/// </summary>
	/// <param name="enemy">Enemy object.</param>
	public void setAttacker (GameObject enemy)
	{
		target = enemy;
	}

	/// <summary>
	/// Starts siege weapon to enter a tower.
	/// </summary>
	/// <param name="enterTarget">Target object.</param>
	public void startEnter (GameObject enterTarget)
	{
		if (debug)
		{
			Debug.Assert(enterTarget.GetComponent<Enterable>() != null);
			Debug.Log("Started entering");
		}

		target = enterTarget;

		if (target.tag != Tags.Tower || target.GetComponent<Team>().team != team.team)
		{
			if (debug)
				Debug.Log("Cannot Enter");
			
			target = null;
			return;
		}

		startMove(target.transform.position);
		entering = true;
	}

	/// <summary>
	/// Enters the tower.
	/// </summary>
	void enter ()
	{
		target.GetComponent<Enterable>().requestEntry(gameObject);
		if (controller)
			controller.GetComponent<Player>().deselect();
	}

	/// <summary>
	/// Called by enemy to inform object to stop attacking.
	/// </summary>
	public void enemyDied ()
	{
		attacking = false;
		target = null;
	}

	/// <summary>
	/// Is target object valid?
	/// </summary>
	/// <returns><c>true</c>, if target is valid, <c>false</c> otherwise.</returns>
	bool validTarget ()
	{
		return (target.tag == Tags.Crab || target.tag == Tags.Block || target.tag == Tags.Castle || IdUtility.isSiegeWeapon(target.tag));
	}

	/// <summary>
	/// Is target an enemy?
	/// </summary>
	/// <returns><c>true</c>, if target is enemy, <c>false</c> otherwise.</returns>
	bool targetIsEnemy ()
	{
		int tempTeam;

		if (target.tag == Tags.Stone || target.tag == Tags.Wood)
			return true;
		else
			tempTeam = target.GetComponent<Team>().team;

		return (tempTeam != team.team);
	}

	/// <summary>
	/// Is siege weapon close enough to the target?
	/// For entering.
	/// </summary>
	/// <returns><c>true</c>, if siege weapon can interact, <c>false</c> otherwise.</returns>
	bool canInteract ()
	{
		return (getDistance(destination) < attackRange / 2);
	}

	/// <summary>
	/// Is crab close enough to the target?
	/// For attacking.
	/// </summary>
	/// <returns><c>true</c>, if crab can interact, <c>false</c> otherwise.</returns>
	bool canAttack ()
	{
		return (getDistance(destination) < attackRange);
	}

	/// <summary>
	/// Gets the distance from siege weapon.
	/// </summary>
	/// <returns>The distance.</returns>
	/// <param name="pos">Position.</param>
	float getDistance (Vector3 pos)
	{
		return Vector3.Distance(gameObject.transform.position, pos);
	}

	/// <summary>
	/// Sets the controller.
	/// </summary>
	/// <param name="obj">Player script.</param>
	public void setController (Player obj)
	{
		controller = obj;
		selected = true;
	}

	/// <summary>
	/// Is siege weapon busy?
	/// </summary>
	/// <returns><c>true</c>, if siege weapon is busy, <c>false</c> otherwise.</returns>
	public bool isBusy ()
	{
		return attacking;
	}

	/// <summary>
	/// Toggles the selected.
	/// </summary>
	public void toggleSelected ()
	{
		selected = !selected;
	}

	/// <summary>
	/// Updates the UI.
	/// </summary>
	/// <param name="gui">GUI script.</param>
	public void updateUI (GUIController gui)
	{
		GetComponent<Attackable>().setHealth(gui.healthSlider);
	}
}
