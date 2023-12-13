using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Apps.Phone
{
    public class ContactListView : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private Object listingPrefab;
        [SerializeField] private Color availableColor;
        [SerializeField] private Color unavailableColor;
        private Dictionary<CallBackend.PhoneContact, TMP_Text> contactAvailabilityTexts = new Dictionary<CallBackend.PhoneContact, TMP_Text>();

        private void Awake()
        {
            GameEvent.OnTimeChange += OnTimeChange;
        }

        private void OnEnable()
        {
            foreach (var contact in CallBackend.Contacts)
            {
                if (!contact.Available) continue;
                var contactGO = Instantiate(listingPrefab, content) as GameObject;
                var fields = contactGO.GetComponentsInChildren<TMP_Text>();
                fields[0].text = contact.ContactName;
                contactAvailabilityTexts.Add(contact, fields[1]);
                UpdateAvailabilityText(contact, fields[1]);
                //contactGO.GetComponent<Button>().onClick.AddListener(() => CallBackend.OutboundCall("x")); 
            }
        }

        private void OnTimeChange(TimeSpan time)
        {
            foreach (var contact in contactAvailabilityTexts)
            {
                UpdateAvailabilityText(contact.Key, contact.Value);
            }
        }
        
        private void UpdateAvailabilityText(CallBackend.PhoneContact contact, TMP_Text field)
        {
            if (!contactAvailabilityTexts.ContainsKey(contact)) return;
            
            bool available = RealtimeManager.Time < contact.EndAvailableTime && RealtimeManager.Time > contact.StartAvailableTime;
            field.text = available
                ? "Available to call!"
                : "Available " + contact.StartAvailableTime + " to " + contact.EndAvailableTime;
            field.color = available ? availableColor : unavailableColor;
        }

        private void OnDestroy()
        {
            GameEvent.OnTimeChange -= OnTimeChange;
        }
    }
}