using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Serialization;

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

    [Header("Building vars")]
    
    [Tooltip("The min distance between junctions before intermediate blocks are placed")]
    public float JunctionDistanceLimit;
    [Tooltip("The size of a block(informs distance between each block")]
    public float BlockSize;
    public bool AutoBuild;

	// reference to gui
	[HideInInspector]
	public GUIController GUI;

	// reference to input
	[HideInInspector]
	public InputController Input;

	// The currently selected object or the first element of selectedList.
	[HideInInspector]
	public GameObject Selected;

	// The list of selected objects
	[HideInInspector]
	public List<GameObject> SelectedList;

 	// is something selected?
    [HideInInspector]
    public bool HasSelected;

 	// can I command the selected object?
    [HideInInspector] 
    public bool CanCommand;

	// The selected objects team
	[HideInInspector]
	public Team SelectedTeam;

	// The list of haloes
	[HideInInspector]
	public List<GameObject> HaloList;

	// action states
	// Assuming crab is selected
 	// set of states to affect behaviour
    public StateController States;
	string[] _stateList = {"Building", "Attacking", "Entering", "Capturing", "Recruiting", "Upgrading", "Repairing"};

	// Designates type selected for building
	[HideInInspector]
	public string BuildingType;

	public GhostBuildingManager GhostManager;
	
	[HideInInspector]
	public HoldsWeapons TargetedArmoury;

	[HideInInspector]
	public bool Paused;
	
	bool _debug;
	
	Team _team;
	
	Canvas _buildingCanvas;
	Vector3 _cameraPos;
	float _cameraXDelta;
	float _cameraZDelta;
	
    public static Player Instance;

    #region Monobehavior functions

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
	    GUI = GetComponent<GUIController>();

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

	    _buildingCanvas = null;

	    TargetedArmoury = null;

	    GUI.StartUI();

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

	    Input.ProcessMouseClick(this, GUI);
		
	    if (HasSelected)
		    UpdateHalos();

	    GhostManager.UpdateGhosts();

	    _cameraPos = GetComponentInParent<Transform>().position;
    }

    #endregion

    #region UI functions

    /// <summary>
    /// Updates the halos' positions.
    /// </summary>
    void UpdateHalos()
    {
	    if (IdUtility.IsMoveable(Selected))
	    {
		    for (int i = 0; i < HaloList.Count; i++)
		    {
			    CrabController SelectedCrab;
			    HaloList[i].SetActive(true);
			    HaloList[i].transform.GetChild(0).position = SelectedList[i].transform.position;

			    if (SelectedList.Count > 1)
			    {
				    if (IdUtility.IsCrab(SelectedList[i]))
				    {
					    SelectedCrab = SelectedList[i].GetComponent<CrabController>();
						
					    if (SelectedCrab.ActionStates.GetState("Attacking"))
					    {
						    HaloList[i].GetComponentInChildren<Image>().color = Color.red;
					    }
					    else if (SelectedCrab.ActionStates.GetState("Building"))
					    {
						    HaloList[i].GetComponentInChildren<Image>().color = Color.blue;
					    }
					    else if (SelectedCrab.ActionStates.GetState("Collecting"))
					    {
						    HaloList[i].GetComponentInChildren<Image>().color = Color.green;
					    }
				    }
				    else if (IdUtility.IsSiegeWeapon(SelectedList[i]))
				    {
					    if (SelectedList[i].GetComponent<SiegeController>().IsBusy())
					    {
						    HaloList[i].GetComponentInChildren<Image>().color = Color.red;
					    }
				    }
			    }
			    else
			    {
				    if (IdUtility.IsCrab(Selected))
				    {
					    SelectedCrab = Selected.GetComponent<CrabController>();
						
					    if (SelectedCrab.ActionStates.GetState("Attacking"))
					    {
						    HaloList[0].GetComponentInChildren<Image>().color = Color.red;
					    }
					    else if (SelectedCrab.ActionStates.GetState("Building"))
					    {
						    HaloList[0].GetComponentInChildren<Image>().color = Color.blue;
					    }
					    else if (SelectedCrab.ActionStates.GetState("Collecting"))
					    {
						    HaloList[0].GetComponentInChildren<Image>().color = Color.green;
					    }
				    }
				    else if (IdUtility.IsSiegeWeapon(Selected))
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
    /// Sets the images for each inventory slot.
    /// </summary>
    /// <param name="inv">Inventory array.</param>
    public void UpdateInventory(string[] inv)
    {
	    GUI.InfoView.SetInvView(inv);
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
	    if (_debug)
		    Debug.Log($"Pressed {buildingType} button!");
	    
	    SetBuildingType(buildingType);
	    SetPlayerState("Building");
	    CreateGhostBuilding();
    }

    #endregion

    #region Selection functions

    /// <summary>
    /// Select the specified obj.
    /// Sets GUI info.
    /// </summary>
    /// <param name="obj">Object.</param>
    public void Select(GameObject obj) 
    {
	    if (_debug)
		    Debug.Log($"Selected {obj.name}");
		
	    Selected = obj;
	    SelectedTeam = obj.GetComponent<Team>();
	    CanCommand = _team.OnTeam(SelectedTeam.team);
	    HasSelected = true;

	    Selected.SendMessage("SetController", this, SendMessageOptions.DontRequireReceiver);

	    // set gui
	    GUI.InfoView.SetLabel(obj);
	    GUI.SetActiveGUIComponents(obj.tag);
	    GUI.InfoView.SelectedImage.texture = Resources.Load<Texture>("Textures/" + obj.tag);
	    GUI.UpdateUI(this);
    }

    /// <summary>
    /// Selects all objects in list.
    /// Sets GUI info.
    /// </summary>
    public void SelectAll() 
    {
	    if (_debug)
		    Debug.Log($"Selected {SelectedList.Count} units.");
		
	    Selected = SelectedList[0];
	    HasSelected = true;
	    SelectedTeam = SelectedList[0].GetComponent<Team>();

	    for (int i = 0; i < SelectedList.Count; i++)
	    {
		    SelectedList[i].SendMessage("SetController", this, SendMessageOptions.DontRequireReceiver); 
	    }

	    CanCommand = (_team.team == SelectedTeam.team);

	    GUI.SetActiveGUIComponents("multi");
	    GUI.UpdateUI(this);
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

		    GUI.Deselect();
			
		    if (_debug)
			    Debug.Log($"Deselected unit(s)");
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
			
	    if (!IsMultiSelected())
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

    #endregion

    #region Win/Lose Condition functions

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
		    GUI.WinMenu.SetActive(true);
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
		    GUI.WinMenu.SetActive(true);
		    GUI.WinMenu.GetComponentInChildren<Text>().text = "You Lost!";
	    }
    }

    #endregion
    
	#region Unit functions

	/// <summary>
	/// Tells crab to take a weapon.
	/// </summary>
	/// <param name="weapon">Hammer, Spear, Bow, or Shield.</param>
	public void TakeWeapon(string weapon)
	{
		if (IdUtility.IsCrab(Selected) && CanCommand)
		{
			Selected.GetComponent<CrabController>().StartTakeWeapon(weapon); 
		}
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
			if (IdUtility.IsCrab(SelectedList[i]))
			{
				SelectedList[i].SendMessage("startMove", dest); 
			}
		}
	}

	#endregion

	#region Building functions

	/// <summary>
	/// Sets the type of the building to build.
	/// </summary>
	/// <param name="type">Building tag.</param>
	public void SetBuildingType(string type)
	{
		if (IdUtility.IsBuilding(type) || IdUtility.IsWallPart(type))
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

	public void CancelBuilding()
	{
		GhostManager.DestroyGhostBuilding();
		GhostManager.DestroyWall();
		States.SetState("Building", false);
		GhostManager.BuildingType = null;
		BuildingType = null;
	}

	#endregion
	
	/// <summary>
	/// Sets the player state.
	/// </summary>
	/// <param name="state">State name.</param>
	public void SetPlayerState(string state)
	{
		States.SetState(state, true);
	}
	
	/// <summary>
	/// Gets status of multiple selected objects.
	/// Are the selected objects a mix of crabs and siege weapons?
	/// </summary>
	/// <returns>Crab for all crabs, Siege for all siege weapons, or Mixed for mixed.</returns>
	public Enum.SelectStatus GetMultiSelectStatus()
	{
		Enum.SelectStatus type = Enum.SelectStatus.NONE;
		for (int i = 0; i < SelectedList.Count; i++)
		{
			if (IdUtility.IsCrab(SelectedList[i]))
			{
				if (type == Enum.SelectStatus.NONE)
                {
                    type = Enum.SelectStatus.CRAB; 
                }
				else if (type == Enum.SelectStatus.SIEGE)
                {
                    type = Enum.SelectStatus.MIXED; 
                }
			}
			if (IdUtility.IsSiegeWeapon(SelectedList[i]))
			{
				if (type == Enum.SelectStatus.NONE)
                {
                    type = Enum.SelectStatus.SIEGE;
                }
				else if (type == Enum.SelectStatus.CRAB)
                {
                    type = Enum.SelectStatus.MIXED; 
                }
			}
		}
		return type;
	}

	public bool IsMultiSelected()
	{
		if (SelectedList != null)
		{
			return SelectedList.Count > 1;
		}

		return false;
	}
}
