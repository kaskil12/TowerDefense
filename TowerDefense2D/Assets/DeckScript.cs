using UnityEngine;
using Save;
using UnityEngine.UI;

public class DeckScript : MonoBehaviour
{
    public Toggle meleeToggle;
    public Toggle witchToggle;
    public Toggle bearToggle;
    public Toggle canonToggle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meleeToggle.isOn = SaveScript._hasMeleeInDeck;
        witchToggle.isOn = SaveScript._hasWitchInDeck;
        bearToggle.isOn = SaveScript._hasBearInDeck;
        canonToggle.isOn = SaveScript._hasCanonInDeck;
    }

    // Update is called once per frame
    void Update()
    {
        SaveScript._hasMeleeInDeck = meleeToggle.isOn;
    }
}
