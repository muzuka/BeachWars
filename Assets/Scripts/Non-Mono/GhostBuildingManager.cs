using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using Math = System.Math;

/// <summary>
/// Ghost building manager.
/// Player holds instance.
/// </summary>
public class GhostBuildingManager
{
	public string BuildingType;

	public bool CanBuild;

	public bool Debug;

	// main ghost building
	public GameObject GhostBuilding;
	public Stack<GameObject> GhostWallSegments;	// stack of blocks

	public float JunctionDistLimit;
	public float BlockSize;

    GameObject _prevJunction;				// tells you if there are three or more junctions
	GameObject _firstJunction;				// starting point
	GameObject _secondJunction;				// gets dragged

	public bool Combined;

	/// <summary>
	/// Initializes a new instance of the <see cref="GhostBuildingManager"/> class.
	/// </summary>
	public GhostBuildingManager()
	{
		CanBuild = true;
		Combined = false;
		BuildingType = null;
		GhostBuilding = null;
		_prevJunction = null;
		_firstJunction = null;
		_secondJunction = null;
		GhostWallSegments = new Stack<GameObject>();
	}

	/// <summary>
	/// Updates the ghost buildings.
	/// Called in Player update function.
	/// </summary>
	public void UpdateGhosts() 
	{
		if (GhostBuilding)
		{
			RaycastHit hit;

			if (GhostBuilding.name.Contains(Tags.Armoury))
            {
                CanBuild = CanBuildArmoury(); 
            }
			else
            {
                CanBuild = GhostBuilding.GetComponent<GhostColorChanger>().CanBuild; 
            }

			hit = Raycaster.ShootMouseRay();

			GhostBuilding.transform.position = new Vector3(hit.point.x, 0f, hit.point.z);

			if (BuildingType == Tags.Junction && (_firstJunction != null && _secondJunction != null))
			{
				CanBuild = CanBuildWall();
				UpdateWallSegments(hit.point);
			}
		}

		if (BuildingType == "Junction")
        {
            CheckJunctionProximity(); 
        }
	}

	/// <summary>
	/// Checks the junction proximity and tells manager whether junctions should combine.
	/// </summary>
	void CheckJunctionProximity()
	{
		if (BuildingType != Tags.Junction)
			return;

		GameObject closestJunction;
		GameObject closestGhostJunction;
		float junctionDist;					// distance to closest junction
		float ghostJunctionDist;			// distance to closest ghost junction

        List<GameObject> gameObjectList = GameObject.FindObjectsOfType<GameObject>().ToList();
        gameObjectList.RemoveAll(x => (x.GetComponent<JunctionController>() == null || x.GetComponent<GhostBuilder>() == null));
        gameObjectList.Remove(GhostBuilding);

        // get closest junction
        List<GameObject> junctionList = gameObjectList.FindAll(x => x.GetComponent<JunctionController>() != null);
        closestJunction = InfoTool.ClosestObject(Raycaster.ShootMouseRay().point, junctionList);
        junctionDist = GetDistanceToGameObject(closestJunction);

        // get closest ghost junction
        List<GameObject> ghostJunctionList = gameObjectList.FindAll(x => x.GetComponent<GhostBuilder>() != null);
        closestGhostJunction = InfoTool.ClosestObject(Raycaster.ShootMouseRay().point, ghostJunctionList);
		ghostJunctionDist = GetDistanceToGameObject(closestGhostJunction);

		if (!Combined)
		{
			if (GhostBuilding != null)
			{
				CombineJunctions(closestJunction, junctionDist, closestGhostJunction, ghostJunctionDist);
			}
		}
		else
        {
            SeparateJunctions(junctionDist, ghostJunctionDist); 
        }
	}

	/// <summary>
	/// Gets the distance to a game object from mouse pos.
	/// </summary>
	/// <returns>The distance to game object, if no object then returns max.</returns>
	/// <param name="obj">Test Object</param>
	float GetDistanceToGameObject(GameObject obj)
	{
		if (obj)
        {
            return Vector3.Distance(obj.transform.position, Raycaster.ShootMouseRay().point); 
        }
		else
        {
            return float.MaxValue; 
        }
	}

	/// <summary>
	/// Combine junctions if close enough.
	/// </summary>
	/// <param name="closestJunction">Closest junction.</param>
	/// <param name="junctionDist">Junction distance.</param>
	/// <param name="closestGhostJunction">Closest ghost junction.</param>
	/// <param name="ghostJunctionDist">Ghost junction distance.</param>
	void CombineJunctions(GameObject closestJunction, float junctionDist, GameObject closestGhostJunction, float ghostJunctionDist)
	{
		// if distance is less than limit
		if (junctionDist < JunctionDistLimit)
        {
            _secondJunction = closestJunction; 
        }
		else if (ghostJunctionDist < JunctionDistLimit)
        {
            _secondJunction = closestGhostJunction; 
        }

		if (junctionDist < JunctionDistLimit || ghostJunctionDist < JunctionDistLimit)
		{
			DestroyGhostBuilding();
			Combined = true;
			UpdateWallSegments(Raycaster.ShootMouseRay().point);
		}
	}

	/// <summary>
	/// Separates the junctions.
	/// Used only in checkJunctionProximity()
	/// </summary>
	/// <param name="junctionDist">Junction distance.</param>
	/// <param name="ghostJunctionDist">Ghost junction distance.</param>
	void SeparateJunctions(float junctionDist, float ghostJunctionDist)
	{
		// if distance is greater than limit
		if ((junctionDist > JunctionDistLimit && ghostJunctionDist > JunctionDistLimit))
		{
			CreateGhostBuilding();
			_secondJunction = GhostBuilding;
			Combined = false;
		}
	}
	
	/// <summary>
	/// Updates the wall segments.
	/// Creates segments and places them between junctions
	/// </summary>
	void UpdateWallSegments(Vector3 mousePos)
	{
		if (_firstJunction == null || _secondJunction == null)
		{
			return;
		}

		mousePos = new Vector3(mousePos.x, 0f, mousePos.z);

		// get number of segments
		float distance = Vector3.Distance(_firstJunction.transform.position, mousePos);
		int targetSegments = (int)Math.Floor(distance / (BlockSize));
		float posStep = (BlockSize / distance);
		
		// moves segments to correct position
		float interval = posStep;
		GameObject[] ghostWallArray = GhostWallSegments.ToArray();

		// add or remove segments as needed
		if (targetSegments < GhostWallSegments.Count)
		{
			GameObject.Destroy(GhostWallSegments.Pop()); 
		}
		else if (targetSegments > GhostWallSegments.Count)
		{
			while (targetSegments > GhostWallSegments.Count)
			{
				GhostWallSegments.Push((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/PreviewStructures/GhostBlock"))); 
			}
		}
		
		for (int i = 0; i < ghostWallArray.Length; i++)
		{
			ghostWallArray[i].transform.position = Vector3.Lerp(_firstJunction.transform.position, mousePos, interval);
			ghostWallArray[i].transform.rotation = Quaternion.LookRotation(mousePos - _firstJunction.transform.position);
			interval += posStep;
		}
		_secondJunction.transform.position = Vector3.Lerp(_firstJunction.transform.position, mousePos, interval + (posStep * 1));
	}

	/// <summary>
	/// Is anything intersecting the armoury?
	/// </summary>
	/// <returns><c>true</c>, if can build armoury, <c>false</c> otherwise.</returns>
	bool CanBuildArmoury()
	{
		// Find model named rack
		// check canBuild
		List<GhostColorChanger> armouryParts = GhostBuilding.GetComponentsInChildren<GhostColorChanger>().ToList();
        GhostColorChanger rack = armouryParts.Find(x => x.gameObject.name == "Rack");
        if (rack)
        {
            return rack.CanBuild; 
        }
        else
        {
            return false; 
        }
	}

	/// <summary>
	/// Is anything intersecting with any part of the wall or junction?
	/// </summary>
	/// <returns><c>true</c>, if entire wall can be built, <c>false</c> otherwise.</returns>
	bool CanBuildWall()
	{
		List<GameObject> walls = GhostWallSegments.ToList().FindAll(x => !x.GetComponent<GhostColorChanger>().CanBuild);
		bool canBuildFirst;
		bool canBuildSecond;

        if (walls.Count > 0)
        {
            return false; 
        }

		if (_firstJunction.CompareTag(Tags.Junction))
        {
            canBuildFirst = true; 
        }
		else
        {
            canBuildFirst = _firstJunction.GetComponent<GhostColorChanger>().CanBuild; 
        }

		if (_secondJunction.CompareTag(Tags.Junction))
        {
            canBuildSecond = true; 
        }
		else
        {
            canBuildSecond = _secondJunction.GetComponent<GhostColorChanger>().CanBuild; 
        }

		return canBuildFirst && canBuildSecond;
	}

	/// <summary>
	/// Current selection has been deselected.
	/// </summary>
	public void Deselect()
	{
		CanBuild = true;
		Combined = false;
		BuildingType = null;
		GhostBuilding = null;
		_prevJunction = null;
		_firstJunction = null;
		_secondJunction = null;
		GhostWallSegments = new Stack<GameObject>();
	}

	/// <summary>
	/// Instantiates ghost building based on current building type
	/// </summary>
	public void CreateGhostBuilding()
	{
		RaycastHit hit = Raycaster.ShootMouseRay();

		GhostBuilding = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/PreviewStructures/Ghost" + BuildingType), 
																	hit.point + new Vector3(0.0f, 1.0f), 
																	Quaternion.identity);
	}

	/// <summary>
	/// Called when junction is placed.
	/// </summary>
	public void PlaceJunction()
	{
		var wall = GhostWallSegments.ToArray();
		for (int i = 0; i < wall.Length; i++)
        {
            wall[i].GetComponent<GhostBuilder>().Placed = true; 
        }
		
		GhostWallSegments.Clear();

		_prevJunction = _firstJunction;
		_firstJunction = GhostBuilding;
		PlaceGhostBuilding();

		CreateGhostBuilding();
		_secondJunction = GhostBuilding;
	}

	/// <summary>
	/// Spawns wall from junction.
	/// </summary>
	public void ExtendJunction()
	{
		if (_firstJunction != null)
			return;

		BuildingType = Tags.Junction;
		_firstJunction = Raycaster.GetObjectAtRay();
	}

	/// <summary>
	/// Places the ghost building.
	/// </summary>
	public void PlaceGhostBuilding()
	{
		if (GhostBuilding == null)
			return;

		GhostBuilding.GetComponent<GhostBuilder>().Placed = true;
		GhostBuilding = null;
	}

	/// <summary>
	/// Destroys the ghost building.
	/// </summary>
	public void DestroyGhostBuilding()
	{
		if (GhostBuilding) {
			if (GhostBuilding.gameObject.tag == Tags.Ghost)
			{
				GhostBuilding.GetComponent<GhostBuilder>().Destroyed();
				GameObject.Destroy(GhostBuilding);
			}
		}
	}

	/// <summary>
	/// Destroys all wall segments.
	/// Destroys junction if it is not connected.
	/// </summary>
	public void DestroyWall() {

		while (GhostWallSegments.Count > 0)
		{
			GameObject wallSegment = GhostWallSegments.Pop();
			wallSegment.GetComponent<GhostBuilder>().Destroyed();
            GameObject.Destroy(wallSegment);
		}

		GhostWallSegments.Clear();

		if (_secondJunction != null)
		{
			if (_prevJunction == null && _firstJunction != null && _firstJunction.tag == Tags.Ghost)
            {
                GameObject.Destroy(_firstJunction); 
            }
			_secondJunction = null;
			_firstJunction = null;
		}
			
		DestroyGhostBuilding();
	}
}
