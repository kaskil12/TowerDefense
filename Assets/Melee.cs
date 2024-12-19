using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;

public class Melee : MonoBehaviourPunCallbacks
{
    public int Team;
    public NavMeshAgent agent;

    public Transform targetBase; // Set this in the inspector or dynamically at runtime
    public Transform HomeBase;
    public int Health;
    public int Damage;
    public float AttackSpeed;
    public bool canAttack = true;
    public bool AttackOpponent;
    public float DetectRange = 10f;
    public float AttackRange = 2f;
    public Slider HealthBar;
    public int MaxHealth;
    public GameObject HealthCanvas;
    void Start(){
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
            //set healthbar to always face the -z direction of the world
            HealthCanvas.transform.rotation = Quaternion.LookRotation(Vector3.back);
            HealthBar.value = Health;
        }else if(Health >= MaxHealth)
        {
            if(HealthBar.gameObject.activeSelf)HealthBar.gameObject.SetActive(false);
        }

    }

    void FindAndAttack()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, DetectRange, LayerMask.GetMask("PawnLayer"));
        bool targetFound = false;

        foreach (Collider collider in colliders)
        {
            Melee melee = collider.GetComponent<Melee>();
            WitchScript witch = collider.GetComponent<WitchScript>();
            if (melee != null && melee.Team != Team || witch != null && witch.Team != Team)
            {
                targetFound = true;
                agent.SetDestination(collider.transform.position);
                agent.stoppingDistance = 0;

                PhotonView pv = collider.GetComponent<PhotonView>();
                if (pv != null && canAttack && Vector3.Distance(transform.position, collider.transform.position) < AttackRange)
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
            agent.stoppingDistance = 0;
            Debug.Log($"Distance to target base: {Vector3.Distance(transform.position, targetBase.position)}");
            if (Vector3.Distance(transform.position, targetBase.position) < 20f)
            {
                PhotonView phv = GameObject.Find("RoundManager").GetComponent<PhotonView>();
                if (Team == 1 && canAttack && Vector3.Distance(transform.position, targetBase.position) < AttackRange + 3f)
                {
                    phv.RPC("PlayerTwoDamageTower", RpcTarget.AllBuffered, Damage);
                    StartCoroutine(Attack());
                }
                else if (Team == 2 && canAttack && Vector3.Distance(transform.position, targetBase.position) < AttackRange + 3f)
                {
                    phv.RPC("PlayerOneDamageTower", RpcTarget.AllBuffered, Damage);
                    StartCoroutine(Attack());
                }
            }
        }
        else if (!targetFound && !AttackOpponent)
        {
            agent.SetDestination(HomeBase.position);
            agent.stoppingDistance = 5;
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
        yield return new WaitForSeconds(AttackSpeed);
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
    //draw gizmos for the melee
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetectRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }
}
