public class ProduceCrabGoal : Goal
{
	int crabAmount;

	public ProduceCrabGoal (int amount)
	{
		crabAmount = amount;
	}

	public bool isFinished (int newAmount)
	{
		return newAmount >= crabAmount;
	}
}
