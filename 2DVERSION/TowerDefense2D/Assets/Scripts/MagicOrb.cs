using Photon.Pun;
using UnityEngine;

public class MagicOrb : MonoBehaviourPunCallbacks
{
    public int Team;
    [Header("Magic Orb Settings")]
    [Range(0.0f, 50.0f)]
    public float DetectAndDestroyRadius;
    [Range(0.0f, 50.0f)]
    public float Damage = 10.0f;
    [Range(0.0f, 10.0f)]
    public float AttackSpeed = 1.0f;
    private float AttackSpeedTimer = 0.0f;
    public LineRenderer lineRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(AttackSpeedTimer > 0){
            AttackSpeedTimer -= Time.deltaTime;
        }
        Collider2D[] collider2D = Physics2D.OverlapCircleAll(transform.position, DetectAndDestroyRadius, LayerMask.GetMask("PawnLayer"));
        foreach (Collider2D collider in collider2D)
        {
            if(AttackSpeedTimer <= 0){
                if (collider.gameObject.tag == "PawnOne" && Team == 2)
                {
                    AttackSpeedTimer = AttackSpeed;
                    photonView.RPC("TakeDamageToEnemy", RpcTarget.All, Damage, collider.gameObject);
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, collider.transform.position);
                }
                else if (collider.gameObject.tag == "PawnTwo" && Team == 1)
                {
                    AttackSpeedTimer = AttackSpeed;
                    photonView.RPC("TakeDamageToEnemy", RpcTarget.All, Damage, collider.gameObject);
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, collider.transform.position);
                }
            }
        }
    }
    [PunRPC]
    public void TakeDamageToEnemy(float damage, GameObject enemy)
    {
        enemy.SendMessage("TakeDamage", damage);
    }
    //draw a circle in the scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetectAndDestroyRadius);
    }
    
}
