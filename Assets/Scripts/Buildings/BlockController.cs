using UnityEngine;

/// <summary>
/// Block controller.
/// Handles upgrading blocks and converting to a tower or gate.
/// </summary>
public class BlockController : MonoBehaviour {

	// maximum healths for upgrade levels
	public int NormalMaxHealth;
	public int WoodMaxHealth;
	public int StoneMaxHealth;

	wallUpgradeType _upgradeLevel;		// current upgrade level

	Transform[] _walls;	// references to upgrade wall elements

	bool _selected;		// is block selected?
	Player _controller;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start() 
	{
		_walls = GetComponentsInChildren<Transform>();
		for (int i = 0; i < _walls.Length; i++) 
		{
			if (_walls[i].gameObject.CompareTag(Tags.Block_Wall))
				_walls[i].gameObject.SetActive(false);
		}

		GetComponent<Attackable>().SetHealth(NormalMaxHealth);
		_upgradeLevel = wallUpgradeType.NORMAL;

		_selected = false;
	}

	/// <summary>
	/// Called before destroying block.
	/// </summary>
	public void Destroyed() 
	{
		if (_selected)
			FindObjectOfType<Player>().Deselect();
	}

	/// <summary>
	/// Upgrades block.
	/// Called by crab that is upgrading block.
	/// </summary>
	/// <param name="newLevel">New level.</param>
	public void UpgradeFinished(wallUpgradeType newLevel) 
	{
		_upgradeLevel = newLevel;
		if (newLevel == wallUpgradeType.WOOD) 
		{
			GetComponent<Attackable>().SetHealth(WoodMaxHealth);
			for (int i = 0; i < _walls.Length; i++) 
			{
				if (_walls[i].gameObject.name.EndsWith("Wood"))
					_walls[i].gameObject.SetActive(true);
			}
		}
		else if (newLevel == wallUpgradeType.STONE) 
		{
			GetComponent<Attackable>().SetHealth(StoneMaxHealth);
			for (int i = 0; i < _walls.Length; i++) 
			{
				if (_walls[i].gameObject.name.EndsWith("Wood"))
					_walls[i].gameObject.SetActive(false);
				if (_walls[i].gameObject.name.EndsWith("Stone"))
					_walls[i].gameObject.SetActive(true);
			}
		}
	}

	/// <summary>
	/// Starts a rebuild of block.
	/// Block can be rebuilt into a tower or a gate.
	/// </summary>
	/// <param name="type">Tower or gate.</param>
	public void ConvertTo(string type)
	{
		// Find idle crab
		// Tell crab to build type
		CrabController crab = InfoTool.FindIdleCrab(GetComponent<Team>().team);
		crab.StartRebuild(gameObject, type);
	}

	/// <summary>
	/// Toggles the selected.
	/// </summary>
	public void ToggleSelected() 
	{
		_selected = !_selected;
	}

	/// <summary>
	/// Sets the controller.
	/// </summary>
	/// <param name="playerObj">Player script.</param>
	public void SetController(Player playerObj)
	{
		_controller = playerObj;
		_selected = true;
	}

	/// <summary>
	/// Placeholder so SendMessage doesn't return null.
	/// </summary>
	public void UpdateUI() {}
}
