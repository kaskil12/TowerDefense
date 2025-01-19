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
    public Transform HomeBasePosition;

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

    public float DistanceToHomeBasePositionLocal;
    public GameObject WitchSprite;
    public NavMeshObstacle obstacle;
    public float HomeBasePositionLocalOffset = 5;
    public Vector3 HomeBasePositionLocal;
    public bool isInvincible = false;
    public Animator animator;

    void Start(){
        canAttack = true;
        HealthBar = GetComponentInChildren<Slider>();
        HealthCanvas = GetComponentInChildren<Canvas>().gameObject;
        HealthBar.gameObject.SetActive(false);
        HealthBar.maxValue = Health;
        HealthBar.value = Health;
        agent = GetComponent<NavMeshAgent>();
        //disable auto rotation
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        obstacle.enabled = false;
    }
    void Update()
    {
        FindAndAttack();
        if(agent.enabled)agent.avoidancePriority = Random.Range(0, 100);
        //animation
        if(agent.enabled && agent.velocity.magnitude > 0.1f)
        {
            animator.SetBool("Walking", true);
        }
        else
        {
            animator.SetBool("Walking", false);
        }
        if(Health < MaxHealth)
        {
            if(!HealthBar.gameObject.activeSelf)HealthBar.gameObject.SetActive(true);
            HealthCanvas.transform.rotation = Quaternion.LookRotation(Vector3.back);
            HealthBar.value = Health;
        }else if(Health >= MaxHealth)
        {
            if(HealthBar.gameObject.activeSelf)HealthBar.gameObject.SetActive(false);
        }
        DistanceToHomeBasePositionLocal = Vector3.Distance(transform.position, HomeBasePositionLocal);
        //make the unit face the direction it is moving towards by rotating the sprite on the Y axis only
        if(agent.enabled){
            Vector3 movementDirection = agent.velocity.normalized;
            if (movementDirection.sqrMagnitude > 0.01f) // Check if the unit is moving
            {
                // Flip sprite if moving in the opposite direction
                if (movementDirection.x > 0)
                {
                    WitchSprite.transform.rotation = Quaternion.Euler(0, 0, 0); // Facing right
                }
                else
                {
                    WitchSprite.transform.rotation = Quaternion.Euler(0, 180, 0); // Facing left
                }
            }
        }
        

    }

    void FindAndAttack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, DetectRange, LayerMask.GetMask("PawnLayer"));
        bool targetFound = false;
        DistanceToHomeBasePositionLocal = Vector3.Distance(transform.position, HomeBasePositionLocal);
        foreach (Collider2D collider in colliders)
        {
            Melee melee = collider.GetComponent<Melee>();
            WitchScript witch = collider.GetComponent<WitchScript>();
            if (melee != null && melee.Team != Team && DistanceToHomeBasePositionLocal < 10 || witch != null && witch.Team != Team && DistanceToHomeBasePositionLocal < 10 || melee != null && melee.Team != Team && AttackOpponent || witch != null && witch.Team != Team && AttackOpponent)
            {
                targetFound = true;
                if(agent.enabled)agent.SetDestination(collider.transform.position);
                if (agent.enabled && agent.velocity.magnitude < 0.1f && !agent.pathPending && agent.remainingDistance > 0.1f)
                {
                    Debug.Log("Agent stuck! Recalculating path.");
                    agent.ResetPath();
                    agent.SetDestination(collider.transform.position);
                }
                if(agent.enabled)agent.stoppingDistance = 0;

                PhotonView pv = collider.GetComponent<PhotonView>();
                if (pv != null && canAttack && Vector3.Distance(transform.position, collider.transform.position) < AttackRange)
                {
                    if(agent.enabled)agent.speed = 0;
                    pv.RPC("TakeDamage", RpcTarget.AllBuffered, Damage);
                    animator.SetTrigger("Attack");
                    StartCoroutine(Attack());
                }
                break; 
            }
        }

        if (!targetFound && AttackOpponent)
        {
            if(agent.enabled)agent.SetDestination(targetBase.position);
            if (agent.enabled && agent.velocity.magnitude < 0.1f && !agent.pathPending && agent.remainingDistance > 0.1f)
            {
                Debug.Log("Agent stuck! Recalculating path.");
                agent.ResetPath();
                agent.SetDestination(targetBase.position);
            }
            if(agent.enabled)agent.stoppingDistance = 0;
            Debug.Log($"Distance to target base: {Vector3.Distance(transform.position, targetBase.position)}");
            if (Vector3.Distance(transform.position, targetBase.position) < 20f)
            {
                PhotonView phv = GameObject.Find("RoundManager").GetComponent<PhotonView>();
                if (Team == 1 && canAttack && Vector3.Distance(transform.position, targetBase.position) < AttackRange + 3f)
                {
                    phv.RPC("PlayerTwoDamageTower", RpcTarget.AllBuffered, Damage);
                    animator.SetTrigger("Attack");
                    StartCoroutine(Attack());
                }
                else if (Team == 2 && canAttack && Vector3.Distance(transform.position, targetBase.position) < AttackRange + 3f)
                {
                    phv.RPC("PlayerOneDamageTower", RpcTarget.AllBuffered, Damage);
                    animator.SetTrigger("Attack");
                    StartCoroutine(Attack());
                }
            }
        }
        else if (!targetFound && !AttackOpponent)
        {
            if(agent.enabled)agent.SetDestination(HomeBasePositionLocal);
            if (agent.enabled && agent.velocity.magnitude < 0.1f && !agent.pathPending && agent.remainingDistance > 0.1f)
            {
                Debug.Log("Agent stuck! Recalculating path.");
                agent.ResetPath();
                agent.SetDestination(HomeBasePositionLocal);
            }
            if(agent.enabled)agent.stoppingDistance = stopDistance;

            // Check if the agent is close to the home base and is stuck
            if (agent.enabled && Vector3.Distance(transform.position, HomeBasePositionLocal) < 5f && agent.velocity.magnitude < 0.1f)
            {
                Debug.Log("Agent close to home base and stuck. Disabling agent and enabling obstacle.");
                agent.enabled = false;
                obstacle.enabled = true;
            }
        }

        // Re-enable the agent if any targets come near or if AttackOpponent is true
        if (targetFound || AttackOpponent)
        {
            if (!agent.enabled)
            {
                Debug.Log("Re-enabling agent.");
                agent.enabled = true;
                obstacle.enabled = false;
            }
        }
    }


    [PunRPC]
    public void SetTeam(int team)
    {
        Team = team;
        StartCoroutine(Invincible());
        if (Team == 1)
        {
            targetBase = GameObject.FindWithTag("PlayerTwoBase").transform;
            HomeBasePosition = GameObject.FindWithTag("HomeBaseOne").transform;
            HomeBasePositionLocal = new Vector3(HomeBasePosition.position.x + HomeBasePositionLocalOffset, HomeBasePosition.position.y, HomeBasePosition.position.z);
            if(agent.enabled)agent.SetDestination(targetBase.position);
        }
        else if (Team == 2)
        {
            targetBase = GameObject.FindWithTag("PlayerOneBase").transform;
            HomeBasePosition = GameObject.FindWithTag("HomeBaseTwo").transform;
            HomeBasePositionLocal = new Vector3(HomeBasePosition.position.x - HomeBasePositionLocalOffset, HomeBasePosition.position.y, HomeBasePosition.position.z);
            if(agent.enabled)agent.SetDestination(targetBase.position);
        }
    }

    IEnumerator Attack()
    {
        canAttack = false;
        yield return new WaitForSeconds(AttackSpeed);
        if(agent.enabled)agent.speed = 3.5f;
        canAttack = true;
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        if(!isInvincible)Health -= damage;
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }
    IEnumerator Invincible()
    {
        isInvincible = true;
        yield return new WaitForSeconds(3f);
        isInvincible = false;
    }
}
