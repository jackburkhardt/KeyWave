using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phone
{
    [Serializable]
    public struct Email
    {
        public string Sender;
        public string Recipient;
        public string Subject;
        public string BodyText;
        public string[] BodyImagePaths;
        public bool Read;

        public Email(string sender, string recipient, string subject, string bodyText, string[] bodyImagePaths)
        {
            Sender = sender;
            Recipient = recipient;
            Subject = subject;
            BodyText = bodyText;
            BodyImagePaths = bodyImagePaths;
            Read = false;
        }
    }

    [Serializable]
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

    [Serializable]
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

    [Serializable]
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