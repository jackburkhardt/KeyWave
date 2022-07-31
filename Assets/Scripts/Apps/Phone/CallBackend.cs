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
            _contactsPath = Application.dataPath + "/GameData/Phone/contacts.json";
            GameEvent.OnGameSave += Save;
            GameEvent.OnGameLoad += Load;
            
        }
        
        public static void OutboundCall(string character, string node)
        {
            
        }
        
        [YarnCommand("receive_phonecall")]
        public static void InboundCall(string character, string node)
        {
            
        }

        [YarnCommand("add_contact")]
        public void AddContact(string contactName, int open, int close)
        {
            _contacts.Add(new PhoneContact(contactName, open, close));
        }

        public static List<PhoneContact> Contacts => _contacts;

        private void Save() => DataManager.SerializeData(_contacts, _contactsPath);
        private void Load() => _contacts = DataManager.DeserializeData<List<PhoneContact>>(_contactsPath);

        public struct PhoneContact
        {
            public string ContactName;
            // time is represented on 24hr clock and uses 4 digits
            // ex: 8:00 am -> 0800 , ex: 3:25pm -> 1525
            public int StartAvailableTime; 
            public int EndAvailableTime;

            public PhoneContact(string contactName, int startAvailableTime, int endAvailableTime)
            {
                ContactName = contactName;
                StartAvailableTime = startAvailableTime;
                EndAvailableTime = endAvailableTime;
            }
        }
    }
}