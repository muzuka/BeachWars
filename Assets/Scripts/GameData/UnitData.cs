using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "UnitData")]
public class UnitData : ScriptableObject
{
    public Dictionary<string, float> WeaponValues = new Dictionary<string, float>()
    {
        { Tags.Spear, 1.0f },
        { Tags.Bow, 1.5f },
        { Tags.Hammer, 1.0f },
        { Tags.Shield, 0.5f }
    };

    public Dictionary<string, float> WeaponRanges = new Dictionary<string, float>()
    {
        { Tags.Bow, 10.0f },
        {Tags.Spear, 1.5f}
    };

    public Dictionary<Enum.CrabSpecies, string> CrabTypes = new Dictionary<Enum.CrabSpecies, string>()
    {
        { Enum.CrabSpecies.ROCK, "Rock Crab" },
        { Enum.CrabSpecies.FIDDLER, "Fiddler Crab" },
        { Enum.CrabSpecies.TGIANT, "Tasmanian Crab" },
        { Enum.CrabSpecies.SPIDER, "Spider Crab" },
        { Enum.CrabSpecies.COCONUT, "Coconut Crab" },
        { Enum.CrabSpecies.HORSESHOE, "Horseshoe Crab" },
        { Enum.CrabSpecies.SEAWEED, "Seaweed Crab" },
        { Enum.CrabSpecies.CALICO, "Calico Crab" },
        { Enum.CrabSpecies.KAKOOTA, "Kakoota Crab" },
        { Enum.CrabSpecies.TRILOBITE, "Trilobite" },
    };

    public Dictionary<Enum.CrabSpecies, int> CrabSacrifices = new Dictionary<Enum.CrabSpecies, int>()
    {
        { Enum.CrabSpecies.TGIANT, 5 },
        { Enum.CrabSpecies.COCONUT, 2 }
    };
}
