using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Workshop controller.
/// Handles building siege weapons.
/// </summary>
public class WorkshopController : MonoBehaviour {

    [Header("Structure Costs:")]
	[Tooltip("Wood cost to build catapult")]
	public int CatapultWoodCost;
    [Tooltip("Stone cost to build catapult")]
    public int CatapultStoneCost;
	[Tooltip("Wood cost to build ballista")]
	public int BallistaWoodCost;
    [Tooltip("Stone cost to build ballista")]
    public int BallistaStoneCost;

    [Header("Other variables:")]
    [Tooltip("Reference to canvas that contains a slider")]
    public GameObject SliderCanvas;
    [Tooltip("Starting health")]
    public int MaxHealth;
    [Tooltip("Time to build a siege engine")]
    public int TimeToBuild;
    [Tooltip("Distance from object to instantiate new objects")]
    public float DistanceToInstantiate;

    public GameObject CurrentCanvas { get; set; }

    Slider _progressSlider;
    
	float _timeConsumed;

    public bool Building { get; set; }
	string _buildingType;

	CastleController _castleMaster;

	bool _debug;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		Building = false;

		_timeConsumed = 0.0f;

		FindClosestCastle();

		GetComponent<Attackable>().SetHealth(MaxHealth);

		_debug = GetComponent<DebugComponent>().Debug;
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update()
	{
		if (Building)
			Build();
	}

	/// <summary>
	/// Called when workshop is destroyed.
	/// </summary>
	public void Destroyed()
	{
		if (_debug)
			Debug.Log("Workshop was destroyed.");
	}

	/// <summary>
	/// Starts building a siege weapon.
	/// </summary>
	/// <param name="type">Catapult or ballista.</param>
	public void StartBuilding(string type)
	{
		Building = true;
		_buildingType = type;

        if (_buildingType == Tags.Catapult)
        {
            if (_castleMaster.GetWoodPieces() < CatapultWoodCost || _castleMaster.GetStonePieces() < CatapultStoneCost)
            {
                if (_debug && _castleMaster.GetWoodPieces() < CatapultWoodCost)
                    Debug.Log("Not enough wood");

                if (_debug && _castleMaster.GetStonePieces() < CatapultStoneCost)
                    Debug.Log("Not enough stone");

                MessageEventManager.AddNewMessage("Insufficient resources to build a catapult.");

                Building = false;
            }
        } else if (_buildingType == Tags.Ballista)
        {
            if (_castleMaster.GetWoodPieces() < BallistaWoodCost || _castleMaster.GetStonePieces() < BallistaStoneCost)
            {
                if (_debug && _castleMaster.GetWoodPieces() < BallistaWoodCost)
                    Debug.Log("Not enough wood");

                if (_debug && _castleMaster.GetStonePieces() < BallistaStoneCost)
                    Debug.Log("Not enough stone");

                MessageEventManager.AddNewMessage("Insufficient resources to build a ballista.");

                Building = false;
            }
        }

		if (Building)
		{
            CurrentCanvas = Instantiate(SliderCanvas);
            CurrentCanvas.transform.position = new Vector3(transform.position.x, CurrentCanvas.transform.position.y, transform.position.z);
            _progressSlider = CurrentCanvas.GetComponentInChildren<Slider>();
        }
	}

	/// <summary>
	/// Builds siege weapon when time is up.
	/// </summary>
	void Build()
	{
		_timeConsumed += Time.deltaTime;
        if (CurrentCanvas != null && _progressSlider != null)
        {
            _progressSlider.value = _timeConsumed / TimeToBuild;
        }
        if (_timeConsumed >= TimeToBuild)
		{
			if (_buildingType == "Catapult")
			{
				for (int i = 0; i < CatapultWoodCost; i++)
                {
                    _castleMaster.Take(Tags.Wood); 
                }
                for (int i = 0; i < CatapultStoneCost; i++)
                {
                    _castleMaster.Take(Tags.Stone); 
                }
			}
			else 
			{
				for (int i = 0; i < BallistaWoodCost; i++)
                {
                    _castleMaster.Take(Tags.Wood); 
                }
                for (int i = 0; i < BallistaStoneCost; i++)
                {
                    _castleMaster.Take(Tags.Stone); 
                }
			}
			Instantiate(Resources.Load("Prefabs/Units/" + _buildingType), GetRandomPos(), Quaternion.identity);
            Destroy(CurrentCanvas);
			_timeConsumed = 0.0f;
			Building = false;
		}
	}

	/// <summary>
	/// Gets a random position to place siege weapon.
	/// </summary>
	/// <returns>A random position.</returns>
	Vector3 GetRandomPos()
	{
		Vector3 pos = transform.position;
		float dist1 = Random.value * DistanceToInstantiate * Random.Range(-1, 1);
		float dist2 = Random.value * DistanceToInstantiate * Random.Range(-1, 1);
		return new Vector3(pos.x + dist1, pos.y, pos.z + dist2);
	}

	/// <summary>
	/// Finds the closest castle.
	/// Decides owner of workshop.
	/// Workshop will take resources from this castle.
	/// </summary>
	void FindClosestCastle()
	{
		CastleController[] castleList = FindObjectsOfType<CastleController>();
		CastleController closestCastle = castleList[0];
		float minDist = Vector3.Distance(closestCastle.transform.position, transform.position);

		for (int i = 0; i < castleList.Length; i++)
		{
			float dist = Vector3.Distance(castleList[i].gameObject.transform.position, transform.position);
			if (dist < minDist)
			{
				minDist = dist;
				closestCastle = castleList[i];
			}
		}

		_castleMaster = closestCastle;
	}
}
