using UnityEngine;
using UnityEngine.AI;

public class Melee : MonoBehaviour
{
    public int Team;
    public NavMeshAgent agent;

    public Transform targetBase; // Set this in the inspector or dynamically at runtime

    void Update()
    {
        if (targetBase != null)
        {
            agent.SetDestination(targetBase.position);
        }
    }

    public void SetTeam(int team)
    {
        Team = team;

        // Assign target base based on team
        if (Team == 1)
        {
            targetBase = GameObject.FindWithTag("PlayerTwoBase").transform;
        }
        else if (Team == 2)
        {
            targetBase = GameObject.FindWithTag("PlayerOneBase").transform;
        }
    }
}
