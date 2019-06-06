using UnityEngine;
using System.Collections.Generic;

public class MultiBuildCommand : Command
{
	public List<GameObject> Crabs { get; set; }
	public Vector3 Location { get; set; }
	public string BuildingType { get; set; }

	public override void Execute()
	{
		if (Crabs.Count > 0) {
			Crabs [0].GetComponent<CrabController> ().SetCrabs (Crabs.Count);
			Crabs [0].GetComponent<CrabController> ().StartBuild (BuildingType, Location);
			for (int i = 1; i < Crabs.Count; i++) {
				Crabs [0].GetComponent<CrabController> ().StartMove (Location);
			}
		}
	}
}
