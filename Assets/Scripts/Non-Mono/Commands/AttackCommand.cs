using UnityEngine;
/// <summary>
/// Attack command.
/// </summary>
public class AttackCommand : Command
{
	public GameObject Attacker { get; set; }
	public GameObject Target { get; set; }

	public override void Execute()
	{
		if (Attacker.tag == "Crab")
        {
            Attacker.GetComponent<CrabController>().StartAttack(Target); 
        }
		else
        {
            Attacker.GetComponent<SiegeController>().StartAttack(Target); 
        }
	}
}
