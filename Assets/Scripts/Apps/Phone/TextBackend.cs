using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

namespace Apps.Phone
{
    public class TextBackend : ScriptableObject
    {
        private Dictionary<string, TextConversation> _conversations = new Dictionary<string, TextConversation>();
        private string _conversationsPath;
        
        private void Awake()
        {
            _conversationsPath = Application.streamingAssetsPath + "/GameData/Texts/conversations.json";
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

        private void Save() => DataManager.SerializeData(_conversations, _conversationsPath);
        private void Load() => _conversations = DataManager.DeserializeData<Dictionary<string, TextConversation>>(_conversationsPath);
        
        public struct TextConversation
        {
            public string Recipient;
            public List<TextMessage> Messages;

            public TextConversation(string recipient, List<TextMessage> messages)
            {
                Recipient = recipient;
                Messages = messages;
            }
        }
        
        public struct TextMessage
        {
            public bool FromPlayer;
            public string Content;

            public TextMessage(bool fromPlayer, string content)
            {
                FromPlayer = fromPlayer;
                Content = content;
            }
        }
    }
}