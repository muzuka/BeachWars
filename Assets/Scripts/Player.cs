using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Main player controller.
/// Handles selected units and delegating actions.
/// </summary>
public class Player : MonoBehaviour
{
    [Tooltip("Is lose condition active?")]
	public bool LoseCondition;
    [Tooltip("Is win condition active?")]
	public bool WinCondition;

    [Tooltip("Maximum units allowed to exist.")]
    public int MaxUnitCount;

    [Tooltip("The min distance between junctions before intermediate blocks are placed")]
    public float JunctionDistanceLimit;
    [Tooltip("The size of a block(informs distance between each block")]
    public float BlockSize;

	Team _team;

	// reference to gui
	public GUIController Gui { get; set; }

	// reference to input
	public InputController Input { get; set; }

	// The currently selected object or the first element of selectedList.
	public GameObject Selected { get; set; }

	// The list of selected objects
	public List<GameObject> SelectedList { get; set; }

 	// is something selected?
	public bool HasSelected { get; set; }

	// is more than one thing selected?
	public bool MultiSelect { get; set; }

 	// can I command the selected object?
	public bool CanCommand { get; set; }

	// The selected objects team
	public Team SelectedTeam { get; set; }

	// The list of haloes
	public List<GameObject> HaloList { get; set; }

	// action states
	// Assuming crab is selected
 	// set of states to affect behaviour
	public StateController States { get; set; }
	string[] _stateList = {"Building", "Attacking", "Entering", "Capturing", "Recruiting", "Upgrading", "Repairing"};

	// Designates type selected for building
	public string BuildingType { get; set; }

	public GhostBuildingManager GhostManager { get; set; }

	public bool Paused { get; set; }

	bool _debug;

	public bool AutoBuild { get; set; }

	Canvas _buildingCanvas;
	Vector3 _cameraPos;
	float _cameraXDelta;
	float _cameraZDelta;

	public HoldsWeapons TargetedArmoury { get; set; }

    public static Player Instance;

    /// <summary>
    /// Wake this instance
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Start this instance
    /// </summary>
    void Start()
	{
		Input = GetComponent<InputController>();
		Gui = GetComponent<GUIController>();

		_team = GetComponent<Team>();
		States = new StateController(_stateList);
		SelectedList = new List<GameObject>();
		HaloList = new List<GameObject>();

		GhostManager = new GhostBuildingManager();
        GhostManager.JunctionDistLimit = JunctionDistanceLimit;
        GhostManager.BlockSize = BlockSize;

		_debug = GetComponent<DebugComponent>().Debug;
		GhostManager.Debug = _debug;
		States.Debug = _debug;

		Paused = false;

		AutoBuild = true;

		_buildingCanvas = null;

		TargetedArmoury = null;

		Gui.StartUI();

		if (_debug)
			Debug.Log("Finished starting.");
	}

    /// <summary>
    /// Update this instance
    /// </summary>
	void Update()
	{
		if (!Selected)
			Deselect();

		if (LoseCondition)
			CheckLoseCondition();
		if (WinCondition)
			CheckWinCondition();

		if (_buildingCanvas)
		{
			_cameraXDelta = GetComponentInParent<Transform>().position.x - _cameraPos.x;
			_cameraZDelta = GetComponentInParent<Transform>().position.z - _cameraPos.z;

			Vector2 tempPos = GetPanel().anchoredPosition;
			tempPos.x -= _cameraXDelta;
			tempPos.y -= _cameraZDelta;
			GetPanel().anchoredPosition = tempPos;
		}

		Input.GetKeyboardInput(this);

		Input.ProcessMouseClick(this, Gui);

		if (SelectedList != null)
			MultiSelect = (SelectedList.Count > 1);
		
		if (HasSelected)
			UpdateHalos();

		GhostManager.updateGhosts();

		_cameraPos = GetComponentInParent<Transform>().position;
	}

	/// <summary>
	/// Updates the halos' positions.
	/// </summary>
	void UpdateHalos()
	{
		if (IdUtility.IsMoveable(Selected.tag))
		{
			for (int i = 0; i < HaloList.Count; i++)
			{
				HaloList[i].SetActive(true);
				HaloList[i].transform.GetChild(0).position = SelectedList[i].transform.position;

				if (SelectedList.Count > 1)
				{
					if (SelectedList[i].tag == Tags.Crab)
					{
						if (SelectedList[i].GetComponent<CrabController>().ActionStates.GetState("Attacking"))
						{
							HaloList[i].GetComponentInChildren<Image>().color = Color.red;
						}
						else if (SelectedList[i].GetComponent<CrabController>().ActionStates.GetState("Building"))
						{
							HaloList[i].GetComponentInChildren<Image>().color = Color.blue;
						}
						else if (SelectedList[i].GetComponent<CrabController>().ActionStates.GetState("Collecting"))
						{
							HaloList[i].GetComponentInChildren<Image>().color = Color.green;
						}
					}
					else if (IdUtility.IsSiegeWeapon(SelectedList[i].tag))
					{
						if (SelectedList[i].GetComponent<SiegeController>().IsBusy())
						{
							HaloList[i].GetComponentInChildren<Image>().color = Color.red;
						}
					}
				}
				else
				{
					if (Selected.tag == Tags.Crab)
					{
						if (Selected.GetComponent<CrabController>().ActionStates.GetState("Attacking"))
						{
							HaloList[0].GetComponentInChildren<Image>().color = Color.red;
						}
						else if (Selected.GetComponent<CrabController>().ActionStates.GetState("Building"))
						{
							HaloList[0].GetComponentInChildren<Image>().color = Color.blue;
						}
						else if (Selected.GetComponent<CrabController>().ActionStates.GetState("Collecting"))
						{
							HaloList[0].GetComponentInChildren<Image>().color = Color.green;
						}
					}
					else if (IdUtility.IsSiegeWeapon(Selected.tag))
					{
						if (Selected.GetComponent<SiegeController>().IsBusy())
						{
							HaloList[0].GetComponentInChildren<Image>().color = Color.red;
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Gets the main panel of the building canvas.
	/// </summary>
	/// <returns>The panel transform.</returns>
	RectTransform GetPanel()
	{
		if (_debug)
			Debug.Assert(_buildingCanvas);

		List<RectTransform> transforms = _buildingCanvas.GetComponentsInChildren<RectTransform>().ToList();

        return transforms.Find(x => x.gameObject.name == "MainPanel");
	}

    public void GeneralBuildButtonPressed(string buildingType)
    {
        SetBuildingType(buildingType);
        SetPlayerState("Building");
        CreateGhostBuilding();
    }

	/// <summary>
	/// Select the specified obj.
	/// Sets GUI info.
	/// </summary>
	/// <param name="obj">Object.</param>
	public void Select(GameObject obj) 
	{
		Selected = obj;
		SelectedTeam = obj.GetComponent<Team>();
		CanCommand = (_team.team == SelectedTeam.team);
		HasSelected = true;

		Selected.SendMessage("SetController", this, SendMessageOptions.DontRequireReceiver);

		// set gui
		Gui.InfoView.SetLabel(obj);
		Gui.SetActiveGUIComponents(obj.tag);
		Gui.InfoView.SelectedImage.texture = Resources.Load<Texture>("Textures/" + obj.tag);
		Gui.UpdateUI(this);
	}

	/// <summary>
	/// Selects all objects in list.
	/// Sets GUI info.
	/// </summary>
	public void SelectAll() 
	{
		Selected = SelectedList[0];
		HasSelected = true;
		SelectedTeam = SelectedList[0].GetComponent<Team>();

		for (int i = 0; i < SelectedList.Count; i++)
        {
            SelectedList[i].SendMessage("SetController", this, SendMessageOptions.DontRequireReceiver); 
        }

		CanCommand = (_team.team == SelectedTeam.team);

		Gui.SetActiveGUIComponents("multi");
		Gui.UpdateUI(this);
	}

	/// <summary>
	/// Deselects the currently selected object(s).
	/// </summary>
	public void Deselect()
	{
		if (HasSelected)
		{
			for (int i = 0; i < SelectedList.Count; i++)
			{
				if (SelectedList[i])
                {
                    SelectedList[i].SendMessage("toggleSelected", SendMessageOptions.DontRequireReceiver); 
                }
			}

			if (_buildingCanvas)
            {
                Destroy(_buildingCanvas); 
            }

			if (TargetedArmoury != null)
			{
				List<Canvas> canvases = FindObjectsOfType<Canvas>().ToList();
                canvases.RemoveAll(x => x.gameObject.name == "WeaponBuildingCanvas");
				TargetedArmoury = null;
			}

			States.ClearStates();
			SelectedList.Clear();
			CanCommand = false;
			HasSelected = false;
			Selected = null;
			SelectedTeam = null;

			ClearIndicators();

			Gui.Deselect();
		}
	}

	/// <summary>
	/// Deselect a specified obj.
	/// Searches for it in selected list.
	/// </summary>
	/// <param name="obj">Object.</param>
	public void Deselect(GameObject obj)
	{
		if (!SelectedList.Contains(obj))
        {
            return; 
        }
			
		if (!MultiSelect)
        {
            Deselect(); 
        }
		else
		{
			Predicate<GameObject> unitFinder = g => g == obj;
			int pos = SelectedList.FindIndex(unitFinder);

			if (pos == 0)
            {
                Selected = SelectedList[1]; 
            }

			SelectedList.RemoveAt(pos);
			Destroy(HaloList[pos]);
			HaloList.RemoveAt(pos);
		}
	}

	/// <summary>
	/// Destroys the indicators.
	/// </summary>
	public void ClearIndicators()
	{
		for (int i = 0; i < HaloList.Count; i++)
        {
            Destroy(HaloList[i]); 
        }
		HaloList.Clear();
	}

	/// <summary>
	/// Checks the win condition.
	/// You win when all castles are under your control.
	/// </summary>
	void CheckWinCondition()
	{
		bool foundCastle = false;
		CastleController[] list = FindObjectsOfType<CastleController>();
		for (int i = 0; i < list.Length; i++)
        {
            foundCastle |= list[i].GetComponent<Team>().team != _team.team; 
        }
		
		if (!foundCastle)
		{
			Debug.Log("You win!");
			Gui.WinMenu.SetActive(true);
		}
	}

	/// <summary>
	/// Checks the lose condition.
	/// You lose when all castles aren't in your control.
	/// </summary>
	void CheckLoseCondition()
	{
		bool foundCastle = false;
		CastleController[] list = FindObjectsOfType<CastleController>();

		for (int i = 0; i < list.Length; i++)
        {
            foundCastle |= list[i].gameObject.GetComponent<Team>().team == _team.team; 
        }

		if (!foundCastle)
		{
			Debug.Log("You lose!");
			Gui.WinMenu.SetActive(true);
			Gui.WinMenu.GetComponentInChildren<Text>().text = "You Lost!";
		}
	}

    public void PressedGeneralBuildButton(string type)
    {
        SetBuildingType(type);
        SetPlayerState(_stateList[0]);
        CreateGhostBuilding();
    }

	/// <summary>
	/// Sets the type of the building to build.
	/// </summary>
	/// <param name="type">Building tag.</param>
	public void SetBuildingType(string type)
	{
		if (IdUtility.IsBuilding(type) || type == Tags.Block || type == Tags.Gate || type == Tags.Junction)
        {
            BuildingType = type; 
        }
		else
        {
            BuildingType = "none"; 
        }

		GhostManager.BuildingType = BuildingType;
	}

	/// <summary>
	/// Tells ghostManager to create a ghost building.
	/// </summary>
	public void CreateGhostBuilding() 
	{
		if (_debug)
			Debug.Log("Building a " + BuildingType);

		GhostManager.CreateGhostBuilding();
	}

	/// <summary>
	/// Tells crab to take a weapon.
	/// </summary>
	/// <param name="weapon">Hammer, Spear, Bow, or Shield.</param>
	public void TakeWeapon(string weapon)
	{
		if (Selected.tag == Tags.Crab && CanCommand)
        {
            Selected.GetComponent<CrabController>().StartTakeWeapon(weapon); 
        }
	}

	/// <summary>
	/// Sets a player state.
	/// </summary>
	/// <param name="state">State name.</param>
	public void SetPlayerState(string state)
	{
		States.SetState(state, true);
	}

	/// <summary>
	/// Sets the images for each inventory slot.
	/// </summary>
	/// <param name="inv">Inventory array.</param>
	public void UpdateInventory(string[] inv)
	{
		Gui.InfoView.SetInvView(inv);
	}

	/// <summary>
	/// Tells crab to craft item.
	/// </summary>
	public void Craft()
	{
        if (Selected.GetComponent<CrabController>() == null)
        {
            Debug.LogWarning("Selected object is not a crab.");
            return;
        }

		if (CanCommand)
		{
            CrabController crab = Selected.GetComponent<CrabController>();
			crab.Craft(crab.GetCraftableType());
			UpdateInventory(crab.GetInventory());
		}
	}

	/// <summary>
	/// Crafts the bow.
	/// called by separate button
	/// separate from craft because of two crafting types at once.
	/// </summary>
	public void CraftBow()
	{
        if (Selected.GetComponent<CrabController>() == null)
        {
            Debug.LogWarning("Selected object is not a crab.");
            return;
        }

        if (CanCommand)
		{
            CrabController crab = Selected.GetComponent<CrabController>();
            crab.Craft(Tags.Bow);
			UpdateInventory(crab.GetInventory());
		}
	}

    /// <summary>
    /// Finds a crab to dismantle the selected building
    /// Called by separate button
    /// Assumes a building is selected that has an available destroy button.
    /// </summary>
    public void StartDismantle()
    {
        CrabController crab = InfoTool.FindIdleCrab(_team.team);

        if (crab != null)
        {
            crab.StartDismantling(Selected);
        }
        else
        {
            if (_debug)
                Debug.Log("No available crab.");
        }
    }

	/// <summary>
	/// Moves multiple crabs at once.
	/// </summary>
	/// <param name="dest">Destination.</param>
	public void MoveMultiple(Vector3 dest)
	{
		for (int i = 1; i < SelectedList.Count; i++)
		{
			if (SelectedList[i].tag == Tags.Crab)
            {
                SelectedList[i].SendMessage("startMove", dest); 
            }
		}
	}

	/// <summary>
	/// Gets status of multiple selected objects.
	/// Are the selected objects a mix of crabs and siege weapons?
	/// </summary>
	/// <returns>Crab for all crabs, Siege for all siege weapons, or Mixed for mixed.</returns>
	public string GetMultiSelectStatus()
	{
		string type = "None";
		for (int i = 0; i < SelectedList.Count; i++)
		{
			if (SelectedList[i].tag == Tags.Crab)
			{
				if (type == "None")
                {
                    type = "Crab"; 
                }
				else if (type == "Siege")
                {
                    type = "Mixed"; 
                }
			}
			if (IdUtility.IsSiegeWeapon(SelectedList[i].tag))
			{
				if (type == "None")
                {
                    type = "Siege"; 
                }
				else if (type == "Crab")
                {
                    type = "Mixed"; 
                }
			}
		}
		return type;
	}
}
