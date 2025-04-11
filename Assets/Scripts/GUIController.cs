using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using UnityEngine.InputSystem;

/// <summary>
/// GUI controller.
/// Controls main gui and it's content
/// </summary>
public class GUIController : MonoBehaviour {

	[Tooltip("The root of the GUI")]
	public GameObject MainGUI;		// public reference to main GUI

    [Tooltip("Reference to Toggle that indicates whether mouse is on gui or not.")]
    public Toggle OnGUI;

    #region UI variables

    public GameObject PauseMenu;
    public GameObject SaveMenu;
    public GameObject WinMenu;
    public GameObject SaveConfirmMenu;
    public GameObject TowerPanel;
    public GameObject WallPanel;
    public GameObject SelectBox;

	RectTransform _selectBoxTransform;

    #endregion

    public ActionViewController ActionView;
    public InfoViewController InfoView;

    Vector2 _start;

	UnityAction _gateButton;
	UnityAction _towerButton;
	UnityAction _junctionTowerButton;

	const string _emptyString = "Nothing";

	DebugComponent _debug;

	/// <summary>
	/// Initializes the UI.
	/// </summary>
	public void StartUI()
	{
		_debug = GetComponent<DebugComponent>();

		if (!GUIHookedIn())
			Debug.Log("GUI isn't completely connected.");

		SetActiveGUIComponents("none");
		
		_selectBoxTransform = SelectBox.GetComponent<RectTransform>();
        ClearBox();

		_gateButton = () => 
		{
			GetComponent<Player>().Selected.GetComponent<BlockController>().ConvertTo(Tags.Gate);
			WallPanel.SetActive(false);
		};

		_towerButton = () =>
		{
			GetComponent<Player>().Selected.GetComponent<BlockController>().ConvertTo(Tags.Tower);
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
		if (player.HasSelected)
		{
			InfoView.SetViewMode(player.IsMultiSelected());

			if (!player.IsMultiSelected())
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
		player.Selected.GetComponent<IUnit>().UpdateUI(InfoView);
		ActionView.SetButtons(player);
	}

	/// <summary>
	/// Updates UI when multiple crabs are selected.
	/// </summary>
	/// <param name="player">Player.</param>
	void MultiUIUpdate(Player player)
	{
		InfoView.PopulateMiniProfiles(player.SelectedList);
		ActionView.SetButtons(player);
	}

	/// <summary>
	/// Called when deselection occurs.
	/// </summary>
	public void Deselect()
	{
		ActionView.DeactivateButtons();
		InfoView.DeactivateGUIComponents();
		InfoView.SelectedImage.GetComponent<RawImage>().texture = null;
		InfoView.LabelText.text = _emptyString;
		InfoView.StoneCount.text = "";
		InfoView.WoodCount.text = "";
	}

	/// <summary>
	/// Are all GUI components instantiated?
	/// </summary>
	/// <returns><c>true</c>, if GUI is hooked in, <c>false</c> otherwise.</returns>
	public bool GUIHookedIn()
	{
		bool menus = (SaveMenu && SaveConfirmMenu && WinMenu && PauseMenu);

		if (!SelectBox)
		{
			_debug.LogMessage("Select box isn't hooked up!");
			return false;
		}
		if (!menus)
		{
			_debug.LogMessage("A menu isn't hooked up!");
			return false;
		}

		return true;
	}

	/// <summary>
	/// Deactivates the GUI components.
	/// </summary>
	public void DeactivateGUIComponents()
	{
		InfoView.DeactivateGUIComponents();
		TowerPanel.gameObject.SetActive(false);
		WallPanel.gameObject.SetActive(false);
		SaveMenu.gameObject.SetActive(false);
		SaveConfirmMenu.gameObject.SetActive(false);
		WinMenu.gameObject.SetActive(false);
		PauseMenu.gameObject.SetActive(false);
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
            InfoView.LabelText.gameObject.SetActive(true);
            InfoView.SelectedImage.gameObject.SetActive(true);
        }

		switch (tag) 
		{
		case Tags.Crab:
			InfoView.SetCrabView();
			break;
		case Tags.Castle:
			InfoView.SetCastleView();
			break;
		case Tags.Ghost:
			InfoView.SetGhostView();
			break;
		case Tags.Block:
			WallPanel.SetActive(true);
            InfoView.DismantleButton.gameObject.SetActive(true);
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
            InfoView.DismantleButton.gameObject.SetActive(true);
			for (int i = 0; i < towerButtons.Length; i++)
			{
				if (towerButtons[i].name == "TowerButton")
                {
                    towerButtons[i].onClick.AddListener(_junctionTowerButton); 
                }
			}
			break;
		case Tags.Armoury:
	        InfoView.SetArmouryView();
	        break;
		case Tags.Catapult:
		case Tags.Ballista:
		case Tags.Tower:
		case Tags.Workshop:
		case Tags.Nest:
			InfoView.SetBuildingView();
			break;
		case "multi":
			_debug.LogMessage("Activated objects");

			InfoView.SetMultiView(GetComponent<Player>());
			break;
		}
	}

	/// <summary>
	/// Is mouse over a GUI object?
	/// </summary>
	/// <returns><c>true</c>, if mouse is over GUI, <c>false</c> otherwise.</returns>
	public bool MouseOnGUI()
	{
		RectTransform[] UITransforms = FindObjectsOfType<RectTransform>();
		// for each gui component
		foreach(RectTransform rectTrans in UITransforms)
		{
			// ignore maingui that takes whole screen
			if (rectTrans.gameObject.GetInstanceID() != MainGUI.GetInstanceID() && rectTrans.gameObject.GetInstanceID() != SelectBox.GetInstanceID()) 
			{
				// gui is active and mouse is in gui
				if (RectTransformUtility.RectangleContainsScreenPoint(rectTrans, Mouse.current.position.ReadValue()) && rectTrans.gameObject.activeSelf) 
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

		_start = ScreenToLocal(anchor);

		_selectBoxTransform.anchoredPosition = _start;
	}

	/// <summary>
	/// Drags the select box.
	/// </summary>
	/// <param name="end">Outer position.</param>
	public void DragSelectBox(Vector3 end)
	{
		end = ScreenToLocal(end);

		Vector2 anchorMin = new Vector2(
			Mathf.Min(_start.x, end.x),
			Mathf.Min(_start.y, end.y));
		Vector2 anchorMax = new Vector2(
			Mathf.Max(_start.x, end.x),
			Math.Max(_start.y, end.y));

		Vector2 boxPos = anchorMin;
		Vector2 boxSize = anchorMax - anchorMin;

		_selectBoxTransform.sizeDelta = boxSize;
		_selectBoxTransform.anchoredPosition = boxPos;
	}

	/// <summary>
	/// Converts screen to local position.
	/// </summary>
	/// <returns>The local position.</returns>
	/// <param name="point">Position.</param>
	Vector2 ScreenToLocal(Vector3 point)
	{
		Vector2 pos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(MainGUI.GetComponent<RectTransform>(), point, null,
			out pos);

		return pos;
	}

	/// <summary>
	/// Clears the select box.
	/// </summary>
	public void ClearBox()
	{
		_debug.LogMessage("Cleared Box.");
		SelectBox.SetActive(false);
		_selectBoxTransform.sizeDelta = new Vector2(0, 0);
	}
}
