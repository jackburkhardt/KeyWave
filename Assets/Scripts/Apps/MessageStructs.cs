using System;
using System.Collections.Generic;
using UnityEngine;

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
