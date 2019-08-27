using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

/// <summary>
/// Main player controller.
/// Handles selected units and delegating actions.
/// </summary>
[RequireComponent(typeof(InputController))]
[RequireComponent(typeof(GUIController))]
[RequireComponent(typeof(Team))]
[RequireComponent(typeof(DebugComponent))]
public class Player : MonoBehaviour
{
    [Tooltip("Is lose condition active?")]
	public bool loseCondition;
    [Tooltip("Is win condition active?")]
	public bool winCondition;

    [Tooltip("Maximum units allowed to exist.")]
    public int maxUnitCount;

    [Tooltip("The min distance between junctions before intermediate blocks are placed")]
    public float junctionDistanceLimit;
    [Tooltip("The size of a block (informs distance between each block")]
    public float blockSize;

	Team team;

	// reference to gui
	public GUIController gui { get; set; }

	// reference to input
	public InputController input { get; set; }

	// The currently selected object or the first element of selectedList.
	public GameObject selected { get; set; }

	// The list of selected objects
	public List<GameObject> selectedList { get; set; }

 	// is something selected?
	public bool hasSelected { get; set; }

	// is more than one thing selected?
	public bool multiSelect { get; set; }

 	// can I command the selected object?
	public bool canCommand { get; set; }

	// The selected objects team
	public Team selectedTeam { get; set; }

	// The list of haloes
	public List<GameObject> haloList { get; set; }

	// action states
	// Assuming crab is selected
 	// set of states to affect behaviour
	public StateController states { get; set; }
	string[] stateList = {"Building", "Attacking", "Entering", "Capturing", "Recruiting", "Upgrading", "Repairing"};

	// Designates type selected for building
	public string buildingType { get; set; }

	public GhostBuildingManager ghostManager { get; set; }

	public bool paused { get; set; }

	bool debug;

	public bool autoBuild { get; set; }

	Canvas buildingCanvas;
	Vector3 cameraPos;
	float cameraXDelta;
	float cameraZDelta;

	public GameObject targetedArmoury { get; set; }

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		input = GetComponent<InputController>();
		gui = GetComponent<GUIController>();

		team = GetComponent<Team>();
		states = new StateController(stateList);
		selectedList = new List<GameObject>();
		haloList = new List<GameObject>();

		ghostManager = new GhostBuildingManager();
        ghostManager.junctionDistLimit = junctionDistanceLimit;
        ghostManager.blockSize = blockSize;

		debug = GetComponent<DebugComponent>().debug;
		ghostManager.debug = debug;
		states.debug = debug;

		paused = false;

		autoBuild = true;

		buildingCanvas = null;

		targetedArmoury = null;

		gui.startUI();

		if (debug)
			Debug.Log("Finished starting.");
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update ()
	{
		if (!selected)
			deselect();

		if(loseCondition)
			checkLoseCondition();
		if(winCondition)
			checkWinCondition();

		if (buildingCanvas)
		{
			cameraXDelta = GetComponentInParent<Transform>().position.x - cameraPos.x;
			cameraZDelta = GetComponentInParent<Transform>().position.z - cameraPos.z;

			Vector2 tempPos = getPanel().anchoredPosition;
			tempPos.x -= cameraXDelta;
			tempPos.y -= cameraZDelta;
			getPanel().anchoredPosition = tempPos;
		}

		input.getKeyboardInput(this);

		input.processMouseClick(this, gui);

		if (selectedList != null)
			multiSelect = (selectedList.Count > 1);

		gui.updateUI(this);
		if (hasSelected)
			updateHalos();

		ghostManager.updateGhosts();

		cameraPos = GetComponentInParent<Transform>().position;
	}

	/// <summary>
	/// Updates the halos' positions.
	/// </summary>
	void updateHalos ()
	{
		if (IdUtility.isMoveable(selected.tag))
		{
			for (int i = 0; i < haloList.Count; i++)
			{
				haloList[i].SetActive(true);
				haloList[i].transform.GetChild(0).position = selectedList[i].transform.position;

                if (selectedList.Count > 1)
                {
                    if (selectedList[i].tag == Tags.Crab)
                    {
                        if (selectedList[i].GetComponent<CrabController>().actionStates.getState("Attacking"))
                        {
                            haloList[i].GetComponentInChildren<Image>().color = Color.red;
                        }
                        else if (selectedList[i].GetComponent<CrabController>().actionStates.getState("Building"))
                        {
                            haloList[i].GetComponentInChildren<Image>().color = Color.blue;
                        }
                        else if (selectedList[i].GetComponent<CrabController>().actionStates.getState("Collecting"))
                        {
                            haloList[i].GetComponentInChildren<Image>().color = Color.green;
                        }
                    }
                    else if (IdUtility.isSiegeWeapon(selectedList[i].tag))
                    {
                        if (selectedList[i].GetComponent<SiegeController>().isBusy())
                        {
                            haloList[i].GetComponentInChildren<Image>().color = Color.red;
                        }
                    }
                }
                else
                {
                    if (selected.tag == Tags.Crab)
                    {
                        if (selected.GetComponent<CrabController>().actionStates.getState("Attacking"))
                        {
                            haloList[0].GetComponentInChildren<Image>().color = Color.red;
                        }
                        else if (selected.GetComponent<CrabController>().actionStates.getState("Building"))
                        {
                            haloList[0].GetComponentInChildren<Image>().color = Color.blue;
                        }
                        else if (selected.GetComponent<CrabController>().actionStates.getState("Collecting"))
                        {
                            haloList[0].GetComponentInChildren<Image>().color = Color.green;
                        }
                    }
                    else if (IdUtility.isSiegeWeapon(selected.tag))
                    {
                        if (selected.GetComponent<SiegeController>().isBusy())
                        {
                            haloList[0].GetComponentInChildren<Image>().color = Color.red;
                        }
                    }
                }
			}
		}
	}

	/// <summary>
	/// Gets the main panel.
	/// </summary>
	/// <returns>The panel transform.</returns>
	RectTransform getPanel ()
	{
		if (debug)
			Debug.Assert(buildingCanvas);

		RectTransform[] transforms = buildingCanvas.GetComponentsInChildren<RectTransform>();

		for (int i = 0; i < transforms.Length; i++)
		{
			if (transforms[i].gameObject.name == "MainPanel")
				return transforms[i];
		}
		return null;
	}

    public void generalBuildButtonPressed (string buildingType)
    {
        setBuildingType(buildingType);
        setPlayerState("Building");
        createGhostBuilding();
    }

	/// <summary>
	/// Select the specified obj.
	/// Sets GUI info.
	/// </summary>
	/// <param name="obj">Object.</param>
	public void select (GameObject obj) 
	{
		selected = obj;
		selectedTeam = obj.GetComponent<Team>();
		canCommand = (team.team == selectedTeam.team);
		hasSelected = true;

		selected.SendMessage("setController", this, SendMessageOptions.DontRequireReceiver);

		// set gui
		gui.setLabel(obj);
		gui.setActiveGUIComponents(obj.tag);
		gui.getActionViewController().setButtons(this);
		gui.selectedImage.texture = Resources.Load<Texture>("Textures/" + obj.tag);
	}

	/// <summary>
	/// Selects all objects in list.
	/// Sets GUI info.
	/// </summary>
	public void selectAll () 
	{
		selected = selectedList[0];
		hasSelected = true;
		selectedTeam = selectedList[0].GetComponent<Team>();

		for (int i = 0; i < selectedList.Count; i++)
			selectedList[i].SendMessage("setController", this, SendMessageOptions.DontRequireReceiver);

		canCommand = (team.team == selectedTeam.team);

		gui.setActiveGUIComponents("multi");
		gui.getActionViewController().setButtons(this);
		gui.setMultiUI();
	}

	/// <summary>
	/// Deselects the currently selected object(s).
	/// </summary>
	public void deselect ()
	{
		if (hasSelected)
		{
			for (int i = 0; i < selectedList.Count; i++)
			{
				if (selectedList[i])
					selectedList[i].SendMessage("toggleSelected", SendMessageOptions.DontRequireReceiver);
			}

			if (buildingCanvas)
				Destroy(buildingCanvas);

			if (targetedArmoury != null)
			{
				Canvas[] canvases = FindObjectsOfType<Canvas>();
				for (int i = 0; i < canvases.Length; i++)
				{
					if (canvases[i].gameObject.name == "WeaponBuildingCanvas")
					{
						Destroy(canvases[i].gameObject);
						break;
					}
				}

				targetedArmoury = null;
			}

			states.clearStates();
			selectedList.Clear();
			canCommand = false;
			hasSelected = false;
			selected = null;
			selectedTeam = null;

			clearIndicators();

			gui.deselect();
		}
	}

	/// <summary>
	/// Deselect a specified obj.
	/// Searches for it in selected list.
	/// </summary>
	/// <param name="obj">Object.</param>
	public void deselect (GameObject obj)
	{
		if (!selectedList.Contains(obj))
			return;
			
		if (!multiSelect)
			deselect();
		else
		{
			Predicate<GameObject> unitFinder = g => g == obj;
			int pos = selectedList.FindIndex(unitFinder);

			if (pos == 0)
				selected = selectedList[1];

			selectedList.RemoveAt(pos);
			Destroy(haloList[pos]);
			haloList.RemoveAt(pos);
		}
	}

	/// <summary>
	/// Destroys the indicators.
	/// </summary>
	public void clearIndicators ()
	{
		for (int i = 0; i < haloList.Count; i++)
			Destroy(haloList[i]);
		haloList.Clear();
	}

	/// <summary>
	/// Checks the win condition.
	/// You win when all castles are under your control.
	/// </summary>
	void checkWinCondition ()
	{
		bool foundCastle = false;
		CastleController[] list = FindObjectsOfType<CastleController>();
		for (int i = 0; i < list.Length; i++)
			foundCastle |= list[i].GetComponent<Team>().team != team.team;
		
		if (!foundCastle)
		{
			Debug.Log("You win!");
			gui.winMenu.SetActive(true);
		}
	}

	/// <summary>
	/// Checks the lose condition.
	/// You lose when all castles aren't in your control.
	/// </summary>
	void checkLoseCondition ()
	{
		bool foundCastle = false;
		CastleController[] list = FindObjectsOfType<CastleController>();

		for (int i = 0; i < list.Length; i++)
			foundCastle |= list[i].gameObject.GetComponent<Team>().team == team.team;

		if (!foundCastle)
		{
				Debug.Log("You lose!");
				gui.winMenu.SetActive(true);
				gui.winMenu.GetComponentInChildren<Text>().text = "You Lost!";
		}
	}

    public void pressedGeneralBuildButton (string type)
    {
        setBuildingType(type);
        setPlayerState(stateList[0]);
        createGhostBuilding();
    }

	/// <summary>
	/// Sets the type of the building to build.
	/// </summary>
	/// <param name="type">Building tag.</param>
	public void setBuildingType (string type)
	{
		if (IdUtility.isBuilding(type) || type == Tags.Block || type == Tags.Gate || type == Tags.Junction)
			buildingType = type;
		else
			buildingType = "none";

		ghostManager.buildingType = buildingType;
	}

	/// <summary>
	/// Tells ghostManager to create a ghost building.
	/// </summary>
	public void createGhostBuilding() 
	{
		if (debug)
			Debug.Log("Building a " + buildingType);

		ghostManager.createGhostBuilding();
	}

	/// <summary>
	/// Tells crab to take a weapon.
	/// </summary>
	/// <param name="weapon">Hammer, Spear, Bow, or Shield.</param>
	public void takeWeapon (string weapon)
	{
		if (selected.tag == Tags.Crab && canCommand)
			selected.GetComponent<CrabController>().startTakeWeapon(weapon);
	}

	/// <summary>
	/// Sets a player state.
	/// </summary>
	/// <param name="state">State name.</param>
	public void setPlayerState (string state)
	{
		states.setState(state, true);
	}

	/// <summary>
	/// Sets the images for each inventory slot.
	/// </summary>
	/// <param name="inv">Inventory array.</param>
	public void getInventory (string[] inv)
	{
		gui.getInvSlot(1).texture = getTexture(inv[0]);
		gui.getInvSlot(2).texture = getTexture(inv[1]);
		gui.getInvSlot(3).texture = getTexture(inv[2]);
	}

	/// <summary>
	/// Gets the texture given the name.
	/// </summary>
	/// <returns>The texture.</returns>
	/// <param name="textureName">texture name.</param>
	Texture getTexture (string textureName)
	{
		Texture newTexture = Resources.Load<Texture>("Textures/Empty");
		if (textureName == null)
			return newTexture;

		newTexture = Resources.Load<Texture>("Textures/" + textureName);
		if (!newTexture)
			newTexture = Resources.Load<Texture>("Textures/Empty");

		return newTexture;
	}

	/// <summary>
	/// Tells crab to craft item.
	/// </summary>
	public void craft ()
	{
		if (canCommand)
		{
			selected.GetComponent<CrabController>().craft(selected.GetComponent<CrabController>().getCraftableType());
			getInventory(selected.GetComponent<CrabController>().getInventory());
		}
	}

	/// <summary>
	/// Crafts the bow.
	/// called by separate button
	/// separate from craft because of two crafting types at once.
	/// </summary>
	public void craftBow ()
	{
		if (canCommand)
		{
			selected.GetComponent<CrabController>().craft(Tags.Bow);
			getInventory(selected.GetComponent<CrabController>().getInventory());
		}
	}

    /// <summary>
    /// Finds a crab to dismantle the selected building
    /// Called by separate button
    /// Assumes a building is selected that has an available destroy button.
    /// </summary>
    public void startDismantle ()
    {
        CrabController crab = InfoTool.findIdleCrab(team.team);

        if (crab != null)
        {
            crab.startDismantling(selected);
        }
        else
        {
            if (debug)
                Debug.Log("No available crab.");
        }
    }

	/// <summary>
	/// Moves multiple crabs at once.
	/// </summary>
	/// <param name="dest">Destination.</param>
	public void moveMultiple (Vector3 dest)
	{
		for (int i = 1; i < selectedList.Count; i++)
		{
			if (selectedList[i].tag == Tags.Crab)
                selectedList[i].SendMessage("startMove", dest);
		}
	}

	/// <summary>
	/// Gets status of multiple selected objects.
	/// Are the selected objects a mix of crabs and siege weapons?
	/// </summary>
	/// <returns>Crab for all crabs, Siege for all siege weapons, or Mixed for mixed.</returns>
	public string getMultiSelectStatus ()
	{
		string type = "None";
		for (int i = 0; i < selectedList.Count; i++)
		{
			if (selectedList[i].tag == Tags.Crab)
			{
				if (type == "None")
					type = "Crab";
				else if (type == "Siege")
					type = "Mixed";
			}
			if (IdUtility.isSiegeWeapon(selectedList[i].tag))
			{
				if (type == "None")
					type = "Siege";
				else if (type == "Crab")
					type = "Mixed";
			}
		}
		return type;
	}
}
