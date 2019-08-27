using UnityEngine;
/// <summary>
/// Collect command.
/// </summary>
public class CollectCommand : Command
{

	public GameObject crab { get; set; }
	public GameObject resource { get; set; }

	public override void execute ()
	{
		crab.GetComponent<CrabController> ().startCollecting (resource);
	}
}
