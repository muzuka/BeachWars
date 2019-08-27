using UnityEngine;

/// <summary>
/// Enter building command.
/// </summary>
public class EnterCommand : Command
{
    public GameObject crab { get; set; }
    public GameObject building { get; set; }

    public override void execute()
    {
        crab.GetComponent<CrabController>().goIdle();
        crab.GetComponent<CrabController>().startEnter(building);
    }
}
