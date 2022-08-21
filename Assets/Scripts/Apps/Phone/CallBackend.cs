using System;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

namespace Apps.Phone
{
    public class CallBackend : ScriptableObject
    {
        private static List<PhoneContact> _contacts = new List<PhoneContact>();
        private string _contactsPath;

        private void Awake()
        {
            _contactsPath = Application.streamingAssetsPath + "/GameData/Phone/contacts.json";
            GameEvent.OnGameSave += Save;
            GameEvent.OnGameLoad += Load;
            
        }
        
        public static void OutboundCall(string character, string node)
        {
            
        }
        
        [YarnCommand("receive_phonecall")]
        public static void InboundCall(string character, string node)
        {
            var contact = _contacts.Find(c => c.ContactName == character);
            if (contact.Equals(default)) return;
            
            Phone.Instance.StartOpenPhone();
            var callScreen = Phone.Instance.SwitchScreen("ActiveCall");
            callScreen.GetComponent<CallView>().ReceiveCall(contact, node);
        }

        /// <summary>
        /// Make a contact visible to the player and able to be called. Equivalent to "giving" the
        /// contact to the player in the context of the game world.
        /// </summary>
        /// <param name="contactName"></param>
        [YarnCommand("enable_contact")]
        public void EnableContact(string contactName)
        {
            PhoneContact contact = _contacts.Find(c => c.ContactName == contactName);
            if (!contact.Equals(default))
            {
                contact.Available = true;
            }
        }

        public static List<PhoneContact> Contacts => _contacts;

        private void Save() => DataManager.SerializeData(_contacts, _contactsPath);
        private void Load() => _contacts = DataManager.DeserializeData<List<PhoneContact>>(_contactsPath);

        public struct PhoneContact
        {
            public string ContactName;
            public bool Available;
            public TimeSpan StartAvailableTime; 
            public TimeSpan EndAvailableTime;

            public PhoneContact(string contactName, TimeSpan startAvailableTime, TimeSpan endAvailableTime, bool available = true)
            {
                ContactName = contactName;
                StartAvailableTime = startAvailableTime;
                EndAvailableTime = endAvailableTime;
                Available = available;
            }
        }
    }
}