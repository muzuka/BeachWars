using UnityEngine;
using System.Collections.Generic;

// A general state class
public class StateController {

	// shared states
	Dictionary<string, bool> _stateDict;
	List<string> _stateList;

	public bool Debug { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="StateController"/> class.
	/// </summary>
	public StateController()
	{
		_stateDict = new Dictionary<string, bool>();
		_stateList = new List<string>();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="StateController"/> class.
	/// </summary>
	/// <param name="states">State array.</param>
	public StateController(string[] states)
	{
		_stateDict = new Dictionary<string, bool>();
		_stateList = new List<string>();
		for (int i = 0; i < states.Length; i++)
		{
			_stateList.Add(states[i]);
			_stateDict.Add(states[i], false);
		}
	}

	/// <summary>
	/// Prints the states formatted in a single column.
	/// </summary>
	public void printStates()
	{
		string message = "";
		for (int i = 0; i < _stateList.Count; i++)
        {
            message = message + _stateList[i] + ": " + _stateDict[_stateList[i]] + "\n"; 
        }

		if (Debug)
            UnityEngine.Debug.Log(message);
	}
		
	/// <summary>
	/// Set an individual state.
	/// </summary>
	/// <param name="state">State name.</param>
	/// <param name="value">New value.</param>
	public void SetState(string state, bool value)
	{
		if (Debug)
            UnityEngine.Debug.Assert(_stateDict.ContainsKey(state));
		_stateDict[state] = value;
	}

	/// <summary>
	/// Gets an individual state.
	/// </summary>
	/// <returns><c>true</c>, if state is true, <c>false</c> otherwise.</returns>
	/// <param name="state">State name.</param>
	public bool GetState(string state)
	{
		if (Debug)
            UnityEngine.Debug.Assert(_stateDict.ContainsKey(state));
		return _stateDict[state];
	}

	/// <summary>
	/// Reverts all states to false;
	/// </summary>
	public void ClearStates()
	{
		for (int i = 0; i < _stateList.Count; i++)
			_stateDict[_stateList[i]] = false;
	}

	/// <summary>
	/// Are any states true?
	/// </summary>
	/// <returns><c>true</c>, if no state is true, <c>false</c> otherwise.</returns>
	public bool IsIdle() 
	{
		for (int i = 0; i < _stateList.Count; i++)
		{
			if (_stateDict[_stateList[i]])
            {
                return false; 
            }
		}
		return true;
	}
}
