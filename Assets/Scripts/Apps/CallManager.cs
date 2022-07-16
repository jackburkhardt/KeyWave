using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Yarn.Unity;

namespace Apps
{
    public class CallManager : ScriptableObject
    {
        private static List<PhoneContact> _contacts = new List<PhoneContact>();
        private string _contactsPath;

        private void Awake()
        {
            _contactsPath = Application.dataPath + "/GameData/Phone/contacts.json";
            GameEvent.OnGameSave += Save;
            GameEvent.OnGameLoad += Load;

            TestContacts();
        }

        private void TestContacts()
        {
            PhoneContact contact = new PhoneContact("Steve Becker", 0900, 1700);
            _contacts.Add(contact);
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
        
        private void Save()
        {
            StreamWriter sw = new StreamWriter(_contactsPath, false);
            string json = JsonConvert.SerializeObject(_contacts, Formatting.Indented);
            sw.Write(json);
            sw.Close();
        }

        public static List<PhoneContact> Contacts => _contacts;

        private void Load()
        {
            if (File.Exists(_contactsPath))
            {
                _contacts = JsonConvert.DeserializeObject<List<PhoneContact>>(File.ReadAllText(_contactsPath));
            }
        }
    }
}