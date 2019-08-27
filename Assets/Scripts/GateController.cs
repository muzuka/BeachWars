using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GateController : MonoBehaviour {

    public int maxHealth;

    public GameObject gateBlock;

    NavMeshObstacle obstacle;
    Animator animator;
    Attackable health;

    string[] states = { "Open", "Closed"};
    StateController gateStates;

	void Start () 
	{
        obstacle = GetComponent<NavMeshObstacle>();
        animator = GetComponent<Animator>();
        health = GetComponent<Attackable>();
        health.setHealth(maxHealth);
        gateStates = new StateController(states);
        gateStates.setState("Closed", true);
	}

    void Update()
    {
        gateBlock.GetComponent<MeshRenderer>().material.color = new Color(0.8f, 0.6f, 0.3f);
    }

    public void openGate ()
    {
        animator.SetTrigger("Open");
        obstacle.enabled = false;
        gateStates.setState("Open", true);
        gateStates.setState("Closed", false);
    }

    public void closeGate ()
    {
        animator.SetTrigger("Close");
        obstacle.enabled = true;
        gateStates.setState("Open", false);
        gateStates.setState("Closed", true);
    }

    public bool isOpen()
    {
        return gateStates.getState("Open");
    }
}
