using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections;

public class WitchScript : MonoBehaviourPunCallbacks
{
    public int Team;
    public NavMeshAgent agent;
    public Transform targetBase;
    public Transform HomeBase;
    public int Health = 100;
    public int Damage;
    public bool canAttack = true;
    public bool OrbShoot = true;
    public bool AttackOpponent;
    public GameObject OrbPrefab;
    public Transform OrbSpawnPoint;

    void Start()
    {
        OrbShoot = true;
        canAttack = true;
    }

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
            WitchScript witch = collider.GetComponent<WitchScript>();

            if ((melee != null && melee.Team != Team) || (witch != null && witch.Team != Team))
            {
                targetFound = true;
                agent.SetDestination(collider.transform.position);
                OrbSpawnPoint.LookAt(collider.transform);

                if (OrbShoot)
                {
                    OrbShoot = false; // Prevent multiple orbs from being fired
                    StartCoroutine(OrbAttack());
                    SpawnOrb();
                }
            }
        }

        if (!targetFound && AttackOpponent)
        {
            agent.SetDestination(targetBase.position);
            if (Vector3.Distance(transform.position, targetBase.position) < 20f && OrbShoot)
            {
                OrbShoot = false;
                StartCoroutine(OrbAttack());
                SpawnOrb();
            }
        }
        else if (!targetFound && !AttackOpponent)
        {
            agent.SetDestination(HomeBase.position);
        }
    }

    void SpawnOrb()
    {
        OrbSpawnPoint.LookAt(targetBase);
        GameObject orbInstance = PhotonNetwork.Instantiate(OrbPrefab.name, OrbSpawnPoint.transform.position, OrbSpawnPoint.transform.rotation);

        OrbScript orbScript = orbInstance.GetComponent<OrbScript>();
        orbScript.Team = Team;
        orbScript.Damage = Damage;
    }

    [PunRPC]
    public void SetTeam(int team)
    {
        Team = team;
        if (Team == 1)
        {
            targetBase = GameObject.FindWithTag("PlayerTwoBase").transform;
            HomeBase = GameObject.FindWithTag("HomeBaseOne").transform;
        }
        else if (Team == 2)
        {
            targetBase = GameObject.FindWithTag("PlayerOneBase").transform;
            HomeBase = GameObject.FindWithTag("HomeBaseTwo").transform;
        }
        agent.SetDestination(targetBase.position);
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Destroy(gameObject);
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
        yield return new WaitForSeconds(3);
        OrbShoot = true;
    }
}
