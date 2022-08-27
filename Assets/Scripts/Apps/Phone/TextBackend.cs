using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

namespace Apps.Phone
{
    public class TextBackend : ScriptableObject
    {
        public static List<TextConversation> Conversations = new List<TextConversation>();
        private static string _conversationsPath;
        
        private void Awake()
        {
            _conversationsPath = Application.streamingAssetsPath + "/GameData/Texts/conversations.json";
            GameEvent.OnGameSave += Save;
            GameEvent.OnGameLoad += Load;
        }
        

        [YarnCommand("player_sendtext")]
        public static void SendTextMessage(string recipient, string message)
        {
            var foundIndex = Conversations.FindIndex(c => c.Recipient == recipient);
            if (foundIndex == -1)
            {
                var newConvo = new TextConversation(recipient, new List<TextMessage>());
                Conversations.Add(newConvo);
                foundIndex = Conversations.Count - 1;
            }

            var foundConvo = Conversations[foundIndex];
            var tm = new TextMessage(message, true);
            foundConvo.Messages.Add(tm);
            GameEvent.SendText(foundConvo);
        }
        
        [YarnCommand("player_receivetext_noreply")]
        public static void ReceiveTextMessage(string sender, string message)
        {
            var foundIndex = Conversations.FindIndex(c => c.Recipient == sender);
            if (foundIndex == -1)
            {
                var newConvo = new TextConversation(sender, new List<TextMessage>());
                Conversations.Add(newConvo);
                foundIndex = Conversations.Count - 1;
            }
            
            var foundConvo = Conversations[foundIndex];
            var tm = new TextMessage(message, false);
            foundConvo.Messages.Add(tm);
            GameEvent.ReceiveText(foundConvo);

        }

        [YarnCommand("player_receivetext")]
        public static void ActivateTextMessage(string yarnNode, string sender)
        {
            
        }

        private void Save() => DataManager.SerializeData(Conversations, _conversationsPath);
        private void Load() => Conversations = DataManager.DeserializeData<List<TextConversation>>(_conversationsPath);
        
        public struct TextConversation
        {
            public string Recipient;
            public List<TextMessage> Messages;
            public bool Read;

            public TextConversation(string recipient, List<TextMessage> messages)
            {
                Recipient = recipient;
                Messages = messages;
                Read = false;
            }
        }
        
        public struct TextMessage
        {
            public string Content;
            public bool FromPlayer;

            public TextMessage(string content, bool fromPlayer)
            {
                FromPlayer = fromPlayer;
                Content = content;
            }
        }
    }
}