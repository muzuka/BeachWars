using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Info gathering functions.
/// Has functions for searching game objects in various ways.
/// </summary>
public static class InfoTool
{
	/// <summary>
	/// Closest object to a position
	/// </summary>
	/// <returns>The object.</returns>
	/// <param name="from">Position to compare.</param>
	/// <param name="to">List to search.</param>
	public static GameObject ClosestObject(Vector3 from, List<GameObject> to)
	{
		float minDistance = float.MaxValue;
		GameObject retObject = null;

		for (int i = 0; i < to.Count; i++) 
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
	public static GameObject ClosestObjectWithTag(GameObject from, string t)
	{
		List<GameObject> to = GameObject.FindObjectsOfType<GameObject>().ToList();
		return ClosestObject(from.transform.position, to.FindAll(x => x.tag == t));
	}

	/// <summary>
	/// Closest object with tag.
	/// </summary>
	/// <returns>The object with tag.</returns>
	/// <param name="from">Position to compare.</param>
	/// <param name="t">Tag filter.</param>
	public static GameObject ClosestObjectWithTag(Vector3 from, string t)
	{
		List<GameObject> temp = new List<GameObject>();
		List<GameObject> to = new List<GameObject>(GameObject.FindObjectsOfType<GameObject>());
		for (int i = 0; i < to.Count; i++)
		{
			if (to[i].tag == t)
				temp.Add(to[i]);
		}

		return ClosestObject(from, temp);
	}

	/// <summary>
	/// Closest object with name.
	/// </summary>
	/// <returns>The object with name.</returns>
	/// <param name="from">Position to compare.</param>
	/// <param name="n">Name.</param>
	public static GameObject ClosestObjectWithName(Vector3 from, string n)
	{
		List<GameObject> temp = new List<GameObject>();
		List<GameObject> to = new List<GameObject>(GameObject.FindObjectsOfType<GameObject>());
		for (int i = 0; i < to.Count; i++)
		{
			if (to[i].name == n)
				temp.Add(to[i]);
		}

		return ClosestObject(from, temp);
	}

	/// <summary>
	/// Closest object with name from list
	/// </summary>
	/// <returns>The object with name.</returns>
	/// <param name="from">Position to compare.</param>
	/// <param name="n">Name.</param>
	/// <param name="to">List to search.</param>
	public static GameObject ClosestObjectWithName(Vector3 from, string n, List<GameObject> to)
	{
		List<GameObject> temp = new List<GameObject>();
		for (int i = 0; i < to.Count; i++)
		{
			if (to[i].name == n)
				temp.Add(to[i]);
		}

		return ClosestObject(from, temp);
	}

	/// <summary>
	/// Finds an idle crab.
	/// Different from Infotool in that it returns the CrabController script.
	/// </summary>
	/// <returns>Crab script.</returns>
	public static CrabController FindIdleCrab(int team)
	{
		List<CrabController> crabList = GameObject.FindObjectsOfType<CrabController>().ToList();
        return crabList.Find(x => x.ActionStates.IsIdle() && team == x.GetComponent<Team>().team);
	}

    public static Vector3 GetAveragePosition(List<GameObject> objects)
    {
        Vector3 sumPosition = new Vector3();

        for (int i = 0; i < objects.Count; i++)
        {
            sumPosition += objects[i].transform.position;
        }

        return new Vector3(sumPosition.x / objects.Count, sumPosition.y / objects.Count, sumPosition.z / objects.Count);
    }

    public static float GetAverageDistance(List<GameObject> objects, Vector3 position)
    {
        float avgDistance = 0.0f;

        for (int i = 0; i < objects.Count; i++)
        {
            avgDistance += Vector3.Distance(objects[i].transform.position, position);
        }

        return avgDistance / objects.Count;
    }

    public static float GetStandardDeviation(float average, List<float> objectDistances)
    {
        return Mathf.Sqrt(StandardDeviationSum(average, objectDistances) / (objectDistances.Count - 1));
    }

    static float StandardDeviationSum(float average, List<float> objectDistances)
    {
        float sum = 0;

        for (int i = 0; i < objectDistances.Count; i++)
        {
            sum += Mathf.Pow(objectDistances[i] - average, 2.0f);
        }

        return sum;
    }
}
