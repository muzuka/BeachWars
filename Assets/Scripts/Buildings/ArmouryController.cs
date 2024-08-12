using UnityEngine;

/// <summary>
/// Armoury controller.
/// Handles model changes and interfaces with HoldsWeapons.
/// </summary>
public class ArmouryController : MonoBehaviour {

	const int MAXHEALTH = 200;

	bool _building;
	string _weaponType;
	float _timeConsumed;
	const float _timeToBuild = 2.0f;

	CastleController _closestCastle;

	HoldsWeapons _weaponHolder;

	GameObject[] _weaponModels;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start() {
		GetComponent<Attackable>().MaxHealth = MAXHEALTH;
		GetComponent<Attackable>().Health = MAXHEALTH;

		transform.Translate(0.0f, 1.0f, 0.0f);

		_weaponType = "none";
		_building = false;
		_timeConsumed = 0.0f;

		_closestCastle = GetClosestCastle();
		_weaponHolder = GetComponent<HoldsWeapons>();
		_weaponModels = GetModels();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update()
	{
		float percent = _weaponHolder.PercentFull();

		if (percent < 20.0f)
			SetModels(0);
		else if (percent < 40.0f && percent > 20.0f)
			SetModels(1);
		else if (percent < 60.0f && percent > 40.0f)
			SetModels(2);
		else if (percent < 80.0f && percent > 60.0f)
			SetModels(3);
		else if (percent < 100.0f && percent > 80.0f)
			SetModels(4);

		if (_building)
			BuildWeapon();
	}

	/// <summary>
	/// Starts building a weapon.
	/// </summary>
	/// <param name="weapon">Weapon name.</param>
	public void StartBuilding(string weapon)
	{
		if (CanBuild(weapon))
		{
			_timeConsumed = 0.0f;
			_weaponType = weapon;
			_building = true;
		}
        else
        {
            MessageEventManager.AddNewMessage("Insufficent resources to build " + weapon.ToLower() + ".");
        }
	}

	/// <summary>
	/// Builds the weapon.
	/// </summary>
	void BuildWeapon()
	{
		if (GetComponent<DebugComponent>().Debug)
			Debug.Log(_timeConsumed + " / " + _timeToBuild);
		
		_timeConsumed += Time.deltaTime;
		if (_timeConsumed > _timeToBuild)
		{
			_weaponHolder.BuildWeapon(_weaponType);

			_timeConsumed = 0.0f;
			_building = false;
			_weaponType = "none";
		}
	}

	/// <summary>
	/// Can the weapon be built?
	/// </summary>
	/// <returns><c>true</c>, if the weapon can be built, <c>false</c> otherwise.</returns>
	/// <param name="weapon">Weapon name.</param>
	bool CanBuild(string weapon)
	{
		switch(weapon)
		{
		case Tags.Spear:
			return (_closestCastle.GetWoodPieces() >= 2 && _closestCastle.GetStonePieces() >= 1);
		case Tags.Hammer:
			return (_closestCastle.GetWoodPieces() >= 1 && _closestCastle.GetStonePieces() >= 2);
		case Tags.Bow:
			return _closestCastle.GetWoodPieces() >= 2;
		case Tags.Shield:
			return _closestCastle.GetWoodPieces() >= 3;
		default:
			return false;
		}
	}

	/// <summary>
	/// Sets the models.
	/// Models appear as inventory increases.
	/// </summary>
	/// <param name="end">End.</param>
	void SetModels(int end)
	{
		for (int i = 0; i < end; i++)
        {
            _weaponModels[i].SetActive(true); 
        }

		for (int i = end + 1; i < _weaponModels.Length; i++)
        {
            _weaponModels[i].SetActive(false); 
        }
	}

	/// <summary>
	/// Returns the array of child weapon objects.
	/// </summary>
	/// <returns>The model array.</returns>
	GameObject[] GetModels()
	{
		GameObject[] temp = new GameObject[5];
		int i = 0;
		foreach (Renderer r in GetComponentsInChildren<Renderer>())
		{
			if (r.gameObject.name != "Rack")
			{
				temp[i] = r.gameObject;
				i++;
			}
		}
		return temp;
	}

	/// <summary>
	/// Updates the UI.
	/// </summary>
	/// <param name="gui">GUI script.</param>
	public void UpdateUI(InfoViewController gui)
	{
		GetComponent<Attackable>().SetHealth(gui.HealthSlider);
		gui.SpearText.text = "Spears: " + _weaponHolder.Weapons[Tags.Spear];
		gui.HammerText.text = "Hammers: " + _weaponHolder.Weapons[Tags.Hammer];
		gui.BowText.text = "Bows: " + _weaponHolder.Weapons[Tags.Bow];
		gui.ShieldText.text = "Shields: " + _weaponHolder.Weapons[Tags.Shield];
	}

	/// <summary>
	/// Placeholder so SendMessage doesn't return null.
	/// </summary>
	public void setController(Player player) {}

	/// <summary>
	/// Gets the closest castle.
	/// </summary>
	/// <returns>The closest castle.</returns>
	CastleController GetClosestCastle()
	{
		CastleController closestCastle = null;
		float minDist = float.MaxValue;
		CastleController[] castles = FindObjectsOfType<CastleController>();
		for (int i = 0; i < castles.Length; i++)
		{
			if (castles[i].GetComponent<Team>().team == GetComponent<Team>().team)
			{
				float distance = Vector3.Distance(gameObject.transform.position, castles[i].gameObject.transform.position);

				if (distance < minDist)
				{
					minDist = distance;
					closestCastle = castles[i];
				}
			}
		}
		return closestCastle;
	}
}
