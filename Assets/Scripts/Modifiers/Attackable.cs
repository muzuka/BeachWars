using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Attackable.
/// Use if object can be attacked and destroyed
/// </summary>
public class Attackable : MonoBehaviour {

	// maximum health of object
	public float MaxHealth;

	// current health of object
	public float Health;

	public UnityAction Reaction;
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update() 
	{
		if (Health <= 0) 
		{
			if (GetComponent<DebugComponent>().IsDebugModeEnabled)
				Debug.Log(gameObject.name + " ran out of health.");
			
			gameObject.GetComponent<IUnit>().Destroyed();
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// Increases health by amount.
	/// </summary>
	/// <param name="repairAmount">Repair amount.</param>
	public void Repair(float repairAmount)
	{
		if (Health + repairAmount >= MaxHealth)
			Health = MaxHealth;
		else
			Health += repairAmount;
	}

	/// <summary>
	/// Sets the health.
	/// For object initialization only.
	/// </summary>
	/// <param name="health">Health.</param>
	public void SetHealth(float health) 
	{
		this.Health = health;
		MaxHealth = health;
	}

	/// <summary>
	/// Returns the current health of object.
	/// </summary>
	/// <returns>The health.</returns>
	public float GetHealth() 
	{
		return Health;
	}

	/// <summary>
	/// Sets the health for slider object.
	/// </summary>
	/// <param name="slider">Slider script.</param>
	public void SetHealth(Slider slider) 
	{
		slider.value = Health;
		slider.minValue = 0;
		slider.maxValue = MaxHealth;
	}

	/// <summary>
	/// Decreases health by amount.
	/// </summary>
	/// <param name="damage">Damage amount.</param>
	public void Attacked(float damage) 
	{
		Health = Health - damage;
		Reaction.Invoke();
	}
}
