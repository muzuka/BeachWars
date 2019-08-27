using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text.RegularExpressions;

/// <summary>
/// GUI controller.
/// Controls main gui and it's content
/// </summary>
[RequireComponent(typeof(Player))]
[RequireComponent(typeof(DebugComponent))]
public class GUIController : MonoBehaviour {

	[Tooltip("The root of the GUI")]
	public GameObject MainGUI;		// public reference to main GUI

    [Tooltip("Reference to Toggle that indicates whether mouse is on gui or not.")]
    public Toggle onGUI;

    #region UI variables

    // Multi-Select UI elements
    RectTransform multiUITransform;

	public RawImage mainProfile { get; set; }

	public Slider mainSlider { get; set; }

	public Button mainSelectButton { get; set; }

	public RawImage[] miniProfiles { get; set; }


	// Single-Select UI elements
	RectTransform singleUITransform;

	public Text labelText { get; set; }

	public Text stoneCount { get; set; }

	public Text woodCount { get; set; }

	public Text levelText { get; set; }

	public Text crabLevelText { get; set; }

	public Slider healthSlider { get; set; }

	public Slider expSlider { get; set; }

	public RawImage selectedImage { get; set; }

	public RawImage invSlot1 { get; set; }

	public RawImage invSlot2 { get; set; }

	public RawImage invSlot3 { get; set; }

	public Image actionView { get; set; }

	public Button craftButton { get; set; }

	public Button subCraftButton { get; set; }

	public GameObject pauseMenu { get; set; }

	public GameObject saveMenu { get; set; }

	public GameObject winMenu { get; set; }

	public GameObject saveConfirmMenu { get; set; }

	public GameObject towerPanel { get; set; }

	public GameObject wallPanel { get; set; }

	public GameObject selectBox { get; set; }

	public Text spearText { get; set; }

	public Text hammerText { get; set; }

	public Text bowText { get; set; }

	public Text shieldText { get; set; }

    public Button dismantleButton { get; set; }

	RectTransform selectBoxTransform;

    #endregion

    Vector2 anchor;

	UnityAction gateButton;
	UnityAction towerButton;
	UnityAction junctionTowerButton;

	const string EmptyString = "Nothing";

	int miniProfileCount = 16;

	bool debug;

	/// <summary>
	/// Initializes the UI.
	/// </summary>
	public void startUI ()
	{
		debug = GetComponent<DebugComponent>().debug;

		connectGUI();

		if (!GUIHookedIn())
			Debug.Log("GUI isn't completely connected.");

		setActiveGUIComponents("none");
		
		selectBoxTransform = selectBox.GetComponent<RectTransform>();
        clearBox();

		selectedImage.texture = null;
		craftButton.gameObject.SetActive(false);
		subCraftButton.gameObject.SetActive(false);

		labelText.text = EmptyString;
		stoneCount.text = "";
		woodCount.text = "";

		gateButton = () => 
		{
			GetComponent<Player>().selected.GetComponent<BlockController>().convertTo("Gate");
			wallPanel.SetActive(false);
		};

		towerButton = () =>
		{
			GetComponent<Player>().selected.GetComponent<BlockController>().convertTo("Tower");
			wallPanel.SetActive(false);
		};

		junctionTowerButton = () =>
		{
			GetComponent<Player>().selected.GetComponent<JunctionController>().convertToTower();			  
			towerPanel.SetActive(false);
		};
	}

	/// <summary>
	/// Updates the UI.
	/// </summary>
	/// <param name="player">Player script.</param>
	public void updateUI (Player player)
	{
		if (debug)
			Debug.Assert(player);

		if (player.hasSelected)
		{
			multiUITransform.gameObject.SetActive(player.multiSelect);
			singleUITransform.gameObject.SetActive(!player.multiSelect);

			if (!player.multiSelect)
				singleUIUpdate(player);
			else
				multiUIUpdate(player);
		}
	}

	/// <summary>
	/// Updates UI when single crab is selected.
	/// </summary>
	/// <param name="player">Player script.</param>
	void singleUIUpdate (Player player) 
	{
		if (debug)
		{
			Debug.Assert(player);
			Debug.Assert(player.selected);
		}

		player.selected.SendMessage("updateUI", this, SendMessageOptions.DontRequireReceiver);

		getActionViewController().setButtons(player);
	}

	/// <summary>
	/// Updates UI when multiple crabs are selected.
	/// </summary>
	/// <param name="player">Player.</param>
	void multiUIUpdate (Player player)
	{
		if (debug)
		{
			Debug.Assert(player);
			Debug.Assert(player.selectedList.Count >= 1);
		}

		if (player.selectedList.Count <= miniProfiles.Length) 
		{
			for (int i = 0; i < player.selectedList.Count; i++)
				player.selectedList [i].GetComponent<Attackable>().setHealth(miniProfiles [i].GetComponentInChildren<Slider>());
		}

		getActionViewController().setButtons(player);
	}

	/// <summary>
	/// Called when deselection occurs.
	/// </summary>
	public void deselect ()
	{
		getActionViewController().deactivateButtons();
		deactivateGUIComponents();
		selectedImage.GetComponent<RawImage>().texture = null;
		labelText.text = EmptyString;
		stoneCount.text = "";
		woodCount.text = "";
	}

	/// <summary>
	/// Are all GUI components instantiated?
	/// </summary>
	/// <returns><c>true</c>, if GUI is hooked in, <c>false</c> otherwise.</returns>
	public bool GUIHookedIn ()
	{
		bool count = (stoneCount && woodCount);
		bool invSlots = (invSlot1 && invSlot2 && invSlot3);
		bool buttons = (craftButton && subCraftButton && dismantleButton);
		bool multiSelect = areAllProfilesHookedIn();
		bool uiTransforms = (multiUITransform && singleUITransform);
		bool menus = (saveMenu && saveConfirmMenu && winMenu && pauseMenu);

		if (!labelText)
		{
			if (debug)
				Debug.Log("LabelText isn't hooked up!");
			return false;
		}
		if (!count)
		{
			if (debug)
				Debug.Log("Stone or wood count isn't hooked up!");
			return false;
		}
		if (!healthSlider)
		{
			if (debug)
				Debug.Log("Health slider isn't hooked up!");
			return false;
		}
		if (!selectedImage)
		{
			if (debug)
				Debug.Log("Selected image isn't hooked up!");
			return false;
		}
		if (!invSlots)
		{
			if (debug)
				Debug.Log("One of the inventory slots isn't hooked up!");
			return false;
		}
		if (!actionView)
		{
			if (debug)
				Debug.Log("stone or wood count isn't hooked up!");
			return false;
		}
		if (!buttons)
		{
			if (debug)
				Debug.Log("A button isn't hooked up!");
			return false;
		}
		if (!selectBox)
		{
			if (debug)
				Debug.Log("Select box isn't hooked up!");
			return false;
		}
		if (!multiSelect)
		{
			if (debug)
				Debug.Log("A multi-select element isn't hooked up!");
			return false;
		}
		if (!uiTransforms)
		{
			if (debug)
				Debug.Log("A transform isn't hooked up!");
			return false;
		}
		if(!menus)
		{
			if(debug)
				Debug.Log("A menu isn't hooked up!");
			return false;
		}

		return true;
	}

	bool areAllProfilesHookedIn () 
	{
		for (int i = 0; i < miniProfileCount; i++)
        {
			if (miniProfiles[i] == null)
			{
				Debug.Log ("Profile " + i + " isn't hooked up");
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Deactivates the GUI components.
	/// </summary>
	public void deactivateGUIComponents ()
	{
		labelText.gameObject.SetActive(false);
		healthSlider.gameObject.SetActive(false);
		selectedImage.gameObject.SetActive(false);
		invSlot1.gameObject.SetActive(false);
		invSlot2.gameObject.SetActive(false);
		invSlot3.gameObject.SetActive(false);
		woodCount.gameObject.SetActive(false);
		stoneCount.gameObject.SetActive(false);
		levelText.gameObject.SetActive(false);
		craftButton.gameObject.SetActive(false);
		subCraftButton.gameObject.SetActive(false);
		crabLevelText.gameObject.SetActive(false);
		expSlider.gameObject.SetActive(false);
		spearText.gameObject.SetActive(false);
		hammerText.gameObject.SetActive(false);
		bowText.gameObject.SetActive(false);
		shieldText.gameObject.SetActive(false);
		towerPanel.gameObject.SetActive(false);
		wallPanel.gameObject.SetActive(false);
        dismantleButton.gameObject.SetActive(false);

		for(int i = 0; i < miniProfiles.Length; i++)
			miniProfiles[i].gameObject.SetActive(false);
	}

	/// <summary>
	/// Sets the active GUI components based on tag.
	/// </summary>
	/// <param name="tag">Tag.</param>
	public void setActiveGUIComponents (string tag)
	{
		Button[] wallButtons = wallPanel.GetComponentsInChildren<Button>();
		Button[] towerButtons = towerPanel.GetComponentsInChildren<Button>();

		deactivateGUIComponents();

        if (tag != "none")
        {
            labelText.gameObject.SetActive(true);
            selectedImage.gameObject.SetActive(true);
        }

		switch (tag) 
		{
		case Tags.Crab:
			healthSlider.gameObject.SetActive(true);
			invSlot1.gameObject.SetActive(true);
			invSlot2.gameObject.SetActive(true);
			invSlot3.gameObject.SetActive(true);
			crabLevelText.gameObject.SetActive(true);
			expSlider.gameObject.SetActive(true);
			break;
		case Tags.Castle:
			healthSlider.gameObject.SetActive(true);
			levelText.gameObject.SetActive(true);
			woodCount.gameObject.SetActive(true);
			stoneCount.gameObject.SetActive(true);
			break;
		case Tags.Ghost:
			woodCount.gameObject.SetActive(true);
			stoneCount.gameObject.SetActive(true);
			break;
		case Tags.Block:
			wallPanel.SetActive(true);
            dismantleButton.gameObject.SetActive(true);
			for (int i = 0; i < wallButtons.Length; i++)
			{
				if (wallButtons[i].name == "GateButton")
					wallButtons[i].onClick.AddListener(gateButton);
				if (wallButtons[i].name == "TowerButton")
					wallButtons[i].onClick.AddListener(towerButton);
			}
			break;
		case Tags.Junction:
			towerPanel.SetActive(true);
            dismantleButton.gameObject.SetActive(true);
			for (int i = 0; i < towerButtons.Length; i++)
			{
				if (towerButtons[i].name == "TowerButton")
					towerButtons[i].onClick.AddListener(junctionTowerButton);
			}
			break;
            case Tags.Armoury:
		case Tags.Catapult:
		case Tags.Ballista:
		case Tags.Tower:
		case Tags.Workshop:
		case Tags.Nest:
			if (tag == Tags.Armoury)
			{
				spearText.gameObject.SetActive(true);
				hammerText.gameObject.SetActive(true);
				bowText.gameObject.SetActive(true);
				shieldText.gameObject.SetActive(true);
			}
			healthSlider.gameObject.SetActive(true);
            dismantleButton.gameObject.SetActive(true);
			break;
		case "multi":
			if (debug)
				Debug.Log("Activated objects");

			Player player = GetComponent<Player>();

			if (player.selectedList.Count > miniProfiles.Length)
			{
				for (int i = 0; i < miniProfiles.Length; i++)
					miniProfiles [i].gameObject.SetActive (true);
			} else 
			{
				for (int i = 0; i < player.selectedList.Count; i++)
					miniProfiles [i].gameObject.SetActive (true);
			}

			break;
		}
	}

	/// <summary>
	/// Sets the UI for multiple selected units.
	/// </summary>
	public void setMultiUI ()
	{
		Player player = GetComponent<Player>();

		for(int i = 0; i < player.selectedList.Count; i++)
		{
            if (i >= miniProfileCount)
            {
                break;
            }

			Debug.Log(i);
			miniProfiles[i].texture = Resources.Load<Texture>("Textures/" + player.selectedList[i].tag);

			player.selectedList[i].GetComponent<Attackable>().setHealth(miniProfiles[i].GetComponentInChildren<Slider>());
		}
	}

	/// <summary>
	/// Sets the profile button.
	/// </summary>
	/// <param name="pos">Position.</param>
	public void setProfileButton (int pos)
	{
		Player player = GetComponent<Player>();
		GameObject crab = player.selectedList[pos];

		if (debug)
			Debug.Log(player.selectedList.Count + " trying to access " + pos);
		
		player.deselect();
		player.selectedList.Add(crab);
		player.haloList.Add(Instantiate(Resources.Load<GameObject>("Prefabs/GUI/HaloCanvas")));
		player.select(crab);
	}

	/// <summary>
	/// Connects the GUI components to the proper references.
	/// </summary>
	public void connectGUI ()
	{
		if (!MainGUI)
		{
			if (debug)
				Debug.Log("Connect the GUI!");
			return;
		}

		miniProfiles = new RawImage[miniProfileCount];
		RectTransform[] menus = MainGUI.GetComponentsInChildren<RectTransform>();
		Regex regexObject = new Regex ("(\\d\\d*)");


		for(int i = 0; i < menus.Length; i++)
		{
			GameObject menuItem = menus[i].gameObject;
			switch (menuItem.name) {
			case "Label Text":
				if (menuItem.GetComponent<Text>())
					labelText = menuItem.GetComponent<Text>();
				break;
			case "Stone Text":
				if (menuItem.GetComponent<Text>())
					stoneCount = menuItem.GetComponent<Text>();
				break;
			case "Wood Text":
				if (menuItem.GetComponent<Text>())
					woodCount = menuItem.GetComponent<Text>();
				break;
			case "Level Text":
				if (menuItem.GetComponent<Text>())
					levelText = menuItem.GetComponent<Text>();
				break;
			case "Crab Level Text":
				if (menuItem.GetComponent<Text>())
					crabLevelText = menuItem.GetComponent<Text>();
				break;
			case "Health Slider":
				if (menuItem.GetComponent<Slider>())
					healthSlider = menuItem.GetComponent<Slider>();
				break;
			case "Exp Slider":
				if (menuItem.GetComponent<Slider>())
					expSlider = menuItem.GetComponent<Slider>();
				break;
			case "SelectedImage":
				if (menuItem.GetComponent<RawImage>())
					selectedImage = menuItem.GetComponent<RawImage>();
				break;
			case "InventorySlot1":
				if (menuItem.GetComponent<RawImage>())
					invSlot1 = menuItem.GetComponent<RawImage>();
				break;
			case "InventorySlot2":
				if (menuItem.GetComponent<RawImage>())
					invSlot2 = menuItem.GetComponent<RawImage>();
				break;
			case "InventorySlot3":
				if (menuItem.GetComponent<RawImage>())
					invSlot3 = menuItem.GetComponent<RawImage>();
				break;
			case "MainCraftButton":
				if (menuItem.GetComponent<Button>())
					craftButton = menuItem.GetComponent<Button>();
				break;
			case "SubCraftButton":
				if (menuItem.GetComponent<Button>())
					subCraftButton = menuItem.GetComponent<Button>();
				break;
            case "DismantleButton":
                if (menuItem.GetComponent<Button>())
                    dismantleButton = menuItem.GetComponent<Button>();
                break;
			case "Unit Action View":
				if (menuItem.GetComponent<Image>())
					actionView = menuItem.GetComponent<Image>();
				break;
			case "Spear Text":
				if (menuItem.GetComponent<Text>())
					spearText = menuItem.GetComponent<Text>();
				break;
			case "Hammer Text":
				if (menuItem.GetComponent<Text>())
					hammerText = menuItem.GetComponent<Text>();
				break;
			case "Bow Text":
				if (menuItem.GetComponent<Text>())
					bowText = menuItem.GetComponent<Text>();
				break;
			case "Shield Text":
				if (menuItem.GetComponent<Text>())
					shieldText = menuItem.GetComponent<Text>();
				break;
			case "Pause Menu":
				pauseMenu = menuItem;
				pauseMenu.SetActive(false);
				break;
			case "Save Menu":
				saveMenu = menuItem;
				saveMenu.SetActive(false);
				break;
			case "Win Menu":
				winMenu = menuItem;
				winMenu.SetActive(false);
				break;
			case "Confirmation Menu":
				saveConfirmMenu = menuItem;
				saveConfirmMenu.SetActive(false);
				break;
			case "Tower Panel":
				towerPanel = menuItem;
				towerPanel.SetActive(false);
				break;
			case "Wall Panel":
				wallPanel = menuItem;
				wallPanel.SetActive(false);
				break;
			case "SelectBox":
				selectBox = menuItem;
				selectBoxTransform = selectBox.GetComponent<RectTransform>();
				break;
			case "MultiSelectUI":
				multiUITransform = menus[i];
				break;
			case "SingleSelectUI":
				singleUITransform = menus[i];
				break;
			case "MainProfile":
				if (menuItem.GetComponent<RawImage>())
					mainProfile = menuItem.GetComponent<RawImage>();
				if (menuItem.GetComponent<Button>())
					mainSelectButton = menuItem.GetComponent<Button>();
				break;
			case "MainSlider":
				if (menuItem.GetComponent<Slider>())
					mainSlider = menuItem.GetComponent<Slider>();
				break;
			default:
				string menuName = menuItem.name;
				if (menuName.StartsWith("MiniProfile"))
				{
					if (regexObject.IsMatch (menuName))
					{
						Match match = regexObject.Match (menuName);
						miniProfiles[int.Parse(match.Captures[0].Value)] = menuItem.GetComponent<RawImage>();
					}
					else
						miniProfiles[0] = menuItem.gameObject.GetComponent<RawImage>();
				}
				break;
			}
		}
	}

	/// <summary>
	/// Sets the label text.
	/// </summary>
	/// <param name="selected">Selected object.</param>
	public void setLabel (GameObject selected)
	{
		if (debug)
			Debug.Assert(selected);

		if (selected.tag == Tags.Crab)
			labelText.text = selected.GetComponent<CrabController>().type.ToString().ToLower() + " crab";
		else if (selected.tag == Tags.Ghost)
			labelText.text = selected.tag + " " + selected.GetComponent<GhostBuilder>().original.tag;
		else
			labelText.text = selected.tag;

		if (!GetComponent<Player>().canCommand && !IdUtility.isResource(selected.tag))
			labelText.text = "Enemy " + labelText.text;
	}

	/// <summary>
	/// Gets the action view controller.
	/// </summary>
	/// <returns>The action view controller.</returns>
	public ActionViewController getActionViewController ()
	{
		return actionView.GetComponent<ActionViewController>();
	}

	/// <summary>
	/// Gets the inv slot image.
	/// </summary>
	/// <returns>The inv slot image.</returns>
	/// <param name="slot">Slot number.</param>
	public RawImage getInvSlot (int slot)
	{
		if (slot == 1)
			return invSlot1;
		else if (slot == 2)
			return invSlot2;
		else if (slot == 3)
			return invSlot3;
		else
			return null;
	}

	/// <summary>
	/// Is mouse over a GUI object?
	/// </summary>
	/// <returns><c>true</c>, if mouse is over GUI, <c>false</c> otherwise.</returns>
	public bool mouseOnGUI ()
	{
		// for each gui component
		foreach(RectTransform rectTrans in FindObjectsOfType<RectTransform>())
		{
			// ignore maingui that takes whole screen
			if (rectTrans.gameObject.GetInstanceID() != MainGUI.GetInstanceID() && rectTrans.gameObject.GetInstanceID() != selectBox.GetInstanceID()) 
			{
				// gui is active and mouse is in gui
				if (RectTransformUtility.RectangleContainsScreenPoint(rectTrans, new Vector2(Input.mousePosition.x, Input.mousePosition.y)) && rectTrans.gameObject.activeSelf) 
				{
                    onGUI.isOn = true;
					return true;
				}
			}
		}
        onGUI.isOn = false;
		return false;
	}

	/// <summary>
	/// Creates the select box.
	/// </summary>
	/// <param name="anchor">Anchor position.</param>
	public void StartSelectBox (Vector3 anchor)
	{
		selectBox.SetActive(true);

		this.anchor = anchor;

		selectBoxTransform.position = new Vector3(this.anchor.x, this.anchor.y, selectBoxTransform.position.z);
	}

	/// <summary>
	/// Drags the select box.
	/// </summary>
	/// <param name="outer">Outer position.</param>
	public void DragSelectBox (Vector3 outer)
	{
		var newSize = new Vector2();
		var newPosition = new Vector3(anchor.x, anchor.y, selectBoxTransform.position.z);

		// if anchor is right of mouse
		if (outer.x - anchor.x < 0)
		{
			// move location
			newSize.x = (anchor.x - outer.x);
			newPosition.x = outer.x;
		}
		else
        {
            newSize.x = (outer.x - anchor.x);
        }


        // if anchor is below mouse
        if (outer.y - anchor.y < 0)
        {
            // move location
            newSize.y = (anchor.y - outer.y);
            newPosition.y = outer.y;
        }
        else
        {
            newSize.y = (outer.y - anchor.y);
        }

		selectBoxTransform.sizeDelta = newSize;
		selectBoxTransform.position = newPosition;
	}

	/// <summary>
	/// Converts screen to local position.
	/// </summary>
	/// <returns>The local position.</returns>
	/// <param name="point">Position.</param>
	Vector2 ScreenToLocal(Vector3 point)
	{
		Vector2 pos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(MainGUI.GetComponent<RectTransform>(), point, Camera.main, out pos);

		return pos;
	}

	/// <summary>
	/// Clears the select box.
	/// </summary>
	public void clearBox ()
	{
		if (debug)
			Debug.Log("Cleared Box.");
		selectBox.SetActive(false);
		selectBoxTransform.sizeDelta = new Vector2(0, 0);
	}
}
