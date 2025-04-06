using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GateController : MonoBehaviour, IUnit {

    public int MaxHealth;

    public GameObject GateBlock;

    NavMeshObstacle _obstacle;
    Animator _animator;
    Attackable _health;

    string[] _states = { "Open", "Closed"};
    StateController _gateStates;

    /// <summary>
    /// Start the instance
    /// </summary>
	void Start() 
	{
        _obstacle = GetComponent<NavMeshObstacle>();
        _animator = GetComponent<Animator>();
        _health = GetComponent<Attackable>();
        _health.SetHealth(MaxHealth);
        _gateStates = new StateController(_states);
        _gateStates.SetState("Closed", true);
	}

    /// <summary>
    /// Update the instance
    /// </summary>
    void Update()
    {
        GateBlock.GetComponent<MeshRenderer>().material.color = new Color(0.8f, 0.6f, 0.3f);
    }

    /// <summary>
    /// Triggers the opening animation and allows units through.
    /// </summary>
    public void OpenGate()
    {
        _animator.SetTrigger("Open");
        _obstacle.enabled = false;
        _gateStates.SetState("Open", true);
        _gateStates.SetState("Closed", false);
    }

    /// <summary>
    /// Triggers the closing animation and stops units from going through.
    /// </summary>
    public void CloseGate()
    {
        _animator.SetTrigger("Close");
        _obstacle.enabled = true;
        _gateStates.SetState("Open", false);
        _gateStates.SetState("Closed", true);
    }

    /// <summary>
    /// Is the gate open?
    /// </summary>
    /// <returns>Gate open?</returns>
    public bool IsOpen()
    {
        return _gateStates.GetState("Open");
    }
    
    public void SetController(Player player) {}
    public void SetAttacker(GameObject enemy) {}
    public void UpdateUI(InfoViewController gui) {}
    public void Deselect() {}
    public void ToggleSelected() {}
    public void Destroyed() {}
    public void EnemyDied() {}
}
