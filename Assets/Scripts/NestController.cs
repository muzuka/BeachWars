using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Nest controller.
/// Handles crab creation.
/// </summary>
[RequireComponent(typeof(Attackable))]
[RequireComponent(typeof(Enterable))]
[RequireComponent(typeof(DebugComponent))]
public class NestController : MonoBehaviour {

	[Tooltip("Time to create crab")]
	public float timeToCreate;
    [Tooltip("Starting health")]
    public int maxHealth;
    [Tooltip("Distance from object to instantiate new objects")]
    public float distanceToInstantiate;

    float timeConsumed;

    Slider progressSlider;

    Player player;
    EnemyKnowledge knowledge;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		timeConsumed = 0.0f;

		GetComponent<Attackable>().setHealth(maxHealth);

        progressSlider = null;

        player = FindObjectOfType<Player>();
        knowledge = FindObjectOfType<EnemyKnowledge>();
	}

    void OnEnable()
    {
        EventManager.StartListening("ObjectEntered", crabEntered);
    }

    void OnDisable()
    {
        EventManager.StopListening("ObjectEntered", crabEntered);
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update ()
	{
        if (FindObjectsOfType<CrabController>().Length <= player.maxUnitCount)
        {
            if (GetComponent<Enterable>().occupied())
            {
                timeConsumed += Time.deltaTime;
                if (GetComponent<Enterable>().currentCanvas != null && progressSlider != null)
                {
                    progressSlider.value = timeConsumed / timeToCreate;
                }
                if (timeConsumed >= timeToCreate)
                {
                    GameObject newCrab;
                    float dist1 = Random.value * distanceToInstantiate * Random.Range(-1, 1);
                    float dist2 = Random.value * distanceToInstantiate * Random.Range(-1, 1);
                    Vector3 pos = gameObject.transform.position;
                    newCrab = (GameObject)Instantiate(Resources.Load("Prefabs/Units/Crab"), new Vector3(pos.x + dist1, pos.y, pos.z + dist2), Quaternion.identity);
                    newCrab.GetComponent<CrabController>().type = GetComponent<Enterable>().occupant.GetComponent<CrabController>().type;
                    newCrab.GetComponent<Team>().team = GetComponent<Team>().team;
                    timeConsumed = 0.0f;
                }
            }
        }
	}

	/// <summary>
	/// Call when nest is destroyed.
	/// </summary>
	public void destroyed ()
	{
		if (GetComponent<DebugComponent>().debug)
			Debug.Log("Nest was destroyed.");
	}

    void crabEntered() 
    {
        if (GetComponent<Enterable>().occupied()) {
            progressSlider = GetComponent<Enterable>().currentCanvas.GetComponentInChildren<Slider>();
        }
    }

	/// <summary>
	/// Placeholder so SendMessage doesn't return null.
	/// </summary>
	public void updateUI () {}
}
