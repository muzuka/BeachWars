public class CollectResourceGoal : Goal
{
	public string resourceType { get; set; }
	int resourceAmount;

	public CollectResourceGoal (string resource, int amount)
	{
		resourceType = resource;
		resourceAmount = amount;
		finished = false;

	}

	public bool isFinished (int newAmount)
	{
		return newAmount >= resourceAmount;
	}
}
