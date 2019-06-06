using UnityEngine;

public class Timer {
	
	float _timeConsumed;
	float _timeLimit;
	public delegate void TimerFunction();

	public Timer(float tl)
	{
		_timeConsumed = 0.0f;
		_timeLimit = tl;
	}

	public void update(TimerFunction func)
	{
		_timeConsumed += Time.deltaTime;
		if (_timeConsumed >= _timeLimit)
		{
			func();
			_timeConsumed = 0.0f;
		}
	}
}
