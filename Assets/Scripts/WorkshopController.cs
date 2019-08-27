using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Workshop controller.
/// Handles building siege weapons.
/// </summary>
[RequireComponent(typeof(Attackable))]
[RequireComponent(typeof(DebugComponent))]
public class WorkshopController : MonoBehaviour {

    [Header("Structure Costs:")]
	[Tooltip("Wood cost to build catapult")]
	public int catapultWoodCost;
    [Tooltip("Stone cost to build catapult")]
    public int catapultStoneCost;
	[Tooltip("Wood cost to build ballista")]
	public int ballistaWoodCost;
    [Tooltip("Stone cost to build ballista")]
    public int ballistaStoneCost;

    [Header("Other variables:")]
    [Tooltip("Reference to canvas that contains a slider")]
    public GameObject sliderCanvas;
    [Tooltip("Starting health")]
    public int maxHealth;
    [Tooltip("Time to build a siege engine")]
    public int timeToBuild;
    [Tooltip("Distance from object to instantiate new objects")]
    public float distanceToInstantiate;

    public GameObject currentCanvas { get; set; }

    Slider progressSlider;
    
	float timeConsumed;

    public bool building { get; set; }
	string buildingType;

	CastleController castleMaster;

	bool debug;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		building = false;

		timeConsumed = 0.0f;

		findClosestCastle();

		GetComponent<Attackable>().setHealth(maxHealth);

		debug = GetComponent<DebugComponent>().debug;
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update ()
	{
		if (building)
			build();
	}

	/// <summary>
	/// Called when workshop is destroyed.
	/// </summary>
	public void destroyed ()
	{
		if (debug)
			Debug.Log("Workshop was destroyed.");
	}

	/// <summary>
	/// Starts building a siege weapon.
	/// </summary>
	/// <param name="type">Catapult or ballista.</param>
	public void startBuilding (string type)
	{
		building = true;
		buildingType = type;

        if (buildingType == Tags.Catapult)
        {
            if (castleMaster.getWoodPieces() < catapultWoodCost || castleMaster.getStonePieces() < catapultStoneCost)
            {
                if (debug && castleMaster.getWoodPieces() < catapultWoodCost)
                    Debug.Log("Not enough wood");

                if (debug && castleMaster.getStonePieces() < catapultStoneCost)
                    Debug.Log("Not enough stone");

                MessageEventManager.addNewMessage("Insufficient resources to build a catapult.");

                building = false;
            }
        } else if (buildingType == Tags.Ballista)
        {
            if (castleMaster.getWoodPieces() < ballistaWoodCost || castleMaster.getStonePieces() < ballistaStoneCost)
            {
                if (debug && castleMaster.getWoodPieces() < ballistaWoodCost)
                    Debug.Log("Not enough wood");

                if (debug && castleMaster.getStonePieces() < ballistaStoneCost)
                    Debug.Log("Not enough stone");

                MessageEventManager.addNewMessage("Insufficient resources to build a ballista.");

                building = false;
            }
        }

		if (building)
		{
            currentCanvas = Instantiate(sliderCanvas);
            currentCanvas.transform.position = new Vector3(transform.position.x, currentCanvas.transform.position.y, transform.position.z);
            progressSlider = currentCanvas.GetComponentInChildren<Slider>();
        }
	}

	/// <summary>
	/// Builds siege weapon when time is up.
	/// </summary>
	void build ()
	{
		timeConsumed += Time.deltaTime;
        if (currentCanvas != null && progressSlider != null)
        {
            progressSlider.value = timeConsumed / timeToBuild;
        }
        if (timeConsumed >= timeToBuild)
		{
			if (buildingType == "Catapult")
			{
				for (int i = 0; i < catapultWoodCost; i++)
					castleMaster.take(Tags.Wood);
                for (int i = 0; i < catapultStoneCost; i++)
                    castleMaster.take(Tags.Stone);
			}
			else 
			{
				for (int i = 0; i < ballistaWoodCost; i++)
					castleMaster.take(Tags.Wood);
                for (int i = 0; i < ballistaStoneCost; i++)
                    castleMaster.take(Tags.Stone);
			}
			Instantiate(Resources.Load("Prefabs/Units/" + buildingType), getRandomPos(), Quaternion.identity);
            Destroy(currentCanvas);
			timeConsumed = 0.0f;
			building = false;
		}
	}

	/// <summary>
	/// Gets a random position to place siege weapon.
	/// </summary>
	/// <returns>A random position.</returns>
	Vector3 getRandomPos()
	{
		Vector3 pos = transform.position;
		float dist1 = Random.value * distanceToInstantiate * Random.Range(-1, 1);
		float dist2 = Random.value * distanceToInstantiate * Random.Range(-1, 1);
		return new Vector3(pos.x + dist1, pos.y, pos.z + dist2);
	}

	/// <summary>
	/// Finds the closest castle.
	/// Decides owner of workshop.
	/// Workshop will take resources from this castle.
	/// </summary>
	void findClosestCastle ()
	{
		CastleController[] castleList = FindObjectsOfType<CastleController>();
		CastleController closestCastle = castleList[0];
		float minDist = Vector3.Distance(closestCastle.transform.position, transform.position);

		for(int i = 0; i < castleList.Length; i++)
		{
			float dist = Vector3.Distance(castleList[i].gameObject.transform.position, transform.position);
			if (dist < minDist)
			{
				minDist = dist;
				closestCastle = castleList[i];
			}
		}

		castleMaster = closestCastle;
	}
}
