using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SacrificeNeutralCrab : NeutralCrab {

	public int SacrificesRequired { get; set; }

	List<RawImage> _checkmarks;

	GameObject _target;
	bool _sacrificing;
	int _sacrifices;

    /// <summary>
    /// Start this instance
    /// </summary>
	void Start()
	{
		Crab = GetComponent<CrabController>();
		Type = Crab.Type;

		_checkmarks = new List<RawImage>();

		_sacrificing = false;
		UIOpen = false;
		PlayerIsNear = false;
		RecruitmentUI = null;
		SetRecruitmentUI();

		_target = null;
		_sacrifices = 0;
	}

    /// <summary>
    /// Update this instance
    /// </summary>
	void Update()
	{
		if (_sacrificing && Crab.GetDistanceToObject(_target) < Crab.AttackRange)
		{
			EatSacrifice();
		}

		RunRecruitUI();
		_checkmarks.Clear();

		if (RecruitmentUI != null)
		{
			GameObject panel = RecruitmentUI.GetComponentInChildren<Image>().gameObject;
			RawImage[] checks = panel.GetComponentsInChildren<RawImage>(true);
			for (int i = 0; i < checks.Length; i++)
			{
				if (checks[i].name.Contains("Checkmark"))
				{
					_checkmarks.Add(checks[i]);
				}
			}

			if (_checkmarks.Count > 0)
			{
				for (int i = 0; i < _sacrifices; i++)
				{
					_checkmarks[i].gameObject.SetActive(true);
				}
				for (int i = _sacrifices; i < _checkmarks.Count; i++)
				{
					_checkmarks[i].gameObject.SetActive(false);
				}
			}
		}
	}

	public void StartSacrifice(GameObject target)
	{
		_sacrificing = true;
		this._target = target;

		Crab.StartMove(target.transform.position);
	}

	public void EatSacrifice() 
	{
		int team = _target.GetComponent<Team>().team;

		_target.GetComponent<CrabController>().Destroyed();
		Destroy(_target);

		_sacrifices++;

		_sacrificing = false;

		if (CanRecruit())
		{
			Destroy(RecruitmentUI);
			GetComponent<Team>().team = team;
			Destroy(this);
		}
	}

	public bool CanRecruit()
	{
		return _sacrifices == SacrificesRequired;
	}
}
