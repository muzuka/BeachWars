using UnityEngine;
/// <summary>
/// Arm command.
/// TODO: ArmCommand should tell a crab to take a weapon from an armoury.
/// </summary>
public class ArmCommand : Command
{

	public GameObject crab { get; set; }
	public GameObject armoury { get; set; }

	public override void execute ()
	{

	}
}
