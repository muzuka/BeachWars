using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Info gathering functions.
/// Has functions for searching game objects in various ways.
/// </summary>
public static class InfoTool {

	/// <summary>
	/// Gets closest object out of a list.
	/// </summary>
	/// <returns>The object.</returns>
	/// <param name="from">Object to compare.</param>
	/// <param name="to">List to search.</param>
	public static GameObject closestObject (GameObject from, List<GameObject> to)
	{
		float minDistance = float.MaxValue;
		GameObject retObject = null;

		for(int i = 0; i < to.Count; i++) 
		{
			float distance = Vector3.Distance(to[i].transform.position, from.transform.position);
			if (distance < minDistance) 
			{
				minDistance = distance;
				retObject = to[i];
			}
		}

		return retObject;
	}

	/// <summary>
	/// Closest object to a position
	/// </summary>
	/// <returns>The object.</returns>
	/// <param name="from">Position to compare.</param>
	/// <param name="to">List to search.</param>
	public static GameObject closestObject (Vector3 from, List<GameObject> to)
	{
		float minDistance = float.MaxValue;
		GameObject retObject = null;

		for(int i = 0; i < to.Count; i++) 
		{
			float distance = Vector3.Distance(to[i].transform.position, from);
			if (distance < minDistance) 
			{
				minDistance = distance;
				retObject = to[i];
			}
		}

		return retObject;
	}

	/// <summary>
	/// Closest object with tag.
	/// </summary>
	/// <returns>The object.</returns>
	/// <param name="from">Object to compare.</param>
	/// <param name="t">Tag filter.</param>
	public static GameObject closestObjectWithTag (GameObject from, string t)
	{
		List<GameObject> temp = new List<GameObject>();
		List<GameObject> to = new List<GameObject>(GameObject.FindObjectsOfType<GameObject>());
		for(int i = 0; i < to.Count; i++)
		{
			if(to[i].tag == t)
				temp.Add(to[i]);
		}

		return closestObject(from, temp);
	}

	/// <summary>
	/// Closest object with tag.
	/// </summary>
	/// <returns>The object with tag.</returns>
	/// <param name="from">Object to compare.</param>
	/// <param name="t">Tag filter.</param>
	/// <param name="to">List to search.</param>
	public static GameObject closestObjectWithTag (GameObject from, string t, List<GameObject> to) 
	{
		List<GameObject> temp = new List<GameObject>();
		for(int i = 0; i < to.Count; i++)
		{
            if (to[i] != null)
            {
                if (to[i].tag == t)
                    temp.Add(to[i]);
            }
		}

		return closestObject(from, temp);
	}

	/// <summary>
	/// Closest object with tag.
	/// </summary>
	/// <returns>The object with tag.</returns>
	/// <param name="from">Position to compare.</param>
	/// <param name="t">Tag filter.</param>
	public static GameObject closestObjectWithTag (Vector3 from, string t)
	{
		List<GameObject> temp = new List<GameObject>();
		List<GameObject> to = new List<GameObject>(GameObject.FindObjectsOfType<GameObject>());
		for(int i = 0; i < to.Count; i++)
		{
			if(to[i].tag == t)
				temp.Add(to[i]);
		}

		return closestObject(from, temp);
	}

	/// <summary>
	/// Closest object with name.
	/// </summary>
	/// <returns>The object with name.</returns>
	/// <param name="from">Position to compare.</param>
	/// <param name="n">Name.</param>
	public static GameObject closestObjectWithName (Vector3 from, string n)
	{
		List<GameObject> temp = new List<GameObject>();
		List<GameObject> to = new List<GameObject>(GameObject.FindObjectsOfType<GameObject>());
		for(int i = 0; i < to.Count; i++)
		{
			if(to[i].name == n)
				temp.Add(to[i]);
		}

		return closestObject(from, temp);
	}

	/// <summary>
	/// Closest object with name from list
	/// </summary>
	/// <returns>The object with name.</returns>
	/// <param name="from">Position to compare.</param>
	/// <param name="n">Name.</param>
	/// <param name="to">List to search.</param>
	public static GameObject closestObjectWithName (Vector3 from, string n, List<GameObject> to)
	{
		List<GameObject> temp = new List<GameObject>();
		for(int i = 0; i < to.Count; i++)
		{
			if(to[i].name == n)
				temp.Add(to[i]);
		}

		return closestObject(from, temp);
	}

	/// <summary>
	/// Gets an idle crab.
	/// </summary>
	/// <returns>The idle crab.</returns>
	/// <param name="team">Team id.</param>
	/// <param name="crabList">List to search.</param>
	public static GameObject getIdleCrab (int team, List<GameObject> crabList)
	{
		for(int i = 0; i < crabList.Count; i++) 
		{
			if (!crabList[i].GetComponent<CrabController>().isBusy() && crabList[i].GetComponent<Team>().team == team)
				return crabList[i];
		}
		return null;
	}

	/// <summary>
	/// Finds an idle crab.
	/// Different from Infotool in that it returns the CrabController script.
	/// </summary>
	/// <returns>Crab script.</returns>
	public static CrabController findIdleCrab (int team)
	{
		var crabList = GameObject.FindObjectsOfType<CrabController>();

		for(int i = 0; i < crabList.Length; i++) 
		{
			if (crabList[i].actionStates.isIdle() && team == crabList[i].GetComponent<Team>().team)
				return crabList[i];
		}
		return null;
	}

    public static Vector3 getAveragePosition(List<GameObject> objects)
    {
        Vector3 sumPosition = new Vector3();

        for (int i = 0; i < objects.Count; i++)
        {
            sumPosition += objects[i].transform.position;
        }

        return new Vector3(sumPosition.x / objects.Count, sumPosition.y / objects.Count, sumPosition.z / objects.Count);
    }

    public static float getAverageDistance(List<GameObject> objects, Vector3 position)
    {
        float avgDistance = 0.0f;

        for(int i = 0; i < objects.Count; i++)
        {
            avgDistance += Vector3.Distance(objects[i].transform.position, position);
        }

        return avgDistance / objects.Count;
    }

    public static float getStandardDeviation(float average, List<float> objectDistances)
    {
        return Mathf.Sqrt(standardDeviationSum(average, objectDistances) / (objectDistances.Count - 1));
    }

    static float standardDeviationSum(float average, List<float> objectDistances)
    {
        float sum = 0;

        for (int i = 0; i < objectDistances.Count; i++)
        {
            sum += Mathf.Pow(objectDistances[i] - average, 2.0f);
        }

        return sum;
    }
}
