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

    public Dictionary<CrabSpecies, string> CrabTypes = new Dictionary<CrabSpecies, string>()
    {
        { CrabSpecies.ROCK, "Rock Crab" },
        { CrabSpecies.FIDDLER, "Fiddler Crab" },
        { CrabSpecies.TGIANT, "Tasmanian Crab" },
        { CrabSpecies.SPIDER, "Spider Crab" },
        { CrabSpecies.COCONUT, "Coconut Crab" },
        { CrabSpecies.HORSESHOE, "Horseshoe Crab" },
        { CrabSpecies.SEAWEED, "Seaweed Crab" },
        { CrabSpecies.CALICO, "Calico Crab" },
        { CrabSpecies.KAKOOTA, "Kakoota Crab" },
        { CrabSpecies.TRILOBITE, "Trilobite" },
    };

    public Dictionary<CrabSpecies, int> CrabSacrifices = new Dictionary<CrabSpecies, int>()
    {
        { CrabSpecies.TGIANT, 5 },
        { CrabSpecies.COCONUT, 2 }
    };
}
