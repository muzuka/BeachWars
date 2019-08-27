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
	public float time;

    public int startingWood;
    public int startingStone;

    public Mesh level2Mesh;

	// max health stuff
	const int level1Health = 1000;
	const int level2Health = 2000;
	const int level3Health = 3000;

	List<GameObject> targets;			// objects currently attacking or attempting to capture castle
	GameObject interactor;				// object trying to unload resources or enter castle
	Player controller;					// player object
	bool selected;						// is castle selected?

	const int resistance = 1;			// resistance level
	int finalResist;                    // final resistance level after calculations

    // wood piece amount
    int woodPieces;

    // stone piece amount
    int stonePieces;

	bool debug;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		GetComponent<Attackable>().setHealth(level1Health);

		woodPieces = startingWood;
		stonePieces = startingStone;
		selected = false;
		interactor = null;
		targets = new List<GameObject>();

		debug = GetComponent<DebugComponent>().debug;
	}

	/// <summary>
	/// Call before destroying castle.
	/// </summary>
	public void destroyed ()
	{
		if (targets.Count > 0) 
		{
			for(int i = 0; i < targets.Count; i++)
				targets[i].SendMessage("enemyDied", SendMessageOptions.DontRequireReceiver);
		}

		if (controller != null)
			controller.GetComponent<Player>().deselect();
	}

	/// <summary>
	/// Called by Upgradable script
	/// Increases upgrade level.
	/// </summary>
	/// <param name="level">New level.</param>
	public void upgradeFinished (int level) 
	{
		woodPieces -= GetComponent<CastleUpgrade>().upgradeCost;
		stonePieces -= GetComponent<CastleUpgrade>().upgradeCost;

        GetComponent<MeshFilter>().mesh = level2Mesh;
        transform.Rotate(new Vector3(90, 0, 0));
        transform.localScale = new Vector3(4f, 4.5f, -4f);

		if (level == 2)
			GetComponent<Attackable>().maxHealth = level2Health;
		else if (level == 3)
			GetComponent<Attackable>().maxHealth = level3Health;
	}

	/// <summary>
	/// Gets the resistance time.
	/// Returns the time to capture given level and occupation.
	/// </summary>
	/// <returns>The resistance time.</returns>
	public float getResistanceTime () 
	{
		finalResist = resistance + GetComponent<CastleUpgrade>().getLevel();
		if (GetComponent<Enterable>().occupied())
			finalResist = finalResist + 2;
		return finalResist * time;
	}

	/// <summary>
	/// Take one wood or stone piece.
	/// </summary>
	/// <param name="tag">Wood or stone.</param>
	public void take (string tag) 
	{
		if (tag == Tags.Wood) 
		{
			if (woodPieces > 0)
				woodPieces--;
		}

		if (tag == Tags.Stone) 
		{
			if (stonePieces > 0)
				stonePieces--;
		}
	}

	/// <summary>
	/// Receive one wood or stone piece.
	/// </summary>
	/// <param name="tag">Wood or stone.</param>
	public void give (string tag) 
	{
		if (tag == Tags.Wood)
			woodPieces++;
		
		if (tag == Tags.Stone)
			stonePieces++;
	}
		
	/// <summary>
	/// Are there enough resources to take?
	/// </summary>
	/// <returns><c>true</c>, if enough, <c>false</c> otherwise.</returns>
	/// <param name="tag">Wood or stone.</param>
	public bool canTake (string tag) 
	{
		if (tag == Tags.Wood)
			return woodPieces > 0;
		
		if (tag == Tags.Stone)
			return stonePieces > 0;

		return false;
	}

	/// <summary>
	/// Saves reference to current attacker.
	/// </summary>
	/// <param name="crab">Crab object.</param>
	public void setAttacker (GameObject crab) 
	{
		if (!targets.Contains(crab))
			targets.Add(crab);
	}

	/// <summary>
	/// Sets the controller.
	/// </summary>
	/// <param name="player">Player script.</param>
	public void setController (Player player) 
	{
		controller = player;
		selected = true;
	}

	/// <summary>
	/// Toggles the selected.
	/// </summary>
	public void toggleSelected () 
	{
		selected = !selected;
	}

	/// <summary>
	/// Prints the inventory.
	/// </summary>
	public void printInventory () 
	{
		if (debug)
			Debug.Log("Wood = " + woodPieces + " Stone = " + stonePieces);
	}

	/// <summary>
	/// Updates the UI.
	/// </summary>
	/// <param name="gui">GUI script.</param>
	public void updateUI (GUIController gui)
	{
		GetComponent<Attackable>().setHealth(gui.healthSlider);
		gui.woodCount.text = "Wood: " + woodPieces;
		gui.stoneCount.text = "Stone: " + stonePieces;
		gui.levelText.text = "Level: " + GetComponent<CastleUpgrade>().getLevel();
	}

    public int getWoodPieces() {
        return woodPieces;
    }

    public int getStonePieces() {
        return stonePieces;
    }
}
