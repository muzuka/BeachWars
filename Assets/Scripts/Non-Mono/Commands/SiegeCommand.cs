using UnityEngine;
/// <summary>
/// Siege building command.
/// </summary>
public class SiegeCommand : Command
{

	public GameObject siegeWorkshop { get; set; }
	public string buildingType { get; set; }

	public override void execute ()
	{
		siegeWorkshop.GetComponent<WorkshopController> ().startBuilding (buildingType);
	}
}