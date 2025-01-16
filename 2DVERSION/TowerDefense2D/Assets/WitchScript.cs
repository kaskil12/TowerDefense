using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System.Collections;
using UnityEngine.UI;
using Unity.VisualScripting;
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
    public Transform HomeBasePosition;

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
    public float stopDistance;

    public float DistanceToHomeBasePositionLocal;
    public GameObject WitchSprite;
    public NavMeshObstacle obstacle;
    public float HomeBasePositionLocalOffset = 2;
    public Vector3 HomeBasePositionLocal;
    public float DetectionRange = 10f;
    public bool isInvincible;



    void Start()
    {
        OrbShoot = true;
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
        if(Health < MaxHealth)
        {
            if(!HealthBar.gameObject.activeSelf)HealthBar.gameObject.SetActive(true);
            HealthBar.value = Health;
            HealthCanvas.transform.rotation = Quaternion.LookRotation(Vector3.back);
        }else if(Health >= MaxHealth)
        {
            if(HealthBar.gameObject.activeSelf)HealthBar.gameObject.SetActive(false);
        }
                DistanceToHomeBasePositionLocal = Vector3.Distance(transform.position, HomeBasePositionLocal);
        //make the unit face the direction it is moving towards by rotating the sprite on the Y axis only
        if(agent.enabled){
            Vector3 movementDirection = agent.velocity.normalized;
            if (agent.enabled && movementDirection.sqrMagnitude > 0.01f) // Check if the unit is moving
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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, DetectionRange, LayerMask.GetMask("PawnLayer"));
        bool targetFound = false;
        Melee nearestUnit = null;
        float nearestDistance = float.MaxValue;
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
                if(agent.enabled)agent.stoppingDistance = 10;
                Vector3 targetCenter = collider.bounds.center;
                OrbSpawnPoint.LookAt(targetCenter);

                if (OrbShoot)
                {
                    OrbShoot = false;
                    StartCoroutine(OrbAttack());
                    SpawnOrb();
                }
            }
            else if (melee != null && melee.Team == Team)
            {
                float distance = Vector3.Distance(transform.position, melee.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestUnit = melee;
                }
            }
        }

        if(Vector3.Distance(transform.position, targetBase.position) < 20f)nearestUnit = null;
        if (!targetFound && AttackOpponent)
        {
            if (nearestUnit != null)
            {
                Vector3 targetPosition = nearestUnit.transform.position - nearestUnit.transform.forward * 5;
                if(agent.enabled)agent.SetDestination(targetPosition);
                if (agent.enabled && agent.velocity.magnitude < 0.1f && !agent.pathPending && agent.remainingDistance > 0.1f)
                {
                    Debug.Log("Agent stuck! Recalculating path.");
                    agent.ResetPath();
                    agent.SetDestination(targetPosition);
                }
                if(agent.enabled)agent.stoppingDistance = 0;

                // Continuously update the destination to follow the melee unit
                if (Vector3.Distance(transform.position, targetPosition) > agent.stoppingDistance)
                {
                    if(agent.enabled)agent.SetDestination(targetPosition);
                }
            }
            else
            {
                if(agent.enabled)agent.SetDestination(targetBase.position);
                if (agent.enabled && agent.velocity.magnitude < 0.1f && !agent.pathPending && agent.remainingDistance > 0.1f)
                {
                    Debug.Log("Agent stuck! Recalculating path.");
                    agent.ResetPath();
                    agent.SetDestination(targetBase.position);
                }
                if(agent.enabled)agent.stoppingDistance = 15;
                if (Vector3.Distance(transform.position, targetBase.position) < 20f && OrbShoot)
                {
                    OrbShoot = false;
                    StartCoroutine(OrbAttack());
                    Vector3 targetCenter = targetBase.gameObject.transform.position;
                    OrbSpawnPoint.LookAt(targetCenter);
                    SpawnOrb();
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
            if (agent.enabled && Vector3.Distance(transform.position, HomeBasePositionLocal) < 5f && agent.velocity.magnitude < 0.1f)
            {
                Debug.Log("Agent close to home base and stuck. Disabling agent and enabling obstacle.");
                agent.enabled = false;
                obstacle.enabled = true;
            }
        }
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
        StartCoroutine(Invincible());
        if (Team == 1)
        {
            targetBase = GameObject.FindWithTag("PlayerTwoBase").transform;
            HomeBasePosition = GameObject.FindWithTag("HomeBaseOne").transform;
            HomeBasePositionLocal = new Vector3(HomeBasePosition.position.x + HomeBasePositionLocalOffset, HomeBasePosition.position.y, HomeBasePosition.position.z);
        }
        else if (Team == 2)
        {
            targetBase = GameObject.FindWithTag("PlayerOneBase").transform;
            HomeBasePosition = GameObject.FindWithTag("HomeBaseTwo").transform;
            HomeBasePositionLocal = new Vector3(HomeBasePosition.position.x - HomeBasePositionLocalOffset, HomeBasePosition.position.y, HomeBasePosition.position.z);
        }
        if(agent.enabled)agent.SetDestination(targetBase.position);
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
    IEnumerator Invincible()
    {
        isInvincible = true;
        yield return new WaitForSeconds(1.5f);
        isInvincible = false;
    }
}
