using System;
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
        [SerializeField] public Image banner;

        private void Start()
        {
            _text.text = location.unlocked ? location.Name : "???";
        }
    }
}