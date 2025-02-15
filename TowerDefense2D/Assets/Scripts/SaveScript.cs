using Photon.Realtime;
using UnityEngine;

namespace Save{
    public class SaveScript : MonoBehaviour
    {
        public static bool _hasMelee = true;
        public static bool _hasWitch = false;
        public static bool _hasBear = false;
        public static bool _hasCanon = false;

        //variables for units being used in deck'
        public static bool _hasMeleeInDeck = false;
        public static bool _hasWitchInDeck = false;
        public static bool _hasBearInDeck = false;
        public static bool _hasCanonInDeck = false;

        void Start()
        {
            _hasMelee = PlayerPrefs.GetInt("hasMelee", 1) == 1;
            _hasWitch = PlayerPrefs.GetInt("hasWitch", 1) == 1;
            _hasBear = PlayerPrefs.GetInt("hasBear", 1) == 1;
            _hasCanon = PlayerPrefs.GetInt("hasCanon", 1) == 1;
        }
        public static void SaveDeck()
        {
            PlayerPrefs.SetInt("hasMelee", _hasMelee ? 1 : 0);
            PlayerPrefs.SetInt("hasWitch", _hasWitch ? 1 : 0);
            PlayerPrefs.SetInt("hasBear", _hasBear ? 1 : 0);
            PlayerPrefs.SetInt("hasCanon", _hasCanon ? 1 : 0);
        }
        public static void SaveDeckInUse()
        {
            PlayerPrefs.SetInt("hasMeleeInDeck", _hasMeleeInDeck ? 1 : 0);
            PlayerPrefs.SetInt("hasWitchInDeck", _hasWitchInDeck ? 1 : 0);
            PlayerPrefs.SetInt("hasBearInDeck", _hasBearInDeck ? 1 : 0);
            PlayerPrefs.SetInt("hasCanonInDeck", _hasCanonInDeck ? 1 : 0);
        }
    }
}
