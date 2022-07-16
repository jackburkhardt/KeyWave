using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Apps
{
    public class ContactListView : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private Object listingPrefab;
        [SerializeField] private Color availableColor;
        [SerializeField] private Color unavailableColor;
        
        private void OnEnable()
        {
            foreach (var contact in CallManager.Contacts)
            {
                var contactGO = Instantiate(listingPrefab, content) as GameObject;
                var fields = contactGO.GetComponentsInChildren<TMP_Text>();
                bool available = Player.Time < contact.EndAvailableTime && Player.Time > contact.StartAvailableTime;
                fields[0].text = contact.ContactName;
                fields[1].text = available
                    ? "Available to call!"
                    : "Available " + contact.StartAvailableTime + " to " + contact.EndAvailableTime;
                fields[1].color = available ? availableColor : unavailableColor;
                //emailGO.GetComponent<Button>().onClick.AddListener(() => OpenEmail(email)); 
            }
        }
        
    }
}