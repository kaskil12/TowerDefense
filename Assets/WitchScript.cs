using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections;
using Unity.VisualScripting;

public class WitchScript : MonoBehaviourPunCallbacks
{
    public int Team;
    public NavMeshAgent agent;

    public Transform targetBase; // Set this in the inspector or dynamically at runtime
    public Transform HomeBase;
    public int Health = 100;
    public int Damage;
    public bool canAttack = true;
    public bool OrbShoot = true;
    public bool AttackOpponent;
    public GameObject Orb;

    void Update()
    {
        FindAndAttack();
        agent.avoidancePriority = Random.Range(0, 100);
    }

    void FindAndAttack()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 16f, LayerMask.GetMask("PawnLayer"));
        bool targetFound = false;

        foreach (Collider collider in colliders)
        {
            Melee melee = collider.GetComponent<Melee>();
            if (melee != null && melee.Team != Team || collider.GetComponent<WitchScript>() != null && collider.GetComponent<WitchScript>().Team != Team)
            {
                targetFound = true;
                agent.SetDestination(collider.transform.position);

                PhotonView pv = collider.GetComponent<PhotonView>();
                if (pv != null)
                {
                    if(OrbShoot){

                        PhotonNetwork.Instantiate(Orb.name, transform.position, Quaternion.identity);
                        //add force on the orb using just the transform.forward
                        GameObject Orbs = PhotonNetwork.Instantiate(Orb.name, transform.position, Quaternion.identity);
                        Rigidbody rigidbody = Orb.GetComponent<Rigidbody>();
                        rigidbody.AddForce(transform.forward * 10, ForceMode.Impulse);
                        StartCoroutine(OrbAttack());
                    }
                    if(canAttack){
                        Collider[] OrbColliders = Physics.OverlapSphere(Orb.transform.position, 10f, LayerMask.GetMask("PawnLayer"));
                        foreach (Collider OrbCollider in OrbColliders)
                        {
                            Melee OrbMelee = OrbCollider.GetComponent<Melee>();
                            WitchScript OrbWitch = OrbCollider.GetComponent<WitchScript>();
                            if (OrbMelee != null && OrbMelee.Team != Team || OrbWitch != null && OrbWitch.Team != Team)
                            {
                                PhotonView OrbPv = OrbCollider.GetComponent<PhotonView>();
                                if (OrbPv != null)
                                {
                                    StartCoroutine(Attack());
                                    OrbPv.RPC("TakeDamage", RpcTarget.AllBuffered, Damage);
                                    Destroy(Orb);
                                }
                                break; // Exit loop after attacking one target
                            }
                        }
                    }
                }
                break; // Exit loop after attacking one target
            }
        }

        if (!targetFound && AttackOpponent)
        {
            agent.SetDestination(targetBase.position);
            Debug.Log($"Distance to target base: {Vector3.Distance(transform.position, targetBase.position)}");
            
            if (Vector3.Distance(transform.position, targetBase.position) < 15f)
            {
                
                PhotonView phv = GameObject.Find("RoundManager").GetComponent<PhotonView>();
                if (Team == 1)
                {
                    if(OrbShoot){
                        PhotonNetwork.Instantiate(Orb.name, transform.position, Quaternion.identity);
                        //add force on the orb using just the transform.forward
                        GameObject Orbs = PhotonNetwork.Instantiate(Orb.name, transform.position, Quaternion.identity);
                        Rigidbody rigidbody = Orb.GetComponent<Rigidbody>();
                        rigidbody.AddForce(transform.forward * 10, ForceMode.Impulse);
                        StartCoroutine(OrbAttack());
                    }
                    if(canAttack){
                        Collider[] OrbColliders = Physics.OverlapSphere(Orb.transform.position, 10f, LayerMask.GetMask("PawnLayer"));
                        foreach (Collider OrbCollider in OrbColliders)
                        {
                            Melee OrbMelee = OrbCollider.GetComponent<Melee>();
                            WitchScript OrbWitch = OrbCollider.GetComponent<WitchScript>();
                            if (OrbMelee != null && OrbMelee.Team != Team || OrbWitch != null && OrbWitch.Team != Team && OrbCollider.CompareTag("PlayerTwoBase"))
                            {
                                PhotonView OrbPv = OrbCollider.GetComponent<PhotonView>();
                                if (OrbPv != null)
                                {
                                    OrbPv.RPC("TakeDamage", RpcTarget.AllBuffered, Damage);
                                    StartCoroutine(Attack());
                                    Destroy(Orb);
                                }
                                break; // Exit loop after attacking one target
                            }
                        }
                    }
                }
                else if (Team == 2 && canAttack)
                {
                    if(OrbShoot){
                        PhotonNetwork.Instantiate(Orb.name, transform.position, Quaternion.identity);
                        //add force on the orb using just the transform.forward
                        GameObject Orbs = PhotonNetwork.Instantiate(Orb.name, transform.position, Quaternion.identity);
                        Rigidbody rigidbody = Orb.GetComponent<Rigidbody>();
                        rigidbody.AddForce(transform.forward * 10, ForceMode.Impulse);
                        StartCoroutine(OrbAttack());
                    }
                    if(canAttack){
                        Collider[] OrbColliders = Physics.OverlapSphere(Orb.transform.position, 10f, LayerMask.GetMask("PawnLayer"));
                        foreach (Collider OrbCollider in OrbColliders)
                        {
                            Melee OrbMelee = OrbCollider.GetComponent<Melee>();
                            WitchScript OrbWitch = OrbCollider.GetComponent<WitchScript>();
                            if (OrbMelee != null && OrbMelee.Team != Team || OrbWitch != null && OrbWitch.Team != Team && OrbCollider.CompareTag("PlayerOneBase"))
                            {
                                PhotonView OrbPv = OrbCollider.GetComponent<PhotonView>();
                                if (OrbPv != null)
                                {
                                    OrbPv.RPC("TakeDamage", RpcTarget.AllBuffered, Damage);
                                    StartCoroutine(Attack());
                                    Destroy(Orb);
                                }
                                break; // Exit loop after attacking one target
                            }
                        }
                    }
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
    IEnumerator OrbAttack()
    {
        OrbShoot = false;
        yield return new WaitForSeconds(1);
        OrbShoot = true;
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
