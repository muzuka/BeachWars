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

    Dictionary<string, Action<Player, GameObject>> _rightClickActions;
    PlayerInput _playerInput;
    BaseControls _clickControls;
    InputAction _select;
    InputAction _use;
    BaseControls.HotKeysActions _hotkeys;
    
    Vector3 _anchor;				// Start point
	Vector3 _outer;				// drag point
	bool _hasActiveBox;			// is SelectBox active?

	RaycastHit _hit;

	Canvas _weaponCanvas;

	DebugComponent _debug;

    List<Vector3> _offsets;
    
    void Awake()
    {
	    _clickControls = new BaseControls();
    }

    /// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		_debug = GetComponent<DebugComponent>();

		_rightClickActions = new Dictionary<string, Action<Player, GameObject>>()
		{
			{ Tags.Crab, RightClickCrab },
			{ Tags.Castle, (x, y) => RightClickCastle(x, y.GetComponent<CastleController>()) },
			{ Tags.Ghost, RightClickGhost },
			{ Tags.Block, RightClickBlock },
			{ Tags.Gate, (x, y) => RightClickGate(y) },
			{ Tags.Catapult, RightClickSiege },
			{ Tags.Ballista, RightClickSiege },
			{ Tags.Wood, RightClickResource },
			{ Tags.Stone, RightClickResource },
			{ Tags.Armoury, RightClickArmoury },
			{ Tags.Beach, (x, y) => RightClickBeach(x) },
		};
		_playerInput = GetComponent<PlayerInput>();
		_select = _clickControls.Units.Select;
		_use = _clickControls.Units.Use;
		_hotkeys = _clickControls.HotKeys;
		
        _offsets = new List<Vector3>();
        for (int i = 0; i < MaxOffsets; i++) {
            _offsets.Add(GetOffset(i, OffsetRadius, 1, 1));
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

	#region Mouse click functions

	/// <summary>
	/// Processes the mouse click.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="gui">GUI script.</param>
	public void ProcessMouseClick(Player player, GUIController gui) 
	{ 
		_debug.Assert(player != null);
		_debug.Assert(gui != null);

		if (!gui.MouseOnGUI())
		{
			_playerInput.enabled = true;
			
			if (player.States.GetState("Building"))
			{
				ClickWhileBuilding(player);
			}
			else
			{
				// left click
				if (_select.WasPressedThisFrame())
				{
					CreateBoxSelection();
				}
				else if (_select.WasReleasedThisFrame())
				{
					SelectEntities(player, gui);
				}
				// right click
				else if (_use.WasReleasedThisFrame()) 
				{
					if (Physics.Raycast(Raycaster.GetRay(), out _hit))
					{
						RightClickTarget(player);
					}
				}

				DragBoxSelection();
			}
		}
		else
		{
			_playerInput.enabled = false;
		}
	}

	void ClickWhileBuilding(Player player)
	{
		GhostBuildingManager ghostManager = player.GhostManager;

		if (player.BuildingType == "Junction")
		{
			ClickWhileBuildingJunction(player);
		}
		else
		{
			if (_select.WasReleasedThisFrame() && ghostManager.CanBuild)
			{
				LeftClickWhileBuilding(player);
			}
			else if (_use.WasReleasedThisFrame())
			{
				RightClickWhileBuilding(player);
			}
		}
	}
	
	void ClickWhileBuildingJunction(Player player)
	{
		GhostBuildingManager ghostManager = player.GhostManager;
	    
		player.Deselect();
		player.States.SetState("Building", true);

		if (_select.WasReleasedThisFrame())
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
		else if (_use.WasReleasedThisFrame())
		{
			player.CancelBuilding();
		}
	}

	#endregion
	
	#region Left click functions

    // Process Left Click
    // ####################################################################################
    /// <summary>
    /// Left click occurred while building
    /// </summary>
    /// <param name="manager">Manager object.</param>
    /// <param name="player">Player script.</param>
    void LeftClickWhileBuilding(Player player)
    {
	    GhostBuildingManager ghostManager = player.GhostManager;

	    if (ghostManager.GhostBuilding)
	    {
		    GhostBuilder buildingGhost = ghostManager.GhostBuilding.GetComponent<GhostBuilder>();
		    buildingGhost.HasBuilder = player.HasSelected;

		    buildingGhost.Placed = true;
		    ghostManager.PlaceGhostBuilding();

		    player.States.SetState("Building", false);
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
    
    void ProcessClickWithNoDrag(Player player)
    {
	    _debug.LogMessage("Single-selected");

        _hit = Raycaster.ShootMouseRay();

        if (_hit.transform.CompareTag(Tags.Beach) && player.CanCommand)
        {
	        _debug.LogMessage("Hit Beach");
            if (player.SelectedList.Count > 1)
            {
                MoveSelectedCrabs(player);
            }
            else
            {
                if (IdUtility.IsCrab(player.Selected))
                {
                    player.Selected.GetComponent<CrabController>().StartNewMove(_hit.point);
                }
                else if (IdUtility.IsSiegeWeapon(player.Selected))
                {
                    player.Selected.GetComponent<SiegeController>().StartMove(_hit.point);
                }
            }
        }
        else
        {
            player.Deselect();
            AddSingleEntity(player);
            if (player.SelectedList.Count == 1)
            {
                player.Select(player.SelectedList[0]); 
            }
        }
    }

    // Spaces crabs out around the selected position
    void MoveSelectedCrabs(Player player)
    {
        Vector3 avgPosition = InfoTool.GetAveragePosition(player.SelectedList);
        float avgDistance = InfoTool.GetAverageDistance(player.SelectedList, avgPosition);
        int avgQuadrantAmount = player.SelectedList.Count / 4;
        List<GameObject> listA = new List<GameObject>();
        List<GameObject> listB = new List<GameObject>();
        List<GameObject> listC = new List<GameObject>();
        List<GameObject> listD = new List<GameObject>();

        foreach (GameObject selected in player.SelectedList)
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

    #endregion
	
    #region Right click functions

    // Process Right Click
    // ####################################################################################
    // Input: The player script.
    /// <summary>
    /// Right click occurred while building
    /// </summary>
    /// <param name="player">Player script.</param>
    void RightClickWhileBuilding(Player player)
	{
		GameObject buildClick = Raycaster.GetObjectAtRay();

		if (buildClick.CompareTag(Tags.Ghost)) 
		{
			buildClick.GetComponent<GhostBuilder>().Destroyed();
			Destroy(buildClick);
		}

		player.States.SetState("Building", false);
	}

	/// <summary>
	/// Right click occurred while not building.
	/// Indicates a command or deselects.
	/// </summary>
	/// <param name="player">Player script.</param>
	void RightClickTarget(Player player) {

		GameObject target = _hit.transform.gameObject;
		string targetName = _hit.transform.tag;

		_debug.LogMessage("Right clicked " + targetName + ".");

		if (player.CanCommand)
		{
			if (_rightClickActions.ContainsKey(target.tag))
			{
				_rightClickActions[target.tag].Invoke(player, target);
			}
			else
			{
				if (IdUtility.IsBuilding(target))
				{
					RightClickBuilding(player, target);
				}
				else if (IdUtility.IsWeapon(target) && IdUtility.IsCrab(player.Selected))
				{
					player.Selected.GetComponent<CrabController>().StartCollecting(target);
				}
			}
		}
	}

	/// <summary>
	/// Right clicks the beach.
	/// Deselects.
	/// </summary>
	/// <param name="player">Player script.</param>
	void RightClickBeach(Player player)
	{
		_debug.LogMessage("Deselected " + player.Selected.name + ".");

		if (player.CanCommand && player.States.GetState("Building"))
		{
			player.StartBuild(_hit.point);
		}
		else
        {
            player.Deselect();
        }
	}

	/// <summary>
	/// Right clicks castle.
	/// Crabs can be entered, given resources, captured, attacked, repaired, etc.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="castle">Castle script.</param>
	void RightClickCastle(Player player, CastleController castle)
	{
		_debug.Assert(castle);
		
        if (castle.GetTeam().OnTeam(player.GetTeam().team))
        {
	        Inventory inv;
	        
            // Evacuate castle
            if (player.Selected.CompareTag(Tags.Castle))
            {
                player.Selected.GetComponent<Enterable>().RemoveOccupant(); 
            }
            // Repair castle if friendly
            else if (player.States.GetState("Repairing") && player.CanCommand)
            {
                player.Selected.GetComponent<CrabController>().StartRepair(castle.gameObject);
                player.Selected.GetComponent<CrabController>().SetCrabs(player.SelectedList.Count);
                player.MoveMultiple(castle.transform.position);
            }
            // Unload resources if friendly
            else
            {
	            if (player.CanCommand && IdUtility.IsCrab(player.Selected))
	            {
		            inv = player.Selected.GetComponent<CrabController>().Inventory;
		            if (inv.Contains(Tags.Stone) || inv.Contains(Tags.Wood))
		            {
			            for (int i = 0; i < player.SelectedList.Count; i++)
			            {
				            if (IdUtility.IsCrab(player.SelectedList[i]))
				            {
					            player.SelectedList[i].GetComponent<CrabController>().StartUnloading(castle.gameObject);
				            }
			            }
		            }
		            else
		            {
			            player.Selected.GetComponent<CrabController>().StartEnter(castle.gameObject);
		            }
	            }
            }
        }
        else {
            // Attack castle if on enemy team
            if (player.States.GetState("Attacking"))
            {
                player.StartAttack(castle.gameObject);
            }
            // Capture castle if on enemy team
            else 
            {
                player.Selected.GetComponent<CrabController>().CaptureCastle(castle.gameObject);
                player.Selected.GetComponent<CrabController>().SetCrabs(player.SelectedList.Count);
                player.MoveMultiple(castle.transform.position);
            }
        }
	}

	/// <summary>
	/// Right clicks a miscellaneous building.
	/// Buildings can be entered, repaired, attacked, or evacuated.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="building">Target object.</param>
	public void RightClickBuilding(Player player, GameObject building) 
	{
        if (building.GetComponent<Team>().OnTeam(player.GetTeam().team))
        {
            if (player.States.GetState("Repairing"))
            {
                player.Selected.GetComponent<CrabController>().StartRepair(building);
                player.Selected.GetComponent<CrabController>().SetCrabs(player.SelectedList.Count);
                player.MoveMultiple(building.transform.position);
            }
            else if (player.States.GetState("Attacking"))
            {
	            player.Selected.GetComponent<CrabController>().StartDismantling(building);
            }
            else if (building.GetComponent<Enterable>())
            {
                if (player.Selected == building)
                {
                    player.Selected.GetComponent<Enterable>().RemoveOccupant();
                }
                else
                {
                    if (IdUtility.IsCrab(player.Selected))
                    {
                        player.Selected.GetComponent<CrabController>().StartEnter(building); 
                    }
                    else if (IdUtility.IsSiegeWeapon(player.Selected) && building.CompareTag(Tags.Tower))
                    {
                        player.Selected.GetComponent<SiegeController>().StartEnter(building); 
                    }
                }
            }
        }
	}

	/// <summary>
	/// Right clicks a resource.
	/// Always collects.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="target">Target object.</param>
	void RightClickResource(Player player, GameObject target) 
	{
		target.GetComponent<ResourceController>().SetCrabs(player.SelectedList);
		for (int i = 0; i < player.SelectedList.Count; i++) 
		{
            if (IdUtility.IsCrab(player.SelectedList[i]))
            {
                player.SelectedList[i].GetComponent<CrabController>().StartCollecting(target); 
            }
		}
	}

	/// <summary>
	/// Right clicks a crab.
	/// Recruits or attacks.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="target">Target object.</param>
	void RightClickCrab(Player player, GameObject target)
	{
		int hitTeam = target.GetComponent<Team>().team;

		if (player.States.GetState("Recruiting") && hitTeam == -1)
        {
            player.Selected.GetComponent<CrabController>().StartRecruiting(target);
        }
		else if (!player.GetTeam().OnTeam(hitTeam)) 
		{
			player.States.ClearStates();
			player.States.SetState("Attacking", true);
			player.StartAttack(target);
		}
	}

    /// <summary>
    /// Right click a ghost structure.
    /// Either deletes it or tells a crab to build it.
    /// </summary>
    /// <param name="player">Player script</param>
    /// <param name="target">Target object</param>
    void RightClickGhost(Player player, GameObject target)
    {
        if (player.CanCommand)
        {
            if (player.Selected.GetComponent<CrabController>() != null)
            {
                player.Selected.GetComponent<CrabController>().BuildFromGhost(target);
            }
        }
        else
        {
	        _debug.LogMessage("Target is not commandable.");
            if (target.GetComponent<Team>().OnTeam(player.GetTeam().team))
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
	/// <param name="player">Player script.</param>
	/// <param name="target">Target object.</param>
	void RightClickBlock(Player player, GameObject target) 
	{
		if (target.GetComponent<Team>().OnTeam(player.GetTeam().team)) 
		{
			if (player.States.GetState("Upgrading")) 
			{
				player.Selected.GetComponent<CrabController>().StartUpgrading(target);
				player.Selected.GetComponent<CrabController>().SetCrabs(player.SelectedList.Count);
				player.MoveMultiple(target.transform.position);
			}
			else if (player.States.GetState("Repairing"))
			{
				player.Selected.GetComponent<CrabController>().StartRepair(target);
				player.Selected.GetComponent<CrabController>().SetCrabs(player.SelectedList.Count);
				player.MoveMultiple(target.transform.position);
			}
            else if (player.States.GetState("Attacking"))
            {
	            player.Selected.GetComponent<CrabController>().StartDismantling(target);
            }
		}
        else
        {
            if (player.States.GetState("Attacking"))
            {
                player.StartAttack(target); 
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
	/// <param name="player">Player script.</param>
	/// <param name="target">Target object.</param>
	void RightClickSiege(Player player, GameObject target) 
	{
		if (target.GetComponent<Team>().OnTeam(player.GetComponent<Team>().team))
        {
            if (player.HasSelected)
            {
                player.Selected.GetComponent<CrabController>().StartEnter(target);
            }
            else
            {
                target.GetComponent<Enterable>().RemoveOccupant();
            }
        }
		else
        {
            player.StartAttack(target);
        }
	}

	/// <summary>
	/// Right click an armoury.
	/// Spawns menu for weapon.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="target">Target object.</param>
	void RightClickArmoury(Player player, GameObject target)
	{
		if (player.GetTeam().OnTeam(target.GetComponent<Team>().team))
		{
			if (IdUtility.IsCrab(player.Selected) && player.CanCommand)
			{
				// open menu to choose weapon
				// set armoury as current

				if (player.States.GetState("Attacking"))
				{
					player.Selected.GetComponent<CrabController>().StartDismantling(target);
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

					player.TargetedArmoury = target.GetComponent<HoldsWeapons>();
				}
			}
		}
	}
    #endregion

    #region Keyboard functions

    /// <summary>
    /// Gets the keyboard input.
    /// </summary>
    /// <param name="player">Player script.</param>
    public void GetKeyboardInput(Player player)
	{
		_debug.Assert(player);

		if (Keyboard.current.anyKey.IsPressed())
		{
			if (_hotkeys.Attack.IsPressed())
			{
				_debug.LogMessage("Click on something to attack.");
				player.States.ClearStates();
				player.States.SetState("Attacking", true);
			}
			if (_hotkeys.Build.IsPressed()) 
			{
				_debug.LogMessage("Start building castle.");
				player.States.ClearStates();
				player.States.SetState("Building", true);
				player.SetBuildingType(Tags.Castle);
				//player.setBuildMode(true);
			}
			if (_hotkeys.Capture.IsPressed()) 
			{
				_debug.LogMessage("Click on castle to capture.");
				player.States.SetState("Capturing", true);
			}
			if (_hotkeys.Enter.IsPressed()) 
			{
				_debug.LogMessage("Click on a castle or siege weapon to enter.");
				player.States.ClearStates();
				player.States.SetState("Entering", true);
			}
			if (_hotkeys.Repair.IsPressed()) 
			{
				_debug.LogMessage("Click an object to repair.");
				player.States.SetState("Repairing", true);
			}
			if (_hotkeys.Info.IsPressed()) 
			{
				for (int i = 0; i < player.SelectedList.Count; i++)
				{
					_debug.LogMessage(player.SelectedList[i].tag);
				}
			}
			if (_hotkeys.Pause.IsPressed()) 
			{
				player.Paused = !player.Paused;
				if (player.Paused)
				{
					Time.timeScale = 0.0f;
					GetComponent<GUIController>().PauseMenu.SetActive(true);
				}
				else
				{
					Time.timeScale = 1.0f;
					GetComponent<GUIController>().PauseMenu.SetActive(false);
				}
			}
			if (_hotkeys.Recruit.IsPressed()) 
			{
				_debug.LogMessage("Click on a neutral crab to recruit.");
				player.States.SetState("Recruiting", true);
			}
			if (_hotkeys.Upgrade.IsPressed()) 
			{
				_debug.LogMessage("Click on a block to upgrade.");
				player.States.SetState("Upgrading", true);
			}
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
    void AddToSelection(Player player, GameObject entity) 
	{
		_debug.Assert(entity);
		_debug.LogMessage("Added " + entity.tag);

		player.SelectedList.Add(entity);

		if (IdUtility.IsMoveable(entity))
			player.HaloList.Add(Instantiate(HaloCanvas));
	}
		
	/// <summary>
	/// Adds all within bounds.
	/// </summary>
	/// <param name="player">Player script.</param>
	void AddAllWithinBounds(Player player)
	{
		Camera mainCamera = Camera.main;
		Team playerTeam = player.GetComponent<Team>();
		Bounds bounds = SelectUtils.GetViewportBounds(Camera.main, _anchor, _outer);

		foreach(CrabController crab in FindObjectsOfType<CrabController>()) 
		{
			if (crab.GetComponent<Team>().OnTeam(playerTeam.team)) 
			{
				if (SelectUtils.IsWithinBounds(mainCamera, bounds, crab.transform.position))
					AddToSelection(player, crab.gameObject);
			}
		}

		foreach(SiegeController siege in FindObjectsOfType<SiegeController>()) 
		{
			if (siege.GetComponent<Enterable>().Occupied()) 
			{
				if (siege.GetComponent<Team>().OnTeam(playerTeam.team))
				{
					if (SelectUtils.IsWithinBounds(mainCamera, bounds, siege.transform.position))
						AddToSelection(player, siege.gameObject);
				}
			}
		}
	}

	/// <summary>
	/// Adds a single entity.
	/// </summary>
	/// <param name="player">Player script.</param>
	void AddSingleEntity(Player player) 
	{
		_debug.Assert(player);

		_hit = Raycaster.ShootMouseRay();
		GameObject entity = _hit.transform.gameObject;
		// entity exists and player hasn't selected already and entity isn't the beach.
		bool selectable = entity != null && !player.SelectedList.Contains(entity) && entity.tag != Tags.Beach;
		
		if (selectable)
		{
			//player.select(entity);
			AddToSelection(player, entity);
		}
	}

	/// <summary>
	/// Selects the entities within the bounds.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="gui">GUI script.</param>
	public void SelectEntities(Player player, GUIController gui) 
	{
		_debug.Assert(gui);
		_debug.Assert(player);

		_hasActiveBox = false;

		// if mouse hasn't moved
		if (Vector3.Distance(_anchor, _outer) < SingleClickMaxDist)
		{
            ProcessClickWithNoDrag(player);
		}
		else 
		{
			_debug.LogMessage("Multi-selected");
			
			player.Deselect();
			AddAllWithinBounds(player);

			if (player.SelectedList.Count == 1) 
			{
				_debug.LogMessage("Selected one object");
				player.Select(player.SelectedList[0]);
			}
			else if (player.SelectedList.Count > 1)
			{
				_debug.LogMessage("Selected multiple objects");
				player.SelectAll();
			}
		}

		gui.ClearBox();
	}

	/// <summary>
	/// Creates the box selection.
	/// </summary>
	public void CreateBoxSelection() 
	{
		_anchor = Mouse.current.position.ReadValue();
		_outer = Mouse.current.position.ReadValue();
		
		_hasActiveBox = true;

		GetComponent<GUIController>().StartSelectBox(_anchor);
	}

	/// <summary>
	/// Drags the box selection.
	/// </summary>
	public void DragBoxSelection() 
	{
		if (_hasActiveBox) 
		{
			_outer = Mouse.current.position.ReadValue();

			GetComponent<GUIController>().DragSelectBox(_outer);
		}
	}

    #endregion
    
    #region helper functions

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
