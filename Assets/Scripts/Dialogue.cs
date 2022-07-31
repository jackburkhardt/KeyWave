using System;
using UnityEngine;
using Yarn;
using Yarn.Markup;
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

        public static void RunLine(string node)
        {
            if (_runner.NodeExists(node)) _runner.StartDialogue(node);
        }
    }
}