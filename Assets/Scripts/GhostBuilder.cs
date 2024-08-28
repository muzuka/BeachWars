using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Ghost builder.
/// Finds a builder for the ghost.
/// </summary>
public class GhostBuilder : MonoBehaviour {

	[Tooltip("Object to instantiate")]
	public GameObject Original;
    [Tooltip("Time to wait until checking for an idle crab.")]
    public float TimeToWait;

    public int WoodRequirement;
    public int StoneRequirement;

    public bool HasBuilder;
    public bool Placed;

    public int WoodAmount;
    public int StoneAmount;

	bool _debug;

	bool _waiting;
	float _timeConsumed;

	List<CrabController> _builders;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		_builders = new List<CrabController>();
		_waiting = false;
		_timeConsumed = 0.0f;
		WoodAmount = 0;
		StoneAmount = 0;
		Placed = false;
		_debug = GetComponent<DebugComponent>().Debug;

		WoodRequirement = BuildingCost.GetWoodRequirement(Original.name);
		StoneRequirement = BuildingCost.GetStoneRequirement(Original.name);
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update()
	{
		if (Player.Instance.AutoBuild)
		{
			if (Placed)
			{
				if (_debug)
				{
					if (Original.name == "Junction")
						Debug.Log("Junction is searching for crabBuilder");
				}

				if (HasBuilder)
				{
					foreach (GameObject crab in Player.Instance.SelectedList)
					{
						CrabController c = crab.GetComponent<CrabController>();
						if (!_builders.Contains(c))
						{
							_builders.Add(crab.GetComponent<CrabController>());
						}
					}
					if (_builders.Count > 0)
					{
						foreach (CrabController crab in _builders)
						{
							crab.BuildFromGhost(gameObject);
						}
						Placed = false;
						HasBuilder = false;
					}
				}
				else
				{
					if (!_waiting)
					{
						CrabController crab = InfoTool.FindIdleCrab(GetComponent<Team>().team);
						if (crab)
						{
							_builders.Add(crab);
							crab.BuildFromGhost(gameObject);
							Placed = false;
						}
						else
						{
							_waiting = true;
							_timeConsumed = 0.0f;
						}
					}
					else
					{
						_timeConsumed += Time.deltaTime;
						if (_timeConsumed >= TimeToWait)
						{
							_waiting = false;
							_timeConsumed = 0.0f;
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Call when ghost is destroyed.
	/// </summary>
	public void Destroyed()
	{
		if (_debug)
			Debug.Log(gameObject.name + " has been destroyed.");

        if (_builders != null)
        {
            if (_builders.Count > 0)
            {
                foreach (CrabController crab in _builders)
                {
                    crab.ActionStates.ClearStates();
                    crab.StopMove();
                }
            }
        }
	}

	/// <summary>
	/// Is there enough resources to build?
	/// </summary>
	/// <returns><c>true</c>, if there are enough resources, <c>false</c> otherwise.</returns>
	public bool CanBuild()
	{
		return (WoodAmount >= WoodRequirement) && (StoneAmount >= StoneRequirement);
	}

    /// <summary>
    /// Does the building need more wood?
    /// </summary>
    /// <returns>Is the requirement met?</returns>
	public bool NeedsWood()
	{
		return WoodAmount < WoodRequirement;
	}

    /// <summary>
    /// Does the building need more stone?
    /// </summary>
    /// <returns>Is the requirement met?</returns>
	public bool NeedsStone()
	{
		return StoneAmount < StoneRequirement;
	}

	/// <summary>
	/// Instantiates the building.
	/// </summary>
	public void Build()
	{
		Instantiate(Original, transform.position, transform.rotation);
		Destroy(gameObject);
	}

    /// <summary>
    /// Updates the UI. Called by the GUI controller.
    /// </summary>
    /// <param name="gui"></param>
	public void UpdateUI(InfoViewController gui)
	{
		gui.WoodCount.text = "Wood: " + WoodAmount + "/" + WoodRequirement;
		gui.StoneCount.text = "Stone: " + StoneAmount + "/" + StoneRequirement;
	}
}
