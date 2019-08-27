using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.AI;
//using UnityEditor;

/// <summary>
/// Input handler.
/// </summary>
[RequireComponent(typeof(DebugComponent))]
[RequireComponent(typeof(GUIController))]
public class InputController : MonoBehaviour {

    [Tooltip("The distance a mouse must be dragged to become a multi-select.")]
	public float singleClickMaxDist;
    public int maxOffsets;
    public float offsetRadius;

    // Can I select more than one unit?
    public bool multiSelect { get; set; }

    Vector3 anchor;				// Start point
	Vector3 outer;				// drag point
	bool hasActiveBox;			// is SelectBox active?

	RaycastHit hit;

	Canvas weaponCanvas;

	bool debug;

    List<Vector3> offsets;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		debug = GetComponent<DebugComponent>().debug;

        offsets = new List<Vector3>();
        for (int i = 0; i < maxOffsets; i++) {
            offsets.Add(getOffset(i, offsetRadius, 1, 1));
        }
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () 
	{
		multiSelect = Input.GetKey(KeyCode.LeftShift);
	}

	/// <summary>
	/// Processes the mouse click.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="gui">GUI script.</param>
	public void processMouseClick (Player player, GUIController gui) 
	{
		if (debug)
		{
			Debug.Assert(player);
			Debug.Assert(gui);
		}

		if (!gui.mouseOnGUI()) 
		{
			if (player.states.getState("Building"))
			{
				GhostBuildingManager manager = player.ghostManager;

				if (player.buildingType == "Junction")
				{
					player.deselect();
					player.states.setState("Building", true);

					if (Input.GetMouseButtonUp(0))
					{
						if (Raycaster.isPointingAt(Tags.Junction))
							manager.extendJunction();
						else if (manager.canBuild)
							manager.placeJunction();
					}
					else if (Input.GetMouseButtonUp(1))
					{
						manager.destroyGhostBuilding();
						manager.destroyWall();
						player.states.setState("Building", false);
						manager.buildingType = null;
						player.buildingType = null;
					}
				}
				else
				{
					if (Input.GetMouseButtonUp(0) && manager.canBuild)
						leftClickWhileBuilding(manager, player);
					else if (Input.GetMouseButtonUp(1))
						rightClickWhileBuilding(player);
				}
			}
			else
			{
				// left click
				if (Input.GetMouseButtonDown(0))
					CreateBoxSelection();
				else if (Input.GetMouseButtonUp(0))
					SelectEntities(player, gui);
				// right click
				else if (Input.GetMouseButtonUp(1)) 
				{
					if (Physics.Raycast(Raycaster.getRay(), out hit))
						rightClickTarget(player);
				}

				DragBoxSelection();
			}
		}
	}

	// Process Left Click
	// ####################################################################################
	/// <summary>
	/// Left click occurred while building
	/// </summary>
	/// <param name="manager">Manager object.</param>
	/// <param name="player">Player script.</param>
	void leftClickWhileBuilding(GhostBuildingManager manager, Player player)
	{
		if (manager.ghostBuilding)
		{
			manager.ghostBuilding.GetComponent<GhostBuilder>().hasBuilder = player.hasSelected;

			manager.ghostBuilding.GetComponent<GhostBuilder>().placed = true;
			manager.placeGhostBuilding();

			player.states.setState("Building", false);
		}
		else
		{
			hit = Raycaster.shootMouseRay();

			if (hit.collider.tag == Tags.Junction)
				manager.extendJunction();
		}
	}

    #region right click code

    // Process Right Click
    // ####################################################################################
    // Input: The player script.
    /// <summary>
    /// Right click occurred while building
    /// </summary>
    /// <param name="player">Player script.</param>
    void rightClickWhileBuilding(Player player)
	{
		GameObject buildClick = Raycaster.getObjectAtRay();

		if (buildClick.tag == Tags.Ghost) 
		{
			buildClick.GetComponent<GhostBuilder>().destroyed();
			Destroy(buildClick);
		}

		player.states.setState("Building", false);
	}

	/// <summary>
	/// Right click occurred while not building.
	/// Usually indicates a command or deselects.
	/// </summary>
	/// <param name="player">Player script.</param>
	void rightClickTarget (Player player) {

		GameObject target = hit.transform.gameObject;
		string targetName = hit.transform.tag;

		if (debug)
		{
			Debug.Assert(player);
			Debug.Log("Hit " + targetName + ".");
		}

		if(target.tag == Tags.Ghost)
		{
            if (player.canCommand)
            {
                player.selected.GetComponent<CrabController>().buildFromGhost(target);
            }
            else
            {
                if (target.GetComponent<Team>().team == GetComponent<Team>().team)
                {
                    target.GetComponent<GhostBuilder>().destroyed();
                    Destroy(target);
                }
            }
		}

        if (target.tag == Tags.Gate)
        {
            GateController controller = target.GetComponent<GateController>();
            if (controller.isOpen())
            {
                controller.closeGate();
                Debug.Log("Closing gate");
            }
            else
            {
                controller.openGate();
                Debug.Log("Opening gate");
            }
        }

		if (player.canCommand)
		{
			switch (target.tag) 
			{
			case Tags.Beach:
				rightClickBeach(player);
				break;
			case Tags.Castle:
				rightClickCastle(player, target.GetComponent<CastleController>());
				break;
			case Tags.Crab:
				rightClickCrab(player, target);
				break;
			case Tags.Block:
				rightClickBlock(player, target);
				break;
			case Tags.Catapult:
			case Tags.Ballista:
				rightClickSiege(player, target);
				break;
			case Tags.Wood:
			case Tags.Stone:
				rightClickResource(player, target);
				break;
			case Tags.Armoury:
				rightClickArmoury(player, target);
				break;
			default:
				if (IdUtility.isBuilding(target.tag))
					rightClickBuilding(player, target);
				else if (IdUtility.isWeapon(target.tag) && player.selected.tag == Tags.Crab)
					player.selected.GetComponent<CrabController>().startCollecting(target);
				break;
			}
		}
	}

	/// <summary>
	/// Right clicks the beach.
	/// Deselects.
	/// </summary>
	/// <param name="player">Player script.</param>
	void rightClickBeach (Player player)
	{
		if (debug)
			Debug.Log("Deselected crab");

		if (player.canCommand && player.states.getState("Building"))
		{
			player.selectedList[0].GetComponent<CrabController>().startBuild(player.buildingType, hit.point);
			player.selectedList[0].GetComponent<CrabController>().setCrabs(player.selectedList.Count);
			player.moveMultiple(hit.point);
			player.ghostManager.destroyGhostBuilding();
		}
		else
			player.deselect();
	}

	/// <summary>
	/// Right clicks castle.
	/// Crabs can be entered, given resources, captured, attacked, repaired, etc.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="castle">Castle script.</param>
	void rightClickCastle (Player player, CastleController castle) 
	{
		if (debug)
			Debug.Assert(castle);

		int castleTeam = castle.GetComponent<Team>().team;

        if (castleTeam == player.GetComponent<Team>().team)
        {
            // Evacuate castle
            if (player.selected.tag == Tags.Castle)
                player.selected.GetComponent<Enterable>().removeOccupant();
            // Repair castle if friendly
            else if (player.states.getState("Repairing") && player.canCommand)
            {
                player.selectedList[0].GetComponent<CrabController>().startRepair(castle.gameObject);
                player.selectedList[0].GetComponent<CrabController>().setCrabs(player.selectedList.Count);
                player.moveMultiple(castle.transform.position);
            }
            // Unload resources if friendly
            else
            {
                if (player.canCommand)
                {
                    if (player.selected.tag == Tags.Crab)
                    {
                        if (player.selected.GetComponent<CrabController>().inventory.contains(Tags.Stone) || player.selected.GetComponent<CrabController>().inventory.contains(Tags.Wood))
                        {
                            for (int i = 0; i < player.selectedList.Count; i++)
                            {
                                if (player.selectedList[i].tag == Tags.Crab)
                                    player.selectedList[i].GetComponent<CrabController>().startUnloading(castle.gameObject);
                            }
                        } 
                        else
                        {
                            player.selected.GetComponent<CrabController>().startEnter(castle.gameObject);
                        }
                    }
                }
            }
        }
        else {
            // Attack castle if on enemy team
            if (player.states.getState("Attacking"))
            {
                for (int i = 0; i < player.selectedList.Count; i++)
                {
                    if (player.selectedList[i].GetComponent<CrabController>())
                        player.selectedList[i].GetComponent<CrabController>().startAttack(castle.gameObject);
                    else if (player.selectedList[i].GetComponent<SiegeController>())
                        player.selectedList[i].GetComponent<SiegeController>().startAttack(castle.gameObject);
                }
            }
            // Capture castle if on enemy team
            else 
            {
                player.selectedList[0].GetComponent<CrabController>().captureCastle(castle.gameObject);
                player.selectedList[0].GetComponent<CrabController>().setCrabs(player.selectedList.Count);
                player.moveMultiple(castle.transform.position);
            }
        }
	}

	/// <summary>
	/// Right clicks a miscellaneous building.
	/// Buildings can be entered, repaired, attacked, or evacuated.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="target">Target object.</param>
	public void rightClickBuilding (Player player, GameObject target) 
	{
		int playerTeam = player.GetComponent<Team>().team;
		int hitTeam = target.GetComponent<Team>().team;

		bool enterCondition = hitTeam == playerTeam && target.GetComponent<Enterable>();

        if (playerTeam == hitTeam)
        {
            if (player.states.getState("Repairing"))
            {
                player.selectedList[0].GetComponent<CrabController>().startRepair(target);
                player.selectedList[0].GetComponent<CrabController>().setCrabs(player.selectedList.Count);
                player.moveMultiple(target.transform.position);
            }
            else if (player.states.getState("Attacking"))
            {
                player.selected.SendMessage("startDismantling", target, SendMessageOptions.DontRequireReceiver);
            }
            else if (target.GetComponent<Enterable>())
            {
                if (player.selected == target)
                {
                    player.selected.GetComponent<Enterable>().removeOccupant();
                }
                else
                {
                    if (player.selected.tag == Tags.Crab)
                        player.selected.GetComponent<CrabController>().startEnter(target);
                    else if (IdUtility.isSiegeWeapon(player.selected.tag) && target.tag == Tags.Crab)
                        player.selected.GetComponent<SiegeController>().startEnter(target);
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
	void rightClickResource (Player player, GameObject target) 
	{
		target.GetComponent<ResourceController>().setCrabs(player.selectedList);
		for(int i = 0; i < player.selectedList.Count; i++) 
		{
			if (player.selectedList[i].tag == Tags.Crab)
				player.selectedList[i].GetComponent<CrabController>().startCollecting(target);
		}
	}

	/// <summary>
	/// Right clicks a crab.
	/// Recruits or attacks.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="target">Target object.</param>
	void rightClickCrab (Player player, GameObject target)
	{
		int hitTeam = target.GetComponent<Team>().team;

		if (player.states.getState("Recruiting") && hitTeam == -1)
			player.selected.GetComponent<CrabController>().startRecruiting(target);
		else if (hitTeam != player.GetComponent<Team>().team) 
		{
			player.states.clearStates();
			player.states.setState("Attacking", true);
			for(int i = 0; i < player.selectedList.Count; i++)
				player.selectedList[i].SendMessage("startAttack", target, SendMessageOptions.DontRequireReceiver);
		}
	}

	/// <summary>
	/// Right click a block.
	/// Attack, upgrade, or repair.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="target">Target object.</param>
	void rightClickBlock (Player player, GameObject target) 
	{
		if (target.GetComponent<Team>().team == player.GetComponent<Team>().team) 
		{
			if (player.states.getState("Upgrading")) 
			{
				player.selectedList[0].GetComponent<CrabController>().startUpgrading(target);
				player.selectedList[0].GetComponent<CrabController>().setCrabs(player.selectedList.Count);
				player.moveMultiple(target.transform.position);
			}
			else if (player.states.getState("Repairing"))
			{
				player.selectedList[0].GetComponent<CrabController>().startRepair(target);
				player.selectedList[0].GetComponent<CrabController>().setCrabs(player.selectedList.Count);
				player.moveMultiple(target.transform.position);
			}
            else if (player.states.getState("Attacking"))
                player.selected.SendMessage("startDismantling", target, SendMessageOptions.DontRequireReceiver);
		}
        else {
            if (player.states.getState("Attacking"))
                player.selected.SendMessage("startAttack", target, SendMessageOptions.DontRequireReceiver);
        }
	}

	/// <summary>
	/// Right click a siege weapon.
	/// Enter or attack.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="target">Target object.</param>
	void rightClickSiege (Player player, GameObject target) 
	{
		if (target.GetComponent<Team>().team == player.GetComponent<Team>().team)
        {
            if (player.hasSelected)
            {
                player.selected.GetComponent<CrabController>().startEnter(target);
            }
            else
            {
                target.GetComponent<Enterable>().removeOccupant();
            }
        }
		else if (target.GetComponent<Team>().team != player.GetComponent<Team>().team)
        {
            player.selected.SendMessage("startAttack", target, SendMessageOptions.DontRequireReceiver);
        }
	}

	/// <summary>
	/// Right click an armoury.
	/// Spawns menu for weapon.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="target">Target object.</param>
	void rightClickArmoury (Player player, GameObject target)
	{
        if (player.GetComponent<Team>().team == target.GetComponent<Team>().team)
        {
            if (player.selected.tag == Tags.Crab && player.canCommand)
            {
                // open menu to choose weapon
                // set armoury as current

                if (player.states.getState("Attacking"))
                {
                    player.selected.SendMessage("startDismantling", target, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    if (!weaponCanvas)
                        weaponCanvas = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/GUI/WeaponBuildingCanvas")).GetComponent<Canvas>();
                    else
                        weaponCanvas.gameObject.SetActive(true);

                    if (debug)
                        Debug.Log("Set up weapon canvas");

                    Button[] buttons = weaponCanvas.GetComponentsInChildren<Button>();
                    for (int i = 0; i < buttons.Length; i++)
                    {
                        if (buttons[i].name == "SpearButton")
                            buttons[i].onClick.AddListener(() => {
                                GetComponent<Player>().takeWeapon(Tags.Spear);
                                weaponCanvas.gameObject.SetActive(false);
                            });
                        if (buttons[i].name == "HammerButton")
                            buttons[i].onClick.AddListener(() => {
                                GetComponent<Player>().takeWeapon(Tags.Hammer);
                                weaponCanvas.gameObject.SetActive(false);
                            });
                        if (buttons[i].name == "BowButton")
                            buttons[i].onClick.AddListener(() => {
                                GetComponent<Player>().takeWeapon(Tags.Bow);
                                weaponCanvas.gameObject.SetActive(false);
                            });
                        if (buttons[i].name == "ShieldButton")
                            buttons[i].onClick.AddListener(() => {
                                GetComponent<Player>().takeWeapon(Tags.Shield);
                                weaponCanvas.gameObject.SetActive(false);
                            });
                    }

                    player.targetedArmoury = target;
                }
            }
        }

		
	}
    #endregion

    #region keyboard input

    /// <summary>
    /// Gets the keyboard input.
    /// </summary>
    /// <param name="player">Player script.</param>
    public void getKeyboardInput (Player player)
	{
		if (debug)
			Debug.Assert(player);

		if (Input.anyKeyDown)
		{
			if (Input.GetKeyDown(KeyCode.A)) 
			{
				if (debug)
					Debug.Log("Click on something to attack.");
				player.states.clearStates();
				player.states.setState("Attacking", true);
			}
			if (Input.GetKeyDown(KeyCode.B)) 
			{
				if (debug)
					Debug.Log("Start building castle.");
				player.states.clearStates();
				player.states.setState("Building", true);
				player.setBuildingType(Tags.Castle);
				//player.setBuildMode(true);
			}
			if (Input.GetKeyDown(KeyCode.C)) 
			{
				if (debug)
					Debug.Log("Click on castle to capture.");
				player.states.setState("Capturing", true);
			}
			if (Input.GetKeyDown(KeyCode.E)) 
			{
				if (debug)
					Debug.Log("Click on a castle or siege weapon to enter.");
				player.states.clearStates();
				player.states.setState("Entering", true);
			}
			if (Input.GetKeyDown(KeyCode.F)) 
			{
				if (debug)
					Debug.Log("Click an object to repair.");
				player.states.setState("Repairing", true);
			}
			if (Input.GetKeyDown(KeyCode.I)) 
			{
				if (debug)
				{
					for(int i = 0; i < player.selectedList.Count; i++)
						Debug.Log(player.selectedList[i].tag);
				}
			}
			if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)) 
			{
				player.paused = !player.paused;
				if(player.paused)
				{
					Time.timeScale = 0.0f;
					GetComponent<GUIController>().pauseMenu.SetActive(true);
				}
				else
				{
					Time.timeScale = 1.0f;
					GetComponent<GUIController>().pauseMenu.SetActive(false);
				}
			}
			if (Input.GetKeyDown(KeyCode.R)) 
			{
				if (debug)
					Debug.Log("Click on a neutral crab to recruit.");
				player.states.setState("Recruiting", true);
			}
			if (Input.GetKeyDown(KeyCode.U)) 
			{
				if (debug)
					Debug.Log("Click on a block to upgrade.");
				player.states.setState("Upgrading", true);
			}
		}
	}
    #endregion

    #region selection code

    // box selection
    // ##################################################################################################################################################
    /// <summary>
    /// Adds to selection.
    /// </summary>
    /// <param name="player">Player script.</param>
    /// <param name="entity">Entity object.</param>
    void AddToSelection (Player player, GameObject entity) 
	{
		if (debug)
		{
			Debug.Assert(entity);
			Debug.Log("Added " + entity.tag);
		}

		player.selectedList.Add(entity);

		if (IdUtility.isMoveable(entity.tag))
			player.haloList.Add(Instantiate(Resources.Load<GameObject>("Prefabs/GUI/HaloCanvas")));
	}
		
	/// <summary>
	/// Adds all within bounds.
	/// </summary>
	/// <param name="player">Player script.</param>
	void AddAllWithinBounds (Player player) 
	{
		Bounds bounds = SelectUtils.GetViewportBounds(Camera.main, anchor, outer);

		foreach(CrabController crab in FindObjectsOfType<CrabController>()) 
		{
			if (crab.GetComponent<Team>().team == GetComponent<Team>().team) 
			{
				if (SelectUtils.IsWithinBounds(Camera.main, bounds, crab.transform.position))
					AddToSelection(player, crab.gameObject);
			}
		}

		foreach(SiegeController siege in FindObjectsOfType<SiegeController>()) 
		{
			if (siege.GetComponent<Enterable>().occupied()) 
			{
				if (siege.GetComponent<Team>().team == GetComponent<Team>().team)
				{
					if (SelectUtils.IsWithinBounds(Camera.main, bounds, siege.transform.position))
						AddToSelection(player, siege.gameObject);
				}
			}
		}
	}

	/// <summary>
	/// Adds a single entity.
	/// </summary>
	/// <param name="player">Player script.</param>
	void AddSingleEntity (Player player) 
	{
		if (debug)
			Debug.Assert(player);

		hit = Raycaster.shootMouseRay();
		GameObject entity = hit.transform.gameObject;
		// entity exists and player hasn't selected already and entity isn't the beach.
		bool selectable = entity != null && !player.selectedList.Contains(entity) && entity.tag != Tags.Beach;

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
	public void SelectEntities (Player player, GUIController gui) 
	{
		if (debug)
		{
			Debug.Assert(gui);
			Debug.Assert(player);
		}

		hasActiveBox = false;

		// if mouse hasn't moved
		if (Vector3.Distance (anchor, outer) < singleClickMaxDist)
		{
            processClickWithNoDrag(player);
		}
		else 
		{
			if (debug)
				Debug.Log("Multi-selected");
			
			player.deselect();
			AddAllWithinBounds(player);

			if (player.selectedList.Count == 1) 
			{
				if (debug)
					Debug.Log("Selected one object");
				player.select(player.selectedList[0]);
			}
			else if (player.selectedList.Count > 1)
			{
				if (debug)
					Debug.Log("Selected multiple objects");
				player.selectAll();
			}
		}

		gui.clearBox();
	}

	/// <summary>
	/// Creates the box selection.
	/// </summary>
	public void CreateBoxSelection () 
	{
		anchor = Input.mousePosition;
		outer = Input.mousePosition;

		hasActiveBox = true;

		GetComponent<GUIController>().StartSelectBox(anchor);
	}

	/// <summary>
	/// Drags the box selection.
	/// </summary>
	public void DragBoxSelection () 
	{
		if (hasActiveBox) 
		{
			outer = Input.mousePosition;

			GetComponent<GUIController>().DragSelectBox(outer);
		}
	}

    #endregion

    #region left click code

    void processClickWithNoDrag(Player player)
    {
        if (debug)
            Debug.Log("Single-selected");

        hit = Raycaster.shootMouseRay();

        if (hit.transform.tag == Tags.Beach && player.canCommand)
        {
            if (debug)
                Debug.Log("Hit Beach");
            if (player.selectedList.Count > 1)
            {
                moveSelectedCrabs(player);
            }
            else
            {
                if (player.selected.tag == Tags.Crab)
                {
                    player.selected.GetComponent<CrabController>().startNewMove(hit.point);
                }
                else if (IdUtility.isSiegeWeapon(player.selected.tag))
                {
                    player.selected.GetComponent<SiegeController>().startMove(hit.point);
                }
            }
        }
        else
        {
            player.deselect();
            AddSingleEntity(player);
            if (player.selectedList.Count == 1)
                player.select(player.selectedList[0]);
        }
    }

    void moveSelectedCrabs(Player player)
    {
        Vector3 avgPosition = InfoTool.getAveragePosition(player.selectedList);
        float avgDistance = InfoTool.getAverageDistance(player.selectedList, avgPosition);
        int avgQuadrantAmount = player.selectedList.Count / 4;
        List<GameObject> listA = new List<GameObject>();
        List<GameObject> listB = new List<GameObject>();
        List<GameObject> listC = new List<GameObject>();
        List<GameObject> listD = new List<GameObject>();

        DebugTools.DrawCircle(hit.point, avgDistance, 1000);

        foreach (GameObject selected in player.selectedList)
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

        sortByPosition(avgPosition, ref listA);
        sortByPosition(avgPosition, ref listB);
        sortByPosition(avgPosition, ref listC);
        sortByPosition(avgPosition, ref listD);

        balanceQuadrants(ref listA, ref listB, ref listC, ref listD, avgQuadrantAmount);

        moveSelected(listA, 1, 1);
        moveSelected(listB, 1, -1);
        moveSelected(listC, -1, -1);
        moveSelected(listD, -1, 1);
    }

    // Makes sure all quadrants have as close to the same amount of crabs as possible.
    void balanceQuadrants(ref List<GameObject> listA, ref List<GameObject> listB, ref List<GameObject> listC, ref List<GameObject> listD, int avgAmount)
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
                        while(lists[i].Count > avgAmount && lists[j].Count < avgAmount)
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
    void moveSelected(List<GameObject> crabs, int xSign, int zSign)
    {
        for (int i = 0; i < crabs.Count; i++)
        {
            Vector3 offset = new Vector3(xSign * offsets[i].x, offsets[i].y, zSign * offsets[i].z);

            if (crabs[i].tag == Tags.Crab)
                crabs[i].GetComponent<CrabController>().actionStates.clearStates();
            if (crabs[i].tag == Tags.Crab)
            {
                crabs[i].GetComponent<CrabController>().startNewMove(hit.point + offset);
            }
            else if (IdUtility.isSiegeWeapon(crabs[i].tag))
            {
                crabs[i].GetComponent<SiegeController>().startMove(hit.point + offset);
            }
        }
    }

    #endregion

    #region helper functions

    // Sorts by distance to the hit point of the mouse click.
    void sortByPosition(Vector3 position, ref List<GameObject> objects)
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

    Vector3 getOffset(int pos, float radius, int xSign, int zSign) 
    {
        float diameter = Mathf.Pow(radius, 2);
        int maxDiameterMultiple = 1;

        float x = xSign * radius;
        float y = 0.0f;
        float z = zSign * radius;

        if(pos == 0)
        {
            Debug.Log("Crab moving to (" + x + " " + y + " " + z + ")");
            return new Vector3(x, y, z);
        } 
        else 
        {
            int count = 1;

            while (count <= pos) {
                for (int i = 0; i <= maxDiameterMultiple; i++) 
                {
                    x = xSign * radius + (diameter * i);
                    z = zSign * radius + (diameter * maxDiameterMultiple);

                    if (count == pos)
                    {
                        Debug.Log("Crab moving to (" + x + " " + y + " " + z + ")");
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
                        Debug.Log("Crab moving to (" + x + " " + y + " " + z + ")");
                        return new Vector3(x, y, z);
                    }

                    count++;
                }
                maxDiameterMultiple++;
            }
            return new Vector3();
        }
    }
    #endregion
}
