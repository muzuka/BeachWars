using UnityEngine;
/// <summary>
/// Attack command.
/// </summary>
public class AttackCommand : Command
{

	public GameObject attacker { get; set; }
	public GameObject target { get; set; }

	public override void execute ()
	{
		if (attacker.tag == "Crab")
			attacker.GetComponent<CrabController> ().startAttack (target);
		else
			attacker.GetComponent<SiegeController> ().startAttack (target);
	}
}
