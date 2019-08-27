using UnityEngine;

/// <summary>
/// Executes commands given by strategyManager.
/// </summary>
[RequireComponent(typeof(DebugComponent))]
[RequireComponent(typeof(StrategyManager))]
public class EnemyAIActor : MonoBehaviour {

	StrategyManager controller;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		controller = GetComponent<StrategyManager>();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () 
	{
		if(controller.commands.Count > 0)
		{
			if (GetComponent<DebugComponent>().debug)
				Debug.Log("Executing command " + controller.commands.Peek().GetType());
			controller.commands.Dequeue().execute();
		}

	}
}
