using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SacrificeNeutralCrab : NeutralCrab {

	public int sacrificesRequired { get; set; }

	List<RawImage> checkmarks;

	GameObject target;
	bool sacrificing;
	int sacrifices;

	void Start ()
	{
		crab = GetComponent<CrabController>();
		type = crab.type;

		checkmarks = new List<RawImage>();

		sacrificing = false;
		uiOpen = false;
		playerIsNear = false;
		recruitmentUI = null;
		setRecruitmentUI();

		target = null;
		sacrifices = 0;
	}

	void Update ()
	{
		if (sacrificing && crab.getDistanceToObject(target) < crab.attackRange)
		{
			eatSacrifice();
		}

		runRecruitUI();
		checkmarks.Clear();

		if (recruitmentUI != null)
		{
			GameObject panel = recruitmentUI.GetComponentInChildren<Image>().gameObject;
			RawImage[] checks = panel.GetComponentsInChildren<RawImage>(true);
			for (int i = 0; i < checks.Length; i++)
			{
				if (checks[i].name.Contains("Checkmark"))
				{
					checkmarks.Add(checks[i]);
				}
			}

			if (checkmarks.Count > 0)
			{
				for (int i = 0; i < sacrifices; i++)
				{
					checkmarks[i].gameObject.SetActive(true);
				}
				for (int i = sacrifices; i < checkmarks.Count; i++)
				{
					checkmarks[i].gameObject.SetActive(false);
				}
			}
		}
	}

	public void startSacrifice (GameObject target)
	{
		sacrificing = true;
		this.target = target;

		crab.startMove(target.transform.position);
	}

	public void eatSacrifice () 
	{
		int team = target.GetComponent<Team>().team;

		target.GetComponent<CrabController>().destroyed();
		Destroy(target);

		sacrifices++;

		sacrificing = false;

		if (canRecruit())
		{
			Destroy(recruitmentUI);
			GetComponent<Team>().team = team;
			Destroy(this);
		}
	}

	public bool canRecruit ()
	{
		return sacrifices == sacrificesRequired;
	}
}
