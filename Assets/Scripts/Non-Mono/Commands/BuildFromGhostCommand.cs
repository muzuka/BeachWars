using UnityEngine;

public class BuildFromGhostCommand : Command
{
    public GameObject Crab { get; set; }
    public GameObject GhostBuilding { get; set; }

    public override void Execute()
    {
        Crab.GetComponent<CrabController>().BuildFromGhost(GhostBuilding);
    }
}