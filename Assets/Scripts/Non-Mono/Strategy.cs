using System;
using System.Collections.Generic;

[System.Serializable]
public class Strategy
{
	// target resources
	public int WoodGoal { get; set; }
	public int StoneGoal { get; set; }

    public float WoodToStoneRatio { get; set; }

	public Queue<string> BuildingQueue { get; set; }

	public Strategy()
	{
		WoodGoal = 0;
		StoneGoal = 0;
		BuildingQueue = new Queue<string>();
	}

	public Strategy(int wood, int stone) {
		WoodGoal = wood;
		StoneGoal = stone;
		BuildingQueue = new Queue<string>();
	}
}
