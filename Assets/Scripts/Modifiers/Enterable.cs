using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enterable.
/// Allows object to be entered
/// </summary>
public class Enterable : MonoBehaviour {

    [Tooltip("Reference to the canvas object to instantiate.")]
    public GameObject OccupiedCanvas;
    [Tooltip("Does the object have functionality that has a time limit?")]
    public bool SliderHidden;
    [Tooltip("Distance from object to instantiate new objects")]
    public float DistanceToInstantiate;

	Team _team;				// team of enterable object

    public GameObject _currentCanvas { get; set; }

	// occupant of enterable object
    public GameObject _occupant { get; set; }

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		_occupant = null;
		_team = GetComponent<Team>();
        _currentCanvas = null;
	}

    /// <summary>
    /// Update this instance
    /// </summary>
    void Update()
    {
        if (_currentCanvas != null)
        {
            _currentCanvas.transform.position = new Vector3(transform.position.x, _currentCanvas.transform.position.y, transform.position.z);
        }
    }

    /// <summary>
    /// Is this object occupied?
    /// </summary>
    /// <returns><c>true</c>, if object is occupied, <c>false</c> otherwise.</returns>
    public bool Occupied()
	{
		return (_occupant != null);
	}

	/// <summary>
	/// Object requests entry.
	/// </summary>
	/// <param name="enteringObject">Crab or siege weapon.</param>
	public void RequestEntry(GameObject enteringObject)
	{
		bool canEnter = !Occupied() && 
		                enteringObject.GetComponent<Team>().team == _team.team &&
		                (IdUtility.IsCrab(enteringObject) || 
		                 IdUtility.IsSiegeWeapon(enteringObject));

		if (canEnter)
		{
			_occupant = enteringObject;
			enteringObject.SetActive(false);
			FindObjectOfType<Player>().Deselect(enteringObject);
            InstantiateCanvas(_occupant.tag);
            EventManager.TriggerEvent("ObjectEntered");
		}
		else if (GetComponent<DebugComponent>().Debug)
			Debug.Log("Couldn't enter " + gameObject.tag);
	}

	/// <summary>
	/// Removes the occupant.
	/// </summary>
	public void RemoveOccupant()
	{
		if (_occupant != null)
		{
			Vector3 pos = transform.position;
			float dist1 = Random.value * DistanceToInstantiate * Random.Range(-1, 1);
			float dist2 = Random.value * DistanceToInstantiate * Random.Range(-1, 1);
			_occupant.SetActive(true);
			_occupant.transform.position = new Vector3(pos.x + dist1, pos.y, pos.z + dist2);
			_occupant = null;
            Destroy(_currentCanvas);
		}
	}

    /// <summary>
    /// Instantiate the occupied canvas
    /// </summary>
    /// <param name="imageTag"></param>
    void InstantiateCanvas(string imageTag) 
    {
        _currentCanvas = Instantiate(OccupiedCanvas);
        _currentCanvas.transform.position = new Vector3(transform.position.x, _currentCanvas.transform.position.y, transform.position.z);
        Image[] images = _currentCanvas.GetComponentsInChildren<Image>();

        if (SliderHidden)
        {
            Destroy(_currentCanvas.GetComponentInChildren<Slider>().gameObject);
        }

        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].name != "Background" && images[i].name != "Fill") 
            {
                if (images[i].name != imageTag)
                {
                    images[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
