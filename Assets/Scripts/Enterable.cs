using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enterable.
/// Allows object to be entered
/// </summary>
[RequireComponent(typeof(Team))]
public class Enterable : MonoBehaviour{

    [Tooltip("Reference to the canvas object to instantiate.")]
    public GameObject occupiedCanvas;
    [Tooltip("Does the object have functionality that has a time limit?")]
    public bool sliderHidden;
    [Tooltip("Distance from object to instantiate new objects")]
    public float distanceToInstantiate;

	Team team;				// team of enterable object

    public GameObject currentCanvas { get; set; }

	// occupant of enterable object
    public GameObject occupant { get; set; }

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		occupant = null;
		team = GetComponent<Team>();
        currentCanvas = null;
	}

    void Update ()
    {
        if (currentCanvas != null)
        {
            currentCanvas.transform.position = new Vector3(transform.position.x, currentCanvas.transform.position.y, transform.position.z);
        }
    }

    /// <summary>
    /// Is this object occupied?
    /// </summary>
    /// <returns><c>true</c>, if object is occupied, <c>false</c> otherwise.</returns>
    public bool occupied ()
	{
		return (occupant != null);
	}

	/// <summary>
	/// Object requests entry.
	/// </summary>
	/// <param name="enteringObject">Crab or siege weapon.</param>
	public void requestEntry (GameObject enteringObject)
	{
		bool canEnter = false;

		if (!occupied() && (enteringObject.tag == Tags.Crab || IdUtility.isSiegeWeapon(enteringObject.tag)))
		{
			canEnter = enteringObject.GetComponent<Team>().team == team.team;
		}

		if (canEnter)
		{
			occupant = enteringObject;
			enteringObject.SetActive(false);
			FindObjectOfType<Player>().deselect(enteringObject);
            instantiateCanvas(occupant.tag);
            EventManager.TriggerEvent("ObjectEntered");
		}
		else if (GetComponent<DebugComponent>().debug)
			Debug.Log("Couldn't enter " + gameObject.tag);
	}

	/// <summary>
	/// Removes the occupant.
	/// </summary>
	public void removeOccupant ()
	{
		if (occupant != null)
		{
			Vector3 pos = transform.position;
			float dist1 = Random.value * distanceToInstantiate * Random.Range(-1, 1);
			float dist2 = Random.value * distanceToInstantiate * Random.Range(-1, 1);
			occupant.SetActive(true);
			occupant.transform.position = new Vector3(pos.x + dist1, pos.y, pos.z + dist2);
			occupant = null;
            Destroy(currentCanvas);
		}
	}

    void instantiateCanvas (string imageTag) 
    {
        currentCanvas = Instantiate(occupiedCanvas);
        currentCanvas.transform.position = new Vector3(transform.position.x, currentCanvas.transform.position.y, transform.position.z);
        Image[] images = currentCanvas.GetComponentsInChildren<Image>();

        if (sliderHidden)
        {
            Destroy(currentCanvas.GetComponentInChildren<Slider>().gameObject);
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
