using System.Collections;
using JetBrains.Annotations;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerScript : MonoBehaviourPunCallbacks
{
    private string localPlayerRole;
    public Button spawnButtonOne;
    public GameObject canvas;
    public TMP_Text CurrentPlayer;
    public TMP_Text coinText;
    public int Coins = 200;
    public Transform CameraTransform;
    public Transform CamPosOne;
    public Transform CamPosTwo;
    public Transform MiddlePos;
    public Vector3 CameraTargetPos;
    public bool AttackMode = false;

    void Start()
    {
        StartCoroutine(GetCoins());
        if (PhotonNetwork.IsMasterClient){
            localPlayerRole = "PlayerOne";
            CameraTargetPos = CamPosOne.position;
        }
        else{
            localPlayerRole = "PlayerTwo";
            CameraTargetPos = CamPosTwo.position;
        }

        Debug.Log($"Local Player Role: {localPlayerRole}");
        CurrentPlayer.text = localPlayerRole;

        if (!photonView.IsMine)
        {
            CameraTransform.gameObject.SetActive(false);
        }
        else
        {
            CameraTransform.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        
        if (canvas != null){
            canvas.SetActive(true);
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
    public void CameraSecondPos()
    {
        CameraTargetPos = CamPosTwo.position;
    }
    public void CameraFirstPos()
    {
        CameraTargetPos = CamPosOne.position;
    }
    public void CameraMiddlePos()
    {
        CameraTargetPos = MiddlePos.position;
    }
    IEnumerator GetCoins()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            Coins += 10;
        }
    }
    public void SpawnButton()
    {
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
