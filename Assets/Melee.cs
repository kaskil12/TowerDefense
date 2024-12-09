using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections;
public class Melee : MonoBehaviourPunCallbacks
{
    public int Team;
    public NavMeshAgent agent;

    public Transform targetBase; // Set this in the inspector or dynamically at runtime
    public int Health = 100;
    public int Damage = 20;
    public bool canAttack = true;

    void Update()
    {
        if (targetBase != null)
        {
            agent.SetDestination(targetBase.position);
        }
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Pawn") && collider.GetComponent<Melee>().Team != Team)
            {
                agent.SetDestination(collider.transform.position);
                PhotonView pv = collider.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine && canAttack)
                {
                    pv.RPC("TakeDamage", RpcTarget.AllBuffered, Damage);
                    StartCoroutine(Attack());
                }
            }else{
                agent.SetDestination(targetBase.position);
            }
            
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
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
