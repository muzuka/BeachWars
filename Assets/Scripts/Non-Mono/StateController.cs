using UnityEngine;
using System.Collections.Generic;

// A general state class
public class StateController {

	// shared states
	Dictionary<string, bool> stateDict;
	List<string> stateList;

	public bool debug { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="StateController"/> class.
	/// </summary>
	public StateController()
	{
		stateDict = new Dictionary<string, bool>();
		stateList = new List<string>();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="StateController"/> class.
	/// </summary>
	/// <param name="states">State array.</param>
	public StateController(string[] states)
	{
		stateDict = new Dictionary<string, bool>();
		stateList = new List<string>();
		for(int i = 0; i < states.Length; i++)
		{
			stateList.Add(states[i]);
			stateDict.Add(states[i], false);
		}
	}

	/// <summary>
	/// Prints the states.
	/// </summary>
	public void printStates ()
	{
		string message = "";
		for(int i = 0; i < stateList.Count; i++)
			message = message + stateList[i] + ": " + stateDict[stateList[i]] + "\n";

		if (debug)
			Debug.Log(message);
	}
		
	/// <summary>
	/// Set an individual state.
	/// </summary>
	/// <param name="state">State name.</param>
	/// <param name="value">New value.</param>
	public void setState (string state, bool value)
	{
		if (debug)
			Debug.Assert(stateDict.ContainsKey(state));
		stateDict[state] = value;
	}

	/// <summary>
	/// Gets an individual state.
	/// </summary>
	/// <returns><c>true</c>, if state is true, <c>false</c> otherwise.</returns>
	/// <param name="state">State name.</param>
	public bool getState (string state)
	{
		if (debug)
			Debug.Assert(stateDict.ContainsKey(state));
		return stateDict[state];
	}

	/// <summary>
	/// Reverts all states to false;
	/// </summary>
	public void clearStates ()
	{
		for(int i = 0; i < stateList.Count; i++)
			stateDict[stateList[i]] = false;
	}

	/// <summary>
	/// Are any states true?
	/// </summary>
	/// <returns><c>true</c>, if no state is true, <c>false</c> otherwise.</returns>
	public bool isIdle () 
	{
		for(int i = 0; i < stateList.Count; i++)
		{
			if (stateDict[stateList[i]])
				return false;
		}
		return true;
	}
}
