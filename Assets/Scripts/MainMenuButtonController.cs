using UnityEngine;

/// <summary>
/// Main menu button controller.
/// </summary>
public class MainMenuButtonController : MonoBehaviour {

	public GameObject main;
	public GameObject load;
	public GameObject scenario;
	public GameObject settings;
	public GameObject credits;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		deactivateMenus();
		main.SetActive(true);
	}

	/// <summary>
	/// Loads level.
	/// </summary>
	public void CampaignButton ()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(2);
	}

	/// <summary>
	/// Opens load menu.
	/// </summary>
	public void LoadGame ()
	{
		deactivateMenus();
		load.SetActive(true);
	}

	/// <summary>
	/// Opens scenario menu.
	/// </summary>
	public void Scenario ()
	{
		deactivateMenus();
		scenario.SetActive(true);
	}

	/// <summary>
	/// Opens the settings menu.
	/// </summary>
	public void Settings ()
	{
		deactivateMenus();
		settings.SetActive(true);
	}

	/// <summary>
	/// Opens the credits menu.
	/// </summary>
	public void Credits ()
	{
		deactivateMenus();
		credits.SetActive(true);
	}

	/// <summary>
	/// Opens main menu.
	/// </summary>
	public void MainMenu ()
	{
		deactivateMenus();
		main.SetActive(true);
	}

	/// <summary>
	/// Deactivates the menus.
	/// </summary>
	void deactivateMenus ()
	{
		main.SetActive(false);
		load.SetActive(false);
		scenario.SetActive(false);
		settings.SetActive(false);
		credits.SetActive(false);
	}
}
