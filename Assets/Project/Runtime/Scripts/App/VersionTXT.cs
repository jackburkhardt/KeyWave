using TMPro;
using UnityEngine;

namespace Project.Runtime.Scripts.App
{
    public class VersionTXT : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI versionText;
        
        private void Start()
        {
            versionText.text = $"Version {Application.version}";
        }
    }
}