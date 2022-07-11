using UnityEngine;
using Yarn.Unity;

namespace Apps
{
    public class CallManager : ScriptableObject
    {
        public static void OutboundCall(string character, string node)
        {
            
        }
        
        [YarnCommand("receive_phonecall")]
        public static void InboundCall(string character, string node)
        {
            
        }
    }
}