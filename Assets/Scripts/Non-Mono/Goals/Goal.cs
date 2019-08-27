using System.Collections;
using System.Collections.Generic;

public abstract class Goal {

	public List<Goal> requirements { get; set; }
	public bool finished { get; set; }

	public bool isFinished() { return false; }
}