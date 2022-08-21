using UnityEngine;
using Yarn.Unity;

namespace KeyWave
{
    public class Dialogue : MonoBehaviour
    {
        private static DialogueRunner _runner;

        private void Start()
        {
            _runner = GetComponent<DialogueRunner>();
        }

        public static void Run(string node, string view = "")
        {
            if (_runner.NodeExists(node)) _runner.StartDialogue(node);
        }

        public static void Stop()
        {
            _runner.Stop();
        }
    }
}