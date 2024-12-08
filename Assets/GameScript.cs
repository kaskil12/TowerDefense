using JetBrains.Annotations;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class GameScript : MonoBehaviourPunCallbacks
{
    private string localPlayerRole;
    public Button spawnButtonOne;
    public GameObject canvas;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            localPlayerRole = "PlayerOne";
        else
            localPlayerRole = "PlayerTwo";

        Debug.Log($"Local Player Role: {localPlayerRole}");
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        
        if (canvas != null){
            canvas.SetActive(true);
        }
        if (localPlayerRole == "PlayerOne")
        {
            HandlePlayerOneInput();
        }
        else if (localPlayerRole == "PlayerTwo")
        {
            HandlePlayerTwoInput();
        }
    }
    public void ButtonClick()
    {
        if(localPlayerRole == "PlayerOne")
        {
            Debug.Log("Player One spawning unit...");
            GameObject unit = PhotonNetwork.Instantiate("MeleeOne", new Vector3(-4f, 0f, 0f), Quaternion.identity);
            unit.GetComponent<Melee>().SetTeam(1);
        }
        else if(localPlayerRole == "PlayerTwo")
        {
            Debug.Log("Player Two spawning unit...");
            GameObject unit = PhotonNetwork.Instantiate("MeleeTwo", new Vector3(4f, 0f, 0f), Quaternion.identity);
            unit.GetComponent<Melee>().SetTeam(2);
        }
    }

    public void HandlePlayerOneInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Player One spawning unit...");
            GameObject unit = PhotonNetwork.Instantiate("MeleeOne", new Vector3(-4f, 0f, 0f), Quaternion.identity);
            unit.GetComponent<Melee>().SetTeam(1);
        }
    }

    public void HandlePlayerTwoInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Player Two spawning unit...");
            GameObject unit = PhotonNetwork.Instantiate("MeleeTwo", new Vector3(4f, 0f, 0f), Quaternion.identity);
            unit.GetComponent<Melee>().SetTeam(2);
        }
    }
}
