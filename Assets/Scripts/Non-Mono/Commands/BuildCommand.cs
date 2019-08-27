using UnityEngine;
/// <summary>
/// Build building command.
/// </summary>
public class BuildCommand : Command
{

	public GameObject crab { get; set; }
	public Vector3 location { get; set; }
	public string buildingType { get; set; }

	public override void execute ()
	{
		crab.GetComponent<CrabController> ().startBuild (buildingType, location);
	}
}
