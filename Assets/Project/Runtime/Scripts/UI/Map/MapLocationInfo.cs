using Project.Runtime.Scripts.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.UI.Map
{
    /// <summary>
    /// Stores information useful for the map, including names, objectives, and other data.
    /// </summary>
    public class MapLocationInfo : MonoBehaviour
    {
        [SerializeField] public Location location;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Image _youAreHere;


        private void Start()
        {
            _text.text = location.unlocked ? location.Name : "???";
            GetComponent<Button>().interactable = location.unlocked;
            _youAreHere.gameObject.SetActive(Location.PlayerLocation == location);


        }
    }
}