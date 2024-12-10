using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public int RoundNumber = 1;
    public int MaxRounds = 3;
    public int PlayerOneWins = 0;
    public int PlayerTwoWins = 0;
    public bool GameOver = false;
    public int PlayerOneTowerHealth = 1000;
    public int PlayerTwoTowerHealth = 1000;
    public GameObject PLayerOneTowerHealthBar;
    public GameObject PLayerTwoTowerHealthBar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Reset(){
        // Reset the game by using photonView.RPC
        

    }
    public void PlayerOneDamageTower(int damage){
        // Reduce PlayerOneTowerHealth by damage
        

    }
    public void PlayerTwoDamageTower(int damage){
        // Reduce PlayerTwoTowerHealth by damage
        

    }

}
