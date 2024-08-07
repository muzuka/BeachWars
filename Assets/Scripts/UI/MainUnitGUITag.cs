using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class MainUnitGUITag : MonoBehaviour {

	Text _text;
	string _selected;

	void Start() 
	{
		_selected = "";
		_text = GetComponent<Text>();
	}

	void Update() 
	{
		_text.text = _selected;
	}

	// Input: THe new string.
	public void ChangeString(string unit) 
	{
		_selected = unit;
	}
}