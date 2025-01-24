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
    [Header("Player Settings")]
    [Tooltip("The role of the local player.")]
    private string localPlayerRole = null;

    [Tooltip("Button for spawning Player One.")]
    public Button spawnButtonOne;

    [Tooltip("Canvas object for the UI.")]
    public GameObject canvas;

    [Tooltip("UI element for displaying 'Waiting for Players' message.")]
    public GameObject WaitingForPlayers;

    [Tooltip("Text element for the current player's name.")]
    public TMP_Text CurrentPlayer;

    [Header("Coin System")]
    [Tooltip("Text element for displaying the coin count.")]
    public TMP_Text coinText;

    [Tooltip("Total coins the player currently has.")]
    public int Coins = 200;

    [Tooltip("Coins gained per tick.")]
    public int CoinPerTick = 10;
    public int CoinPerTickUpgradeCost = 100;
    public int CoinPerTickUpgrade = 5;

    [Tooltip("Text element for displaying coins gained per tick.")]
    public TMP_Text CoinPerTickText;

    [Header("Camera Settings")]
    [Tooltip("Transform of the camera.")]
    public Transform CameraTransform;

    [Tooltip("Array of camera positions.")]
    public Transform[] CameraPositions;

    [Tooltip("Target position for the camera.")]
    public Vector3 CameraTargetPos;

    [Tooltip("Minimum transform for camera movement boundaries.")]
    public Transform minTransform;

    [Tooltip("Maximum transform for camera movement boundaries.")]
    public Transform maxTransform;

    [Tooltip("Camera sensitivity for movement.")]
    public float sensitivity = 0.01f;

    [Tooltip("Damping factor for camera momentum.")]
    public float momentumDamp = 0.9f;

    [Tooltip("Threshold for camera momentum to stop.")]
    public float momentumThreshold = 0.1f;

    [Tooltip("Smoothness of camera swiping.")]
    public float swipeSmoothness = 10f;

    [Tooltip("Smoothness of camera momentum.")]
    public float momentumSmoothness = 10f;

    [Header("Attack Mode")]
    [Tooltip("Indicates if the player is in attack mode.")]
    public bool AttackMode = false;

    [Header("Purchase Settings")]
    [Tooltip("Indicates if the player can buy melee units.")]
    public bool CanBuyMelee = true;

    [Tooltip("Indicates if the player can buy witch units.")]
    public bool CanBuyWitch = true;

    [Tooltip("Indicates if the player can buy bear units.")]
    public bool CanBuyBear = true;

    [Tooltip("Cost of melee units.")]
    public int MeleeCost = 50;

    [Tooltip("Cost of witch units.")]
    public int WitchCost = 100;

    [Tooltip("Cost of bear units.")]
    public int BearCost = 300;
    public int magicOrbCost = 500;

    [Header("Cooldowns and Timers")]
    [Tooltip("Cooldown for melee units.")]
    public float MeleeCooldown;

    [Tooltip("Cooldown for witch units.")]
    public float WitchCooldown;

    [Tooltip("Cooldown for bear units.")]
    public float BearCooldown;

    [Tooltip("Timer for melee unit cooldown.")]
    public float MeleeTimer = 0;

    [Tooltip("Timer for witch unit cooldown.")]
    public float WitchTimer = 0;

    [Tooltip("Timer for bear unit cooldown.")]
    public float BearTimer = 0;

    [Header("Unit Buttons")]
    [Tooltip("Text for the melee unit button.")]
    public TMP_Text MeleeButtonText;

    [Tooltip("Text for the witch unit button.")]
    public TMP_Text WitchButtonText;

    [Tooltip("Text for the bear unit button.")]
    public TMP_Text BearButtonText;

    [Header("Touch Controls")]
    [Tooltip("Start position of the touch input.")]
    private Vector2 touchStartPosition;

    [Tooltip("Start position of the camera.")]
    private Vector3 cameraStartPosition;

    [Tooltip("Indicates if the player is swiping.")]
    private bool isSwiping = false;

    [Tooltip("Momentum of the camera during movement.")]
    private float momentum = 0f;

    public Button AttackButton;
    
    [Header("Magic Orb Settings")]
    public MagicOrb magicOrb;
    public TMP_Text magicOrbCostText;

    [Header("Max Units")]
    public int MaxUnits = 100;
    public TMP_Text MaxUnitsText;
    public int CurrentUnits = 0;
    [Header("Unit sizes")]
    public int MeleeSize = 3;
    public int WitchSize = 5;
    public int BearSize = 8;
    
    void Start()
    {
        StartCoroutine(GetCoins());

        
        if (!photonView.IsMine)
        {
            CameraTransform.gameObject.SetActive(false);
        }
        else
        {
            CameraTransform.gameObject.SetActive(true);
        }
        AssignPlayerRole();
        Application.targetFrameRate = 120;
        CoinPerTickText.text = $"{CoinPerTickUpgradeCost}";
        MeleeButtonText.text = $"{MeleeCost}";
        WitchButtonText.text = $"{WitchCost}";
        magicOrbCostText.text = $"{magicOrbCost}";
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

        bool isPlayerOneTaken = roomProperties.ContainsKey("PlayerOne");
        bool isPlayerTwoTaken = roomProperties.ContainsKey("PlayerTwo");

        bool isPlayerOneAssigned = isPlayerOneTaken && PhotonNetwork.CurrentRoom.Players.Values.Any(p => p.ActorNumber == (int)roomProperties["PlayerOne"]);
        bool isPlayerTwoAssigned = isPlayerTwoTaken && PhotonNetwork.CurrentRoom.Players.Values.Any(p => p.ActorNumber == (int)roomProperties["PlayerTwo"]);

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
            CameraTargetPos = CameraPositions[1].position; 
            return;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);

        photonView.RPC("SyncRole", RpcTarget.AllBuffered, localPlayerRole);

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
    #region Melee
    public void SpawnButtonMelee()
    {
        if(MeleeSize + CurrentUnits > MaxUnits) return;
        Debug.Log(localPlayerRole);
        RoundManager roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();
        if(roundManager.Started == false) return;
        if(localPlayerRole == "PlayerOne" && Coins >= MeleeCost && CanBuyMelee)
        {
            Debug.Log("Player One spawning unit...");
            Transform PlayerOneSpawn = GameObject.FindGameObjectWithTag("HomeBaseOne").transform;
            GameObject unit = PhotonNetwork.Instantiate("Melee", PlayerOneSpawn.position, Quaternion.identity);
            unit.GetComponent<Melee>().SetTeam(1);
            unit.GetComponent<Melee>().UnitSize = MeleeSize;
            unit.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllBuffered, 1);
            Coins -= 50;
            MeleeTimer = MeleeCooldown;
            CurrentUnits += MeleeSize;
            MaxUnitsText.text = $"{CurrentUnits}/{MaxUnits}";
        }
        else if(localPlayerRole == "PlayerTwo" && Coins >= MeleeCost && CanBuyMelee)
        {
            Debug.Log("Player Two spawning unit...");
            Transform PlayerTwoSpawn = GameObject.FindGameObjectWithTag("HomeBaseTwo").transform;
            GameObject unit = PhotonNetwork.Instantiate("Melee", PlayerTwoSpawn.position, Quaternion.identity);
            unit.GetComponent<Melee>().SetTeam(2);
            unit.GetComponent<Melee>().UnitSize = MeleeSize;
            unit.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllBuffered, 2);
            Coins -= 50;
            MeleeTimer = MeleeCooldown;
            CurrentUnits += MeleeSize;
            MaxUnitsText.text = $"{CurrentUnits}/{MaxUnits}";
        }
        if(localPlayerRole == "PlayerOne")
        {
            Debug.Log("Player One toggling attack...");
            GameObject[] units = GameObject.FindGameObjectsWithTag("PawnOne");
            foreach (GameObject unit in units)
            {
                unit.SendMessage("ToggleAttack", AttackMode);
            }
        }
        else if(localPlayerRole == "PlayerTwo")
        {
            GameObject[] units = GameObject.FindGameObjectsWithTag("PawnTwo");
            foreach (GameObject unit in units)
            {
                unit.SendMessage("ToggleAttack", AttackMode);
            }
        }
    }
    #endregion Melee

    #region Witch
    public void SpawnWitch(){
        if(WitchSize + CurrentUnits > MaxUnits) return;
        Debug.Log(localPlayerRole);
        RoundManager roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();
        if(roundManager.Started == false) return;
        if(localPlayerRole == "PlayerOne" && Coins >= WitchCost && CanBuyWitch)
        {   
            Debug.Log("Player One spawning unit...");
            Transform PlayerOneSpawn = GameObject.FindGameObjectWithTag("HomeBaseOne").transform;
            GameObject unit = PhotonNetwork.Instantiate("Witch", PlayerOneSpawn.position, Quaternion.identity);
            unit.GetComponent<WitchScript>().SetTeam(1);
            unit.GetComponent<WitchScript>().UnitSize = WitchSize;
            unit.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllBuffered, 1);
            Coins -= 100;
            WitchTimer = WitchCooldown;
            CurrentUnits += WitchSize;
        }
        else if(localPlayerRole == "PlayerTwo" && Coins >= WitchCost && CanBuyWitch)
        {
            Debug.Log("Player Two spawning unit...");
            Transform PlayerTwoSpawn = GameObject.FindGameObjectWithTag("HomeBaseTwo").transform;
            GameObject unit = PhotonNetwork.Instantiate("Witch", PlayerTwoSpawn.position, Quaternion.identity);
            unit.GetComponent<WitchScript>().SetTeam(2);
            unit.GetComponent<WitchScript>().UnitSize = WitchSize;
            unit.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllBuffered, 2);
            Coins -= 100;
            WitchTimer = WitchCooldown;
            CurrentUnits += WitchSize;
        }
        if(localPlayerRole == "PlayerOne")
        {
            Debug.Log("Player One toggling attack...");
            GameObject[] units = GameObject.FindGameObjectsWithTag("PawnOne");
            foreach (GameObject unit in units)
            {
                unit.SendMessage("ToggleAttack", AttackMode);
            }
        }
        else if(localPlayerRole == "PlayerTwo")
        {
            GameObject[] units = GameObject.FindGameObjectsWithTag("PawnTwo");
            foreach (GameObject unit in units)
            {
                unit.SendMessage("ToggleAttack", AttackMode);
            }
        }
        MaxUnitsText.text = $"{CurrentUnits}/{MaxUnits}";
    }
    #endregion Witch

    #region Bear
    public void SpawnBear(){
        if(BearSize + CurrentUnits > MaxUnits) return;
        Debug.Log(localPlayerRole);
        RoundManager roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();
        if(roundManager.Started == false) return;
        if(localPlayerRole == "PlayerOne" && Coins >= BearCost && CanBuyBear)
        {   
            Debug.Log("Player One spawning unit...");
            Transform PlayerOneSpawn = GameObject.FindGameObjectWithTag("HomeBaseOne").transform;
            GameObject unit = PhotonNetwork.Instantiate("Bear", PlayerOneSpawn.position, Quaternion.identity);
            unit.GetComponent<Melee>().SetTeam(1);
            unit.GetComponent<Melee>().UnitSize = BearSize;
            unit.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllBuffered, 1);
            Coins -= BearCost;
            BearTimer = BearCooldown;
            CurrentUnits += BearSize;
            MaxUnitsText.text = $"{CurrentUnits}/{MaxUnits}";
        }
        else if(localPlayerRole == "PlayerTwo" && Coins >= BearCost && CanBuyBear)
        {
            Debug.Log("Player Two spawning unit...");
            Transform PlayerTwoSpawn = GameObject.FindGameObjectWithTag("HomeBaseTwo").transform;
            GameObject unit = PhotonNetwork.Instantiate("Bear", PlayerTwoSpawn.position, Quaternion.identity);
            unit.GetComponent<Melee>().SetTeam(2);
            unit.GetComponent<Melee>().UnitSize = BearSize;
            unit.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllBuffered, 2);
            Coins -= BearCost;
            BearTimer = BearCooldown;
            CurrentUnits += BearSize;
            MaxUnitsText.text = $"{CurrentUnits}/{MaxUnits}";
        }
        if(localPlayerRole == "PlayerOne")
        {
            Debug.Log("Player One toggling attack...");
            GameObject[] units = GameObject.FindGameObjectsWithTag("PawnOne");
            foreach (GameObject unit in units)
            {
                unit.SendMessage("ToggleAttack", AttackMode);
            }
        }
        else if(localPlayerRole == "PlayerTwo")
        {
            GameObject[] units = GameObject.FindGameObjectsWithTag("PawnTwo");
            foreach (GameObject unit in units)
            {
                unit.SendMessage("ToggleAttack", AttackMode);
            }
        }
    }
    #endregion Bear
    #region Coins
    public void CoinUpgrade()
    {
        if(Coins >= CoinPerTickUpgradeCost)
        {
            CoinPerTick += 5;
            Coins -= CoinPerTickUpgradeCost;
            CoinPerTickUpgradeCost += CoinPerTickUpgrade + 50;
            CoinPerTickUpgrade += 5;
            CoinPerTickText.text = $"{CoinPerTickUpgradeCost}";
        }
    }
    #endregion Coins
    #region AttackMode
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
                unit.SendMessage("ToggleAttack", AttackMode);
            }
        }
        else if(localPlayerRole == "PlayerTwo")
        {
            GameObject[] units = GameObject.FindGameObjectsWithTag("PawnTwo");
            foreach (GameObject unit in units)
            {
                unit.SendMessage("ToggleAttack", AttackMode);
            }
        }
        //change the color of the button to indicate attack mode
        if(AttackMode)
        {
            AttackButton.GetComponent<Image>().color = Color.red;
        }
        else
        {
            AttackButton.GetComponent<Image>().color = Color.white;
        }
    }
    #endregion AttackMode

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
    #region MagicOrb
    public bool CanUpgradeMagicOrb = true;
    public void UpgradeMagicOrb()
    {
        if(localPlayerRole == "PlayerOne")magicOrb = GameObject.Find("OrbOne").GetComponent<MagicOrb>();
        else if(localPlayerRole == "PlayerTwo")magicOrb = GameObject.Find("OrbTwo").GetComponent<MagicOrb>();
        if(magicOrb.MagicOrbLevel == 5)
        {
            CanUpgradeMagicOrb = false;
            magicOrbCostText.text = "Max Level";
            
        }else{
            CanUpgradeMagicOrb = true;
        }
        if(Coins >= magicOrbCost && CanUpgradeMagicOrb)
        {
            if(localPlayerRole == "PlayerOne")magicOrb = GameObject.Find("OrbOne").GetComponent<MagicOrb>();
            else if(localPlayerRole == "PlayerTwo")magicOrb = GameObject.Find("OrbTwo").GetComponent<MagicOrb>();
            PhotonView pv = magicOrb.GetComponent<PhotonView>();
            pv.RPC("Upgrade", RpcTarget.AllBuffered);
            Coins -= magicOrbCost;
            magicOrbCost += 100;
            magicOrbCostText.text = $"{magicOrbCost}";
            
        }
        if(magicOrb.MagicOrbLevel == 5)
        {
            CanUpgradeMagicOrb = false;
            magicOrbCostText.text = "Max Level";
            
        }else{
            CanUpgradeMagicOrb = true;
        }
    }
    #endregion MagicOrb
    public void RemoveUnit(int size)
    {
        CurrentUnits -= size;
        MaxUnitsText.text = $"{CurrentUnits}/{MaxUnits}";
    }
    #region Swipes
    public void Swipes()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPosition = touch.position;
                    cameraStartPosition = CameraTransform.position;
                    isSwiping = true;
                    momentum = 0f; 
                    break;

                case TouchPhase.Moved:
                    if (isSwiping)
                    {
                        Vector2 swipeDelta = touch.position - touchStartPosition;

                        float newX = cameraStartPosition.x - swipeDelta.x * sensitivity;
                        CameraTransform.position = Vector3.Lerp(CameraTransform.position, new Vector3(newX, CameraTransform.position.y, CameraTransform.position.z), Time.deltaTime * swipeSmoothness);

                        momentum = -touch.deltaPosition.x * sensitivity;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isSwiping = false;
                    break;
            }
        }

        if (!isSwiping && Mathf.Abs(momentum) > momentumThreshold)
        {
            ApplyMomentum();
        }

        ClampCameraPosition();
    }
    #endregion Swipes

    private void ApplyMomentum()
    {
        float newX = CameraTransform.position.x + momentum;
        CameraTransform.position = Vector3.Lerp(CameraTransform.position, new Vector3(newX, CameraTransform.position.y, CameraTransform.position.z), Time.deltaTime * momentumSmoothness);

        momentum *= momentumDamp;

        if (Mathf.Abs(momentum) <= momentumThreshold)
        {
            momentum = 0f;
        }
    }

    private void ClampCameraPosition()
    {
        float clampedX = Mathf.Clamp(CameraTransform.position.x, minTransform.position.x, maxTransform.position.x);
        CameraTransform.position = new Vector3(clampedX, CameraTransform.position.y, CameraTransform.position.z);
    }
}
