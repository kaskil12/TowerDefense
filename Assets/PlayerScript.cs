using System.Collections;
using JetBrains.Annotations;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using System.Linq;

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

    public bool CanBuyMelee = true;
    public bool CanBuyWitch = true;
    public bool CanBuyBear = true;
    public int CoinPerTick = 10;
    public int MeleeCost = 50;
    public int WitchCost = 100;
    public int BearCost = 100;
    public float WitchTimer = 0;
    public float MeleeTimer = 0;
    public float BearTimer = 0;
    public TMP_Text CoinPerTickText;
    public TMP_Text MeleeButtonText;
    public TMP_Text WitchButtonText;
    public TMP_Text BearButtonText;
    public Transform minTransform; // Minimum boundary
    public Transform maxTransform; // Maximum boundary
    public float sensitivity = 0.01f; // Swipe sensitivity
    public float momentumDamp = 0.9f; // Damping for momentum
    public float momentumThreshold = 0.1f; // Stop momentum below this value
    public float swipeSmoothness = 10f; // Smoothness for swipe
    public float momentumSmoothness = 10f; // Smoothness for momentum

    private Vector2 touchStartPosition;
    private Vector3 cameraStartPosition;
    private bool isSwiping = false;
    private float momentum = 0f; // Store swipe momentum
    
    void Start()
    {
        StartCoroutine(GetCoins());

        // Determine the local player's index in the player list
        
        if (!photonView.IsMine)
        {
            CameraTransform.gameObject.SetActive(false);
        }
        else
        {
            CameraTransform.gameObject.SetActive(true);
        }
        AssignPlayerRole();
        //set fps to 120
        Application.targetFrameRate = 120;
        //set text for melee and witch buttons and coin per tick
        CoinPerTickText.text = $"{CoinPerTickUpgradeCost}";
        MeleeButtonText.text = $"{MeleeCost}";
        WitchButtonText.text = $"{WitchCost}";
    }
    bool player1Taken = false;
    bool player2Taken = false;
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player entered room");
        
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
{
    Debug.Log($"Player {otherPlayer.NickName} left the room");

    ExitGames.Client.Photon.Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

    if (roomProperties.ContainsKey("PlayerOne") && (int)roomProperties["PlayerOne"] == otherPlayer.ActorNumber)
    {
        roomProperties.Remove("PlayerOne");
        Debug.Log("PlayerOne role cleared.");
    }
    else if (roomProperties.ContainsKey("PlayerTwo") && (int)roomProperties["PlayerTwo"] == otherPlayer.ActorNumber)
    {
        roomProperties.Remove("PlayerTwo");
        Debug.Log("PlayerTwo role cleared.");
    }

    PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
}
    public void AssignPlayerRole()
{
    ExitGames.Client.Photon.Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

    // Check which roles are already taken
    bool isPlayerOneTaken = roomProperties.ContainsKey("PlayerOne");
    bool isPlayerTwoTaken = roomProperties.ContainsKey("PlayerTwo");

    // Check if the roles are already assigned to any player
    bool isPlayerOneAssigned = isPlayerOneTaken && PhotonNetwork.CurrentRoom.Players.Values.Any(p => p.ActorNumber == (int)roomProperties["PlayerOne"]);
    bool isPlayerTwoAssigned = isPlayerTwoTaken && PhotonNetwork.CurrentRoom.Players.Values.Any(p => p.ActorNumber == (int)roomProperties["PlayerTwo"]);

    // Assign roles based on availability
    if (!isPlayerOneAssigned && localPlayerRole == null)
    {
        localPlayerRole = "PlayerOne";
        roomProperties["PlayerOne"] = PhotonNetwork.LocalPlayer.ActorNumber;
    }
    else if (!isPlayerTwoAssigned && localPlayerRole == null)
    {
        localPlayerRole = "PlayerTwo";
        roomProperties["PlayerTwo"] = PhotonNetwork.LocalPlayer.ActorNumber;
    }
    else
    {
        Debug.LogWarning("No available roles. Room might be full.");
        CameraTargetPos = CameraPositions[1].position; // Spectator view or default position
        return;
    }

    // Update Room Properties
    PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

    // Sync role to all clients
    photonView.RPC("SyncRole", RpcTarget.AllBuffered, localPlayerRole);

    // Update UI and log
    CameraTargetPos = localPlayerRole == "PlayerOne" ? CameraPositions[0].position : CameraPositions[2].position;
    CurrentPlayer.text = localPlayerRole;
    Debug.Log($"Local Player Role: {localPlayerRole}");
}


[PunRPC]
public void SyncRole(string role)
{
    gameObject.name = role;
    localPlayerRole = role;
    Debug.Log($"Role synced: {role}");
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
        // CameraTransform.position = Vector3.Lerp(CameraTransform.position, CameraTargetPos, 1f * Time.deltaTime);
        if(MeleeTimer > 0){
            MeleeTimer -= Time.deltaTime;
            CanBuyMelee = false;
            //Shorten the float text to 1 decimal places
            MeleeButtonText.text = $"{MeleeTimer:F1}";
        }else{
            CanBuyMelee = true;
            MeleeButtonText.text = $"{MeleeCost:F1}";
        }
        if(WitchTimer > 0){
            WitchTimer -= Time.deltaTime;
            CanBuyWitch = false;
            WitchButtonText.text = $"{WitchTimer:F1}";
        }else{
            CanBuyWitch = true;
            WitchButtonText.text = $"{WitchCost:F1}";
        }
        if(BearTimer > 0){
            BearTimer -= Time.deltaTime;
            CanBuyBear = false;
            BearButtonText.text = $"{BearTimer:F1}";
        }else{
            CanBuyBear = true;
            BearButtonText.text = $"{BearCost:F1}";
        }
        Swipes();
    }
    // public void NextPos()
    // {
    //     //go to the next camera position in the array of 3 CamPositions array
    //     for (int i = 0; i < CameraPositions.Length; i++)
    //     {
    //         if (CameraTargetPos == CameraPositions[i].position)
    //         {
    //         CameraTargetPos = CameraPositions[(i + 1) % CameraPositions.Length].position;
    //         break;
    //         }
    //     }


    // }
    // public void PreviousPos()
    // {
    //     //go to the previous camera position in the array of 3 CamPositions array
    //     for (int i = 0; i < CameraPositions.Length; i++)
    //     {
    //         if (CameraTargetPos == CameraPositions[i].position)
    //         {
    //         CameraTargetPos = CameraPositions[(i + 2) % CameraPositions.Length].position;
    //         break;
    //         }
    //     }
    // }
    IEnumerator GetCoins()
    {
        RoundManager roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();
       
        while (true)
        {
            yield return new WaitForSeconds(3);
             if(roundManager.Started == true)
            {
                Coins += CoinPerTick;   
            }
        }
    }
  
    public void SpawnButtonMelee()
    {
        
        Debug.Log(localPlayerRole);
        RoundManager roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();
        if(roundManager.Started == false) return;
        if(localPlayerRole == "PlayerOne" && Coins >= MeleeCost && CanBuyMelee)
        {
            Debug.Log("Player One spawning unit...");
            Transform PlayerOneSpawn = GameObject.FindGameObjectWithTag("HomeBaseOne").transform;
            GameObject unit = PhotonNetwork.Instantiate("MeleeOne", PlayerOneSpawn.position, Quaternion.identity);
            unit.GetComponent<Melee>().SetTeam(1);
            unit.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllBuffered, 1);
            Coins -= 50;
            MeleeTimer = 5;
        }
        else if(localPlayerRole == "PlayerTwo" && Coins >= MeleeCost && CanBuyMelee)
        {
            Debug.Log("Player Two spawning unit...");
            Transform PlayerTwoSpawn = GameObject.FindGameObjectWithTag("HomeBaseTwo").transform;
            GameObject unit = PhotonNetwork.Instantiate("MeleeTwo", PlayerTwoSpawn.position, Quaternion.identity);
            unit.GetComponent<Melee>().SetTeam(2);
            unit.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllBuffered, 2);
            Coins -= 50;
            MeleeTimer = 5;
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
    
    public void SpawnWitch(){
        Debug.Log(localPlayerRole);
        RoundManager roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();
        if(roundManager.Started == false) return;
        if(localPlayerRole == "PlayerOne" && Coins >= WitchCost && CanBuyWitch)
        {   
            Debug.Log("Player One spawning unit...");
            Transform PlayerOneSpawn = GameObject.FindGameObjectWithTag("HomeBaseOne").transform;
            GameObject unit = PhotonNetwork.Instantiate("WitchOne", PlayerOneSpawn.position, Quaternion.identity);
            unit.GetComponent<WitchScript>().SetTeam(1);
            unit.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllBuffered, 1);
            Coins -= 100;
            WitchTimer = 5;
        }
        else if(localPlayerRole == "PlayerTwo" && Coins >= WitchCost && CanBuyWitch)
        {
            Debug.Log("Player Two spawning unit...");
            Transform PlayerTwoSpawn = GameObject.FindGameObjectWithTag("HomeBaseTwo").transform;
            GameObject unit = PhotonNetwork.Instantiate("WitchTwo", PlayerTwoSpawn.position, Quaternion.identity);
            unit.GetComponent<WitchScript>().SetTeam(2);
            unit.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllBuffered, 2);
            Coins -= 100;
            WitchTimer = 5;
        }
        if(localPlayerRole == "PlayerOne")
        {
            Debug.Log("Player One toggling attack...");
            GameObject[] units = GameObject.FindGameObjectsWithTag("PawnOne");
            foreach (GameObject unit in units)
            {
                var witch = unit.GetComponent<WitchScript>();
                if (witch != null)
                {
                    witch.AttackOpponent = AttackMode;
                    Debug.Log($"{unit.name}'s AttackOpponent set to {AttackMode}");
                }
                else
                {
                    Debug.LogWarning($"witch component not found on {unit.name}");
                }
            }
        }
        else if(localPlayerRole == "PlayerTwo")
        {
            GameObject[] units = GameObject.FindGameObjectsWithTag("PawnTwo");
            foreach (GameObject unit in units)
            {
                var witch = unit.GetComponent<WitchScript>();
                if (witch != null)
                {
                    witch.AttackOpponent = AttackMode;
                    Debug.Log($"{unit.name}'s AttackOpponent set to {AttackMode}");
                }
                else
                {
                    Debug.LogWarning($"Melee component not found on {unit.name}");
                }
            }
        }
    }

    #region Bear
    public void SpawnBear(){
        Debug.Log(localPlayerRole);
        RoundManager roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();
        if(roundManager.Started == false) return;
        if(localPlayerRole == "PlayerOne" && Coins >= BearCost && CanBuyBear)
        {   
            Debug.Log("Player One spawning unit...");
            Transform PlayerOneSpawn = GameObject.FindGameObjectWithTag("HomeBaseOne").transform;
            GameObject unit = PhotonNetwork.Instantiate("BearOne", PlayerOneSpawn.position, Quaternion.identity);
            unit.GetComponent<Melee>().SetTeam(1);
            unit.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllBuffered, 1);
            Coins -= BearCost;
            BearTimer = 5;
        }
        else if(localPlayerRole == "PlayerTwo" && Coins >= BearCost && CanBuyBear)
        {
            Debug.Log("Player Two spawning unit...");
            Transform PlayerTwoSpawn = GameObject.FindGameObjectWithTag("HomeBaseTwo").transform;
            GameObject unit = PhotonNetwork.Instantiate("BearTwo", PlayerTwoSpawn.position, Quaternion.identity);
            unit.GetComponent<Melee>().SetTeam(2);
            unit.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllBuffered, 2);
            Coins -= 100;
            BearTimer = 5;
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
                    Debug.LogWarning($"witch component not found on {unit.name}");
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
    #endregion Bear
    public int CoinPerTickUpgradeCost = 100;
    public void CoinUpgrade()
    {
        if(Coins >= CoinPerTickUpgradeCost)
        {
            CoinPerTick += 5;
            Coins -= CoinPerTickUpgradeCost;
            CoinPerTickUpgradeCost += 100;
            CoinPerTickText.text = $"{CoinPerTickUpgradeCost}";
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
                var witch = unit.GetComponent<WitchScript>();
                if (melee != null || witch != null)
                {
                    if(melee)melee.AttackOpponent = AttackMode;
                    if(witch)witch.AttackOpponent = AttackMode;
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
                var witch = unit.GetComponent<WitchScript>();
                if (melee != null || witch != null)
                {
                    if(melee)melee.AttackOpponent = AttackMode;
                    if(witch)witch.AttackOpponent = AttackMode;
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

public void Swipes()
{
    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                // Store initial touch and camera position
                touchStartPosition = touch.position;
                cameraStartPosition = CameraTransform.position;
                isSwiping = true;
                momentum = 0f; // Reset momentum
                break;

            case TouchPhase.Moved:
                if (isSwiping)
                {
                    // Calculate swipe delta (reversed)
                    Vector2 swipeDelta = touch.position - touchStartPosition;

                    // Update camera position based on reversed swipe
                    float newX = cameraStartPosition.x - swipeDelta.x * sensitivity;
                    CameraTransform.position = Vector3.Lerp(CameraTransform.position, new Vector3(newX, CameraTransform.position.y, CameraTransform.position.z), Time.deltaTime * swipeSmoothness);

                    // Update momentum based on reversed swipe velocity
                    momentum = -touch.deltaPosition.x * sensitivity;
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                isSwiping = false;
                break;
        }
    }

    // Apply momentum when swipe ends
    if (!isSwiping && Mathf.Abs(momentum) > momentumThreshold)
    {
        ApplyMomentum();
    }

    // Clamp the parent object's position to boundaries
    ClampCameraPosition();
}

private void ApplyMomentum()
{
    // Add reversed momentum to the camera's parent position
    float newX = CameraTransform.position.x + momentum;
    CameraTransform.position = Vector3.Lerp(CameraTransform.position, new Vector3(newX, CameraTransform.position.y, CameraTransform.position.z), Time.deltaTime * momentumSmoothness);

    // Gradually reduce momentum
    momentum *= momentumDamp;

    // Stop momentum if it's too small
    if (Mathf.Abs(momentum) <= momentumThreshold)
    {
        momentum = 0f;
    }
}

private void ClampCameraPosition()
{
    // Restrict the parent object's position within boundaries
    float clampedX = Mathf.Clamp(CameraTransform.position.x, minTransform.position.x, maxTransform.position.x);
    CameraTransform.position = new Vector3(clampedX, CameraTransform.position.y, CameraTransform.position.z);
}
}
