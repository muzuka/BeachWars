using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Castle controller.
/// Handles resources and upgrading.
/// </summary>
[RequireComponent(typeof(Attackable))]
[RequireComponent(typeof(DebugComponent))]
[RequireComponent(typeof(WallUpgrade))]
public class CastleController : MonoBehaviour {

	// public variables
	[Tooltip("Time to conquer")]
	public float Time;

    public int StartingWood;
    public int StartingStone;

    public Mesh Level2Mesh;

	// max health stuff
	const int _level1Health = 1000;
	const int _level2Health = 2000;
	const int _level3Health = 3000;

	List<GameObject> _targets;			// objects currently attacking or attempting to capture castle
	GameObject _interactor;				// object trying to unload resources or enter castle
	Player _controller;					// player object
	bool _selected;						// is castle selected?

	const int _resistance = 1;			// resistance level
	int _finalResist;                    // final resistance level after calculations

    // wood piece amount
    int _woodPieces;

    // stone piece amount
    int _stonePieces;

	bool _debug;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		GetComponent<Attackable>().SetHealth(_level1Health);

		_woodPieces = StartingWood;
		_stonePieces = StartingStone;
		_selected = false;
		_interactor = null;
		_targets = new List<GameObject>();

		_debug = GetComponent<DebugComponent>().Debug;
	}

	/// <summary>
	/// Call before destroying castle.
	/// </summary>
	public void Destroyed()
	{
		if (_targets.Count > 0) 
		{
			for (int i = 0; i < _targets.Count; i++)
            {
                _targets[i].SendMessage("enemyDied", SendMessageOptions.DontRequireReceiver); 
            }
		}

		if (_controller != null)
        {
            _controller.GetComponent<Player>().Deselect(); 
        }
	}

	/// <summary>
	/// Called by Upgradable script
	/// Increases upgrade level.
	/// </summary>
	/// <param name="level">New level.</param>
	public void UgradeFinished(int level) 
	{
		_woodPieces -= GetComponent<CastleUpgrade>().UpgradeCost;
		_stonePieces -= GetComponent<CastleUpgrade>().UpgradeCost;

        GetComponent<MeshFilter>().mesh = Level2Mesh;
        transform.Rotate(new Vector3(90, 0, 0));
        transform.localScale = new Vector3(4f, 4.5f, -4f);

		if (level == 2)
        {
            GetComponent<Attackable>().MaxHealth = _level2Health; 
        }
		else if (level == 3)
        {
            GetComponent<Attackable>().MaxHealth = _level3Health; 
        }
	}

	/// <summary>
	/// Gets the resistance time.
	/// Returns the time to capture given level and occupation.
	/// </summary>
	/// <returns>The resistance time.</returns>
	public float GetResistanceTime() 
	{
		_finalResist = _resistance + GetComponent<CastleUpgrade>().GetLevel();
		if (GetComponent<Enterable>().Occupied())
        {
            _finalResist = _finalResist + 2; 
        }
		return _finalResist * Time;
	}

	/// <summary>
	/// Take one wood or stone piece.
	/// </summary>
	/// <param name="tag">Wood or stone.</param>
	public void Take(string tag) 
	{
		if (tag == Tags.Wood) 
		{
			if (_woodPieces > 0)
            {
                _woodPieces--; 
            }
		}

		if (tag == Tags.Stone) 
		{
			if (_stonePieces > 0)
            {
                _stonePieces--; 
            }
		}
	}

	/// <summary>
	/// Receive one wood or stone piece.
	/// </summary>
	/// <param name="tag">Wood or stone.</param>
	public void Give(string tag) 
	{
		if (tag == Tags.Wood)
			_woodPieces++;
		
		if (tag == Tags.Stone)
			_stonePieces++;
	}
		
	/// <summary>
	/// Are there enough resources to take?
	/// </summary>
	/// <returns><c>true</c>, if enough, <c>false</c> otherwise.</returns>
	/// <param name="tag">Wood or stone.</param>
	public bool CanTake(string tag) 
	{
		if (tag == Tags.Wood)
			return _woodPieces > 0;
		
		if (tag == Tags.Stone)
			return _stonePieces > 0;

		return false;
	}

	/// <summary>
	/// Saves reference to current attacker.
	/// </summary>
	/// <param name="crab">Crab object.</param>
	public void SetAttacker(GameObject crab) 
	{
		if (!_targets.Contains(crab))
			_targets.Add(crab);
	}

	/// <summary>
	/// Sets the controller.
	/// </summary>
	/// <param name="player">Player script.</param>
	public void SetController(Player player) 
	{
		_controller = player;
		_selected = true;
	}

	/// <summary>
	/// Toggles the selected.
	/// </summary>
	public void ToggleSelected() 
	{
		_selected = !_selected;
	}

	/// <summary>
	/// Prints the inventory.
	/// </summary>
	public void PrintInventory() 
	{
		if (_debug)
			Debug.Log("Wood = " + _woodPieces + " Stone = " + _stonePieces);
	}

	/// <summary>
	/// Updates the UI.
	/// </summary>
	/// <param name="gui">GUI script.</param>
	public void UpdateUI(InfoViewController gui)
	{
		GetComponent<Attackable>().SetHealth(gui.HealthSlider);
		gui.WoodCount.text = "Wood: " + _woodPieces;
		gui.StoneCount.text = "Stone: " + _stonePieces;
		gui.LevelText.text = "Level: " + GetComponent<CastleUpgrade>().GetLevel();
	}

    public int GetWoodPieces() {
        return _woodPieces;
    }

    public int GetStonePieces() {
        return _stonePieces;
    }
}
