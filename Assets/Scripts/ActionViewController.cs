using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Action view controller.
/// Handles buttons in mainGUI.
/// Sets button test and events.
/// </summary>
public class ActionViewController : MonoBehaviour {

	// buttons fill action view from left to right and up to down
	public Button button1;
	public Button button2;
	public Button button3;
	public Button button4;
	public Button button5;
	public Button button6;
	public Button button7;
	public Button button8;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () 
	{
		deactivateButtons();
	}

	/// <summary>
	/// Called when build button is pressed.
	/// </summary>
	/// <param name="player">Player script.</param>
	public void setBuildButtons (Player player) 
	{
		deactivateButtons();
		button1.gameObject.SetActive(true);
		button1.gameObject.GetComponentInChildren<Text>().text = "Nest";
		button1.onClick.AddListener(() => {player.setBuildingType ("Nest");
			player.createGhostBuilding();});
		button2.gameObject.SetActive(true);
		button2.gameObject.GetComponentInChildren<Text>().text = "SiegeWorkshop";
		button2.onClick.AddListener(() => {player.setBuildingType ("Workshop");
			player.createGhostBuilding();});
		button3.gameObject.SetActive(true);
		button3.gameObject.GetComponentInChildren<Text>().text = "Tower";
		button3.onClick.AddListener(() => {player.setBuildingType ("Tower");
			player.createGhostBuilding();});
		button4.gameObject.SetActive(true);
		button4.gameObject.GetComponentInChildren<Text>().text = "Amoury";
		button4.onClick.AddListener(() => {player.setBuildingType ("Armoury");
			player.createGhostBuilding();});
	}

	/// <summary>
	/// Called when a crab is selected.
	/// </summary>
	/// <param name="player">Player script.</param>
	public void setCrabButtons (Player player) 
	{
		deactivateButtons();
		button1.gameObject.SetActive(true);
		button1.gameObject.GetComponentInChildren<Text>().text = "Attack";
		button1.onClick.AddListener(() => player.setPlayerState ("Attacking"));
		button2.gameObject.SetActive(true);
		button2.gameObject.GetComponentInChildren<Text>().text = "Build";
		button2.onClick.AddListener(() => player.setPlayerState ("Building"));
		button3.gameObject.SetActive(true);
		button3.gameObject.GetComponentInChildren<Text>().text = "Enter";
		button3.onClick.AddListener(() => player.setPlayerState ("Entering"));
		button4.gameObject.SetActive(true);
		button4.gameObject.GetComponentInChildren<Text>().text = "Upgrade";
		button4.onClick.AddListener(() => player.setPlayerState ("Upgrading"));
		button5.gameObject.SetActive(true);
		button5.gameObject.GetComponentInChildren<Text>().text = "Recruit";
		button5.onClick.AddListener(() => player.setPlayerState ("Recruiting"));
		button6.gameObject.SetActive(true);
		button6.gameObject.GetComponentInChildren<Text>().text = "Repair";
		button6.onClick.AddListener(() => player.setPlayerState ("Repairing"));
		button7.gameObject.SetActive(true);
		button7.gameObject.GetComponentInChildren<Text>().text = "Capture";
		button7.onClick.AddListener(() => player.setPlayerState ("Capturing"));
	}

	/// <summary>
	/// Called when more than one crab is selected.
	/// </summary>
	/// <param name="player">Player script.</param>
	public void setMultiButtonsAllCrabs (Player player)
	{
		deactivateButtons();
		button1.gameObject.SetActive(true);
		button1.gameObject.GetComponentInChildren<Text>().text = "Attack";
		button1.onClick.AddListener(() => player.setPlayerState ("Attacking"));
		button2.gameObject.SetActive(true);
		button2.gameObject.GetComponentInChildren<Text>().text = "Build";
		button2.onClick.AddListener(() => player.setPlayerState ("Building"));
		button3.gameObject.SetActive(true);
		button3.gameObject.GetComponentInChildren<Text>().text = "Upgrade";
		button3.onClick.AddListener(() => player.setPlayerState ("Upgrading"));
		button4.gameObject.SetActive(true);
		button4.gameObject.GetComponentInChildren<Text>().text = "Repair";
		button4.onClick.AddListener(() => player.setPlayerState ("Repairing"));
		button5.gameObject.SetActive(true);
		button5.gameObject.GetComponentInChildren<Text>().text = "Capture";
		button5.onClick.AddListener(() => player.setPlayerState ("Capturing"));
	}

	/// <summary>
	/// Called when a siege weapon is selected.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="script">Enterable script.</param>
	public void setSiegeButtons (Player player, Enterable script) 
	{
		deactivateButtons();
		button1.gameObject.SetActive(true);
		button1.gameObject.GetComponentInChildren<Text>().text = "Attack";
		button1.onClick.AddListener(() => player.setPlayerState ("Attacking"));
		button2.gameObject.SetActive(true);
		button2.gameObject.GetComponentInChildren<Text>().text = "Enter";
		button2.onClick.AddListener(() => player.setPlayerState ("Entering"));
		button3.gameObject.SetActive(true);
		button3.gameObject.GetComponentInChildren<Text>().text = "Unload";
		button3.onClick.AddListener (script.removeOccupant);
	}

	/// <summary>
	/// Called when crabs and siege weapons are selected together.
	/// </summary>
	/// <param name="player">Player script.</param>
	public void setMultiButtonsMixed (Player player)
	{
		deactivateButtons();
		button1.gameObject.SetActive(true);
		button1.gameObject.GetComponentInChildren<Text>().text = "Attack";
		button1.onClick.AddListener(() => player.setPlayerState ("Attacking"));
	}

	/// <summary>
	/// Called when a castle is selected.
	/// </summary>
	/// <param name="eScript">Enterable script.</param>
	/// <param name="cScript">Upgradable script.</param>
	public void setCastleButtons (Enterable eScript, CastleUpgrade cScript) 
	{
		CastleController castle = cScript.gameObject.GetComponent<CastleController>();

		deactivateButtons();
		button1.gameObject.SetActive(true);
		button1.gameObject.GetComponentInChildren<Text>().text = "Unload";
		button1.onClick.AddListener(eScript.removeOccupant);
		button2.gameObject.SetActive(true);
		button2.gameObject.GetComponentInChildren<Text>().text = "Upgrade";
        button2.onClick.AddListener(() => cScript.startCastleUpgrade(castle.getWoodPieces(), castle.getStonePieces()));
	}

	/// <summary>
	/// Called when craft button is pressed.
	/// </summary>
	/// <param name="aScript">Armoury script.</param>
	public void setWeaponBuildButtons (ArmouryController aScript) 
	{
		deactivateButtons();
		button1.gameObject.SetActive(true);
		button1.gameObject.GetComponentInChildren<Text>().text = "Spear";
		button1.onClick.AddListener(() => aScript.startBuilding("Spear"));
		button2.gameObject.SetActive(true);
		button2.gameObject.GetComponentInChildren<Text>().text = "Hammer";
		button2.onClick.AddListener(() => aScript.startBuilding("Hammer"));
		button3.gameObject.SetActive(true);
		button3.gameObject.GetComponentInChildren<Text>().text = "Bow";
		button3.onClick.AddListener(() => aScript.startBuilding("Bow"));
		button4.gameObject.SetActive(true);
		button4.gameObject.GetComponentInChildren<Text>().text = "Shield";
		button4.onClick.AddListener(() => aScript.startBuilding("Shield"));
	}

	/// <summary>
	/// Called when a nest is selected.
	/// </summary>
	/// <param name="eScript">Enterable script.</param>
	public void setNestButtons (Enterable eScript) 
	{
		deactivateButtons();
		button1.gameObject.SetActive(true);
		button1.gameObject.GetComponentInChildren<Text>().text = "Unload";
		button1.onClick.AddListener(eScript.removeOccupant);
	}

	/// <summary>
	/// Called when a workshop is selected.
	/// </summary>
	/// <param name="wScript">Workshop script.</param>
	public void setWorkshopButtons (WorkshopController wScript) 
	{
		deactivateButtons();
		button1.gameObject.SetActive(true);
		button1.gameObject.GetComponentInChildren<Text>().text = "Catapult";
		button1.onClick.AddListener(() => wScript.startBuilding ("Catapult"));
		button2.gameObject.SetActive(true);
		button2.gameObject.GetComponentInChildren<Text>().text = "Ballista";
		button2.onClick.AddListener(() => wScript.startBuilding ("Ballista"));
	}

	/// <summary>
	/// Called when a tower is selected.
	/// </summary>
	/// <param name="eScript">Enterable script.</param>
	public void setTowerButtons (Enterable eScript) 
	{
		deactivateButtons();
		button1.gameObject.SetActive(true);
		button1.gameObject.GetComponentInChildren<Text>().text = "Unload";
		button1.onClick.AddListener (eScript.removeOccupant);
	}

	/// <summary>
	/// Calls function depending on selected object.
	/// Is called by GUIController every update
	/// </summary>
	/// <param name="player">Player script.</param>
	public void setButtons (Player player) {
		if (player.selectedList.Count == 1 && player.canCommand)
		{
			switch (player.selected.tag)
			{
			case Tags.Crab:
				if (player.states.getState("Building"))
					setBuildButtons(player);
				else
					setCrabButtons(player);
				break;
			case Tags.Catapult:
			case Tags.Ballista:
				setSiegeButtons(player, player.selected.GetComponent<Enterable>());
				break;
			case Tags.Armoury:
				setWeaponBuildButtons(player.selected.GetComponent<ArmouryController>());
				break;
			case Tags.Castle:
				setCastleButtons(player.selected.GetComponent<Enterable>(), player.selected.GetComponent<CastleUpgrade>());
				break;
			case Tags.Nest:
				setNestButtons(player.selected.GetComponent<Enterable>());
				break;
			case Tags.Workshop:
				setWorkshopButtons(player.selected.GetComponent<WorkshopController>());
				break;
			case Tags.Tower:
				setTowerButtons(player.selected.GetComponent<Enterable>());
				break;
			}
		}
		else if (player.selectedList.Count > 1)
		{
			string type = player.getMultiSelectStatus();
			if (type == "Mixed" || type == "Siege")
				setMultiButtonsMixed(player);
			else if (type == "Crab")
			{
				if (player.states.getState("Building"))
					setBuildButtons(player);
				else
					setMultiButtonsAllCrabs(player);
			}
		}
	}

	/// <summary>
	/// Deactivates the buttons.
	/// </summary>
	public void deactivateButtons () 
	{
		button1.onClick.RemoveAllListeners();
		button1.gameObject.SetActive(false);
		button2.onClick.RemoveAllListeners();
		button2.gameObject.SetActive(false);
		button3.onClick.RemoveAllListeners();
		button3.gameObject.SetActive(false);
		button4.onClick.RemoveAllListeners();
		button4.gameObject.SetActive(false);
		button5.onClick.RemoveAllListeners();
		button5.gameObject.SetActive(false);
		button6.onClick.RemoveAllListeners();
		button6.gameObject.SetActive(false);
		button7.onClick.RemoveAllListeners();
		button7.gameObject.SetActive(false);
		button8.onClick.RemoveAllListeners();
		button8.gameObject.SetActive(false);
	}
}
