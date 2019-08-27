using UnityEngine;

/// <summary>
/// Button controller.
/// Provides functionality to main game UI.
/// </summary>
[RequireComponent(typeof(GUIController))]
[RequireComponent(typeof(DebugComponent))]
public class ButtonController : MonoBehaviour {

	string pendingAction;

	/// <summary>
	/// Resumes the game.
	/// </summary>
	public void resumeGame ()
	{
		GetComponent<GUIController>().pauseMenu.SetActive(false);
		Time.timeScale = 1.0f;
	}

	/// <summary>
	/// Opens the save menu.
	/// </summary>
	public void openSave ()
	{
		GetComponent<GUIController>().saveMenu.SetActive(true);
	}

	/// <summary>
	/// Opens the confirmation menu.
	/// </summary>
	/// <param name="type">Quit or Main Menu.</param>
	public void openConfirmation (string type)
	{
		pendingAction = type;

		GetComponent<GUIController>().saveConfirmMenu.SetActive(true);
	}

	/// <summary>
	/// Continues pending action without saving.
	/// </summary>
	public void continueWithoutSaving ()
	{
		if(pendingAction == "Quit")
			quitGame();
		else if(pendingAction == "MainMenu")
			mainMenu();
	}

	/// <summary>
	/// Saves the game.
	/// TODO: implement function
	/// </summary>
	public void saveGame ()
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
	public void cancelConfirmation ()
	{
		GetComponent<GUIController>().saveConfirmMenu.SetActive(false);
	}

	/// <summary>
	/// Loads main menu.
	/// TODO: implement function
	/// </summary>
	public void mainMenu ()
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
	public void newGame ()
	{

	}

	/// <summary>
	/// Quits the game.
	/// </summary>
	public void quitGame ()
	{
		//if(GetComponent<DebugComponent>().debug)
		//	EditorApplication.isPlaying = false;
		//else 
			Application.Quit();
		// Check if save is up to date.
	}

	/// <summary>
	/// Closes save menu.
	/// </summary>
	public void cancelSave ()
	{
		GetComponent<GUIController>().saveMenu.SetActive(false);
	}
}
