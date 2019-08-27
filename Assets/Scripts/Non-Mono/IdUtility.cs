/// <summary>
/// Some functions for identifying a tag.
/// </summary>
public static class IdUtility {

	/// <summary>
	/// Is tag a weapon?
	/// </summary>
	/// <returns><c>true</c>, if weapon, <c>false</c> otherwise.</returns>
	/// <param name="weaponTag">tag</param>
	public static bool isWeapon(string weaponTag)
	{
		return weaponTag == Tags.Spear || weaponTag == Tags.Bow || weaponTag == Tags.Hammer || weaponTag == Tags.Shield;
	}

	/// <summary>
	/// Is tag a resource?
	/// </summary>
	/// <returns><c>true</c>, if resource, <c>false</c> otherwise.</returns>
	/// <param name="resourceTag">tag</param>
	public static bool isResource(string resourceTag)
	{
		return resourceTag == Tags.Stone || resourceTag == Tags.Wood;
	}

	/// <summary>
	/// Is tag a siege weapon?
	/// </summary>
	/// <returns><c>true</c>, if siege weapon, <c>false</c> otherwise.</returns>
	/// <param name="siegeTag">tag</param>
	public static bool isSiegeWeapon(string siegeTag)
	{
		return (siegeTag == Tags.Ballista || siegeTag == Tags.Catapult);
	}

	/// <summary>
	/// Is tag a building?
	/// </summary>
	/// <returns><c>true</c>, if building, <c>false</c> otherwise.</returns>
	/// <param name="buildingTag">tag</param>
	public static bool isBuilding(string buildingTag)
	{
		return (buildingTag == Tags.Nest || buildingTag == Tags.Workshop || buildingTag == Tags.Tower ||  buildingTag == Tags.Armoury);
	}

	/// <summary>
	/// Is tag a moveable object.
	/// </summary>
	/// <returns><c>true</c>, if moveable, <c>false</c> otherwise.</returns>
	/// <param name="unitTag">tag</param>
	public static bool isMoveable(string unitTag)
	{
		return (isSiegeWeapon(unitTag) || unitTag == Tags.Crab);
	}
}
