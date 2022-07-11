using System.Linq;
using DefaultNamespace;
using Interaction;
using JetBrains.Annotations;
using UnityEngine;
using Yarn.Unity;

namespace Apps
{
    public class TextManager : ScriptableObject
    {
        [YarnCommand("player_sendtext")]
        public void SendTextMessage(string recipient, string message)
        {
            
        }
        
        [YarnCommand("player_receivetext_noreply")]
        public void ReceiveTextMessage(string sender, string message)
        {
            
        }

        [YarnCommand("player_receivetext")]
        public void ActivateTextMessage(string yarnNode, string sender)
        {
            
        }
    }
}