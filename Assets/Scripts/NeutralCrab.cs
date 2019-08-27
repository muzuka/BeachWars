using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeutralCrab : MonoBehaviour {

	protected CrabSpecies type;

	protected CrabController crab;

	// recruitment variables
	protected string recruitmentUIName;					// name of the prefab file for the respective recruitment UI
	protected GameObject recruitmentUI;					// Canvas of ui that appears when player approaches
	protected bool playerIsNear;						// is player nearby?
	protected bool uiOpen;								// is the UI open

	// Use this for initialization
	void Start () {
		crab = GetComponent<CrabController>();
		type = crab.type;

		uiOpen = false;
		playerIsNear = false;
		recruitmentUI = null;
		setRecruitmentUI();
	}
	
	// Update is called once per frame
	void Update () {
		runRecruitUI();
	}

	/// <summary>
	/// Runs the recruit UI.
	/// Opens menu when player approaches.
	/// </summary>
	protected void runRecruitUI ()
	{
		playerIsNear = GetComponent<CrabController>().isCrabNear();

		if (playerIsNear)
		{
			if (GetComponent<DebugComponent>().debug)
				Debug.Log("Player is near!");

			if (uiOpen)
				recruitmentUI.GetComponentInChildren<Image>().gameObject.transform.position = gameObject.transform.position + new Vector3(0.0f, 5.0f, 0.0f);
			else
			{
				if (GetComponent<DebugComponent>().debug)
					Debug.Log("Opened recruitment panel!");

				recruitmentUI = Instantiate(Resources.Load<GameObject>("Prefabs/GUI/RecruitmentPanels/" + recruitmentUIName), gameObject.transform.position, Quaternion.identity);
				uiOpen = true;
			}
		}
		else
		{
			uiOpen = false;
			Destroy(recruitmentUI);
		}
	}

	/// <summary>
	/// Sets the recruitment UI to prefab name.
	/// </summary>
	protected void setRecruitmentUI ()
	{
		switch (type) {
		case CrabSpecies.FIDDLER:
			recruitmentUIName = "FiddlerCrabRecruitUI";
			break;
		case CrabSpecies.TGIANT:
			recruitmentUIName = "TGiantCrabRecruitUI";
			break;
		case CrabSpecies.SPIDER:
			recruitmentUIName = "SpiderCrabRecruitUI";
			break;
		case CrabSpecies.COCONUT:
			recruitmentUIName = "CoconutCrabRecruitUI";
			break;
		case CrabSpecies.HORSESHOE:
			recruitmentUIName = "HorseshoeCrabRecruitUI";
			break;
		case CrabSpecies.SEAWEED:
		case CrabSpecies.CALICO:
		case CrabSpecies.KAKOOTA:
		case CrabSpecies.TRILOBITE:
		case CrabSpecies.ROCK:
			recruitmentUIName = "AutoSuccessRecruitUI";
			break;
		}
	}
}
