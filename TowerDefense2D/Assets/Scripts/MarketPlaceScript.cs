using UnityEngine;

public class MarketPlaceScript : MonoBehaviour
{
    public GameObject MarketPlaceObject;
    public GameObject InventoryObject;
    public GameObject ShopObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ToggeMarket(){
        MarketPlaceObject.SetActive(!MarketPlaceObject.activeSelf);
    }
    public void Inventory(){
        // Open Inventory
        InventoryObject.SetActive(true);
        ShopObject.SetActive(false);
    }
    public void Shop(){
        // Open Shop
        InventoryObject.SetActive(false);
        ShopObject.SetActive(true);
    }
}
