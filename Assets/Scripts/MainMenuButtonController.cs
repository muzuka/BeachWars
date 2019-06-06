using UnityEngine;

/// <summary>
/// Main menu button controller.
/// </summary>
public class MainMenuButtonController : MonoBehaviour {

	public GameObject Main;
	public GameObject Load;
	public GameObject Scenario;
	public GameObject Settings;
	public GameObject Credits;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		DeactivateMenus();
		Main.SetActive(true);
	}

	/// <summary>
	/// Loads level.
	/// </summary>
	public void CampaignButton()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(2);
	}

	/// <summary>
	/// Opens load menu.
	/// </summary>
	public void LoadGame()
	{
		DeactivateMenus();
		Load.SetActive(true);
	}

	/// <summary>
	/// Opens scenario menu.
	/// </summary>
	public void OpenScenario()
	{
		DeactivateMenus();
		Scenario.SetActive(true);
	}

	/// <summary>
	/// Opens the settings menu.
	/// </summary>
	public void OpenSettings()
	{
		DeactivateMenus();
		Settings.SetActive(true);
	}

	/// <summary>
	/// Opens the credits menu.
	/// </summary>
	public void OpenCredits()
	{
		DeactivateMenus();
		Credits.SetActive(true);
	}

	/// <summary>
	/// Opens main menu.
	/// </summary>
	public void OpenMainMenu()
	{
		DeactivateMenus();
		Main.SetActive(true);
	}

	/// <summary>
	/// Deactivates the menus.
	/// </summary>
	void DeactivateMenus()
	{
		Main.SetActive(false);
		Load.SetActive(false);
		Scenario.SetActive(false);
		Settings.SetActive(false);
		Credits.SetActive(false);
	}
}
