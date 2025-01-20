using UnityEngine;
using Photon.Pun;


public class OrbScript : MonoBehaviourPunCallbacks
{
    public int Team { get; set; }
    public int Damage { get; set; }
    public bool HasDoneDamage = false;

    private void Start()
    {
        Destroy(gameObject, 2f); 
    }
    public void Update()
    {
        if(HasDoneDamage){
            return;
        }
        transform.position += transform.forward * Time.deltaTime * 10f;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f, LayerMask.GetMask("PawnLayer"));
        foreach (Collider2D collider in colliders)
        {
            if(collider.gameObject.tag == "PawnOne" || collider.gameObject.tag == "PawnTwo"){
                PhotonView targetPv = collider.GetComponent<PhotonView>();
                if (targetPv != null)
                {
                    Melee melee = collider.GetComponent<Melee>();
                    WitchScript witch = collider.GetComponent<WitchScript>();

                    if ((melee != null && melee.Team != Team) || (witch != null && witch.Team != Team))
                    {
                        targetPv.RPC("TakeDamage", RpcTarget.AllBuffered, Damage);
                        HasDoneDamage = true;
                        Destroy(gameObject); 
                    }
                }
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
