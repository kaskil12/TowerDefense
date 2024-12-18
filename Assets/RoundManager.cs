using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
public class RoundManager : MonoBehaviourPunCallbacks
{
    public int RoundNumber = 1;
    public int MaxRounds = 3;
    public int PlayerOneWins = 0;
    public int PlayerTwoWins = 0;
    public bool GameOver = false;
    public bool Started = false;
    public int PlayerOneTowerHealth = 1000;
    public int PlayerTwoTowerHealth = 1000;
    public GameObject PLayerOneTowerHealthBar;
    public GameObject PLayerTwoTowerHealthBar;
    public TMP_Text PlayerOneWinsText;
    public TMP_Text PlayerTwoWinsText;
    public TMP_Text CountdownText;

    public int Players = 0;
    public float Countdown = 5;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set the initial values of the PlayerOneTowerHealth and PlayerTwoTowerHealth
        PLayerOneTowerHealthBar.transform.localScale = new Vector3(1, 1, (float)PlayerOneTowerHealth / 100);
        PLayerTwoTowerHealthBar.transform.localScale = new Vector3(1, 1, (float)PlayerTwoTowerHealth / 100);
    }

    // Update is called once per frame
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
            CountdownText.text = "Resetting the game in " + Countdown.ToString("0");
        }
    }
    public void PlayerAmountCheck(){
        // Check the number of players in the room. If the number of players is 2, start the game. If the number of players is 1, wait for another player to join.
        GameObject[] playerScript = GameObject.FindGameObjectsWithTag("Player");
        Players = playerScript.Length;
        if (Players == 2 && Started == false){
            // Start the game
            photonView.RPC("StartGame", RpcTarget.All);
            // StartGame();
        }
    }
    [PunRPC]
    public void Reset(){
        if(PlayerTwoWins == 3 || PlayerOneWins == 3){
            //Kick all players from the room
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("Loading");
        }
        GameOver = true;

        // Reset the game by using photonView.RPC
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


    }
    [PunRPC]
    public void PlayerOneDamageTower(int damage){
        // Reduce PlayerOneTowerHealth by damage
        PlayerOneTowerHealth -= damage;
        PLayerOneTowerHealthBar.transform.localScale = new Vector3(1, 1, (float)PlayerOneTowerHealth / 100);
        if (PlayerOneTowerHealth <= 0){
            // Player Two wins the round
            PlayerTwoWinsRound();
        }

    }
    [PunRPC]
    public void PlayerTwoDamageTower(int damage){
        // Reduce PlayerTwoTowerHealth by damage
        PlayerTwoTowerHealth -= damage;
        PLayerTwoTowerHealthBar.transform.localScale = new Vector3(1, 1, (float)PlayerTwoTowerHealth / 100);
        if (PlayerTwoTowerHealth <= 0){
            // Player One wins the round
            PlayerOneWinsRound();
        }

    }
    [PunRPC]
    public void PlayerOneWinsRound(){
        // Increase PlayerOneWins by 1
        PlayerOneWins += 1;
        PlayerOneWinsText.text = PlayerOneWins.ToString();
        Reset();
    }
    [PunRPC]
    public void PlayerTwoWinsRound(){
        // Increase PlayerTwoWins by 1
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
        // Start the game by using photonView.RPC
        Started = true;

    }

}
