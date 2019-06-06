using UnityEngine;

/// <summary>
/// Button controller.
/// Provides functionality to main game UI.
/// </summary>
[RequireComponent(typeof(GUIController))]
[RequireComponent(typeof(DebugComponent))]
public class ButtonController : MonoBehaviour {

	string _pendingAction;

	/// <summary>
	/// Resumes the game.
	/// </summary>
	public void ResumeGame()
	{
		GetComponent<GUIController>().PauseMenu.SetActive(false);
		Time.timeScale = 1.0f;
	}

	/// <summary>
	/// Opens the save menu.
	/// </summary>
	public void OpenSave()
	{
		GetComponent<GUIController>().SaveMenu.SetActive(true);
	}

	/// <summary>
	/// Opens the confirmation menu.
	/// </summary>
	/// <param name="type">Quit or Main Menu.</param>
	public void OpenConfirmation(string type)
	{
		_pendingAction = type;

		GetComponent<GUIController>().SaveConfirmMenu.SetActive(true);
	}

	/// <summary>
	/// Continues pending action without saving.
	/// </summary>
	public void ContinueWithoutSaving()
	{
		if (_pendingAction == "Quit")
			QuitGame();
		else if (_pendingAction == "MainMenu")
			MainMenu();
	}

	/// <summary>
	/// Saves the game.
	/// TODO: implement function
	/// </summary>
	public void SaveGame()
	{
		// Check selected file
		// Ask to overwrite
		// else if new file specified
		// create new file with name
		// save the game
	}

	/// <summary>
	/// Closes the confirmation menu.
	/// </summary>
	public void CancelConfirmation()
	{
		GetComponent<GUIController>().SaveConfirmMenu.SetActive(false);
	}

	/// <summary>
	/// Loads main menu.
	/// TODO: implement function
	/// </summary>
	public void MainMenu()
	{
		// Ask to save first
		// if continue
		// Move to Main Menu
		// else if cancel
		// move back to Pause
		// else if save
		// move to save menu

		UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
	}

	/// <summary>
	/// Opens new game menu.
	/// TODO: implement function
	/// </summary>
	public void NewGame()
	{

	}

	/// <summary>
	/// Quits the game.
	/// </summary>
	public void QuitGame()
	{
		//if (GetComponent<DebugComponent>().debug)
		//	EditorApplication.isPlaying = false;
		//else 
			Application.Quit();
		// Check if save is up to date.
	}

	/// <summary>
	/// Closes save menu.
	/// </summary>
	public void CancelSave()
	{
		GetComponent<GUIController>().SaveMenu.SetActive(false);
	}
}
