using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections;
using Unity.VisualScripting;

public class Melee : MonoBehaviourPunCallbacks
{
    public int Team;
    public NavMeshAgent agent;

    public Transform targetBase; // Set this in the inspector or dynamically at runtime
    public Transform HomeBase;
    public int Health = 100;
    public int Damage = 20;
    public bool canAttack = true;
    public bool AttackOpponent;

    void Update()
    {
        FindAndAttack();
        agent.avoidancePriority = Random.Range(0, 100);
    }

    void FindAndAttack()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f, LayerMask.GetMask("PawnLayer"));
        bool targetFound = false;

        foreach (Collider collider in colliders)
        {
            Melee melee = collider.GetComponent<Melee>();
            WitchScript witch = collider.GetComponent<WitchScript>();
            if (melee != null && melee.Team != Team || witch != null && witch.Team != Team)
            {
                targetFound = true;
                agent.SetDestination(collider.transform.position);

                PhotonView pv = collider.GetComponent<PhotonView>();
                if (pv != null && canAttack)
                {
                    pv.RPC("TakeDamage", RpcTarget.AllBuffered, Damage);
                    StartCoroutine(Attack());
                }
                break; // Exit loop after attacking one target
            }
        }

        if (!targetFound && AttackOpponent)
        {
            agent.SetDestination(targetBase.position);
            Debug.Log($"Distance to target base: {Vector3.Distance(transform.position, targetBase.position)}");
            if (Vector3.Distance(transform.position, targetBase.position) < 20f)
            {
                PhotonView phv = GameObject.Find("RoundManager").GetComponent<PhotonView>();
                if (Team == 1 && canAttack)
                {
                    phv.RPC("PlayerTwoDamageTower", RpcTarget.AllBuffered, Damage);
                    StartCoroutine(Attack());
                }
                else if (Team == 2 && canAttack)
                {
                    phv.RPC("PlayerOneDamageTower", RpcTarget.AllBuffered, Damage);
                    StartCoroutine(Attack());
                }
            }
        }
        else if (!targetFound && !AttackOpponent)
        {
            agent.SetDestination(HomeBase.position);
        }

        // Debug logs
        Debug.Log($"Agent Destination: {agent.destination}");
        Debug.Log($"Agent Speed: {agent.speed}");
        Debug.Log($"Agent Remaining Distance: {agent.remainingDistance}");
    }

    [PunRPC]
    public void SetTeam(int team)
    {
        Team = team;

        // Assign target base based on team
        if (Team == 1)
        {
            targetBase = GameObject.FindWithTag("PlayerTwoBase").transform;
            HomeBase = GameObject.FindWithTag("HomeBaseOne").transform;
            agent.SetDestination(targetBase.position);
        }
        else if (Team == 2)
        {
            targetBase = GameObject.FindWithTag("PlayerOneBase").transform;
            HomeBase = GameObject.FindWithTag("HomeBaseTwo").transform;
            agent.SetDestination(targetBase.position);
        }
    }

    IEnumerator Attack()
    {
        canAttack = false;
        yield return new WaitForSeconds(1);
        canAttack = true;
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        Health -= damage;
        // Destroy the game object across the network
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
