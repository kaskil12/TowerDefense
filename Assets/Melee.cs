using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;

public class Melee : MonoBehaviourPunCallbacks
{
    [Header("Team Settings")]
    [Tooltip("The team this unit belongs to.")]
    public int Team;

    [Header("Navigation")]
    [Tooltip("NavMeshAgent for controlling the unit's movement.")]
    public NavMeshAgent agent;

    [Tooltip("The target base the unit is attacking.")]
    public Transform targetBase;

    [Tooltip("The home base of the unit.")]
    public Transform HomeBase;

    [Header("Health Settings")]
    [Tooltip("Current health of the unit.")]
    public int Health;

    [Tooltip("Maximum health of the unit.")]
    public int MaxHealth;

    [Tooltip("Slider UI for displaying the unit's health.")]
    public Slider HealthBar;

    [Tooltip("Canvas object containing the health UI.")]
    public GameObject HealthCanvas;

    [Header("Attack Settings")]
    [Tooltip("Damage dealt by the unit.")]
    public int Damage;

    [Tooltip("Attack speed of the unit.")]
    public float AttackSpeed;

    [Tooltip("Indicates if the unit can attack.")]
    public bool canAttack = true;

    [Tooltip("Indicates if the unit is attacking an opponent.")]
    public bool AttackOpponent;

    [Header("Detection and Attack Ranges")]
    [Tooltip("Range at which the unit detects enemies.")]
    public float DetectRange = 10f;

    [Tooltip("Range at which the unit can attack enemies.")]
    public float AttackRange = 2f;
    [Tooltip("Range at which the unit stops moving towards the target.")]
    public float stopDistance;

    public float DistanceToHomeBase;
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
            HealthCanvas.transform.rotation = Quaternion.LookRotation(Vector3.back);
            HealthBar.value = Health;
        }else if(Health >= MaxHealth)
        {
            if(HealthBar.gameObject.activeSelf)HealthBar.gameObject.SetActive(false);
        }
        DistanceToHomeBase = Vector3.Distance(transform.position, HomeBase.position);

    }

    void FindAndAttack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, DetectRange, LayerMask.GetMask("PawnLayer"));
        bool targetFound = false;

        foreach (Collider2D collider in colliders)
        {
            Melee melee = collider.GetComponent<Melee>();
            WitchScript witch = collider.GetComponent<WitchScript>();
            if (melee != null && melee.Team != Team || witch != null && witch.Team != Team)
            {
                targetFound = true;
                agent.SetDestination(collider.transform.position);
                if (agent.velocity.magnitude < 0.1f && !agent.pathPending && agent.remainingDistance > 0.1f)
                {
                    Debug.Log("Agent stuck! Recalculating path.");
                    agent.ResetPath();
                    agent.SetDestination(collider.transform.position);
                }
                agent.stoppingDistance = 0;

                PhotonView pv = collider.GetComponent<PhotonView>();
                if (pv != null && canAttack && Vector3.Distance(transform.position, collider.transform.position) < AttackRange)
                {
                    pv.RPC("TakeDamage", RpcTarget.AllBuffered, Damage);
                    StartCoroutine(Attack());
                }
                break; 
            }
        }

        if (!targetFound && AttackOpponent)
        {
            agent.SetDestination(targetBase.position);
            if (agent.velocity.magnitude < 0.1f && !agent.pathPending && agent.remainingDistance > 0.1f)
            {
                Debug.Log("Agent stuck! Recalculating path.");
                agent.ResetPath();
                agent.SetDestination(targetBase.position);
            }
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
            if (agent.velocity.magnitude < 0.1f && !agent.pathPending && agent.remainingDistance > 0.1f)
            {
                Debug.Log("Agent stuck! Recalculating path.");
                agent.ResetPath();
                agent.SetDestination(HomeBase.position);
            }
            agent.stoppingDistance = stopDistance;
        }

        Debug.Log($"Agent Destination: {agent.destination}");
        Debug.Log($"Agent Speed: {agent.speed}");
        Debug.Log($"Agent Remaining Distance: {agent.remainingDistance}");
    }

    [PunRPC]
    public void SetTeam(int team)
    {
        Team = team;

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
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetectRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }
}
