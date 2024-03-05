using UnityEngine;
using Yarn.Unity;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;


namespace Interaction
{
    public class Dialogue : MonoBehaviour, IInteractable
    {

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
          //    GameEvent.InteractionStart(this);
            }
        }

        public void Interact()
        {
          // GameEvent.InteractionStart(this);
          // GameEvent.AnyEvent();
        }

        public void EndInteraction()
        {

        }

        public void OnPointerEnter(PointerEventData eventData)
        {

        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }

        public bool PreviouslyInteractedWith { get; private set; }


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

        public static DialogueRunner Runner => _runner;
    }
}