using UnityEngine;
using Photon.Pun;

public class Spawner : MonoBehaviour
{
    public GameObject playerOnePrefab; // Player prefab for Master Client
    public GameObject playerTwoPrefab; // Player prefab for other players

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate(playerOnePrefab.name, new Vector3(0, 0f, 0f), Quaternion.identity, 0);
        }
        else
        {
            PhotonNetwork.Instantiate(playerTwoPrefab.name, new Vector3(1, 0f, 0f), Quaternion.identity, 0);
        }
    }
}
