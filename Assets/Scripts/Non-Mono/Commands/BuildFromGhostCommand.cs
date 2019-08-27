using UnityEngine;

public class BuildFromGhostCommand : Command
{

    public GameObject crab { get; set; }
    public GameObject ghostBuilding { get; set; }

    public override void execute()
    {
        crab.GetComponent<CrabController>().buildFromGhost(ghostBuilding);
    }
}