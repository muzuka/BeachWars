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
    public Toggle OnGUI;

    #region UI variables

    // Multi-Select UI elements
    RectTransform _multiUITransform;

	public RawImage MainProfile { get; set; }

	public Slider MainSlider { get; set; }

	public Button MainSelectButton { get; set; }

	public RawImage[] MiniProfiles { get; set; }


	// Single-Select UI elements
	RectTransform _singleUITransform;

	public Text LabelText { get; set; }

	public Text StoneCount { get; set; }

	public Text WoodCount { get; set; }

	public Text LevelText { get; set; }

	public Text CrabLevelText { get; set; }

	public Slider HealthSlider { get; set; }

	public Slider ExpSlider { get; set; }

	public RawImage SelectedImage { get; set; }

	public RawImage InvSlot1 { get; set; }

	public RawImage InvSlot2 { get; set; }

	public RawImage InvSlot3 { get; set; }

	public Image ActionView { get; set; }

	public Button CraftButton { get; set; }

	public Button SubCraftButton { get; set; }

	public GameObject PauseMenu { get; set; }

	public GameObject SaveMenu { get; set; }

	public GameObject WinMenu { get; set; }

	public GameObject SaveConfirmMenu { get; set; }

	public GameObject TowerPanel { get; set; }

	public GameObject WallPanel { get; set; }

	public GameObject SelectBox { get; set; }

	public Text SpearText { get; set; }

	public Text HammerText { get; set; }

	public Text BowText { get; set; }

	public Text ShieldText { get; set; }

    public Button DismantleButton { get; set; }

	RectTransform _selectBoxTransform;

    #endregion

    Vector2 _anchor;

	UnityAction _gateButton;
	UnityAction _towerButton;
	UnityAction _junctionTowerButton;

	const string _emptyString = "Nothing";

	int _miniProfileCount = 16;

	bool _debug;

	/// <summary>
	/// Initializes the UI.
	/// </summary>
	public void StartUI()
	{
		_debug = GetComponent<DebugComponent>().Debug;

		ConnectGUI();

		if (!GUIHookedIn())
			Debug.Log("GUI isn't completely connected.");

		SetActiveGUIComponents("none");
		
		_selectBoxTransform = SelectBox.GetComponent<RectTransform>();
        ClearBox();

		SelectedImage.texture = null;
		CraftButton.gameObject.SetActive(false);
		SubCraftButton.gameObject.SetActive(false);

		LabelText.text = _emptyString;
		StoneCount.text = "";
		WoodCount.text = "";

		_gateButton = () => 
		{
			GetComponent<Player>().Selected.GetComponent<BlockController>().ConvertTo("Gate");
			WallPanel.SetActive(false);
		};

		_towerButton = () =>
		{
			GetComponent<Player>().Selected.GetComponent<BlockController>().ConvertTo("Tower");
			WallPanel.SetActive(false);
		};

		_junctionTowerButton = () =>
		{
			GetComponent<Player>().Selected.GetComponent<JunctionController>().ConvertToTower();			  
			TowerPanel.SetActive(false);
		};
	}

	/// <summary>
	/// Updates the UI.
	/// </summary>
	/// <param name="player">Player script.</param>
	public void UpdateUI(Player player)
	{
		if (_debug)
			Debug.Assert(player);

		if (player.HasSelected)
		{
			_multiUITransform.gameObject.SetActive(player.MultiSelect);
			_singleUITransform.gameObject.SetActive(!player.MultiSelect);

			if (!player.MultiSelect)
            {
                SingleUIUpdate(player); 
            }
			else
            {
                MultiUIUpdate(player); 
            }
		}
	}

	/// <summary>
	/// Updates UI when single crab is selected.
	/// </summary>
	/// <param name="player">Player script.</param>
	void SingleUIUpdate(Player player) 
	{
		if (_debug)
		{
			Debug.Assert(player);
			Debug.Assert(player.Selected);
		}

		player.Selected.SendMessage("UpdateUI", this, SendMessageOptions.DontRequireReceiver);

		GetActionViewController().SetButtons(player);
	}

	/// <summary>
	/// Updates UI when multiple crabs are selected.
	/// </summary>
	/// <param name="player">Player.</param>
	void MultiUIUpdate(Player player)
	{
		if (_debug)
		{
			Debug.Assert(player);
			Debug.Assert(player.SelectedList.Count >= 1);
		}

		if (player.SelectedList.Count <= MiniProfiles.Length) 
		{
			for (int i = 0; i < player.SelectedList.Count; i++)
            {
                player.SelectedList[i].GetComponent<Attackable>().SetHealth(MiniProfiles[i].GetComponentInChildren<Slider>()); 
            }
		}

		GetActionViewController().SetButtons(player);
	}

	/// <summary>
	/// Called when deselection occurs.
	/// </summary>
	public void Deselect()
	{
		GetActionViewController().DeactivateButtons();
		DeactivateGUIComponents();
		SelectedImage.GetComponent<RawImage>().texture = null;
		LabelText.text = _emptyString;
		StoneCount.text = "";
		WoodCount.text = "";
	}

	/// <summary>
	/// Are all GUI components instantiated?
	/// </summary>
	/// <returns><c>true</c>, if GUI is hooked in, <c>false</c> otherwise.</returns>
	public bool GUIHookedIn()
	{
		bool count = (StoneCount && WoodCount);
		bool invSlots = (InvSlot1 && InvSlot2 && InvSlot3);
		bool buttons = (CraftButton && SubCraftButton && DismantleButton);
		bool multiSelect = AreAllProfilesHookedIn();
		bool uiTransforms = (_multiUITransform && _singleUITransform);
		bool menus = (SaveMenu && SaveConfirmMenu && WinMenu && PauseMenu);

		if (!LabelText)
		{
			if (_debug)
				Debug.Log("LabelText isn't hooked up!");
			return false;
		}
		if (!count)
		{
			if (_debug)
				Debug.Log("Stone or wood count isn't hooked up!");
			return false;
		}
		if (!HealthSlider)
		{
			if (_debug)
				Debug.Log("Health slider isn't hooked up!");
			return false;
		}
		if (!SelectedImage)
		{
			if (_debug)
				Debug.Log("Selected image isn't hooked up!");
			return false;
		}
		if (!invSlots)
		{
			if (_debug)
				Debug.Log("One of the inventory slots isn't hooked up!");
			return false;
		}
		if (!ActionView)
		{
			if (_debug)
				Debug.Log("stone or wood count isn't hooked up!");
			return false;
		}
		if (!buttons)
		{
			if (_debug)
				Debug.Log("A button isn't hooked up!");
			return false;
		}
		if (!SelectBox)
		{
			if (_debug)
				Debug.Log("Select box isn't hooked up!");
			return false;
		}
		if (!multiSelect)
		{
			if (_debug)
				Debug.Log("A multi-select element isn't hooked up!");
			return false;
		}
		if (!uiTransforms)
		{
			if (_debug)
				Debug.Log("A transform isn't hooked up!");
			return false;
		}
		if (!menus)
		{
			if (_debug)
				Debug.Log("A menu isn't hooked up!");
			return false;
		}

		return true;
	}

	bool AreAllProfilesHookedIn() 
	{
		for (int i = 0; i < _miniProfileCount; i++)
        {
			if (MiniProfiles[i] == null)
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
	public void DeactivateGUIComponents()
	{
		LabelText.gameObject.SetActive(false);
		HealthSlider.gameObject.SetActive(false);
		SelectedImage.gameObject.SetActive(false);
		InvSlot1.gameObject.SetActive(false);
		InvSlot2.gameObject.SetActive(false);
		InvSlot3.gameObject.SetActive(false);
		WoodCount.gameObject.SetActive(false);
		StoneCount.gameObject.SetActive(false);
		LevelText.gameObject.SetActive(false);
		CraftButton.gameObject.SetActive(false);
		SubCraftButton.gameObject.SetActive(false);
		CrabLevelText.gameObject.SetActive(false);
		ExpSlider.gameObject.SetActive(false);
		SpearText.gameObject.SetActive(false);
		HammerText.gameObject.SetActive(false);
		BowText.gameObject.SetActive(false);
		ShieldText.gameObject.SetActive(false);
		TowerPanel.gameObject.SetActive(false);
		WallPanel.gameObject.SetActive(false);
        DismantleButton.gameObject.SetActive(false);

		for (int i = 0; i < MiniProfiles.Length; i++)
        {
            MiniProfiles[i].gameObject.SetActive(false); 
        }
	}

	/// <summary>
	/// Sets the active GUI components based on tag.
	/// </summary>
	/// <param name="tag">Tag.</param>
	public void SetActiveGUIComponents(string tag)
	{
		Button[] wallButtons = WallPanel.GetComponentsInChildren<Button>();
		Button[] towerButtons = TowerPanel.GetComponentsInChildren<Button>();

		DeactivateGUIComponents();

        if (tag != "none")
        {
            LabelText.gameObject.SetActive(true);
            SelectedImage.gameObject.SetActive(true);
        }

		switch (tag) 
		{
		case Tags.Crab:
			HealthSlider.gameObject.SetActive(true);
			InvSlot1.gameObject.SetActive(true);
			InvSlot2.gameObject.SetActive(true);
			InvSlot3.gameObject.SetActive(true);
			CrabLevelText.gameObject.SetActive(true);
			ExpSlider.gameObject.SetActive(true);
			break;
		case Tags.Castle:
			HealthSlider.gameObject.SetActive(true);
			LevelText.gameObject.SetActive(true);
			WoodCount.gameObject.SetActive(true);
			StoneCount.gameObject.SetActive(true);
			break;
		case Tags.Ghost:
			WoodCount.gameObject.SetActive(true);
			StoneCount.gameObject.SetActive(true);
			break;
		case Tags.Block:
			WallPanel.SetActive(true);
            DismantleButton.gameObject.SetActive(true);
			for (int i = 0; i < wallButtons.Length; i++)
			{
				if (wallButtons[i].name == "GateButton")
                {
                    wallButtons[i].onClick.AddListener(_gateButton); 
                }
				if (wallButtons[i].name == "TowerButton")
                {
                    wallButtons[i].onClick.AddListener(_towerButton); 
                }
			}
			break;
		case Tags.Junction:
			TowerPanel.SetActive(true);
            DismantleButton.gameObject.SetActive(true);
			for (int i = 0; i < towerButtons.Length; i++)
			{
				if (towerButtons[i].name == "TowerButton")
                {
                    towerButtons[i].onClick.AddListener(_junctionTowerButton); 
                }
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
				SpearText.gameObject.SetActive(true);
				HammerText.gameObject.SetActive(true);
				BowText.gameObject.SetActive(true);
				ShieldText.gameObject.SetActive(true);
			}
			HealthSlider.gameObject.SetActive(true);
            DismantleButton.gameObject.SetActive(true);
			break;
		case "multi":
			if (_debug)
				Debug.Log("Activated objects");

			Player player = GetComponent<Player>();

			if (player.SelectedList.Count > MiniProfiles.Length)
			{
				for (int i = 0; i < MiniProfiles.Length; i++)
                {
                    MiniProfiles[i].gameObject.SetActive(true); 
                }
			}
            else 
			{
				for (int i = 0; i < player.SelectedList.Count; i++)
                {
                    MiniProfiles[i].gameObject.SetActive(true); 
                }
			}

			break;
		}
	}

	/// <summary>
	/// Sets the UI for multiple selected units.
	/// </summary>
	public void SetMultiUI()
	{
		Player player = GetComponent<Player>();

		for (int i = 0; i < player.SelectedList.Count; i++)
		{
            if (i >= _miniProfileCount)
            {
                break;
            }

			Debug.Log(i);
			MiniProfiles[i].texture = Resources.Load<Texture>("Textures/" + player.SelectedList[i].tag);

			player.SelectedList[i].GetComponent<Attackable>().SetHealth(MiniProfiles[i].GetComponentInChildren<Slider>());
		}
	}

	/// <summary>
	/// Sets the profile button.
	/// </summary>
	/// <param name="pos">Position.</param>
	public void SetProfileButton(int pos)
	{
		Player player = GetComponent<Player>();
		GameObject crab = player.SelectedList[pos];

		if (_debug)
			Debug.Log(player.SelectedList.Count + " trying to access " + pos);
		
		player.Deselect();
		player.SelectedList.Add(crab);
		player.HaloList.Add(Instantiate(Resources.Load<GameObject>("Prefabs/GUI/HaloCanvas")));
		player.Select(crab);
	}

	/// <summary>
	/// Connects the GUI components to the proper references.
	/// </summary>
	public void ConnectGUI()
	{
		if (!MainGUI)
		{
			if (_debug)
				Debug.Log("Connect the GUI!");
			return;
		}

		MiniProfiles = new RawImage[_miniProfileCount];
		RectTransform[] menus = MainGUI.GetComponentsInChildren<RectTransform>();
		Regex regexObject = new Regex("(\\d\\d*)");


		for (int i = 0; i < menus.Length; i++)
		{
			GameObject menuItem = menus[i].gameObject;
			switch (menuItem.name) {
			case "Label Text":
				if (menuItem.GetComponent<Text>())
					LabelText = menuItem.GetComponent<Text>();
				break;
			case "Stone Text":
				if (menuItem.GetComponent<Text>())
					StoneCount = menuItem.GetComponent<Text>();
				break;
			case "Wood Text":
				if (menuItem.GetComponent<Text>())
					WoodCount = menuItem.GetComponent<Text>();
				break;
			case "Level Text":
				if (menuItem.GetComponent<Text>())
					LevelText = menuItem.GetComponent<Text>();
				break;
			case "Crab Level Text":
				if (menuItem.GetComponent<Text>())
					CrabLevelText = menuItem.GetComponent<Text>();
				break;
			case "Health Slider":
				if (menuItem.GetComponent<Slider>())
					HealthSlider = menuItem.GetComponent<Slider>();
				break;
			case "Exp Slider":
				if (menuItem.GetComponent<Slider>())
					ExpSlider = menuItem.GetComponent<Slider>();
				break;
			case "SelectedImage":
				if (menuItem.GetComponent<RawImage>())
					SelectedImage = menuItem.GetComponent<RawImage>();
				break;
			case "InventorySlot":
				if (menuItem.GetComponent<RawImage>())
					InvSlot1 = menuItem.GetComponent<RawImage>();
				break;
			case "InventorySlot (1)":
				if (menuItem.GetComponent<RawImage>())
					InvSlot2 = menuItem.GetComponent<RawImage>();
				break;
			case "InventorySlot (2)":
				if (menuItem.GetComponent<RawImage>())
					InvSlot3 = menuItem.GetComponent<RawImage>();
				break;
			case "MainCraftButton":
				if (menuItem.GetComponent<Button>())
					CraftButton = menuItem.GetComponent<Button>();
				break;
			case "SubCraftButton":
				if (menuItem.GetComponent<Button>())
					SubCraftButton = menuItem.GetComponent<Button>();
				break;
            case "DismantleButton":
                if (menuItem.GetComponent<Button>())
                    DismantleButton = menuItem.GetComponent<Button>();
                break;
			case "Unit Action View":
				if (menuItem.GetComponent<Image>())
					ActionView = menuItem.GetComponent<Image>();
				break;
			case "Spear Text":
				if (menuItem.GetComponent<Text>())
					SpearText = menuItem.GetComponent<Text>();
				break;
			case "Hammer Text":
				if (menuItem.GetComponent<Text>())
					HammerText = menuItem.GetComponent<Text>();
				break;
			case "Bow Text":
				if (menuItem.GetComponent<Text>())
					BowText = menuItem.GetComponent<Text>();
				break;
			case "Shield Text":
				if (menuItem.GetComponent<Text>())
					ShieldText = menuItem.GetComponent<Text>();
				break;
			case "Pause Menu":
				PauseMenu = menuItem;
				PauseMenu.SetActive(false);
				break;
			case "Save Menu":
				SaveMenu = menuItem;
				SaveMenu.SetActive(false);
				break;
			case "Win Menu":
				WinMenu = menuItem;
				WinMenu.SetActive(false);
				break;
			case "Confirmation Menu":
				SaveConfirmMenu = menuItem;
				SaveConfirmMenu.SetActive(false);
				break;
			case "Tower Panel":
				TowerPanel = menuItem;
				TowerPanel.SetActive(false);
				break;
			case "Wall Panel":
				WallPanel = menuItem;
				WallPanel.SetActive(false);
				break;
			case "SelectBox":
				SelectBox = menuItem;
				_selectBoxTransform = SelectBox.GetComponent<RectTransform>();
				break;
			case "MultiSelectUI":
				_multiUITransform = menus[i];
				break;
			case "SingleSelectUI":
				_singleUITransform = menus[i];
				break;
			case "MainProfile":
				if (menuItem.GetComponent<RawImage>())
					MainProfile = menuItem.GetComponent<RawImage>();
				if (menuItem.GetComponent<Button>())
					MainSelectButton = menuItem.GetComponent<Button>();
				break;
			case "MainSlider":
				if (menuItem.GetComponent<Slider>())
					MainSlider = menuItem.GetComponent<Slider>();
				break;
			default:
				string menuName = menuItem.name;
				if (menuName.StartsWith("MiniProfile"))
				{
					if (regexObject.IsMatch (menuName))
					{
						Match match = regexObject.Match (menuName);
						MiniProfiles[int.Parse(match.Captures[0].Value)] = menuItem.GetComponent<RawImage>();
					}
					else
                    {
                        MiniProfiles[0] = menuItem.gameObject.GetComponent<RawImage>(); 
                    }
				}
				break;
			}
		}
	}

	/// <summary>
	/// Sets the label text.
	/// </summary>
	/// <param name="selected">Selected object.</param>
	public void SetLabel(GameObject selected)
	{
		if (_debug)
			Debug.Assert(selected);

		if (selected.tag == Tags.Crab)
        {
            LabelText.text = selected.GetComponent<CrabController>().Type.ToString().ToLower() + " crab"; 
        }
		else if (selected.tag == Tags.Ghost)
        {
            LabelText.text = selected.tag + " " + selected.GetComponent<GhostBuilder>().Original.tag; 
        }
		else
        {
            LabelText.text = selected.tag; 
        }

		if (!GetComponent<Player>().CanCommand && !IdUtility.IsResource(selected.tag))
        {
            LabelText.text = "Enemy " + LabelText.text; 
        }
	}

	/// <summary>
	/// Gets the action view controller.
	/// </summary>
	/// <returns>The action view controller.</returns>
	public ActionViewController GetActionViewController()
	{
		return ActionView.GetComponent<ActionViewController>();
	}

	/// <summary>
	/// Gets the inv slot image.
	/// </summary>
	/// <returns>The inv slot image.</returns>
	/// <param name="slot">Slot number.</param>
	public RawImage GetInvSlot(int slot)
	{
		return slot switch
		{
			1 => InvSlot1,
			2 => InvSlot2,
			3 => InvSlot3,
			_ => null
		};
	}

	/// <summary>
	/// Is mouse over a GUI object?
	/// </summary>
	/// <returns><c>true</c>, if mouse is over GUI, <c>false</c> otherwise.</returns>
	public bool MouseOnGUI()
	{
		// for each gui component
		foreach(RectTransform rectTrans in FindObjectsOfType<RectTransform>())
		{
			// ignore maingui that takes whole screen
			if (rectTrans.gameObject.GetInstanceID() != MainGUI.GetInstanceID() && rectTrans.gameObject.GetInstanceID() != SelectBox.GetInstanceID()) 
			{
				// gui is active and mouse is in gui
				if (RectTransformUtility.RectangleContainsScreenPoint(rectTrans, new Vector2(Input.mousePosition.x, Input.mousePosition.y)) && rectTrans.gameObject.activeSelf) 
				{
                    OnGUI.isOn = true;
					return true;
				}
			}
		}
        OnGUI.isOn = false;
		return false;
	}

	/// <summary>
	/// Creates the select box.
	/// </summary>
	/// <param name="anchor">Anchor position.</param>
	public void StartSelectBox(Vector3 anchor)
	{
		SelectBox.SetActive(true);

		this._anchor = anchor;

		_selectBoxTransform.position = new Vector3(this._anchor.x, this._anchor.y, _selectBoxTransform.position.z);
	}

	/// <summary>
	/// Drags the select box.
	/// </summary>
	/// <param name="outer">Outer position.</param>
	public void DragSelectBox(Vector3 outer)
	{
		var newSize = new Vector2();
		var newPosition = new Vector3(_anchor.x, _anchor.y, _selectBoxTransform.position.z);

		// if anchor is right of mouse
		if (outer.x - _anchor.x < 0)
		{
			// move location
			newSize.x = (_anchor.x - outer.x);
			newPosition.x = outer.x;
		}
		else
        {
            newSize.x = (outer.x - _anchor.x);
        }


        // if anchor is below mouse
        if (outer.y - _anchor.y < 0)
        {
            // move location
            newSize.y = (_anchor.y - outer.y);
            newPosition.y = outer.y;
        }
        else
        {
            newSize.y = (outer.y - _anchor.y);
        }

		_selectBoxTransform.sizeDelta = newSize;
		_selectBoxTransform.position = newPosition;
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
	public void ClearBox()
	{
		if (_debug)
			Debug.Log("Cleared Box.");
		SelectBox.SetActive(false);
		_selectBoxTransform.sizeDelta = new Vector2(0, 0);
	}
}
