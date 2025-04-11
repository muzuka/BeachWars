using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

//using UnityEditor;

/// <summary>
/// Input handler.
/// </summary>
public class InputController : MonoBehaviour {

    [Tooltip("The distance a mouse must be dragged to become a multi-select.")]
	public float SingleClickMaxDist;
    public int MaxOffsets;
    public float OffsetRadius;

    public GameObject HaloCanvas;
    public GameObject BuildingCanvas;

    Dictionary<string, Action<GameObject>> _rightClickActions;
    Dictionary<InputAction, Action> _keyboardActions;
    PlayerInput _playerInput;
    BaseControls _clickControls;
    InputAction _leftClick;
    InputAction _rightClick;
    BaseControls.HotKeysActions _hotkeys;

    Player _player;
    GUIController _gui;
    
    Vector3 _anchor;				// Start point
	Vector3 _outer;					// drag point
	bool _hasActiveBox;				// is SelectBox active?
	Vector2 _mouseDownPos;

	RaycastHit _hit;

	Canvas _weaponCanvas;

	DebugComponent _debug;

    List<Vector3> _offsets;

    #region Monobehaviour functions

    void Awake()
    {
	    _clickControls = new BaseControls();
	    _playerInput = GetComponent<PlayerInput>();
	    _leftClick = _clickControls.Units.Select;
	    _rightClick = _clickControls.Units.Use;

	    _clickControls.Units.Select.started += 
		    x => LeftClickStart();
	    _clickControls.Units.Select.canceled +=
		    x => LeftClickEnd();
	    _clickControls.Units.Use.performed +=
		    x => RightClickWithoutBuilding();
	    _clickControls.Building.Place.performed +=
		    x => LeftClickWithBuilding();
	    _clickControls.Building.Cancel.performed +=
		    x => RightClickWithBuilding();
	    
	    _hotkeys = _clickControls.HotKeys;
	    
	    _keyboardActions = new Dictionary<InputAction, Action>()
	    {
		    { _hotkeys.Attack, AttackAction },
		    { _hotkeys.Build, BuildAction },
		    { _hotkeys.Capture, CaptureAction },
		    { _hotkeys.Enter, EnterAction },
		    { _hotkeys.Repair, RepairAction },
		    { _hotkeys.Recruit, RecruitAction },
		    { _hotkeys.Upgrade, UpgradeAction },
		    { _hotkeys.Info, InfoAction },
		    { _hotkeys.Pause, PauseAction }
	    };

	    foreach (InputAction a in _keyboardActions.Keys)
	    {
		    a.performed += x => _keyboardActions[a]();
	    }
    }

    /// <summary>
    /// Start this instance.
    /// </summary>
    void Start()
    {
	    _debug = GetComponent<DebugComponent>();
	    _player = GetComponent<Player>();
	    _gui = GetComponent<GUIController>();

	    _rightClickActions = new Dictionary<string, Action<GameObject>>()
	    {
		    { Tags.Crab, RightClickCrab },
		    { Tags.Castle, (x) => RightClickCastle(x.GetComponent<CastleController>()) },
		    { Tags.Ghost, RightClickGhost },
		    { Tags.Block, RightClickBlock },
		    { Tags.Gate, RightClickGate },
		    { Tags.Catapult, RightClickSiege },
		    { Tags.Ballista, RightClickSiege },
		    { Tags.Wood, RightClickResource },
		    { Tags.Stone, RightClickResource },
		    { Tags.Armoury, RightClickArmoury },
		    { Tags.Beach, (x) => RightClickBeach() }
	    };
	    
	    _offsets = new List<Vector3>();
	    for (int i = 0; i < MaxOffsets; i++) {
		    _offsets.Add(GetOffset(i, OffsetRadius, 1, 1));
	    }
    }

    void Update()
    {
	    if (_hasActiveBox)
	    {
		    if (!_gui.MouseOnGUI())
		    {
			    DragBoxSelection();
		    }
	    }
    }

    void OnEnable()
    {
	    _clickControls.Enable();
    }

    void OnDisable()
    {
	    _clickControls.Disable();
    }

    #endregion

    #region Building functions

    public void EnterBuildingMode()
    {
	    _clickControls.Units.Disable();
	    _clickControls.Building.Enable();
	    _leftClick = _clickControls.Building.Place;
	    _rightClick = _clickControls.Building.Cancel;
    }

    public void ExitBuildingMode()
    {
	    _clickControls.Units.Enable();
	    _clickControls.Building.Disable();
	    _leftClick = _clickControls.Units.Select;
	    _rightClick = _clickControls.Units.Use;
    }
    
    #endregion
    
	#region Mouse click functions

	void LeftClickWithBuilding()
	{
		if (!_gui.MouseOnGUI())
		{
			_playerInput.enabled = true;
			GhostBuildingManager ghostManager = _player.GhostManager;
			if (_player.BuildingType == "Junction")
			{
				ClickWhileBuildingJunction();
			}
			else if (_leftClick.WasReleasedThisFrame() && ghostManager.CanBuild)
			{
				LeftClickWhileBuilding();
			}
		}
		else
		{
			_playerInput.enabled = false;
		}
	}

	void RightClickWithBuilding()
	{
		if (!_gui.MouseOnGUI())
		{
			_playerInput.enabled = true;
			if (_rightClick.WasReleasedThisFrame())
			{
				RightClickWhileBuilding();
			}
		}
		else
		{
			_playerInput.enabled = false;
		}
	}

	void LeftClickStart()
	{
		_debug.LogMessage("Left clicked!");
		if (!_gui.MouseOnGUI())
		{
			_playerInput.enabled = true;
			_mouseDownPos = Mouse.current.position.ReadValue();
			_hasActiveBox = true;
			CreateBoxSelection();
		}
		else
		{
			_playerInput.enabled = false;
		}
	}

	void LeftClickEnd()
	{
		if (!_gui.MouseOnGUI() && _hasActiveBox)
		{
			Vector2 releasePos = Mouse.current.position.ReadValue();
			float dragDist = Vector2.Distance(_mouseDownPos, releasePos);

			if (dragDist < SingleClickMaxDist)
			{
				ProcessClickWithNoDrag();
			}
			else
			{
				SelectEntities();
			}

			_hasActiveBox = false;
		}
	}

	void RightClickWithoutBuilding()
	{
		if (!_gui.MouseOnGUI())
		{
			_playerInput.enabled = true;
			if (Physics.Raycast(Raycaster.GetRay(), out _hit))
			{
				RightClickTarget();
			}
		}
		else
		{
			_playerInput.enabled = false;
		}
	}
	
	void ClickWhileBuildingJunction()
	{
		GhostBuildingManager ghostManager = _player.GhostManager;
	    
		_player.Deselect();
		_player.States.SetState("Building", true);

		if (_leftClick.WasReleasedThisFrame())
		{
			_debug.LogMessage("Pressed select");
			if (Raycaster.IsPointingAt(Tags.Junction))
			{
				ghostManager.ExtendJunction();
			}
			else if (ghostManager.CanBuild)
			{
				ghostManager.PlaceJunction();
			}
		}
		else if (_rightClick.WasReleasedThisFrame())
		{
			_player.CancelBuilding();
		}
	}

    // Process Left Click
    // ####################################################################################
    /// <summary>
    /// Left click occurred while building
    /// </summary>
    /// <param name="manager">Manager object.</param>
    /// <param name="player">Player script.</param>
    void LeftClickWhileBuilding()
    {
	    GhostBuildingManager ghostManager = _player.GhostManager;

	    if (ghostManager.GhostBuilding)
	    {
		    GhostBuilder buildingGhost = ghostManager.GhostBuilding.GetComponent<GhostBuilder>();
		    buildingGhost.HasBuilder = _player.HasSelected;

		    buildingGhost.Placed = true;
		    ghostManager.PlaceGhostBuilding();

		    _player.States.SetState("Building", false);
	    }
	    else
	    {
		    _hit = Raycaster.ShootMouseRay();

		    if (_hit.collider.CompareTag(Tags.Junction))
		    {
			    ghostManager.ExtendJunction();
		    }
	    }
    }
    
    void ProcessClickWithNoDrag()
    {
	    _debug.LogMessage("Single-selected");

        _hit = Raycaster.ShootMouseRay();

        if (_hit.transform.CompareTag(Tags.Beach) && _player.CanCommand)
        {
	        _debug.LogMessage("Hit Beach");
            if (_player.SelectedList.Count > 1)
            {
                MoveSelectedCrabs();
            }
            else
            {
                if (IdUtility.IsCrab(_player.Selected))
                {
	                _player.Selected.GetComponent<CrabController>().StartNewMove(_hit.point);
                }
                else if (IdUtility.IsSiegeWeapon(_player.Selected))
                {
	                _player.Selected.GetComponent<SiegeController>().StartMove(_hit.point);
                }
            }
        }
        else
        {
	        _player.Deselect();
            AddSingleEntity();
            if (_player.SelectedList.Count == 1)
            {
	            _player.Select(_player.SelectedList[0]); 
            }
        }
    }

    #endregion
	
    #region Right click functions

    // Process Right Click
    // ####################################################################################
    // Input: The player script.
    /// <summary>
    /// Right click occurred while building
    /// </summary>
    void RightClickWhileBuilding()
	{
		GameObject buildClick = Raycaster.GetObjectAtRay();

		if (buildClick.CompareTag(Tags.Ghost)) 
		{
			buildClick.GetComponent<GhostBuilder>().Destroyed();
			Destroy(buildClick);
		}

		_player.States.SetState("Building", false);
	}

	/// <summary>
	/// Right click occurred while not building.
	/// Indicates a command or deselects.
	/// </summary>
	void RightClickTarget() {

		GameObject target = _hit.transform.gameObject;
		string targetName = _hit.transform.tag;

		_debug.LogMessage("Right clicked " + targetName + ".");

		if (_player.CanCommand)
		{
			if (_rightClickActions.ContainsKey(target.tag))
			{
				_rightClickActions[target.tag].Invoke(target);
			}
			else
			{
				if (IdUtility.IsBuilding(target))
				{
					RightClickBuilding(target);
				}
				else if (IdUtility.IsWeapon(target) && IdUtility.IsCrab(_player.Selected))
				{
					_player.Selected.GetComponent<CrabController>().StartCollecting(target);
				}
			}
		}
	}

	/// <summary>
	/// Right clicks the beach.
	/// Deselects.
	/// </summary>
	void RightClickBeach()
	{
		_debug.LogMessage("Deselected " + _player.Selected.name + ".");

		if (_player.CanCommand && _player.States.GetState("Building"))
		{
			_player.StartBuild(_hit.point);
		}
		else
        {
            _player.Deselect();
        }
	}

	/// <summary>
	/// Right clicks castle.
	/// Crabs can be entered, given resources, captured, attacked, repaired, etc.
	/// </summary>
	/// <param name="_player">Player script.</param>
	/// <param name="castle">Castle script.</param>
	void RightClickCastle(CastleController castle)
	{
        if (castle.GetTeam().OnTeam(_player.GetTeam().team))
        {
	        Inventory inv;
	        
            // Evacuate castle
            if (_player.Selected.CompareTag(Tags.Castle))
            {
                _player.Selected.GetComponent<Enterable>().RemoveOccupant(); 
            }
            // Repair castle if friendly
            else if (_player.States.GetState("Repairing") && _player.CanCommand)
            {
                _player.Selected.GetComponent<CrabController>().StartRepair(castle.gameObject);
                _player.Selected.GetComponent<CrabController>().SetCrabs(_player.SelectedList.Count);
                _player.MoveMultiple(castle.transform.position);
            }
            // Unload resources if friendly
            else
            {
	            if (_player.CanCommand && IdUtility.IsCrab(_player.Selected))
	            {
		            inv = _player.Selected.GetComponent<CrabController>().Inventory;
		            if (inv.Contains(Tags.Stone) || inv.Contains(Tags.Wood))
		            {
			            for (int i = 0; i < _player.SelectedList.Count; i++)
			            {
				            if (IdUtility.IsCrab(_player.SelectedList[i]))
				            {
					            _player.SelectedList[i].GetComponent<CrabController>().StartUnloading(castle.gameObject);
				            }
			            }
		            }
		            else
		            {
			            _player.Selected.GetComponent<CrabController>().StartEnter(castle.gameObject);
		            }
	            }
            }
        }
        else {
            // Attack castle if on enemy team
            if (_player.States.GetState("Attacking"))
            {
                _player.StartAttack(castle.gameObject);
            }
            // Capture castle if on enemy team
            else 
            {
                _player.Selected.GetComponent<CrabController>().CaptureCastle(castle.gameObject);
                _player.Selected.GetComponent<CrabController>().SetCrabs(_player.SelectedList.Count);
                _player.MoveMultiple(castle.transform.position);
            }
        }
	}

	/// <summary>
	/// Right clicks a miscellaneous building.
	/// Buildings can be entered, repaired, attacked, or evacuated.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="building">Target object.</param>
	void RightClickBuilding(GameObject building) 
	{
        if (building.GetComponent<Team>().OnTeam(_player.GetTeam().team))
        {
            if (_player.States.GetState("Repairing"))
            {
                _player.Selected.GetComponent<CrabController>().StartRepair(building);
                _player.Selected.GetComponent<CrabController>().SetCrabs(_player.SelectedList.Count);
                _player.MoveMultiple(building.transform.position);
            }
            else if (_player.States.GetState("Attacking"))
            {
	            _player.Selected.GetComponent<CrabController>().StartDismantling(building);
            }
            else if (building.GetComponent<Enterable>())
            {
                if (_player.Selected == building)
                {
                    _player.Selected.GetComponent<Enterable>().RemoveOccupant();
                }
                else
                {
                    if (IdUtility.IsCrab(_player.Selected))
                    {
                        _player.Selected.GetComponent<CrabController>().StartEnter(building); 
                    }
                    else if (IdUtility.IsSiegeWeapon(_player.Selected) && building.CompareTag(Tags.Tower))
                    {
                        _player.Selected.GetComponent<SiegeController>().StartEnter(building); 
                    }
                }
            }
        }
	}

	/// <summary>
	/// Right clicks a resource.
	/// Always collects.
	/// </summary>
	/// <param name="_player">Player script.</param>
	/// <param name="target">Target object.</param>
	void RightClickResource(GameObject target) 
	{
		target.GetComponent<ResourceController>().SetCrabs(_player.SelectedList);
		for (int i = 0; i < _player.SelectedList.Count; i++) 
		{
            if (IdUtility.IsCrab(_player.SelectedList[i]))
            {
                _player.SelectedList[i].GetComponent<CrabController>().StartCollecting(target); 
            }
		}
	}

	/// <summary>
	/// Right clicks a crab.
	/// Recruits or attacks.
	/// </summary>
	/// <param name="_player">Player script.</param>
	/// <param name="target">Target object.</param>
	void RightClickCrab(GameObject target)
	{
		int hitTeam = target.GetComponent<Team>().team;

		if (_player.States.GetState("Recruiting") && hitTeam == -1)
        {
            _player.Selected.GetComponent<CrabController>().StartRecruiting(target);
        }
		else if (!_player.GetTeam().OnTeam(hitTeam)) 
		{
			_player.States.ClearStates();
			_player.States.SetState("Attacking", true);
			_player.StartAttack(target);
		}
	}

    /// <summary>
    /// Right click a ghost structure.
    /// Either deletes it or tells a crab to build it.
    /// </summary>
    /// <param name="_player">Player script</param>
    /// <param name="target">Target object</param>
    void RightClickGhost(GameObject target)
    {
        if (_player.CanCommand)
        {
            if (_player.Selected.GetComponent<CrabController>() != null)
            {
                _player.Selected.GetComponent<CrabController>().BuildFromGhost(target);
            }
        }
        else
        {
	        _debug.LogMessage("Target is not commandable.");
            if (target.GetComponent<Team>().OnTeam(_player.GetTeam().team))
            {
                target.GetComponent<GhostBuilder>().Destroyed();
                Destroy(target);
            }
        }
    }

	/// <summary>
	/// Right click a block.
	/// Attack, upgrade, or repair.
	/// </summary>
	/// <param name="_player">Player script.</param>
	/// <param name="target">Target object.</param>
	void RightClickBlock(GameObject target) 
	{
		if (target.GetComponent<Team>().OnTeam(_player.GetTeam().team)) 
		{
			if (_player.States.GetState("Upgrading")) 
			{
				_player.Selected.GetComponent<CrabController>().StartUpgrading(target);
				_player.Selected.GetComponent<CrabController>().SetCrabs(_player.SelectedList.Count);
				_player.MoveMultiple(target.transform.position);
			}
			else if (_player.States.GetState("Repairing"))
			{
				_player.Selected.GetComponent<CrabController>().StartRepair(target);
				_player.Selected.GetComponent<CrabController>().SetCrabs(_player.SelectedList.Count);
				_player.MoveMultiple(target.transform.position);
			}
            else if (_player.States.GetState("Attacking"))
            {
	            _player.Selected.GetComponent<CrabController>().StartDismantling(target);
            }
		}
        else
        {
            if (_player.States.GetState("Attacking"))
            {
                _player.StartAttack(target); 
            }
        }
	}

    /// <summary>
    /// Right click a Gate.
    /// Opens and closes depending on the state.
    /// </summary>
    /// <param name="target">Target object</param>
    void RightClickGate(GameObject target)
    {
        GateController controller = target.GetComponent<GateController>();
        if (controller.IsOpen())
        {
            controller.CloseGate();
            _debug.LogMessage("Closing gate");
        }
        else
        {
            controller.OpenGate();
            _debug.LogMessage("Opening gate");
        }
    }

	/// <summary>
	/// Right click a siege weapon.
	/// Enter or attack.
	/// </summary>
	/// <param name="_player">Player script.</param>
	/// <param name="target">Target object.</param>
	void RightClickSiege(GameObject target) 
	{
		if (target.GetComponent<Team>().OnTeam(_player.GetComponent<Team>().team))
        {
            if (_player.HasSelected)
            {
                _player.Selected.GetComponent<CrabController>().StartEnter(target);
            }
            else
            {
                target.GetComponent<Enterable>().RemoveOccupant();
            }
        }
		else
        {
            _player.StartAttack(target);
        }
	}

	/// <summary>
	/// Right click an armoury.
	/// Spawns menu for weapon.
	/// </summary>
	/// <param name="_player">Player script.</param>
	/// <param name="target">Target object.</param>
	void RightClickArmoury(GameObject target)
	{
		if (_player.GetTeam().OnTeam(target.GetComponent<Team>().team))
		{
			if (IdUtility.IsCrab(_player.Selected) && _player.CanCommand)
			{
				// open menu to choose weapon
				// set armoury as current

				if (_player.States.GetState("Attacking"))
				{
					_player.Selected.GetComponent<CrabController>().StartDismantling(target);
				}
				else
				{
					_debug.LogMessage("Set up weapon canvas");

					if (!_weaponCanvas)
					{
						_weaponCanvas = Instantiate<GameObject>(BuildingCanvas).GetComponent<Canvas>();
					}
					else
					{
						_weaponCanvas.gameObject.SetActive(true);
					}

					Button[] buttons = _weaponCanvas.GetComponentsInChildren<Button>();
					for (int i = 0; i < buttons.Length; i++)
					{
						if (buttons[i].name == "SpearButton")
							buttons[i].onClick.AddListener(() =>
							{
								GetComponent<Player>().TakeWeapon(Tags.Spear);
								_weaponCanvas.gameObject.SetActive(false);
							});
						if (buttons[i].name == "HammerButton")
							buttons[i].onClick.AddListener(() =>
							{
								GetComponent<Player>().TakeWeapon(Tags.Hammer);
								_weaponCanvas.gameObject.SetActive(false);
							});
						if (buttons[i].name == "BowButton")
							buttons[i].onClick.AddListener(() =>
							{
								GetComponent<Player>().TakeWeapon(Tags.Bow);
								_weaponCanvas.gameObject.SetActive(false);
							});
						if (buttons[i].name == "ShieldButton")
							buttons[i].onClick.AddListener(() =>
							{
								GetComponent<Player>().TakeWeapon(Tags.Shield);
								_weaponCanvas.gameObject.SetActive(false);
							});
					}

					_player.TargetedArmoury = target.GetComponent<HoldsWeapons>();
				}
			}
		}
	}
    #endregion

    #region Keyboard functions
    
	void AttackAction()
	{
		_debug.LogMessage("Click on something to attack.");
		_player.States.ClearStates();
		_player.States.SetState("Attacking", true);
	}

	void BuildAction()
	{
		_debug.LogMessage("Start building castle.");
		_player.States.ClearStates();
		_player.States.SetState("Building", true);
		_player.SetBuildingType(Tags.Castle);
	}

	void CaptureAction()
	{
		_debug.LogMessage("Click on castle to capture.");
		_player.States.SetState("Capturing", true);
	}
	
	void EnterAction()
	{
		_debug.LogMessage("Click on a castle or siege weapon to enter.");
		_player.States.ClearStates();
		_player.States.SetState("Entering", true);
	}
	
	void RepairAction()
	{
		_debug.LogMessage("Click an object to repair.");
		_player.States.SetState("Repairing", true);
	}
	
	void RecruitAction()
	{
		_debug.LogMessage("Click on a neutral crab to recruit.");
		_player.States.SetState("Recruiting", true);
	}
	
	void UpgradeAction()
	{
		_debug.LogMessage("Click on a block to upgrade.");
		_player.States.SetState("Upgrading", true);
	}
	
	void InfoAction()
	{
		for (int i = 0; i < _player.SelectedList.Count; i++)
		{
			_debug.LogMessage(_player.SelectedList[i].tag);
		}
	}
	
	void PauseAction()
	{
		_player.Paused = !_player.Paused;
		if (_player.Paused)
		{
			Time.timeScale = 0.0f;
			_gui.PauseMenu.SetActive(true);
		}
		else
		{
			Time.timeScale = 1.0f;
			_gui.PauseMenu.SetActive(false);
		}
	}

    #endregion

    #region Selection functions

    // box selection
    // ##################################################################################################################################################
    /// <summary>
    /// Adds to selection.
    /// </summary>
    /// <param name="player">Player script.</param>
    /// <param name="entity">Entity object.</param>
    void AddToSelection(GameObject entity) 
	{
		_debug.LogMessage("Added " + entity.tag);

		_player.SelectedList.Add(entity);

		if (IdUtility.IsMoveable(entity))
			_player.HaloList.Add(Instantiate(HaloCanvas));
	}
		
	/// <summary>
	/// Adds all within bounds.
	/// </summary>
	/// <param name="player">Player script.</param>
	void AddAllWithinBounds()
	{
		Camera mainCamera = Camera.main;
		Team playerTeam = _player.GetComponent<Team>();
		Bounds bounds = SelectUtils.GetViewportBounds(mainCamera, _anchor, _outer);
		
		foreach(CrabController crab in FindObjectsOfType<CrabController>()) 
		{
			if (crab.GetComponent<Team>().OnTeam(playerTeam.team)) 
			{
				if (SelectUtils.IsWithinBounds(mainCamera, bounds, crab.transform.position))
					AddToSelection(crab.gameObject);
			}
		}

		foreach(SiegeController siege in FindObjectsOfType<SiegeController>()) 
		{
			if (siege.GetComponent<Enterable>().Occupied()) 
			{
				if (siege.GetComponent<Team>().OnTeam(playerTeam.team))
				{
					if (SelectUtils.IsWithinBounds(mainCamera, bounds, siege.transform.position))
						AddToSelection(siege.gameObject);
				}
			}
		}
	}

	/// <summary>
	/// Adds a single entity.
	/// </summary>
	/// <param name="player">Player script.</param>
	void AddSingleEntity() 
	{
		_hit = Raycaster.ShootMouseRay();
		GameObject entity = _hit.transform.gameObject;
		// entity exists and player hasn't selected already and entity isn't the beach.
		bool selectable = entity != null && !_player.SelectedList.Contains(entity) && entity.tag != Tags.Beach;
		
		if (selectable)
		{
			//player.select(entity);
			AddToSelection(entity);
		}
	}

	/// <summary>
	/// Selects the entities within the bounds.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="gui">GUI script.</param>
	void SelectEntities() 
	{
		_hasActiveBox = false;

		// if mouse hasn't moved
		if (Vector3.Distance(_anchor, _outer) < SingleClickMaxDist)
		{
            ProcessClickWithNoDrag();
		}
		else 
		{
			_debug.LogMessage("Multi-selected");
			
			_player.Deselect();
			AddAllWithinBounds();

			if (_player.SelectedList.Count == 1) 
			{
				_debug.LogMessage("Selected one object");
				_player.Select(_player.SelectedList[0]);
			}
			else if (_player.SelectedList.Count > 1)
			{
				_debug.LogMessage("Selected multiple objects");
				_player.SelectAll();
			}
		}

		_gui.ClearBox();
	}

	/// <summary>
	/// Creates the box selection.
	/// </summary>
	void CreateBoxSelection() 
	{
		_anchor = Mouse.current.position.ReadValue();
		_outer = Mouse.current.position.ReadValue();
		
		_hasActiveBox = true;

		_gui.StartSelectBox(_anchor);
	}

	/// <summary>
	/// Drags the box selection.
	/// </summary>
	void DragBoxSelection() 
	{
		if (_hasActiveBox) 
		{
			_gui.DragSelectBox(Mouse.current.position.ReadValue());
		}
	}

    #endregion
    
    #region Helper functions

        // Spaces crabs out around the selected position
    void MoveSelectedCrabs()
    {
        Vector3 avgPosition = InfoTool.GetAveragePosition(_player.SelectedList);
        float avgDistance = InfoTool.GetAverageDistance(_player.SelectedList, avgPosition);
        int avgQuadrantAmount = _player.SelectedList.Count / 4;
        List<GameObject> listA = new List<GameObject>();
        List<GameObject> listB = new List<GameObject>();
        List<GameObject> listC = new List<GameObject>();
        List<GameObject> listD = new List<GameObject>();

        foreach (GameObject selected in _player.SelectedList)
        {
            if (selected.transform.position.x > avgPosition.x && selected.transform.position.z > avgPosition.z)
            {
                listA.Add(selected);
            }
            else if (selected.transform.position.x > avgPosition.x && selected.transform.position.z < avgPosition.z)
            {
                listB.Add(selected);
            }
            else if (selected.transform.position.x < avgPosition.x && selected.transform.position.z > avgPosition.z)
            {
                listD.Add(selected);
            }
            else if (selected.transform.position.x < avgPosition.x && selected.transform.position.z < avgPosition.z)
            {
                listC.Add(selected);
            }
        }

        SortByPosition(avgPosition, ref listA);
        SortByPosition(avgPosition, ref listB);
        SortByPosition(avgPosition, ref listC);
        SortByPosition(avgPosition, ref listD);

        BalanceQuadrants(ref listA, ref listB, ref listC, ref listD, avgQuadrantAmount);

        MoveSelected(listA, 1, 1);
        MoveSelected(listB, 1, -1);
        MoveSelected(listC, -1, -1);
        MoveSelected(listD, -1, 1);
    }

    // Makes sure all quadrants have as close to the same amount of crabs as possible.
    void BalanceQuadrants(ref List<GameObject> listA, ref List<GameObject> listB, ref List<GameObject> listC, ref List<GameObject> listD, int avgAmount)
    {
        List<GameObject>[] lists = { listA, listB, listC, listD };

        for (int i = 0; i < lists.Length; i++)
        {
            if (lists[i].Count > avgAmount)
            {
                for (int j = i + 1; j < lists.Length; j++)
                {
                    if (lists[j].Count < avgAmount)
                    {
                        while (lists[i].Count > avgAmount && lists[j].Count < avgAmount)
                        {
                            lists[j].Add(lists[i][0]);
                            lists[i].RemoveAt(0);
                        }
                    }
                }
            }
            else if (lists[i].Count < avgAmount)
            {
                for (int j = i + 1; j < lists.Length; j++)
                {
                    if (lists[j].Count > avgAmount)
                    {
                        while (lists[j].Count > avgAmount && lists[i].Count < avgAmount)
                        {
                            lists[i].Add(lists[j][0]);
                            lists[j].RemoveAt(0);
                        }
                    }
                }
            }
        }
    }

    // Tells crabs in list to move to the assigned offset position
    void MoveSelected(List<GameObject> crabs, int xSign, int zSign)
    {
        for (int i = 0; i < crabs.Count; i++)
        {
            Vector3 offset = new Vector3(xSign * _offsets[i].x, _offsets[i].y, zSign * _offsets[i].z);

            if (IdUtility.IsCrab(crabs[i]))
            {
                crabs[i].GetComponent<CrabController>().ActionStates.ClearStates(); 
                crabs[i].GetComponent<CrabController>().StartNewMove(_hit.point + offset);
            }
            else if (IdUtility.IsSiegeWeapon(crabs[i]))
            {
                crabs[i].GetComponent<SiegeController>().StartMove(_hit.point + offset);
            }
        }
    }
    
    // Sorts by distance to the hit point of the mouse click.
    void SortByPosition(Vector3 position, ref List<GameObject> objects)
    {
        objects.Sort(delegate (GameObject a, GameObject b)
        {
            if (a == null)
            {
                return -1;
            } 
            else if (b == null)
            {
                return 1;
            }

            float distA = Vector3.Distance(a.transform.position, position);
            float distB = Vector3.Distance(b.transform.position, position);
            if (distA < distB)
            {
                return -1;
            } 
            else if (distB < distA)
            {
                return 1;
            } 
            else
            {
                return 0;
            }
        });
    }

    Vector3 GetOffset(int pos, float radius, int xSign, int zSign) 
    {
	    float diameter = Mathf.Pow(radius, 2);
	    int maxDiameterMultiple = 1;

	    float x = xSign * radius;
	    float y = 0.0f;
	    float z = zSign * radius;

	    if (pos == 0)
	    {
		    _debug.LogMessage("Crab moving to(" + x + " " + y + " " + z + ")");
		    return new Vector3(x, y, z);
	    }

	    int count = 1;

	    while (count <= pos)
	    {
		    for (int i = 0; i <= maxDiameterMultiple; i++)
		    {
			    x = xSign * radius + (diameter * i);
			    z = zSign * radius + (diameter * maxDiameterMultiple);

			    if (count == pos)
			    {
				    _debug.LogMessage("Crab moving to(" + x + " " + y + " " + z + ")");
				    return new Vector3(x, y, z);
			    }

			    count++;
		    }

		    for (int i = maxDiameterMultiple - 1; i >= 0; i--)
		    {
			    x = xSign * radius + (diameter * maxDiameterMultiple);
			    z = zSign * radius + (diameter * i);

			    if (count == pos)
			    {
				    _debug.LogMessage("Crab moving to(" + x + " " + y + " " + z + ")");
				    return new Vector3(x, y, z);
			    }

			    count++;
		    }

		    maxDiameterMultiple++;
	    }

	    return new Vector3();
    }
    #endregion
}
