using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Action view controller.
/// Handles contextual buttons based on the selected unit.
/// </summary>
public class ActionViewController : MonoBehaviour
{
	public GameObject ButtonPrefab;
	public GameObject ButtonParent;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start() 
	{
		DeactivateButtons();
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
			Enum.SelectStatus type = player.GetMultiSelectStatus();
			if (type == Enum.SelectStatus.MIXED || type == Enum.SelectStatus.SIEGE)
			{
				SetMultiButtonsMixed(player); 
			}
			else if (type == Enum.SelectStatus.CRAB)
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
	/// Called when build button is pressed.
	/// </summary>
	/// <param name="player">Player script.</param>
	void SetBuildButtons(Player player) 
	{
		DeactivateButtons();
		CreateButton("Nest", () => {player.SetBuildingType ("Nest");
			player.CreateGhostBuilding();});
		CreateButton("SiegeWorkshop", () => {player.SetBuildingType ("SiegeWorkshop");
			player.CreateGhostBuilding();});
		CreateButton("Tower", () => {player.SetBuildingType ("Tower");
			player.CreateGhostBuilding();});
		CreateButton("Armoury", () => {player.SetBuildingType ("Armoury");
			player.CreateGhostBuilding();});
	}

	/// <summary>
	/// Called when a crab is selected.
	/// </summary>
	/// <param name="player">Player script.</param>
	void SetCrabButtons(Player player) 
	{
		DeactivateButtons();
		CreateButton("Attack", () => player.SetPlayerState ("Attacking"));
		CreateButton("Build", () => player.SetPlayerState ("Building"));
		CreateButton("Enter", () => player.SetPlayerState ("Entering"));
		CreateButton("Upgrade", () => player.SetPlayerState ("Upgrading"));
		CreateButton("Recruit", () => player.SetPlayerState ("Recruiting"));
		CreateButton("Repair", () => player.SetPlayerState ("Repairing"));
		CreateButton("Capture", () => player.SetPlayerState ("Capturing"));
	}

	/// <summary>
	/// Called when more than one crab is selected.
	/// </summary>
	/// <param name="player">Player script.</param>
	void SetMultiButtonsAllCrabs(Player player)
	{
		DeactivateButtons();
		CreateButton("Attack", () => player.SetPlayerState ("Attacking"));
		CreateButton("Build", () => player.SetPlayerState ("Building"));
		CreateButton("Upgrade", () => player.SetPlayerState ("Upgrading"));
		CreateButton("Repair", () => player.SetPlayerState ("Repairing"));
		CreateButton("Capture", () => player.SetPlayerState ("Capturing"));
	}

	/// <summary>
	/// Called when a siege weapon is selected.
	/// </summary>
	/// <param name="player">Player script.</param>
	/// <param name="script">Enterable script.</param>
	void SetSiegeButtons(Player player, Enterable script) 
	{
		DeactivateButtons();
		CreateButton("Attack", () => player.SetPlayerState ("Attacking"));
		CreateButton("Enter", () => player.SetPlayerState ("Entering"));
		CreateButton("Unload", script.RemoveOccupant);
	}

	/// <summary>
	/// Called when crabs and siege weapons are selected together.
	/// </summary>
	/// <param name="player">Player script.</param>
	void SetMultiButtonsMixed(Player player)
	{
		DeactivateButtons();
		CreateButton("Attack", () => player.SetPlayerState ("Attacking"));
	}

	/// <summary>
	/// Called when a castle is selected.
	/// </summary>
	/// <param name="eScript">Enterable script.</param>
	/// <param name="cScript">Upgradable script.</param>
	void SetCastleButtons(Enterable eScript, CastleUpgrade cScript) 
	{
		CastleController castle = cScript.gameObject.GetComponent<CastleController>();

		DeactivateButtons();
		CreateButton("Unload", eScript.RemoveOccupant);
		CreateButton("Upgrade", () => cScript.StartCastleUpgrade(castle.GetWoodPieces(), castle.GetStonePieces()));
	}

	/// <summary>
	/// Called when craft button is pressed.
	/// </summary>
	/// <param name="aScript">Armoury script.</param>
	void SetWeaponBuildButtons(ArmouryController aScript) 
	{
		DeactivateButtons();
		CreateButton("Spear", () => aScript.StartBuilding("Spear"));
		CreateButton("Hammer", () => aScript.StartBuilding("Hammer"));
		CreateButton("Bow", () => aScript.StartBuilding("Bow"));
		CreateButton("Shield", () => aScript.StartBuilding("Shield"));
	}

	/// <summary>
	/// Called when a nest is selected.
	/// </summary>
	/// <param name="eScript">Enterable script.</param>
	void SetNestButtons(Enterable eScript) 
	{
		DeactivateButtons();
		CreateButton("Unload", eScript.RemoveOccupant);
	}

	/// <summary>
	/// Called when a workshop is selected.
	/// </summary>
	/// <param name="wScript">Workshop script.</param>
	void SetWorkshopButtons(WorkshopController wScript) 
	{
		DeactivateButtons();
		CreateButton("Catapult", () => wScript.StartBuilding ("Catapult"));
		CreateButton("Ballista", () => wScript.StartBuilding ("Ballista"));
	}

	/// <summary>
	/// Called when a tower is selected.
	/// </summary>
	/// <param name="eScript">Enterable script.</param>
	void SetTowerButtons(Enterable eScript) 
	{
		DeactivateButtons();
		CreateButton("Unload", eScript.RemoveOccupant);
	}
	
	void CreateButton(string text, UnityAction buttonAction)
	{
		Button b = Instantiate(ButtonPrefab, ButtonParent.transform).GetComponent<Button>();
		b.gameObject.GetComponentInChildren<Text>().text = text;
		b.onClick.AddListener(buttonAction);
	}

	/// <summary>
	/// Deactivates the buttons.
	/// </summary>
	public void DeactivateButtons()
	{
		Button[] buttons = ButtonParent.GetComponentsInChildren<Button>();

		for (int i = buttons.Length - 1; i >= 0; i--)
		{
			Destroy(buttons[i].gameObject);
		}
	}
}
