using UnityEngine;

/// <summary>
/// Enter building command.
/// </summary>
public class EnterCommand : Command
{
    public GameObject Crab { get; set; }
    public GameObject Building { get; set; }

    public override void Execute()
    {
        Crab.GetComponent<CrabController>().GoIdle();
        Crab.GetComponent<CrabController>().StartEnter(Building);
    }
}
