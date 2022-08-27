using System;
using System.Collections.Generic;
using System.Linq;
using Assignments;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Apps.Phone
{
    /*
    * This class handles the text messaging for the Phone.
     * This should be attached to the MessageView app prefab for the phone.
    */
    public class MessagesView : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private Object readTextPrefab;
        [SerializeField] private Object unreadTextPrefab;

        [SerializeField] private Object playerTextBubblePrefab;
        [SerializeField] private Object otherTextBubblePrefab;
        
        // just keeps track of the text listings and their gameobjects so they can easily be found
        private Dictionary<TextBackend.TextConversation, GameObject> textToGameObject = new Dictionary<TextBackend.TextConversation, GameObject>();

        private void OnEnable()
        {
            GameEvent.OnTextReceive += AddConversationToInbox;
            GameEvent.OnConversationOpen += OpenConversation;
            foreach (var convo in TextBackend.Conversations)
            {
                AddConversationToInbox(convo);
            }
        }

        private void AddConversationToInbox(TextBackend.TextConversation convo)
        {
            if (textToGameObject.ContainsKey(convo))
            {
                Destroy(textToGameObject[convo]);
                textToGameObject.Remove(convo);
                AddConversationToInbox(convo);
                return;
            }
            
            var textGO = Instantiate(convo.Read ? readTextPrefab : unreadTextPrefab, content) as GameObject;
            var fields = textGO.GetComponentsInChildren<TMP_Text>();
            fields[0].text = convo.Recipient;
            fields[1].text = convo.Messages.Last().Content;
            textGO.GetComponent<Button>().onClick.AddListener(() => GameEvent.OpenConversation(convo));
            
            textToGameObject.Add(key: convo, value: textGO);
        }

        private void OpenConversation(TextBackend.TextConversation convo)
        {
            var textScreenGO = Phone.Instance.SwitchScreen("TextBody");
            var textScreenContent = textScreenGO.GetComponent<ContentSizeFitter>().transform;

            foreach (var tm in convo.Messages)
            {
                var textBubbleGO = Instantiate(tm.FromPlayer ? playerTextBubblePrefab : otherTextBubblePrefab
                    , textScreenContent) as GameObject;
                textBubbleGO.GetComponent<TMP_Text>().text = tm.Content;
            }
        }

        private void OnDestroy()
        {
            GameEvent.OnTextReceive -= AddConversationToInbox;
            GameEvent.OnConversationOpen -= OpenConversation;
        }
    }
}