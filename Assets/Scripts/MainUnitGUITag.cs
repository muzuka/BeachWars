using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class MainUnitGUITag : MonoBehaviour {

	Text t;
	string selected;

	void Start () 
	{
		selected = "";
		t = GetComponent<Text>();
	}

	void Update () 
	{
		t.text = selected;
	}

	// Input: THe new string.
	public void changeString (string unit) 
	{
		selected = unit;
	}
}