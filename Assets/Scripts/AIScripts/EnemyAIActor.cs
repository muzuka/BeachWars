using UnityEngine;

/// <summary>
/// Executes commands given by strategyManager.
/// </summary>
[RequireComponent(typeof(DebugComponent))]
[RequireComponent(typeof(StrategyManager))]
public class EnemyAIActor : MonoBehaviour {

	StrategyManager _controller;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		_controller = GetComponent<StrategyManager>();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update() 
	{
		if (_controller.Commands.Count > 0)
		{
			if (GetComponent<DebugComponent>().IsDebugModeEnabled)
				Debug.Log("Executing command " + _controller.Commands.Peek().GetType());
			_controller.Commands.Dequeue().Execute();
		}

	}
}
