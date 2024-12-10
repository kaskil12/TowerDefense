using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Launchers : MonoBehaviourPunCallbacks
{
    public GameObject playerOnePrefab;
    public GameObject playerTwoPrefab;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.JoinRandomOrCreateRoom(null, 0, MatchmakingMode.FillRoom, null, null, null, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room.");
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Instantiating Player One...");
            PhotonNetwork.Instantiate(playerOnePrefab.name, new Vector3(0, 0f, 0f), Quaternion.identity);
        }
        else
        {
            Debug.Log("Instantiating Player Two...");
            PhotonNetwork.Instantiate(playerTwoPrefab.name, new Vector3(0, 0f, 0f), Quaternion.identity);
        }
    }
}
