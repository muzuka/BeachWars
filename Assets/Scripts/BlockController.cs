using UnityEngine;

/// <summary>
/// Block controller.
/// Handles upgrading blocks and converting to a tower or gate.
/// </summary>
[RequireComponent(typeof(Attackable))]
[RequireComponent(typeof(Team))]
public class BlockController : MonoBehaviour {

	// maximum healths for upgrade levels
	public int normalMaxHealth;
	public int woodMaxHealth;
	public int stoneMaxHealth;

	wallUpgradeType upgradeLevel;		// current upgrade level

	Transform[] walls;	// references to upgrade wall elements

	bool selected;		// is block selected?
	Player controller;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () 
	{
		walls = GetComponentsInChildren<Transform>();
		for(int i = 0; i < walls.Length; i++) 
		{
			if (walls[i].gameObject.tag == Tags.Block_Wall)
				walls[i].gameObject.SetActive(false);
		}

		GetComponent<Attackable>().setHealth(normalMaxHealth);
		upgradeLevel = wallUpgradeType.NORMAL;

		selected = false;
	}

	/// <summary>
	/// Called before destroying block.
	/// </summary>
	public void destroyed () 
	{
		if (selected)
			FindObjectOfType<Player>().deselect();
	}

	/// <summary>
	/// Upgrades block.
	/// Called by crab that is upgrading block.
	/// </summary>
	/// <param name="newLevel">New level.</param>
	public void upgradeFinished (wallUpgradeType newLevel) 
	{
		upgradeLevel = newLevel;
		if (newLevel == wallUpgradeType.WOOD) 
		{
			GetComponent<Attackable>().setHealth(woodMaxHealth);
			for(int i = 0; i < walls.Length; i++) 
			{
				if (walls[i].gameObject.name.EndsWith("Wood"))
					walls[i].gameObject.SetActive(true);
			}
		}
		else if (newLevel == wallUpgradeType.STONE) 
		{
			GetComponent<Attackable>().setHealth(stoneMaxHealth);
			for(int i = 0; i < walls.Length; i++) 
			{
				if (walls[i].gameObject.name.EndsWith("Wood"))
					walls[i].gameObject.SetActive(false);
				if (walls[i].gameObject.name.EndsWith("Stone"))
					walls[i].gameObject.SetActive(true);
			}
		}
	}

	/// <summary>
	/// Starts a rebuild of block.
	/// Block can be rebuilt into a tower or a gate.
	/// </summary>
	/// <param name="type">Tower or gate.</param>
	public void convertTo(string type)
	{
		// Find idle crab
		// Tell crab to build type
		CrabController crab = InfoTool.findIdleCrab(GetComponent<Team>().team);
		crab.startRebuild(gameObject, type);
	}

	/// <summary>
	/// Toggles the selected.
	/// </summary>
	public void toggleSelected () 
	{
		selected = !selected;
	}

	/// <summary>
	/// Sets the controller.
	/// </summary>
	/// <param name="playerObj">Player script.</param>
	public void setController (Player playerObj)
	{
		controller = playerObj;
		selected = true;
	}

	/// <summary>
	/// Placeholder so SendMessage doesn't return null.
	/// </summary>
	public void updateUI () {}
}
