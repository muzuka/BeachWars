using UnityEngine;
/// <summary>
/// Collect command.
/// </summary>
public class CollectCommand : Command
{
	public GameObject Crab { get; set; }
	public GameObject Resource { get; set; }

	public override void Execute()
	{
		Crab.GetComponent<CrabController> ().StartCollecting (Resource);
	}
}
