using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Ghost building manager.
/// Player holds instance.
/// </summary>
public class GhostBuildingManager {

	public string buildingType { get; set; }

	public bool canBuild { get; set; }

	public bool debug { get; set; }

	// main ghost building
	public GameObject ghostBuilding { get; set; }
	public Stack<GameObject> ghostWallSegments { get; set; }	// stack of blocks

	public float junctionDistLimit { get; set; }
    public float blockSize { get; set; }

    GameObject prevJunction;				// tells you if there are three or more junctions
	GameObject firstJunction;				// starting point
	GameObject secondJunction;				// gets dragged

	public bool combined { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="GhostBuildingManager"/> class.
	/// </summary>
	public GhostBuildingManager ()
	{
		canBuild = true;
		combined = false;
		buildingType = null;
		ghostBuilding = null;
		prevJunction = null;
		firstJunction = null;
		secondJunction = null;
		ghostWallSegments = new Stack<GameObject>();
	}

	/// <summary>
	/// Updates the ghost buildings.
	/// Called in Player update function.
	/// </summary>
	public void updateGhosts () 
	{
		if (ghostBuilding)
		{
			RaycastHit hit;

			if (ghostBuilding.name.Contains(Tags.Armoury))
				canBuild = canBuildArmoury();
			else
				canBuild = ghostBuilding.GetComponent<GhostColorChanger>().canBuild;

			hit = Raycaster.shootMouseRay();

			ghostBuilding.transform.position = new Vector3(hit.point.x, 0.0f, hit.point.z);

			if (buildingType == Tags.Junction && (firstJunction != null && secondJunction != null))
			{
				canBuild = canBuildWall();
				updateWallSegments();
			}
		}

		if(buildingType == "Junction")
			checkJunctionProximity();
	}

	/// <summary>
	/// Checks the junction proximity and tells manager whether junctions should combine.
	/// </summary>
	void checkJunctionProximity ()
	{
		if(buildingType != Tags.Junction)
			return;

		GameObject closestJunction;
		GameObject closestGhostJunction;
		float junctionDist;					// distance to closest junction
		float ghostJunctionDist;			// distance to closest ghost junction

		// get closest junction
		closestJunction = InfoTool.closestObjectWithTag(Raycaster.shootMouseRay().point, Tags.Junction);
		junctionDist = getDistanceToGameObject(closestJunction);

		var searchList = new List<GameObject>(UnityEngine.Object.FindObjectsOfType<GameObject>());
		searchList.Remove(ghostBuilding);

		// get closest ghost junction
		closestGhostJunction = InfoTool.closestObjectWithName(Raycaster.shootMouseRay().point, "GhostJunction(Clone)", searchList);
		ghostJunctionDist = getDistanceToGameObject(closestGhostJunction);

		if (!combined)
		{
			if(ghostBuilding != null)
			{
				combineJunctions(closestJunction, junctionDist, closestGhostJunction, ghostJunctionDist);
			}
		}
		else
			separateJunctions(junctionDist, ghostJunctionDist);
	}

	/// <summary>
	/// Gets the distance to a game object from mouse pos.
	/// </summary>
	/// <returns>The distance to game object, if no object then returns max.</returns>
	/// <param name="obj">Test Object</param>
	float getDistanceToGameObject (GameObject obj)
	{
		if (obj)
			return Vector3.Distance(obj.transform.position, Raycaster.shootMouseRay().point);
		else
			return float.MaxValue;
	}

	/// <summary>
	/// Combine junctions if close enough.
	/// Used only in checkJunctionProximity()
	/// </summary>
	/// <param name="closestJunction">Closest junction.</param>
	/// <param name="junctionDist">Junction distance.</param>
	/// <param name="closestGhostJunction">Closest ghost junction.</param>
	/// <param name="ghostJunctionDist">Ghost junction distance.</param>
	void combineJunctions (GameObject closestJunction, float junctionDist, GameObject closestGhostJunction, float ghostJunctionDist)
	{
		// if distance is less than limit
		if (junctionDist < junctionDistLimit)
			secondJunction = closestJunction;
		else if (ghostJunctionDist < junctionDistLimit)
			secondJunction = closestGhostJunction;

		if(junctionDist < junctionDistLimit || ghostJunctionDist < junctionDistLimit)
		{
			destroyGhostBuilding();
			combined = true;
			updateWallSegments();
		}
	}

	/// <summary>
	/// Separates the junctions.
	/// Used only in checkJunctionProximity()
	/// </summary>
	/// <param name="junctionDist">Junction distance.</param>
	/// <param name="ghostJunctionDist">Ghost junction distance.</param>
	void separateJunctions (float junctionDist, float ghostJunctionDist)
	{
		// if distance is greater than limit
		if ((junctionDist > junctionDistLimit && ghostJunctionDist > junctionDistLimit))
		{
			createGhostBuilding();
			secondJunction = ghostBuilding;
			combined = false;
		}
	}

	/// <summary>
	/// Is anything intersecting the armoury?
	/// Armoury has multiple parts so necessary to check all parts.
	/// </summary>
	/// <returns><c>true</c>, if can build armoury, <c>false</c> otherwise.</returns>
	bool canBuildArmoury()
	{
		// Find model named rack
		// check canBuild
		GhostColorChanger[] armouryParts = ghostBuilding.GetComponentsInChildren<GhostColorChanger>();
		for (int i = 0; i < armouryParts.Length; i++)
		{
			if (armouryParts[i].gameObject.name == "Rack")
				return armouryParts[i].canBuild;
		}

		return false;
	}

	/// <summary>
	/// Is anything intersecting with any part of the wall or junction?
	/// </summary>
	/// <returns><c>true</c>, if entire wall can be built, <c>false</c> otherwise.</returns>
	bool canBuildWall()
	{
		GameObject[] walls = ghostWallSegments.ToArray();
		bool canBuildFirst;
		bool canBuildSecond;
		for (int i = 0; i < walls.Length; i++)
		{
			if (!walls[i].GetComponent<GhostColorChanger>().canBuild)
				return false;
		}

		if (firstJunction.tag == Tags.Junction)
			canBuildFirst = true;
		else
			canBuildFirst = firstJunction.GetComponent<GhostColorChanger>().canBuild;

		if (secondJunction.tag == Tags.Junction)
			canBuildSecond = true;
		else
			canBuildSecond = secondJunction.GetComponent<GhostColorChanger>().canBuild;

		return canBuildFirst && canBuildSecond;
	}

	/// <summary>
	/// Current selection has been deselected.
	/// </summary>
	public void deselect ()
	{
		canBuild = true;
		combined = false;
		buildingType = null;
		ghostBuilding = null;
		prevJunction = null;
		firstJunction = null;
		secondJunction = null;
		ghostWallSegments = new Stack<GameObject>();
	}

	/// <summary>
	/// Instantiates ghost building based on current building type
	/// </summary>
	public void createGhostBuilding ()
	{
		RaycastHit hit = Raycaster.shootMouseRay();

		ghostBuilding = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/PreviewStructures/Ghost" + buildingType), 
																	hit.point + new Vector3(0.0f, 1.0f), 
																	Quaternion.identity);
	}

	/// <summary>
	/// Called when junction is placed.
	/// </summary>
	public void placeJunction ()
	{
		var wall = ghostWallSegments.ToArray();
		for (int i = 0; i < wall.Length; i++)
			wall[i].GetComponent<GhostBuilder>().placed = true;
		
		ghostWallSegments.Clear();

		prevJunction = firstJunction;
		firstJunction = ghostBuilding;
		placeGhostBuilding();

		createGhostBuilding();
		secondJunction = ghostBuilding;
	}

	/// <summary>
	/// Spawns wall from junction.
	/// </summary>
	public void extendJunction ()
	{
		if(firstJunction != null)
			return;

		buildingType = Tags.Junction;
		firstJunction = Raycaster.getObjectAtRay();
	}

	/// <summary>
	/// Places the ghost building.
	/// </summary>
	public void placeGhostBuilding ()
	{
		if(ghostBuilding == null)
			return;

		ghostBuilding.GetComponent<GhostBuilder>().placed = true;
		ghostBuilding = null;
	}

	/// <summary>
	/// Destroys the ghost building.
	/// </summary>
	public void destroyGhostBuilding ()
	{
		if (ghostBuilding) {
			if (ghostBuilding.gameObject.tag == Tags.Ghost)
			{
				ghostBuilding.GetComponent<GhostBuilder>().destroyed();
				UnityEngine.Object.Destroy(ghostBuilding);
			}
		}
	}

	/// <summary>
	/// Destroys all wall segments.
	/// Destroys junction if it is not connected.
	/// </summary>
	public void destroyWall () {

		while (ghostWallSegments.Count > 0)
		{
			GameObject seg = ghostWallSegments.Pop();
			seg.GetComponent<GhostBuilder>().destroyed();
			UnityEngine.Object.Destroy(seg);
		}

		ghostWallSegments.Clear();

		if (secondJunction != null)
		{
			if (prevJunction == null && firstJunction != null && firstJunction.tag == Tags.Ghost)
				UnityEngine.Object.Destroy(firstJunction);
			secondJunction = null;
			firstJunction = null;
		}
			
		destroyGhostBuilding();

	}

	/// <summary>
	/// Updates the wall segments.
	/// Creates segments and places them between junctions
	/// </summary>
	void updateWallSegments ()
	{
		if (firstJunction == null || secondJunction == null)
		{
			return;
		}

		// get number of segments
		
		float distance = Vector3.Distance(firstJunction.transform.position, secondJunction.transform.position);
		int targetSegments = (int)Math.Floor(distance / blockSize);
		float posStep = (blockSize / distance);

		// add or remove segments as needed
		if (targetSegments < ghostWallSegments.Count)
			UnityEngine.Object.Destroy(ghostWallSegments.Pop());
		else if (targetSegments > ghostWallSegments.Count)
		{
			while (targetSegments > ghostWallSegments.Count)
				ghostWallSegments.Push((GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/PreviewStructures/GhostBlock")));
		}
	
		// moves segments to correct position
		float interval = posStep;
		GameObject[] ghostWallArray = ghostWallSegments.ToArray();
		for (int i = 0; i < ghostWallArray.Length; i++)
		{
			ghostWallArray[i].transform.position = Vector3.Lerp(firstJunction.transform.position, secondJunction.transform.position, interval);
			ghostWallArray[i].transform.rotation = Quaternion.LookRotation(secondJunction.transform.position - firstJunction.transform.position);
			interval += posStep;
		}
	}
}
