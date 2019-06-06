using UnityEngine;

/// <summary>
/// Build building command.
/// </summary>
public class BuildCommand : Command
{
	public GameObject Crab { get; set; }
	public Vector3 Location { get; set; }
	public string BuildingType { get; set; }

	public override void Execute()
	{
		Crab.GetComponent<CrabController> ().StartBuild (BuildingType, Location);
	}
}
