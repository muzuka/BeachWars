using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Ghost builder.
/// Finds a builder for the ghost.
/// </summary>
[RequireComponent(typeof(Team))]
public class GhostBuilder : MonoBehaviour {

	[Tooltip("Object to instantiate")]
	public GameObject original;
    [Tooltip("Time to wait until checking for an idle crab.")]
    public float timeToWait;

	public int woodRequirement { get; set; }
	public int stoneRequirement { get; set; }

	public bool hasBuilder { get; set; }
	public bool placed { get; set; }

	public int woodAmount { get; set; }
	public int stoneAmount { get; set; }

	bool debug;

	bool waiting;
	float timeConsumed;

	List<CrabController> builders;

	Player player;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		builders = new List<CrabController>();
		player = FindObjectOfType<Player>();
		waiting = false;
		timeConsumed = 0.0f;
		woodAmount = 0;
		stoneAmount = 0;
		placed = false;
		debug = GetComponent<DebugComponent>().debug;

		woodRequirement = BuildingCost.getWoodRequirement(original.name);
		stoneRequirement = BuildingCost.getStoneRequirement(original.name);
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update ()
	{
		if (player.autoBuild)
		{
			if (placed)
			{
				if(debug)
				{
					if(original.name == "Junction")
						Debug.Log("Junction is searching for crabBuilder");
				}

				if (hasBuilder)
				{
					foreach (GameObject crab in player.selectedList)
					{
						CrabController c = crab.GetComponent<CrabController>();
						if (!builders.Contains(c))
						{
							builders.Add(crab.GetComponent<CrabController>());
						}
					}
					if (builders.Count > 0)
					{
						foreach (CrabController crab in builders)
						{
							crab.buildFromGhost(gameObject);
						}
						placed = false;
						hasBuilder = false;
					}
				}
				else
				{
					if(!waiting)
					{
						CrabController crab = InfoTool.findIdleCrab(GetComponent<Team>().team);
						if(crab)
						{
							builders.Add(crab);
							crab.buildFromGhost(gameObject);
							placed = false;
						}
						else
						{
							waiting = true;
							timeConsumed = 0.0f;
						}
					}
					else
					{
						timeConsumed += Time.deltaTime;
						if(timeConsumed >= timeToWait)
						{
							waiting = false;
							timeConsumed = 0.0f;
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Call when ghost is destroyed.
	/// </summary>
	public void destroyed ()
	{
		if(debug)
			Debug.Log(gameObject.name + " has been destroyed.");

        if (builders != null)
        {
            if (builders.Count > 0)
            {
                foreach (CrabController crab in builders)
                {
                    crab.GetComponent<CrabController>().actionStates.clearStates();
                    crab.GetComponent<CrabController>().stopMove();
                }
            }
        }
	}

	/// <summary>
	/// Is there enough resources to build?
	/// </summary>
	/// <returns><c>true</c>, if there are enough resources, <c>false</c> otherwise.</returns>
	public bool canBuild ()
	{
		return (woodAmount >= woodRequirement) && (stoneAmount >= stoneRequirement);
	}

	public bool needsWood ()
	{
		return woodAmount < woodRequirement;
	}

	public bool needsStone ()
	{
		return stoneAmount < stoneRequirement;
	}

	/// <summary>
	/// Build the building.
	/// </summary>
	public void build ()
	{
		Instantiate(original, transform.position, transform.rotation);
		Destroy(gameObject);
	}

	public void updateUI (GUIController gui)
	{
		gui.woodCount.text = "Wood: " + woodAmount + "/" + woodRequirement;
		gui.stoneCount.text = "Stone: " + stoneAmount + "/" + stoneRequirement;
	}
}
