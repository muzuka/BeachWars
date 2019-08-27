using UnityEngine;

public class ProjectileController : MonoBehaviour {

    [Tooltip("Speed of projectile")]
	public float speed;
	public Vector3 destination { get; set; }
	public bool moving { get; set; }

	Quaternion velocity;

	void Start () {
		moving = false;
		destination = new Vector3();
		velocity = new Quaternion();
	}

	void Update () {
		if (moving)
		{
			if (Vector3.Distance(transform.position, destination) < 0.1f)
				Destroy(gameObject);
			else
			{
				// get new velocity
				moveProjectile();
			}
		}
	}

	void moveProjectile ()
	{
		//transform.position += velocity * speed;
	}

}
