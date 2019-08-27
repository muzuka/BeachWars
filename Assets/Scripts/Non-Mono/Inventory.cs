using UnityEngine;

/// <summary>
/// Inventory class
/// Used by CrabController
/// </summary>
public class Inventory {

	const int invCapacity = 3;
	public string[] inventory { get; set; }
	public bool debug { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Inventory"/> class.
	/// </summary>
	public Inventory()
	{
		inventory = new string[invCapacity];
	}

	/// <summary>
	/// Sets flags according to weapon equipped
	/// </summary>
	/// <param name="weaponStates">Weapon states.</param>
	public void setWeaponFlags (StateController weaponStates)
	{
		weaponStates.clearStates();

		if (!full())
			return;

		for(int i = 0; i < invCapacity; i++) 
		{
			if (inventory[i] != null && IdUtility.isWeapon(inventory[i])) 
			{
				if (inventory[i] == Tags.Spear)
					weaponStates.setState(Tags.Spear, true);
				else if (inventory[i] == Tags.Bow)
					weaponStates.setState(Tags.Bow, true);
				else if (inventory[i] == Tags.Hammer)
					weaponStates.setState(Tags.Hammer, true);
				else if (inventory[i] == Tags.Shield)
					weaponStates.setState(Tags.Shield, true);
			}
		}
	}

	/// <summary>
	/// Adds item to inventory
	/// </summary>
	/// <param name="objectTag">Item tag.</param>
	public void addToInventory (string objectTag)
	{
		for(int i = 0; i < invCapacity; i++) 
		{
			if (inventory[i] == null) 
			{
				if (debug)
					Debug.Log("Item received.");
				inventory[i] = objectTag;
				return;
			}
		}

		if (debug)
			Debug.Log("Inventory is full.");
	}

	/// <summary>
	/// Sorts the inventory.
	/// Object values:
	/// weapons = 0, resources = 1, empty = 2
	/// </summary>
	public void sortInventory ()
	{
		int k;
		string tag1, tag2;

		for(int i = 0; i < invCapacity; i++) 
		{
			k = i;
			for(int j = i+1; j < invCapacity; j++) 
			{

				if (inventory[k] == null)
					tag1 = "null";
				else
					tag1 = inventory[k];

				if (inventory[j] == null)
					tag2 = "null";
				else
					tag2 = inventory[j];

				if (getObjectValue(tag2) < getObjectValue(tag1))
					k = j;
			}

			string temp = inventory[k];
			inventory[k] = inventory[i];
			inventory[i] = temp;
		}
	}

	/// <summary>
	/// Gets the item value.
	/// </summary>
	/// <returns>The item value.</returns>
	/// <param name="tag">Item tag.</param>
	int getObjectValue (string tag)
	{
		switch (tag) {
		case Tags.Stone:
		case Tags.Wood:
			return 3;
		case Tags.Hammer:
		case Tags.Bow:
		case Tags.Spear:
			return 0;
		case Tags.Shield:
			return 1;
		case "null":
			return 4;
		default:
			return -1;
		}
	}

	/// <summary>
	/// Empties the inventory.
	/// </summary>
	public void emptyInventory ()
	{
		for(int i = 0; i < invCapacity; i++)
			inventory[i] = null;
	}

	/// <summary>
	/// Empties the inventory of items with tag.
	/// </summary>
	/// <param name="tag">Tag to delete.</param>
	public void emptyInventory (string tag)
	{
		for(int i = 0; i < invCapacity; i++)
		{
			if(inventory[i] == tag)
				inventory[i] = null;
		}
	}

	/// <summary>
	/// Is the inventory full?
	/// </summary>
	public bool full ()
	{
		for(int i = 0; i < invCapacity; i++)
		{
			if (inventory[i] == null)
				return false;
		}
		return true;
	}

	/// <summary>
	/// Is the inventory empty?
	/// </summary>
	public bool empty ()
	{
		for(int i = 0; i < invCapacity; i++)
		{
			if (inventory[i] != null)
				return false;
		}
		return true;
	}

	/// <summary>
	/// Does inventory contain item with the specified tag?
	/// </summary>
	/// <param name="tag">Item tag.</param>
	public bool contains (string tag)
	{
		for(int i = 0; i < invCapacity; i++)
		{
			if(inventory[i] == tag)
				return true;
		}

		return false;
	}

	/// <summary>
	/// Empties inventory and instantiates contents on the ground.
	/// Usually for when a crab dies.
	/// </summary>
	/// <param name="crab">Reference to crab.</param>
	public void dropInventory (GameObject crab) 
	{
		for(int i = 0; i < invCapacity; i++) 
		{
			if (inventory[i] != null) 
			{
				float dist1 = Random.value * 2 * Random.Range(-1, 1);
				float dist2 = Random.value * 2 * Random.Range(-1, 1);
				Vector3 pos = crab.transform.position;
				Object.Instantiate(getObjectFromTag(inventory[i]), new Vector3(pos.x + dist1, pos.y, pos.z + dist2), Quaternion.identity);
				inventory[i] = null;
			}
		}
	}

	/// <summary>
	/// Removes one item with tag.
	/// Used for crafting.
	/// </summary>
	/// <param name="tag">Item tag.</param>
	public void removeItem (string tag) 
	{
		for(int i = 0; i < invCapacity; i++) 
		{
			if (inventory[i] == tag) 
			{
				inventory[i] = null;
				break;
			}
		}
		sortInventory();
	}

	/// <summary>
	/// Prints the inventory contents.
	/// </summary>
	public void printInventory () 
	{
		string invList = "";
		for(int i = 0; i < inventory.Length; i++) 
		{
			if (inventory[i] != null)
				invList = invList + inventory[i] + " ";
			else
				invList = invList + "nothing ";
		}
			
		if (debug)
			Debug.Log(invList);
	}

	/// <summary>
	/// Counts items in inventory with tag.
	/// For checking crafting conditions.
	/// </summary>
	/// <returns>The count result.</returns>
	/// <param name="tag">Item tag.</param>
	public int countInventory (string tag)
	{
		int count = 0;

		for(int i = 0; i < invCapacity; i++)
		{
			if(inventory[i] == tag)
				count++;
		}

		return count;
	}

	/// <summary>
	/// Does inventory have a weapon in it?
	/// </summary>
	/// <returns><c>true</c>, if armed, <c>false</c> otherwise.</returns>
	public bool isArmed () 
	{
		for(int i = 0; i < invCapacity; i++)
		{
			if (inventory[i] != null) 
			{
				if (getObjectValue(inventory[i]) == 0)
					return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Do all items have the same tag?
	/// </summary>
	/// <returns><c>true</c>, if all items have the same tag, <c>false</c> otherwise.</returns>
	/// <param name="tagName">Item tag.</param>
	public bool isAllOneType (string tagName) 
	{
		for(int i = 0; i < invCapacity; i++) 
		{
			if (inventory[i] != tagName)
				return false;
		}
		return true;
	}

	/// <summary>
	/// Instantiates object from tag.
	/// </summary>
	/// <returns>The object from tag.</returns>
	/// <param name="tag">Tag.</param>
	GameObject getObjectFromTag (string tag) 
	{
		if (IdUtility.isResource(tag))
			return Resources.Load<GameObject>("Prefabs/Resource Objects/" + tag);
		else if (IdUtility.isWeapon(tag))
			return Resources.Load<GameObject>("Prefabs/Weapons/" + tag);
		else
			return null;
	}
}
