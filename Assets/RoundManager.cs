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
    public int PlayerOneTowerHealth = 1000;

    [Tooltip("Health of Player Two's tower.")]
    public int PlayerTwoTowerHealth = 1000;

    [Tooltip("Wins by Player One.")]
    public int PlayerOneWins = 0;

    [Tooltip("Wins by Player Two.")]
    public int PlayerTwoWins = 0;

    [Tooltip("Number of players in the game.")]
    public int Players = 0;

    [Header("UI Elements")]
    [Tooltip("Health bar for Player One's tower.")]
    public GameObject PLayerOneTowerHealthBar;

    [Tooltip("Health bar for Player Two's tower.")]
    public GameObject PLayerTwoTowerHealthBar;

    [Tooltip("Text element displaying Player One's wins.")]
    public TMP_Text PlayerOneWinsText;

    [Tooltip("Text element displaying Player Two's wins.")]
    public TMP_Text PlayerTwoWinsText;

    [Tooltip("Text element displaying the countdown timer.")]
    public TMP_Text CountdownText;
    void Start()
    {
        PLayerOneTowerHealthBar.transform.localScale = new Vector3(1, 1, (float)PlayerOneTowerHealth / 100);
        PLayerTwoTowerHealthBar.transform.localScale = new Vector3(1, 1, (float)PlayerTwoTowerHealth / 100);
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
        foreach (GameObject unit in unitsone){
            Destroy(unit);
        }
        foreach (GameObject unit in unitstwo){
            Destroy(unit);
        }
        PlayerOneTowerHealth = 1000;
        PlayerTwoTowerHealth = 1000;
        GameObject[] playerScript = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in playerScript){
            PlayerScript playerScriptComponent = player.GetComponent<PlayerScript>();
            playerScriptComponent.Coins = 200;
            playerScriptComponent.CoinPerTickUpgradeCost = 100;
            playerScriptComponent.CoinPerTick = 10;
        }
        PLayerOneTowerHealthBar.transform.localScale = new Vector3(1, 1, (float)PlayerOneTowerHealth / 100);
        PLayerTwoTowerHealthBar.transform.localScale = new Vector3(1, 1, (float)PlayerTwoTowerHealth / 100);


    }
    [PunRPC]
    public void PlayerOneDamageTower(int damage){
        PlayerOneTowerHealth -= damage;
        PLayerOneTowerHealthBar.transform.localScale = new Vector3(1, 1, (float)PlayerOneTowerHealth / 100);
        if (PlayerOneTowerHealth <= 0){
            PlayerTwoWinsRound();
        }

    }
    [PunRPC]
    public void PlayerTwoDamageTower(int damage){
        PlayerTwoTowerHealth -= damage;
        PLayerTwoTowerHealthBar.transform.localScale = new Vector3(1, 1, (float)PlayerTwoTowerHealth / 100);
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
