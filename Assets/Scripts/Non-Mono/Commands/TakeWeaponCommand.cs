using UnityEngine;

class TakeWeaponCommand : Command
{
    public string Weapon { get; set; }
    public GameObject Crab { get; set; }
    public GameObject Armoury { get; set; }

    public override void Execute()
    {
        Crab.GetComponent<CrabController>().StartTakeWeapon(Weapon, Armoury);
    }
}
