using UnityEngine;
using Photon.Pun;


public class OrbScript : MonoBehaviourPunCallbacks
{
    public int Team { get; set; }
    public int Damage { get; set; }
    public bool HasDoneDamage = false;
    private SpriteRenderer OrbSprite;
    public Transform Target { get; set; }
    

    private void Start()
    {
        Destroy(gameObject, 2f); 
        OrbSprite = GetComponentInChildren<SpriteRenderer>();
        OrbSprite.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    public void Update()
    {
        if(HasDoneDamage){
            return;
        }
        //make the orb move forward to the target
        if (Target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, Target.position, Time.deltaTime * 10f);
            Vector3 direction = Target.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            OrbSprite.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            transform.position += transform.forward * Time.deltaTime * 10f;
        }
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 2f, LayerMask.GetMask("PawnLayer"));
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.tag == "PawnOne" && Team == 2 || collider.gameObject.tag == "PawnTwo" && Team == 1)
            {
                PhotonView targetPv = collider.GetComponent<PhotonView>();
                targetPv.RPC("TakeDamage", RpcTarget.AllBuffered, Damage);
                HasDoneDamage = true;
                Destroy(gameObject); 
            }
            
        }
        Collider2D[] TowerCollider = Physics2D.OverlapCircleAll(transform.position, 1f);
        foreach (Collider2D collider in TowerCollider)
        {
            if(collider.gameObject.tag == "PlayerOneBase" || collider.gameObject.tag == "PlayerTwoBase"){
                PhotonView phv = GameObject.Find("RoundManager").GetComponent<PhotonView>();   
                if (phv != null)
                {
                    if (Team == 1)
                    {
                        phv.RPC("PlayerTwoDamageTower", RpcTarget.AllBuffered, Damage);
                        HasDoneDamage = true;
                        Destroy(gameObject);
                    }
                    else if (Team == 2)
                    {
                        phv.RPC("PlayerOneDamageTower", RpcTarget.AllBuffered, Damage);
                        HasDoneDamage = true;
                        Destroy(gameObject);
                    }
                }
            }
        }

    }
}
