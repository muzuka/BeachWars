using UnityEngine;

public class ProjectileController : MonoBehaviour {

    [Tooltip("Speed of projectile")]
	public float Speed;
	public Vector3 Destination { get; set; }
	public bool Moving { get; set; }

	Quaternion _velocity;

	void Start() {
		Moving = false;
		Destination = new Vector3();
		_velocity = new Quaternion();
	}

	void Update() {
		if (Moving)
		{
			if (Vector3.Distance(transform.position, Destination) < 0.1f)
            {
                Destroy(gameObject); 
            }
			else
			{
				// get new velocity
				MoveProjectile();
			}
		}
	}

	void MoveProjectile()
	{
		//transform.position += velocity * speed;
	}

}
