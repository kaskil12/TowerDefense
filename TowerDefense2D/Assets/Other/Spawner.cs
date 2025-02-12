using UnityEngine;
using Photon.Pun;

public class Spawner : MonoBehaviour
{
    public GameObject playerOnePrefab; // Player prefab for Master Client
    public GameObject playerTwoPrefab; // Player prefab for other players

    void Start()
    {
       
        PhotonNetwork.Instantiate(playerOnePrefab.name, new Vector3(0, 0f, 0f), Quaternion.identity, 0);
        
        
    }
}
