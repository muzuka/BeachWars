using UnityEngine;
using System.Collections.Generic;
public class MultiBuildCommand : Command
{

	public List<GameObject> crabs { get; set; }
	public Vector3 location { get; set; }
	public string buildingType { get; set; }

	public override void execute ()
	{
		if (crabs.Count > 0) {
			crabs [0].GetComponent<CrabController> ().setCrabs (crabs.Count);
			crabs [0].GetComponent<CrabController> ().startBuild (buildingType, location);
			for (int i = 1; i < crabs.Count; i++) {
				crabs [0].GetComponent<CrabController> ().startMove (location);
			}
		}
	}
}
