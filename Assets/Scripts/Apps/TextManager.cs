using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Yarn.Unity;

namespace Apps
{
    public class TextManager : ScriptableObject
    {
        private Dictionary<string, TextConversation> _conversations = new Dictionary<string, TextConversation>();
        private string _conversationsPath;
        
        private void Awake()
        {
            _conversationsPath = Application.dataPath + "/GameData/Texts/conversations.json";
            GameEvent.OnGameSave += Save;
            GameEvent.OnGameLoad += Load;
        }
        

        [YarnCommand("player_sendtext")]
        public void SendTextMessage(string recipient, string message)
        {
            if (!_conversations.TryGetValue(recipient, out var convo))
            {
                convo = new TextConversation(recipient, new List<TextMessage>());
                _conversations.Add(recipient, convo);
            }

            var tm = new TextMessage(true, message);
            convo.Messages.Add(tm);
            GameEvent.SendText(convo, tm);
        
        }
        
        [YarnCommand("player_receivetext_noreply")]
        public void ReceiveTextMessage(string sender, string message)
        {
            if (!_conversations.TryGetValue(sender, out var convo))
            {
                convo = new TextConversation(sender, new List<TextMessage>());
                _conversations.Add(sender, convo);
            }
            
            var tm = new TextMessage(false, message);
            convo.Messages.Add(tm);
            GameEvent.ReceiveText(convo, tm);

        }

        [YarnCommand("player_receivetext")]
        public void ActivateTextMessage(string yarnNode, string sender)
        {
            
        }
        
        private void Save()
        {
            StreamWriter sw = new StreamWriter(_conversationsPath, false);
            string json = JsonConvert.SerializeObject(_conversations, Formatting.Indented);
            sw.Write(json);
            sw.Close();
        }

        private void Load()
        {
            if (File.Exists(_conversationsPath))
            {
                _conversations = JsonConvert.DeserializeObject<Dictionary<string, TextConversation>>(File.ReadAllText(_conversationsPath));
            }
        }
    }
}