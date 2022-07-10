using Interaction;
using UnityEngine;
using Yarn.Unity;

namespace Apps
{
    public class TextManager : ScriptableObject
    {
        [YarnCommand("func_player_sendtext")]
        public void SendText(string text)
        {
            
        }

        public void ActivateTextMessage(Character character, string yarnNode)
        {
            
        }
    }
}