using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeutralCrab : MonoBehaviour {

	protected Enum.CrabSpecies Type;

	protected CrabController Crab;

	// recruitment variables
	protected string RecruitmentUIName;					// name of the prefab file for the respective recruitment UI
	protected GameObject RecruitmentUI;					// Canvas of ui that appears when player approaches
	protected bool PlayerIsNear;						// is player nearby?
	protected bool UIOpen;								// is the UI open

	// Use this for initialization
	void Start() {
		Crab = GetComponent<CrabController>();
		Type = Crab.Type;

		UIOpen = false;
		PlayerIsNear = false;
		RecruitmentUI = null;
		SetRecruitmentUI();
	}
	
	// Update is called once per frame
	void Update() {
		RunRecruitUI();
	}

	/// <summary>
	/// Runs the recruit UI.
	/// Opens menu when player approaches.
	/// </summary>
	protected void RunRecruitUI()
	{
		PlayerIsNear = GetComponent<CrabController>().IsCrabNear();

		if (PlayerIsNear)
		{
			if (GetComponent<DebugComponent>().IsDebugModeEnabled)
				Debug.Log("Player is near!");

			if (UIOpen)
            {
                RecruitmentUI.GetComponentInChildren<Image>().gameObject.transform.position = gameObject.transform.position + new Vector3(0.0f, 5.0f, 0.0f); 
            }
			else
			{
				if (GetComponent<DebugComponent>().IsDebugModeEnabled)
					Debug.Log("Opened recruitment panel!");

				RecruitmentUI = Instantiate(Resources.Load<GameObject>("Prefabs/GUI/RecruitmentPanels/" + RecruitmentUIName), gameObject.transform.position, Quaternion.identity);
				UIOpen = true;
			}
		}
		else
		{
			UIOpen = false;
			Destroy(RecruitmentUI);
		}
	}

	/// <summary>
	/// Sets the recruitment UI to prefab name.
	/// </summary>
	protected void SetRecruitmentUI()
	{
		switch (Type) {
		case Enum.CrabSpecies.FIDDLER:
			RecruitmentUIName = "FiddlerCrabRecruitUI";
			break;
		case Enum.CrabSpecies.TGIANT:
			RecruitmentUIName = "TGiantCrabRecruitUI";
			break;
		case Enum.CrabSpecies.SPIDER:
			RecruitmentUIName = "SpiderCrabRecruitUI";
			break;
		case Enum.CrabSpecies.COCONUT:
			RecruitmentUIName = "CoconutCrabRecruitUI";
			break;
		case Enum.CrabSpecies.HORSESHOE:
			RecruitmentUIName = "HorseshoeCrabRecruitUI";
			break;
		case Enum.CrabSpecies.SEAWEED:
		case Enum.CrabSpecies.CALICO:
		case Enum.CrabSpecies.KAKOOTA:
		case Enum.CrabSpecies.TRILOBITE:
		case Enum.CrabSpecies.ROCK:
			RecruitmentUIName = "AutoSuccessRecruitUI";
			break;
		}
	}
}
