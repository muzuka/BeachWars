using UnityEngine;

class TakeWeaponCommand : Command
{
    public string weapon { get; set; }
    public GameObject crab { get; set; }
    public GameObject armoury { get; set; }

    public override void execute()
    {
        crab.GetComponent<CrabController>().startTakeWeapon(weapon, armoury);
    }
}
