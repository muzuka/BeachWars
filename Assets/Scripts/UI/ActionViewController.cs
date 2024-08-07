using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Action view controller.
/// Handles buttons in mainGUI.
/// Sets button test and events.
/// </summary>
public class ActionViewController : MonoBehaviour {

	// buttons fill action view from left to right and up to down
	public Button Button1;
	public Button Button2;
	public Button Button3;
	public Button Button4;
	public Button Button5;
	public Button Button6;
	public Button Button7;
	public Button Button8;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start() 
	{
		DeactivateButtons();
	}

	/// <summary>
	/// Called when build button is pressed.
	/// </summary>
	/// <param name="player">Player script.</param>
	public void SetBuildButtons(Player player) 
	{
		DeactivateButtons();
		Button1.gameObject.SetActive(true);
		Button1.gameObject.GetComponentInChildren<Text>().text = "Nest";
		Button1.onClick.AddListener(() => {player.SetBuildingType ("Nest");
			player.CreateGhostBuilding();});
		Button2.gameObject.SetActive(true);
		Button2.gameObject.GetComponentInChildren<Text>().text = "SiegeWorkshop";
		Button2.onClick.AddListener(() => {player.SetBuildingType ("Workshop");
			player.CreateGhostBuilding();});
		Button3.gameObject.SetActive(true);
		Button3.gameObject.GetComponentInChildren<Text>().text = "Tower";
		Button3.onClick.AddListener(() => {player.SetBuildingType ("Tower");
			player.CreateGhostBuilding();});
		Button4.gameObject.SetActive(true);
		Button4.gameObject.GetComponentInChildren<Text>().text = "Amoury";
		Button4.onClick.AddListener(() => {player.SetBuildingType ("Armoury");
			player.CreateGhostBuilding();});
	}

	/// <summary>
	/// Called when a crab is selected.
	/// </summary>
	/// <param name="player">Player script.</param>
	public void SetCrabButtons(Player player) 
	{
		DeactivateButtons();
		Button1.gameObject.SetActive(true);
		Button1.gameObject.GetComponentInChildren<Text>().text = "Attack";
		Button1.onClick.AddListener(() => player.SetPlayerState ("Attacking"));
		Button2.gameObject.SetActive(true);
		Button2.gameObject.GetComponentInChildren<Text>().text = "Build";
		Button2.onClick.AddListener(() => player.SetPlayerState ("Building"));
		Button3.gameObject.SetActive(true);
		Button3.gameObject.GetComponentInChildren<Text>().text = "Enter";
		Button3.onClick.AddListener(() => player.SetPlayerState ("Entering"));
		Button4.gameObject.SetActive(true);
		Button4.gameObject.GetComponentInChildren<Text>().text = "Upgrade";
		Button4.onClick.AddListener(() => player.SetPlayerState ("Upgrading"));
		Button5.gameObject.SetActive(true);
		Button5.gameObject.GetComponentInChildren<Text>().text = "Recruit";
		Button5.onClick.AddListener(() => player.SetPlayerState ("Recruiting"));
		Button6.gameObject.SetActive(true);
		Button6.gameObject.GetComponentInChildren<Text>().text = "Repair";
		Button6.onClick.AddListener(() => player.SetPlayerState ("Repairing"));
		Button7.gameObject.SetActive(true);
		Button7.gameObject.GetComponentInChildren<Text>().text = "Capture";
		Button7.onClick.AddListener(() => player.SetPlayerState ("Capturing"));
	}

	/// <summary>
	/// Called when more than one crab is selected.
	/// </summary>
	/// <param name="player">Player script.</param>
	public void SetMultiButtonsAllCrabs(Player player)
	{
		DeactivateButtons();
		Button1.gameObject.SetActive(true);
		Button1.gameObject.GetComponentInChildren<Text>().text = "Attack";
		Button1.onClick.AddListener(() => player.SetPlayerState ("Attacking"));
		Button2.gameObject.SetActive(true);
		Button2.gameObject.GetComponentInChildren<Text>().text = "Build";
		Button2.onClick.AddListener(() => player.SetPlayerState ("Building"));
		Button3.gameObject.SetActive(true);
		Button3.gameObject.GetComponentInChildren<Text>().text = "Upgrade";
		Button3.onClick.AddListener(() => player.SetPlayerState ("Upgrading"));
		Button4.gameObject.SetActive(true);
		Button4.gameObject.GetComponentInChildren<Text>().text = "Repair";
		Button4.onClick.AddListener(() => player.SetPlayerState ("Repairing"));
		Button5.gameObject.SetActive(true);
		Button5.gameObject.GetComponentInChildren<Text>().text = "Capture";
		Button5.onClick.AddListener(() => player.SetPlayerState ("Capturing"));
	}

	/// <summary>
	/// Called when a siege weapon is selected.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="script">Enterable script.</param>
	public void SetSiegeButtons(Player player, Enterable script) 
	{
		DeactivateButtons();
		Button1.gameObject.SetActive(true);
		Button1.gameObject.GetComponentInChildren<Text>().text = "Attack";
		Button1.onClick.AddListener(() => player.SetPlayerState ("Attacking"));
		Button2.gameObject.SetActive(true);
		Button2.gameObject.GetComponentInChildren<Text>().text = "Enter";
		Button2.onClick.AddListener(() => player.SetPlayerState ("Entering"));
		Button3.gameObject.SetActive(true);
		Button3.gameObject.GetComponentInChildren<Text>().text = "Unload";
		Button3.onClick.AddListener (script.RemoveOccupant);
	}

	/// <summary>
	/// Called when crabs and siege weapons are selected together.
	/// </summary>
	/// <param name="player">Player script.</param>
	public void SetMultiButtonsMixed(Player player)
	{
		DeactivateButtons();
		Button1.gameObject.SetActive(true);
		Button1.gameObject.GetComponentInChildren<Text>().text = "Attack";
		Button1.onClick.AddListener(() => player.SetPlayerState ("Attacking"));
	}

	/// <summary>
	/// Called when a castle is selected.
	/// </summary>
	/// <param name="eScript">Enterable script.</param>
	/// <param name="cScript">Upgradable script.</param>
	public void SetCastleButtons(Enterable eScript, CastleUpgrade cScript) 
	{
		CastleController castle = cScript.gameObject.GetComponent<CastleController>();

		DeactivateButtons();
		Button1.gameObject.SetActive(true);
		Button1.gameObject.GetComponentInChildren<Text>().text = "Unload";
		Button1.onClick.AddListener(eScript.RemoveOccupant);
		Button2.gameObject.SetActive(true);
		Button2.gameObject.GetComponentInChildren<Text>().text = "Upgrade";
        Button2.onClick.AddListener(() => cScript.StartCastleUpgrade(castle.GetWoodPieces(), castle.GetStonePieces()));
	}

	/// <summary>
	/// Called when craft button is pressed.
	/// </summary>
	/// <param name="aScript">Armoury script.</param>
	public void SetWeaponBuildButtons(ArmouryController aScript) 
	{
		DeactivateButtons();
		Button1.gameObject.SetActive(true);
		Button1.gameObject.GetComponentInChildren<Text>().text = "Spear";
		Button1.onClick.AddListener(() => aScript.StartBuilding("Spear"));
		Button2.gameObject.SetActive(true);
		Button2.gameObject.GetComponentInChildren<Text>().text = "Hammer";
		Button2.onClick.AddListener(() => aScript.StartBuilding("Hammer"));
		Button3.gameObject.SetActive(true);
		Button3.gameObject.GetComponentInChildren<Text>().text = "Bow";
		Button3.onClick.AddListener(() => aScript.StartBuilding("Bow"));
		Button4.gameObject.SetActive(true);
		Button4.gameObject.GetComponentInChildren<Text>().text = "Shield";
		Button4.onClick.AddListener(() => aScript.StartBuilding("Shield"));
	}

	/// <summary>
	/// Called when a nest is selected.
	/// </summary>
	/// <param name="eScript">Enterable script.</param>
	public void SetNestButtons(Enterable eScript) 
	{
		DeactivateButtons();
		Button1.gameObject.SetActive(true);
		Button1.gameObject.GetComponentInChildren<Text>().text = "Unload";
		Button1.onClick.AddListener(eScript.RemoveOccupant);
	}

	/// <summary>
	/// Called when a workshop is selected.
	/// </summary>
	/// <param name="wScript">Workshop script.</param>
	public void SetWorkshopButtons(WorkshopController wScript) 
	{
		DeactivateButtons();
		Button1.gameObject.SetActive(true);
		Button1.gameObject.GetComponentInChildren<Text>().text = "Catapult";
		Button1.onClick.AddListener(() => wScript.StartBuilding ("Catapult"));
		Button2.gameObject.SetActive(true);
		Button2.gameObject.GetComponentInChildren<Text>().text = "Ballista";
		Button2.onClick.AddListener(() => wScript.StartBuilding ("Ballista"));
	}

	/// <summary>
	/// Called when a tower is selected.
	/// </summary>
	/// <param name="eScript">Enterable script.</param>
	public void SetTowerButtons(Enterable eScript) 
	{
		DeactivateButtons();
		Button1.gameObject.SetActive(true);
		Button1.gameObject.GetComponentInChildren<Text>().text = "Unload";
		Button1.onClick.AddListener (eScript.RemoveOccupant);
	}

	/// <summary>
	/// Calls function depending on selected object.
	/// Is called by GUIController every update
	/// </summary>
	/// <param name="player">Player script.</param>
	public void SetButtons(Player player) {
		if (player.SelectedList.Count == 1 && player.CanCommand)
		{
			switch (player.Selected.tag)
			{
			case Tags.Crab:
				if (player.States.GetState("Building"))
					SetBuildButtons(player);
				else
					SetCrabButtons(player);
				break;
			case Tags.Catapult:
			case Tags.Ballista:
				SetSiegeButtons(player, player.Selected.GetComponent<Enterable>());
				break;
			case Tags.Armoury:
				SetWeaponBuildButtons(player.Selected.GetComponent<ArmouryController>());
				break;
			case Tags.Castle:
				SetCastleButtons(player.Selected.GetComponent<Enterable>(), player.Selected.GetComponent<CastleUpgrade>());
				break;
			case Tags.Nest:
				SetNestButtons(player.Selected.GetComponent<Enterable>());
				break;
			case Tags.Workshop:
				SetWorkshopButtons(player.Selected.GetComponent<WorkshopController>());
				break;
			case Tags.Tower:
				SetTowerButtons(player.Selected.GetComponent<Enterable>());
				break;
			}
		}
		else if (player.SelectedList.Count > 1)
		{
			string type = player.GetMultiSelectStatus();
			if (type == "Mixed" || type == "Siege")
            {
                SetMultiButtonsMixed(player); 
            }
			else if (type == "Crab")
			{
				if (player.States.GetState("Building"))
                {
                    SetBuildButtons(player); 
                }
				else
                {
                    SetMultiButtonsAllCrabs(player); 
                }
			}
		}
	}

	/// <summary>
	/// Deactivates the buttons.
	/// </summary>
	public void DeactivateButtons() 
	{
		Button1.onClick.RemoveAllListeners();
		Button1.gameObject.SetActive(false);
		Button2.onClick.RemoveAllListeners();
		Button2.gameObject.SetActive(false);
		Button3.onClick.RemoveAllListeners();
		Button3.gameObject.SetActive(false);
		Button4.onClick.RemoveAllListeners();
		Button4.gameObject.SetActive(false);
		Button5.onClick.RemoveAllListeners();
		Button5.gameObject.SetActive(false);
		Button6.onClick.RemoveAllListeners();
		Button6.gameObject.SetActive(false);
		Button7.onClick.RemoveAllListeners();
		Button7.gameObject.SetActive(false);
		Button8.onClick.RemoveAllListeners();
		Button8.gameObject.SetActive(false);
	}
}
