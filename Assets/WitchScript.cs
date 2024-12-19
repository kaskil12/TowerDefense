using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections;
using UnityEngine.UI;
public class WitchScript : MonoBehaviourPunCallbacks
{
    [Header("Team Settings")]
    [Tooltip("The team this unit belongs to.")]
    public int Team;

    [Header("Navigation")]
    [Tooltip("NavMeshAgent for controlling the unit's movement.")]
    public NavMeshAgent agent;

    [Tooltip("Target base to attack.")]
    public Transform targetBase;

    [Tooltip("Home base for the unit.")]
    public Transform HomeBase;

    [Header("Health Settings")]
    [Tooltip("Current health of the unit.")]
    public int Health;

    [Tooltip("Maximum health of the unit.")]
    public int MaxHealth;

    [Tooltip("Slider UI for displaying the unit's health.")]
    public Slider HealthBar;

    [Tooltip("Canvas containing the health bar.")]
    public GameObject HealthCanvas;

    [Header("Attack Settings")]
    [Tooltip("Damage dealt by the unit.")]
    public int Damage;

    [Tooltip("Indicates if the unit can attack.")]
    public bool canAttack = true;

    [Tooltip("Indicates if the unit can shoot orbs.")]
    public bool OrbShoot = true;

    [Tooltip("Indicates if the unit is attacking an opponent.")]
    public bool AttackOpponent;

    [Header("Orb Settings")]
    [Tooltip("Prefab for the orb projectile.")]
    public GameObject OrbPrefab;

    [Tooltip("Spawn point for the orb projectile.")]
    public Transform OrbSpawnPoint;


    void Start()
    {
        OrbShoot = true;
        canAttack = true;
        HealthBar = GetComponentInChildren<Slider>();
        HealthCanvas = GetComponentInChildren<Canvas>().gameObject;
        HealthBar.gameObject.SetActive(false);
        HealthBar.maxValue = Health;
        HealthBar.value = Health;
    }

    void Update()
    {
        FindAndAttack();
        agent.avoidancePriority = Random.Range(0, 100);
        if(Health < MaxHealth)
        {
            if(!HealthBar.gameObject.activeSelf)HealthBar.gameObject.SetActive(true);
            HealthBar.value = Health;
            HealthCanvas.transform.rotation = Quaternion.LookRotation(Vector3.back);
        }else if(Health >= MaxHealth)
        {
            if(HealthBar.gameObject.activeSelf)HealthBar.gameObject.SetActive(false);
        }
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
                agent.stoppingDistance = 15;
                Vector3 targetCenter = collider.bounds.center;
                OrbSpawnPoint.LookAt(targetCenter);

                if (OrbShoot)
                {
                    OrbShoot = false;
                    StartCoroutine(OrbAttack());
                    SpawnOrb();
                }
            }
        }

        if (!targetFound && AttackOpponent)
        {
            agent.SetDestination(targetBase.position);
            agent.stoppingDistance = 15;
            if (Vector3.Distance(transform.position, targetBase.position) < 20f && OrbShoot)
            {
                OrbShoot = false;
                StartCoroutine(OrbAttack());
                Vector3 targetCenter = targetBase.gameObject.transform.position;
                OrbSpawnPoint.LookAt(targetCenter);
                SpawnOrb();
            }
        }
        else if (!targetFound && !AttackOpponent)
        {
            agent.SetDestination(HomeBase.position);
            agent.stoppingDistance = 0;
        }
    }

    void SpawnOrb()
    {
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
