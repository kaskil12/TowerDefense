using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
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
    public int Players = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        PlayerAmountCheck();
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
        Debug.Log("Resetting the game...");
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
        }


    }
    [PunRPC]
    public void PlayerOneDamageTower(int damage){
        // Reduce PlayerOneTowerHealth by damage
        PlayerOneTowerHealth -= damage;
        PLayerOneTowerHealthBar.transform.localScale = new Vector3((float)PlayerOneTowerHealth / 1000, 1, 1);
        if (PlayerOneTowerHealth <= 0){
            // Player Two wins the round
            PlayerTwoWinsRound();
        }

    }
    [PunRPC]
    public void PlayerTwoDamageTower(int damage){
        // Reduce PlayerTwoTowerHealth by damage
        PlayerTwoTowerHealth -= damage;
        PLayerTwoTowerHealthBar.transform.localScale = new Vector3((float)PlayerTwoTowerHealth / 1000, 1, 1);
        if (PlayerTwoTowerHealth <= 0){
            // Player One wins the round
            PlayerOneWinsRound();
        }

    }
    [PunRPC]
    public void PlayerOneWinsRound(){
        // Increase PlayerOneWins by 1
        PlayerOneWins += 1;
        Reset();
    }
    [PunRPC]
    public void PlayerTwoWinsRound(){
        // Increase PlayerTwoWins by 1
        PlayerTwoWins += 1;
        Reset();

    }
    [PunRPC]
    public void StartGame(){
        // Start the game by using photonView.RPC
        Started = true;

    }

}
