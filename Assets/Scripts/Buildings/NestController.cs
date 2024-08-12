using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Nest controller.
/// Handles crab creation.
/// </summary>
public class NestController : MonoBehaviour {

    public GameObject Crab;

	[Tooltip("Time to create crab")]
	public float TimeToCreate;
    [Tooltip("Starting health")]
    public int MaxHealth;
    [Tooltip("Distance from object to instantiate new objects")]
    public float DistanceToInstantiate;

    float _timeConsumed;

    Slider _progressSlider;

    Player _player;
    EnemyKnowledge _knowledge;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		_timeConsumed = 0.0f;

		GetComponent<Attackable>().SetHealth(MaxHealth);

        _progressSlider = null;

        _player = FindObjectOfType<Player>();
        _knowledge = FindObjectOfType<EnemyKnowledge>();
	}

    void OnEnable()
    {
        EventManager.StartListening("ObjectEntered", CrabEntered);
    }

    void OnDisable()
    {
        EventManager.StopListening("ObjectEntered", CrabEntered);
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
	{
        if (FindObjectsOfType<CrabController>().Length <= _player.MaxUnitCount)
        {
            if (GetComponent<Enterable>().Occupied())
            {
                _timeConsumed += Time.deltaTime;
                if (GetComponent<Enterable>()._currentCanvas != null && _progressSlider != null)
                {
                    _progressSlider.value = _timeConsumed / TimeToCreate;
                }
                if (_timeConsumed >= TimeToCreate)
                {
                    CreateCrab();
                }
            }
        }
	}

	/// <summary>
	/// Call when nest is destroyed.
	/// </summary>
	public void Destroyed()
	{
		if (GetComponent<DebugComponent>().Debug)
			Debug.Log("Nest was destroyed.");
	}

    /// <summary>
    /// Spawns a crab in a random position
    /// </summary>
    void CreateCrab()
    {
        GameObject newCrab;
        float dist1 = Random.value * DistanceToInstantiate * Random.Range(-1, 1);
        float dist2 = Random.value * DistanceToInstantiate * Random.Range(-1, 1);
        Vector3 pos = gameObject.transform.position;
        newCrab = Instantiate(Crab, new Vector3(pos.x + dist1, pos.y, pos.z + dist2), Quaternion.identity);
        newCrab.GetComponent<CrabController>().Type = GetComponent<Enterable>()._occupant.GetComponent<CrabController>().Type;
        newCrab.GetComponent<Team>().team = GetComponent<Team>().team;
        _timeConsumed = 0.0f;
    }

    void CrabEntered() 
    {
        if (GetComponent<Enterable>().Occupied()) {
            _progressSlider = GetComponent<Enterable>()._currentCanvas.GetComponentInChildren<Slider>();
        }
    }

	/// <summary>
	/// Placeholder so SendMessage doesn't return null.
	/// </summary>
	public void UpdateUI() {}
}
