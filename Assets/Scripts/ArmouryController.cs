using UnityEngine;

/// <summary>
/// Armoury controller.
/// Handles model changes and interfaces with HoldsWeapons.
/// </summary>
[RequireComponent(typeof(Attackable))]
[RequireComponent(typeof(DebugComponent))]
public class ArmouryController : MonoBehaviour {

	const int maxHealth = 200;

	bool building;
	string weaponType;
	float timeConsumed;
	const float timeToBuild = 2.0f;

	CastleController closestCastle;

	HoldsWeapons weaponHolder;

	GameObject[] weaponModels;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () {
		GetComponent<Attackable>().maxHealth = maxHealth;
		GetComponent<Attackable>().health = maxHealth;

		transform.Translate(0.0f, 1.0f, 0.0f);

		weaponType = "none";
		building = false;
		timeConsumed = 0.0f;

		closestCastle = getClosestCastle();
		weaponHolder = GetComponent<HoldsWeapons>();
		weaponModels = getModels();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update ()
	{
		float percent = weaponHolder.percentFull();

		if (percent < 20.0f)
			setModels(0);
		else if (percent < 40.0f && percent > 20.0f)
			setModels(1);
		else if (percent < 60.0f && percent > 40.0f)
			setModels(2);
		else if (percent < 80.0f && percent > 60.0f)
			setModels(3);
		else if (percent < 100.0f && percent > 80.0f)
			setModels(4);

		if (building)
			buildWeapon();
	}

	/// <summary>
	/// Starts building a weapon.
	/// </summary>
	/// <param name="weapon">Weapon name.</param>
	public void startBuilding (string weapon)
	{
		if (canBuild(weapon))
		{
			timeConsumed = 0.0f;
			weaponType = weapon;
			building = true;
		}
        else
        {
            MessageEventManager.addNewMessage("Insufficent resources to build " + weapon.ToLower() + ".");
        }
	}

	/// <summary>
	/// Builds the weapon.
	/// </summary>
	void buildWeapon ()
	{
		if (GetComponent<DebugComponent>().debug)
			Debug.Log(timeConsumed + " / " + timeToBuild);
		
		timeConsumed += Time.deltaTime;
		if (timeConsumed > timeToBuild)
		{
			weaponHolder.buildWeapon(weaponType);

			timeConsumed = 0.0f;
			building = false;
			weaponType = "none";
		}
	}

	/// <summary>
	/// Can the weapon be built?
	/// </summary>
	/// <returns><c>true</c>, if the weapon can be built, <c>false</c> otherwise.</returns>
	/// <param name="weapon">Weapon name.</param>
	bool canBuild (string weapon)
	{
		switch(weapon)
		{
		case Tags.Spear:
			return (closestCastle.getWoodPieces() >= 2 && closestCastle.getStonePieces() >= 1);
		case Tags.Hammer:
			return (closestCastle.getWoodPieces() >= 1 && closestCastle.getStonePieces() >= 2);
		case Tags.Bow:
			return closestCastle.getWoodPieces() >= 2;
		case Tags.Shield:
			return closestCastle.getWoodPieces() >= 3;
		default:
			return false;
		}
	}

	/// <summary>
	/// Sets the models.
	/// Models appear as inventory increases.
	/// </summary>
	/// <param name="end">End.</param>
	void setModels (int end)
	{
		for (int i = 0; i < end; i++)
			weaponModels[i].SetActive(true);

		for (int i = end + 1; i < weaponModels.Length; i++)
			weaponModels[i].SetActive(false);
	}

	/// <summary>
	/// Returns the array of child weapon objects.
	/// </summary>
	/// <returns>The model array.</returns>
	GameObject[] getModels ()
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
	public void updateUI (GUIController gui)
	{
		GetComponent<Attackable>().setHealth(gui.healthSlider);
		gui.spearText.text = "Spears: " + weaponHolder.weapons[Tags.Spear];
		gui.hammerText.text = "Hammers: " + weaponHolder.weapons[Tags.Hammer];
		gui.bowText.text = "Bows: " + weaponHolder.weapons[Tags.Bow];
		gui.shieldText.text = "Shields: " + weaponHolder.weapons[Tags.Shield];
	}

	/// <summary>
	/// Placeholder so SendMessage doesn't return null.
	/// </summary>
	public void setController (Player player) {}

	/// <summary>
	/// Gets the closest castle.
	/// </summary>
	/// <returns>The closest castle.</returns>
	CastleController getClosestCastle ()
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
