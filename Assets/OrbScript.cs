using UnityEngine;
using Photon.Pun;


public class OrbScript : MonoBehaviourPunCallbacks
{
    public int Team { get; set; }
    public int Damage { get; set; }

    private void Start()
    {
        Destroy(gameObject, 2f); 
    }
    public void Update()
    {
        transform.position += transform.forward * Time.deltaTime * 10f;
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f, LayerMask.GetMask("PawnLayer"));
        foreach (Collider collider in colliders)
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
                        Destroy(gameObject); 
                    }
                }
            }
        }
        Collider[] TowerCollider = Physics.OverlapSphere(transform.position, 1f);
        foreach (Collider collider in TowerCollider)
        {
            if(collider.gameObject.tag == "PlayerOneBase" || collider.gameObject.tag == "PlayerTwoBase"){
                PhotonView phv = GameObject.Find("RoundManager").GetComponent<PhotonView>();   
                if (phv != null)
                {
                    if (Team == 1)
                    {
                        phv.RPC("PlayerTwoDamageTower", RpcTarget.AllBuffered, Damage);
                        Destroy(gameObject);
                    }
                    else if (Team == 2)
                    {
                        phv.RPC("PlayerOneDamageTower", RpcTarget.AllBuffered, Damage);
                        Destroy(gameObject);
                    }
                }
            }
        }

    }
}
