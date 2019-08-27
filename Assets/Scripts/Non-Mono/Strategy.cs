using System;
using System.Collections.Generic;

[System.Serializable]
public class Strategy
{
	// target resources
	public int woodGoal { get; set; }
	public int stoneGoal { get; set; }

    public float woodToStoneRatio { get; set; }

	public Queue<string> buildingQueue { get; set; }

	public Strategy()
	{
		woodGoal = 0;
		stoneGoal = 0;
		buildingQueue = new Queue<string>();
	}

	public Strategy(int wood, int stone) {
		woodGoal = wood;
		stoneGoal = stone;
		buildingQueue = new Queue<string>();
	}
}
