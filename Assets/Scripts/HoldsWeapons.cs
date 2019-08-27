using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages weapons.
/// </summary>
[RequireComponent(typeof(Team))]
public class HoldsWeapons : MonoBehaviour {

    public Dictionary<string, int> weapons { get; set; }

    public int[] weaponAmounts;

	public const int capacity = 20;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		weapons = new Dictionary<string, int>();
		weapons.Add(Tags.Spear, 0);
		weapons.Add(Tags.Hammer, 0);
		weapons.Add(Tags.Bow, 0);
		weapons.Add(Tags.Shield, 0);

        weaponAmounts = new int[weapons.Count];
	}

    void Update()
    {
        weaponAmounts[0] = weapons[Tags.Spear];
        weaponAmounts[1] = weapons[Tags.Hammer];
        weaponAmounts[2] = weapons[Tags.Bow];
        weaponAmounts[3] = weapons[Tags.Shield];
    }

    /// <summary>
    /// Builds the weapon.
    /// </summary>
    /// <param name="weapon">Hammer, Spear, Bow, or Shield tags.</param>
    public void buildWeapon (string weapon)
	{
		weapons[weapon]++;
	}

	/// <summary>
	/// Can crab take a weapon?
	/// </summary>
	/// <returns><c>true</c>, if crab is correct team, <c>false</c> otherwise.</returns>
	/// <param name="crab">Crab object.</param>
	/// <param name="weapon">Hammer, Spear, Bow, or Shield tags.</param>
	public bool requestWeapon (GameObject crab, string weapon)
	{
		if (crab.GetComponent<Team>().team == GetComponent<Team>().team && weapons[weapon] > 0)
		{
			weapons[weapon]--;
			return true;
		}
		else
			return false;
	}

	/// <summary>
	/// Empty the armoury.
	/// </summary>
	public bool empty ()
	{
		return (weapons[Tags.Spear] == 0 && weapons[Tags.Hammer] == 0 && weapons[Tags.Bow] == 0 && weapons[Tags.Shield] == 0);
	}

	/// <summary>
	/// Does the armoury have enough room?
	/// </summary>
	/// <returns><c>true</c>, if armoury has room, <c>false</c> otherwise.</returns>
	/// <param name="weapon">Hammer, Spear, Bow, or Shield tags.</param>
	public bool hasRoom (string weapon)
	{
		return weapons[weapon] != capacity;
	}

	/// <summary>
	/// Percent the armoury is full.
	/// </summary>
	/// <returns>The percentage.</returns>
	public float percentFull ()
	{
		return ((weapons[Tags.Spear] + weapons[Tags.Hammer] + weapons[Tags.Bow] + weapons[Tags.Shield]) / 80.0f) * 100.0f;
	}
}
