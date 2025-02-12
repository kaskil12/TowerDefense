using Photon.Pun;
using UnityEngine;
using TMPro;
using System.Collections;
public class MagicOrb : MonoBehaviourPunCallbacks
{
    public int Team;
    [Header("Magic Orb Settings")]
    [Range(0.0f, 50.0f)]
    public float DetectAndDestroyRadius;
    [Range(0.0f, 50.0f)]
    public int Damage = 10;
    [Range(0.0f, 10.0f)]
    public float AttackSpeed = 1.0f;
    private float AttackSpeedTimer = 0.0f;
    public LineRenderer lineRenderer;
    public bool enabled = true;
    private SpriteRenderer OrbSprite;
    public int MagicOrbLevel = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OrbSprite = GetComponentInChildren<SpriteRenderer>();
        OrbSprite.enabled = false;
        enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(enabled == false){
            return;
        }
        switch (MagicOrbLevel)
        {
            case 0:
                enabled = false;
                OrbSprite.enabled = false;
                break;
            case 1:
                Damage = 5;
                AttackSpeed = 3f;
                break;
            case 2:
                Damage = 10;
                AttackSpeed = 2.5f;
                break;
            case 3:
                Damage = 20;
                AttackSpeed = 2f;
                break;
            case 4:
                Damage = 30;
                AttackSpeed = 1.5f;
                break;
            case 5:
                Damage = 40;
                AttackSpeed = 1f;
                break;
            default:
                break;
        }
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
                    PhotonView pv = collider.gameObject.transform.GetComponent<PhotonView>();
                    Debug.Log("MagicOrb 2 : " + Damage);
                    pv.RPC("TakeDamage", RpcTarget.All, Damage);
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, transform.position);
                    lineRenderer.SetPosition(1, collider.transform.position);
                    StartCoroutine(LineRendererOff());
                }
                else if (collider.gameObject.tag == "PawnTwo" && Team == 1)
                {
                    AttackSpeedTimer = AttackSpeed;
                    PhotonView pv = collider.gameObject.transform.GetComponent<PhotonView>();
                    pv.RPC("TakeDamage", RpcTarget.All, Damage);
                    Debug.Log("MagicOrb 1 : " + Damage);
                    lineRenderer.positionCount = 2;
                    //call the line renderer with photon rpc
                    photonView.RPC("ShootLine", RpcTarget.All, collider.transform.position);
                }
            }
        }
    }
    [PunRPC]
    public void Upgrade(){
        if(OrbSprite.enabled == false && enabled == false){
            OrbSprite.enabled = true;
            enabled = true;
        }
        MagicOrbLevel++;
    }
    [PunRPC]
    public void ShootLine(Vector3 target){
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, target);
        StartCoroutine(LineRendererOff());
    }
    IEnumerator LineRendererOff(){
        yield return new WaitForSeconds(0.1f);
        lineRenderer.positionCount = 0;
    }
    //draw a circle in the scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetectAndDestroyRadius);
    }
    
}
