using UnityEngine;
using Photon.Pun;


public class OrbScript : MonoBehaviourPunCallbacks
{
    public int Team { get; set; }
    public int Damage { get; set; }

    private void Start()
    {
        // Orb lifespan can be managed here, if necessary
        Destroy(gameObject, 5f); // Destroy after 5 seconds as a fallback
    }
    public void Update()
    {
        // Move the orb forward
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
                        Destroy(gameObject); // Destroy the orb after hitting a target
                    }
                }
            }
        }
        Collider[] TowerCollider = Physics.OverlapSphere(transform.position, 1f);
        foreach (Collider collider in TowerCollider)
        {
            if(collider.gameObject.tag == "TowerOne" || collider.gameObject.tag == "TowerTwo"){
                PhotonView phv = GameObject.Find("RoundManager").GetComponent<PhotonView>();   
                if (phv != null)
                {
                    if (Team == 1)
                    {
                        phv.RPC("PlayerTwoDamageTower", RpcTarget.AllBuffered, Damage);
                        Destroy(gameObject); // Destroy the orb after hitting a target
                    }
                    else if (Team == 2)
                    {
                        phv.RPC("PlayerOneDamageTower", RpcTarget.AllBuffered, Damage);
                        Destroy(gameObject); // Destroy the orb after hitting a target
                    }
                }
            }
        }

    }
}
