using UnityEngine;

/// <summary>
/// Inventory class
/// Used by CrabController
/// </summary>
public class Inventory {

	const int _invCapacity = 3;
	public string[] Items { get; set; }
	public bool Debug { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Inventory"/> class.
	/// </summary>
	public Inventory()
	{
		Items = new string[_invCapacity];
	}

	/// <summary>
	/// Sets flags according to weapon equipped
	/// </summary>
	/// <param name="weaponStates">Weapon states.</param>
	public void SetWeaponFlags(StateController weaponStates)
	{
		weaponStates.ClearStates();

		if (!Full())
			return;

		for (int i = 0; i < _invCapacity; i++) 
		{
			if (Items[i] != null && IdUtility.IsWeapon(Items[i])) 
			{
				if (Items[i] == Tags.Spear)
					weaponStates.SetState(Tags.Spear, true);
				else if (Items[i] == Tags.Bow)
					weaponStates.SetState(Tags.Bow, true);
				else if (Items[i] == Tags.Hammer)
					weaponStates.SetState(Tags.Hammer, true);
				else if (Items[i] == Tags.Shield)
					weaponStates.SetState(Tags.Shield, true);
			}
		}
	}

	/// <summary>
	/// Adds item to inventory
	/// </summary>
	/// <param name="objectTag">Item tag.</param>
	public void AddToInventory(string objectTag)
	{
		for (int i = 0; i < _invCapacity; i++) 
		{
			if (Items[i] == null) 
			{
				if (Debug)
                    UnityEngine.Debug.Log("Item received.");
				Items[i] = objectTag;
				return;
			}
		}

		if (Debug)
            UnityEngine.Debug.Log("Inventory is full.");
	}

	/// <summary>
	/// Sorts the inventory.
	/// Object values:
	/// weapons = 0, resources = 1, empty = 2
	/// </summary>
	public void SortInventory()
	{
		int k;
		string tag1, tag2;

		for (int i = 0; i < _invCapacity; i++) 
		{
			k = i;
			for (int j = i+1; j < _invCapacity; j++) 
			{

				if (Items[k] == null)
					tag1 = "null";
				else
					tag1 = Items[k];

				if (Items[j] == null)
					tag2 = "null";
				else
					tag2 = Items[j];

				if (GetObjectValue(tag2) < GetObjectValue(tag1))
					k = j;
			}

			string temp = Items[k];
			Items[k] = Items[i];
			Items[i] = temp;
		}
	}

	/// <summary>
	/// Gets the item value.
	/// </summary>
	/// <returns>The item value.</returns>
	/// <param name="tag">Item tag.</param>
	int GetObjectValue(string tag)
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
	public void EmptyInventory()
	{
		for (int i = 0; i < _invCapacity; i++)
        {
            Items[i] = null; 
        }
	}

	/// <summary>
	/// Empties the inventory of items with tag.
	/// </summary>
	/// <param name="tag">Tag to delete.</param>
	public void EmptyInventory(string tag)
	{
		for (int i = 0; i < _invCapacity; i++)
		{
			if (Items[i] == tag)
            {
                Items[i] = null; 
            }
		}
	}

	/// <summary>
	/// Is the inventory full?
	/// </summary>
	public bool Full()
	{
		for (int i = 0; i < _invCapacity; i++)
		{
			if (Items[i] == null)
            {
                return false; 
            }
		}
		return true;
	}

	/// <summary>
	/// Is the inventory empty?
	/// </summary>
	public bool Empty()
	{
		for (int i = 0; i < _invCapacity; i++)
		{
			if (Items[i] != null)
            {
                return false; 
            }
		}
		return true;
	}

	/// <summary>
	/// Does inventory contain item with the specified tag?
	/// </summary>
	/// <param name="tag">Item tag.</param>
	public bool Contains(string tag)
	{
		for (int i = 0; i < _invCapacity; i++)
		{
			if (Items[i] == tag)
            {
                return true; 
            }
		}

		return false;
	}

	/// <summary>
	/// Empties inventory and instantiates contents on the ground.
	/// Usually for when a crab dies.
	/// </summary>
	/// <param name="crab">Reference to crab.</param>
	public void DropInventory(GameObject crab) 
	{
		for (int i = 0; i < _invCapacity; i++) 
		{
			if (Items[i] != null) 
			{
				float dist1 = Random.value * 2 * Random.Range(-1, 1);
				float dist2 = Random.value * 2 * Random.Range(-1, 1);
				Vector3 pos = crab.transform.position;
				Object.Instantiate(GetObjectFromTag(Items[i]), new Vector3(pos.x + dist1, pos.y, pos.z + dist2), Quaternion.identity);
				Items[i] = null;
			}
		}
	}

	/// <summary>
	/// Removes one item with tag.
	/// Used for crafting.
	/// </summary>
	/// <param name="tag">Item tag.</param>
	public void RemoveItem(string tag) 
	{
		for (int i = 0; i < _invCapacity; i++) 
		{
			if (Items[i] == tag) 
			{
				Items[i] = null;
				break;
			}
		}
		SortInventory();
	}

	/// <summary>
	/// Prints the inventory contents.
	/// </summary>
	public void PrintInventory() 
	{
		string invList = "";
		for (int i = 0; i < Items.Length; i++) 
		{
			if (Items[i] != null)
            {
                invList = invList + Items[i] + " "; 
            }
			else
            {
                invList = invList + "nothing "; 
            }
		}
			
		if (Debug)
            UnityEngine.Debug.Log(invList);
	}

	/// <summary>
	/// Counts items in inventory with tag.
	/// For checking crafting conditions.
	/// </summary>
	/// <returns>The count result.</returns>
	/// <param name="tag">Item tag.</param>
	public int CountInventory(string tag)
	{
		int count = 0;

		for (int i = 0; i < _invCapacity; i++)
		{
			if (Items[i] == tag)
            {
                count++; 
            }
		}

		return count;
	}

	/// <summary>
	/// Does inventory have a weapon in it?
	/// </summary>
	/// <returns><c>true</c>, if armed, <c>false</c> otherwise.</returns>
	public bool IsArmed() 
	{
		for (int i = 0; i < _invCapacity; i++)
		{
			if (Items[i] != null) 
			{
				if (GetObjectValue(Items[i]) == 0)
                {
                    return true; 
                }
			}
		}
		return false;
	}

	/// <summary>
	/// Do all items have the same tag?
	/// </summary>
	/// <returns><c>true</c>, if all items have the same tag, <c>false</c> otherwise.</returns>
	/// <param name="tagName">Item tag.</param>
	public bool IsAllOneType(string tagName) 
	{
		for (int i = 0; i < _invCapacity; i++) 
		{
			if (Items[i] != tagName)
            {
                return false; 
            }
		}
		return true;
	}

	/// <summary>
	/// Instantiates object from tag.
	/// </summary>
	/// <returns>The object from tag.</returns>
	/// <param name="tag">Tag.</param>
	GameObject GetObjectFromTag(string tag) 
	{
		if (IdUtility.IsResource(tag))
        {
            return Resources.Load<GameObject>("Prefabs/Resource Objects/" + tag); 
        }
		else if (IdUtility.IsWeapon(tag))
        {
            return Resources.Load<GameObject>("Prefabs/Weapons/" + tag); 
        }
		else
        {
            return null; 
        }
	}
}
