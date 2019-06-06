using UnityEngine;

class StopCommand : Command
{
    public GameObject Crab { get; set; }

    public override void Execute()
    {
        Crab.GetComponent<CrabController>().GoIdle();
    }
}
