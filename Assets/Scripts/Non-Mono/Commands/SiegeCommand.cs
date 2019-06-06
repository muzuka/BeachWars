using UnityEngine;

/// <summary>
/// Siege building command.
/// </summary>
public class SiegeCommand : Command
{
	public GameObject SiegeWorkshop { get; set; }
	public string BuildingType { get; set; }

	public override void Execute()
	{
		SiegeWorkshop.GetComponent<WorkshopController> ().StartBuilding (BuildingType);
	}
}