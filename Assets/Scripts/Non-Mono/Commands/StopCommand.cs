using UnityEngine;

class StopCommand : Command
{
    public GameObject crab { get; set; }

    public override void execute ()
    {
        crab.GetComponent<CrabController>().goIdle();
    }
}
