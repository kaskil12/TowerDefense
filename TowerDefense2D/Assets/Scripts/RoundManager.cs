using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
public class RoundManager : MonoBehaviourPunCallbacks
{
 [Header("Game State")]
    [Tooltip("Indicates if the game is over.")]
    public bool GameOver = false;

    [Tooltip("Indicates if the game has started.")]
    public bool Started = false;

    [Header("Countdown Timer")]
    [Tooltip("Time left for the countdown.")]
    public float Countdown = 5;

    [Header("Round Settings")]
    [Tooltip("Maximum number of rounds.")]
    public int MaxRounds = 3;

    [Tooltip("Current round number.")]
    public int RoundNumber = 1;

    [Header("Player Stats")]
    [Tooltip("Health of Player One's tower.")]
    public int PlayerOneTowerHealth = 2500;

    [Tooltip("Health of Player Two's tower.")]
    public int PlayerTwoTowerHealth = 2500;

    [Tooltip("Wins by Player One.")]
    public int PlayerOneWins = 0;

    [Tooltip("Wins by Player Two.")]
    public int PlayerTwoWins = 0;

    [Tooltip("Number of players in the game.")]
    public int Players = 0;

    [Header("UI Elements")]
    [Tooltip("Health bar for Player One's tower.")]
    public Slider PLayerOneTowerHealthBar;

    [Tooltip("Health bar for Player Two's tower.")]
    public Slider PLayerTwoTowerHealthBar;

    [Tooltip("Text element displaying Player One's wins.")]
    public TMP_Text PlayerOneWinsText;

    [Tooltip("Text element displaying Player Two's wins.")]
    public TMP_Text PlayerTwoWinsText;

    [Tooltip("Text element displaying the countdown timer.")]
    public TMP_Text CountdownText;
    void Start()
    {
        PLayerOneTowerHealthBar.maxValue = PlayerOneTowerHealth;
        PLayerTwoTowerHealthBar.maxValue = PlayerTwoTowerHealth;
        PLayerOneTowerHealthBar.value = PlayerOneTowerHealth;
        PLayerTwoTowerHealthBar.value = PlayerTwoTowerHealth;
    }

    void Update()
    {
        PlayerAmountCheck();
        if (Countdown <= 0){
            Countdown = 5;
            GameOver = false;
            CountdownText.text = "";
        }
        if (GameOver == true){
            Countdown -= Time.deltaTime;
            CountdownText.text = Countdown.ToString("0");
        }
    }
    public void PlayerAmountCheck(){
        GameObject[] playerScript = GameObject.FindGameObjectsWithTag("Player");
        Players = playerScript.Length;
        if (Players == 2 && Started == false){
            photonView.RPC("StartGame", RpcTarget.All);
        }
    }
    [PunRPC]
    public void Reset(){
        if(PlayerTwoWins == 3 || PlayerOneWins == 3){
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("Loading");
        }

        GameOver = true;

        GameObject[] unitsone = GameObject.FindGameObjectsWithTag("PawnOne");
        GameObject[] unitstwo = GameObject.FindGameObjectsWithTag("PawnTwo");
        GameObject[] Projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        foreach (GameObject projectile in Projectiles){
            Destroy(projectile);
        }
        foreach (GameObject unit in unitsone){
            Destroy(unit);
        }
        foreach (GameObject unit in unitstwo){
            Destroy(unit);
        }
        PlayerOneTowerHealth = 2500;
        PlayerTwoTowerHealth = 2500;
        GameObject[] playerScript = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in playerScript){
            PlayerScript playerScriptComponent = player.GetComponent<PlayerScript>();
            playerScriptComponent.Coins = 200;
            playerScriptComponent.CoinPerTickUpgradeCost = 100;
            playerScriptComponent.CoinPerTick = 10;
            playerScriptComponent.CoinPerTickText.text = playerScriptComponent.CoinPerTick.ToString();
            playerScriptComponent.CoinPerTickUpgrade = 5;
            playerScriptComponent.magicOrbCost = 500;
            playerScriptComponent.CurrentUnits = 0;
            playerScriptComponent.MaxUnits = 100;
            playerScriptComponent.MaxUnitsText.text = $"{playerScriptComponent.CurrentUnits}/{playerScriptComponent.MaxUnits}";
            playerScriptComponent.magicOrbCostText.text = playerScriptComponent.magicOrbCost.ToString();
            MagicOrb magicOrbOne = GameObject.Find("OrbOne").GetComponent<MagicOrb>();
            MagicOrb magicOrbTwo = GameObject.Find("OrbTwo").GetComponent<MagicOrb>();
            magicOrbOne.MagicOrbLevel = 0;
            magicOrbTwo.MagicOrbLevel = 0;
            playerScriptComponent.CanUpgradeMagicOrb = true;
        }
        PLayerOneTowerHealthBar.value = PlayerOneTowerHealth;
        PLayerTwoTowerHealthBar.value = PlayerTwoTowerHealth;


    }
    [PunRPC]
    public void PlayerOneDamageTower(int damage){
        PlayerOneTowerHealth -= damage;
        PLayerOneTowerHealthBar.value = PlayerOneTowerHealth;
        if (PlayerOneTowerHealth <= 0){
            PlayerTwoWinsRound();
        }

    }
    [PunRPC]
    public void PlayerTwoDamageTower(int damage){
        PlayerTwoTowerHealth -= damage;
        PLayerTwoTowerHealthBar.value = PlayerTwoTowerHealth;

        if (PlayerTwoTowerHealth <= 0){
            PlayerOneWinsRound();
        }

    }
    [PunRPC]
    public void PlayerOneWinsRound(){
        PlayerOneWins += 1;
        PlayerOneWinsText.text = PlayerOneWins.ToString();
        Reset();
    }
    [PunRPC]
    public void PlayerTwoWinsRound(){
        PlayerTwoWins += 1;
        PlayerTwoWinsText.text = PlayerTwoWins.ToString();
        Reset();
    }
    // [PunRPC]
    // IEnumerator WaitBeforeReset(){
    //     GameOver = true;
    //     yield return new WaitForSeconds(5);
    //     if(PlayerTwoWins == 3 || PlayerOneWins == 3){
    //         //Kick all players from the room
    //         PhotonNetwork.CurrentRoom.IsVisible = false;
    //         PhotonNetwork.CurrentRoom.IsOpen = false;
    //         PhotonNetwork.LeaveRoom();
    //         GameOver = true;
    //     }
    //     Reset();
    // }
    [PunRPC]
    public void StartGame(){
        Started = true;

    }

}
