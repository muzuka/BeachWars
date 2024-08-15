using UnityEngine;

/// <summary>
/// Some functions for identifying a tag.
/// </summary>
public static class IdUtility 
{

	/// <summary>
	/// Is tag a crab?
	/// </summary>
	/// <param name="tag"></param>
	/// <returns></returns>
	public static bool IsCrab(GameObject obj)
	{
		return obj.CompareTag(Tags.Crab);
	}
	
	/// <summary>
	/// Is tag a weapon?
	/// </summary>
	/// <returns><c>true</c>, if weapon, <c>false</c> otherwise.</returns>
	/// <param name="weaponTag">tag</param>
	public static bool IsWeapon(GameObject obj)
	{
		return obj.CompareTag(Tags.Spear) || 
		       obj.CompareTag(Tags.Bow) || 
		       obj.CompareTag(Tags.Hammer) || 
		       obj.CompareTag(Tags.Shield);
	}
	
	/// <summary>
	/// Is tag a weapon?
	/// </summary>
	/// <returns><c>true</c>, if weapon, <c>false</c> otherwise.</returns>
	/// <param name="weaponTag">tag</param>
	public static bool IsWeapon(string tag)
	{
		return tag == Tags.Spear || 
		       tag == Tags.Bow || 
		       tag == Tags.Hammer || 
		       tag == Tags.Shield;
	}

	/// <summary>
	/// Is tag a resource?
	/// </summary>
	/// <returns><c>true</c>, if resource, <c>false</c> otherwise.</returns>
	/// <param name="resourceTag">tag</param>
	public static bool IsResource(GameObject obj)
	{
		return obj.CompareTag(Tags.Stone) || obj.CompareTag(Tags.Wood);
	}
	
	/// <summary>
	/// Is tag a resource?
	/// </summary>
	/// <returns><c>true</c>, if resource, <c>false</c> otherwise.</returns>
	/// <param name="resourceTag">tag</param>
	public static bool IsResource(string tag)
	{
		return tag == Tags.Stone || tag == Tags.Wood;
	}

	/// <summary>
	/// Is tag a siege weapon?
	/// </summary>
	/// <returns><c>true</c>, if siege weapon, <c>false</c> otherwise.</returns>
	/// <param name="siegeTag">tag</param>
	public static bool IsSiegeWeapon(GameObject obj)
	{
		return obj.CompareTag(Tags.Ballista) || obj.CompareTag(Tags.Catapult);
	}

	/// <summary>
	/// Is tag a building?
	/// </summary>
	/// <returns><c>true</c>, if building, <c>false</c> otherwise.</returns>
	/// <param name="buildingTag">tag</param>
	public static bool IsBuilding(GameObject obj)
	{
		return obj.CompareTag(Tags.Nest) || 
		        obj.CompareTag(Tags.Workshop) || 
		        obj.CompareTag(Tags.Tower) ||  
		        obj.CompareTag(Tags.Armoury);
	}
	
	/// <summary>
	/// Is tag a building?
	/// </summary>
	/// <returns><c>true</c>, if building, <c>false</c> otherwise.</returns>
	/// <param name="buildingTag">tag</param>
	public static bool IsBuilding(string tag)
	{
		return tag == Tags.Nest || 
		       tag == Tags.Workshop || 
		       tag == Tags.Tower ||  
		       tag == Tags.Armoury;
	}

	public static bool IsWallPart(string tag)
	{
		return tag == Tags.Block || 
		       tag == Tags.Gate || 
		       tag == Tags.Junction;
	}

	/// <summary>
	/// Is tag a moveable object.
	/// </summary>
	/// <returns><c>true</c>, if moveable, <c>false</c> otherwise.</returns>
	/// <param name="unitTag">tag</param>
	public static bool IsMoveable(GameObject obj)
	{
		return IsSiegeWeapon(obj) || IsCrab(obj);
	}
}
