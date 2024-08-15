using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages weapons.
/// </summary>
public class HoldsWeapons : MonoBehaviour {

    public Dictionary<string, int> Weapons { get; set; }

    public int[] WeaponAmounts;

	public static int Capacity = 20;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		Weapons = new Dictionary<string, int>();
		Weapons.Add(Tags.Spear, 0);
		Weapons.Add(Tags.Hammer, 0);
		Weapons.Add(Tags.Bow, 0);
		Weapons.Add(Tags.Shield, 0);

        WeaponAmounts = new int[Weapons.Count];
	}

    /// <summary>
    /// Update this instance
    /// </summary>
    void Update()
    {
        WeaponAmounts[0] = Weapons[Tags.Spear];
        WeaponAmounts[1] = Weapons[Tags.Hammer];
        WeaponAmounts[2] = Weapons[Tags.Bow];
        WeaponAmounts[3] = Weapons[Tags.Shield];
    }

    /// <summary>
    /// Builds the weapon.
    /// </summary>
    /// <param name="weapon">Hammer, Spear, Bow, or Shield tags.</param>
    public void BuildWeapon(string weapon)
	{
		Weapons[weapon]++;
	}

	/// <summary>
	/// Can crab take a weapon?
	/// </summary>
	/// <returns><c>true</c>, if crab is correct team, <c>false</c> otherwise.</returns>
	/// <param name="crab">Crab object.</param>
	/// <param name="weapon">Hammer, Spear, Bow, or Shield tags.</param>
	public bool RequestWeapon(GameObject crab, string weapon)
	{
		if (crab.GetComponent<Team>().OnTeam(GetComponent<Team>().team) && Weapons[weapon] > 0)
		{
			Weapons[weapon]--;
			return true;
		}
		else
        {
            return false; 
        }
	}

	/// <summary>
	/// Empty the armoury.
	/// </summary>
	public bool Empty()
	{
		return (Weapons[Tags.Spear] == 0 && Weapons[Tags.Hammer] == 0 && Weapons[Tags.Bow] == 0 && Weapons[Tags.Shield] == 0);
	}

	/// <summary>
	/// Does the armoury have enough room?
	/// </summary>
	/// <returns><c>true</c>, if armoury has room, <c>false</c> otherwise.</returns>
	/// <param name="weapon">Hammer, Spear, Bow, or Shield tags.</param>
	public bool HasRoom(string weapon)
	{
		return Weapons[weapon] != Capacity;
	}

	/// <summary>
	/// Percent the armoury is full.
	/// </summary>
	/// <returns>The percentage.</returns>
	public float PercentFull()
	{
		return ((Weapons[Tags.Spear] + Weapons[Tags.Hammer] + Weapons[Tags.Bow] + Weapons[Tags.Shield]) / 80.0f) * 100.0f;
	}
}
