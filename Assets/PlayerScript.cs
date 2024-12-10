using System.Collections;
using JetBrains.Annotations;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class PlayerScript : MonoBehaviourPunCallbacks
{
    private string localPlayerRole = null;
    public Button spawnButtonOne;
    public GameObject canvas;
    public GameObject WaitingForPlayers;
    public TMP_Text CurrentPlayer;
    public TMP_Text coinText;
    public int Coins = 200;
    public Transform CameraTransform;
    public Transform[] CameraPositions;
    public Vector3 CameraTargetPos;
    public bool AttackMode = false;

    void Start()
    {
        localPlayerRole = null;
        StartCoroutine(GetCoins());

        // Determine the local player's index in the player list
        
        AssignPlayerRole();
        if (!photonView.IsMine)
        {
            CameraTransform.gameObject.SetActive(false);
        }
        else
        {
            CameraTransform.gameObject.SetActive(true);
        }
    }
    bool player1Taken = false;
    bool player2Taken = false;
    public void AssignPlayerRole(){
        // int playerIndex = System.Array.IndexOf(PhotonNetwork.PlayerList, PhotonNetwork.LocalPlayer);
        
        if(GameObject.Find("One") != null)
        {
            player1Taken = true;
        }
        if(GameObject.Find("Two") != null)
        {
            player2Taken = true;
        }
        // Assign roles based on the player's index
        if (!player1Taken && localPlayerRole == null)
        {
            localPlayerRole = "PlayerOne";
            CameraTargetPos = CameraPositions[0].position;
            photonView.RPC("SyncRole", RpcTarget.AllBuffered, "One");
        }
        else if (!player2Taken && localPlayerRole == null)
        {
            localPlayerRole = "PlayerTwo";
            CameraTargetPos = CameraPositions[2].position;
            photonView.RPC("SyncRole", RpcTarget.AllBuffered, "Two");
        }
        else
        {
            Debug.LogWarning("More than two players in the room or roles already assigned.");
            CameraTargetPos = CameraPositions[1].position; // Adjust as necessary
        }

        Debug.Log($"Local Player Role: {localPlayerRole}");
        CurrentPlayer.text = localPlayerRole;
    }
    [PunRPC]
    public void SyncRole(string role)
    {
        gameObject.name = role;
        CurrentPlayer.text = localPlayerRole;
    }


    void Update()
    {
        if (!photonView.IsMine) return;
        
        if (canvas != null){
            canvas.SetActive(true);
        }
        RoundManager roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();
        if(roundManager.Started == false && WaitingForPlayers.activeSelf == false)
        {
            WaitingForPlayers.SetActive(true);
        }
        else if(WaitingForPlayers.activeSelf == true && roundManager.Started == true)
        {
            WaitingForPlayers.SetActive(false);
        }
        // if (localPlayerRole == "PlayerOne")
        // {
        //     HandlePlayerOneInput();
        // }
        // else if (localPlayerRole == "PlayerTwo")
        // {
        //     HandlePlayerTwoInput();
        // }
        coinText.text = $"Coins: {Coins}";
        CameraTransform.position = Vector3.Lerp(CameraTransform.position, CameraTargetPos, 1f * Time.deltaTime);
    }
    public void NextPos()
    {
        //go to the next camera position in the array of 3 CamPositions array
        for (int i = 0; i < CameraPositions.Length; i++)
        {
            if (CameraTargetPos == CameraPositions[i].position)
            {
            CameraTargetPos = CameraPositions[(i + 1) % CameraPositions.Length].position;
            break;
            }
        }


    }
    public void PreviousPos()
    {
        //go to the previous camera position in the array of 3 CamPositions array
        for (int i = 0; i < CameraPositions.Length; i++)
        {
            if (CameraTargetPos == CameraPositions[i].position)
            {
            CameraTargetPos = CameraPositions[(i + 2) % CameraPositions.Length].position;
            break;
            }
        }
    }
    IEnumerator GetCoins()
    {
        RoundManager roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();
       
        while (true)
        {
            yield return new WaitForSeconds(1);
             if(roundManager.Started == true)
            {
                Coins += 10;   
            }
        }
    }
    public void SpawnButton()
    {
        RoundManager roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();
        if(roundManager.Started == false) return;
        if(localPlayerRole == "PlayerOne" && Coins >= 50)
        {
            Debug.Log("Player One spawning unit...");
            Transform PlayerOneSpawn = GameObject.FindGameObjectWithTag("HomeBaseOne").transform;
            GameObject unit = PhotonNetwork.Instantiate("MeleeOne", PlayerOneSpawn.position, Quaternion.identity);
            unit.GetComponent<Melee>().SetTeam(1);
            unit.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllBuffered, 1);
            Coins -= 50;
        }
        else if(localPlayerRole == "PlayerTwo" && Coins >= 50)
        {
            Debug.Log("Player Two spawning unit...");
            Transform PlayerTwoSpawn = GameObject.FindGameObjectWithTag("HomeBaseTwo").transform;
            GameObject unit = PhotonNetwork.Instantiate("MeleeTwo", PlayerTwoSpawn.position, Quaternion.identity);
            unit.GetComponent<Melee>().SetTeam(2);
            unit.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllBuffered, 2);
            Coins -= 50;
        }
        if(localPlayerRole == "PlayerOne")
        {
            Debug.Log("Player One toggling attack...");
            GameObject[] units = GameObject.FindGameObjectsWithTag("PawnOne");
            foreach (GameObject unit in units)
            {
                var melee = unit.GetComponent<Melee>();
                if (melee != null)
                {
                    melee.AttackOpponent = AttackMode;
                    Debug.Log($"{unit.name}'s AttackOpponent set to {AttackMode}");
                }
                else
                {
                    Debug.LogWarning($"Melee component not found on {unit.name}");
                }
            }
        }
        else if(localPlayerRole == "PlayerTwo")
        {
            GameObject[] units = GameObject.FindGameObjectsWithTag("PawnTwo");
            foreach (GameObject unit in units)
            {
                var melee = unit.GetComponent<Melee>();
                if (melee != null)
                {
                    melee.AttackOpponent = AttackMode;
                    Debug.Log($"{unit.name}'s AttackOpponent set to {AttackMode}");
                }
                else
                {
                    Debug.LogWarning($"Melee component not found on {unit.name}");
                }
            }
        }
    }
    public void ToggleAttack()
    {
        AttackMode = !AttackMode;
        Debug.Log($"AttackMode set to {AttackMode}");
        if(localPlayerRole == "PlayerOne")
        {
            Debug.Log("Player One toggling attack...");
            GameObject[] units = GameObject.FindGameObjectsWithTag("PawnOne");
            foreach (GameObject unit in units)
            {
                var melee = unit.GetComponent<Melee>();
                if (melee != null)
                {
                    melee.AttackOpponent = AttackMode;
                    Debug.Log($"{unit.name}'s AttackOpponent set to {AttackMode}");
                }
                else
                {
                    Debug.LogWarning($"Melee component not found on {unit.name}");
                }
            }
        }
        else if(localPlayerRole == "PlayerTwo")
        {
            GameObject[] units = GameObject.FindGameObjectsWithTag("PawnTwo");
            foreach (GameObject unit in units)
            {
                var melee = unit.GetComponent<Melee>();
                if (melee != null)
                {
                    melee.AttackOpponent = AttackMode;
                    Debug.Log($"{unit.name}'s AttackOpponent set to {AttackMode}");
                }
                else
                {
                    Debug.LogWarning($"Melee component not found on {unit.name}");
                }
            }
        }
    }

    // public void HandlePlayerOneInput()
    // {
    //     if (Input.GetKeyDown(KeyCode.Alpha1))
    //     {
    //         Debug.Log("Player One spawning unit...");
    //         GameObject unit = PhotonNetwork.Instantiate("MeleeOne", new Vector3(-4f, 0f, 0f), Quaternion.identity);
    //         unit.GetComponent<Melee>().SetTeam(1);
    //     }
    //     //toggle attack
    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         GameObject.FindWithTag("PlayerOne").GetComponent<PlayerOne>().ToggleAttack();
    //     }
    // }

    // public void HandlePlayerTwoInput()
    // {
    //     if (Input.GetKeyDown(KeyCode.Alpha1))
    //     {
    //         Debug.Log("Player Two spawning unit...");
    //         GameObject unit = PhotonNetwork.Instantiate("MeleeTwo", new Vector3(4f, 0f, 0f), Quaternion.identity);
    //         unit.GetComponent<Melee>().SetTeam(2);
    //     }
    //     //toggle attack
    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         GameObject.FindWithTag("PlayerTwo").GetComponent<PlayerTwo>().ToggleAttack();
    //     }
    // }
}
