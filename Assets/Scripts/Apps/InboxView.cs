using System;
using Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Apps
{
    public class InboxView : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private Object readEmailPrefab;
        [SerializeField] private Object unreadEmailPrefab;
        
        private void OnEnable()
        {
            foreach (var email in EmailManager.PlayerInbox)
            {
                var emailGO = Instantiate(email.Read ? readEmailPrefab : unreadEmailPrefab, content) as GameObject;
                var fields = emailGO.GetComponentsInChildren<TMP_Text>();
                fields[0].text = email.Sender;
                fields[1].text = email.Subject;
                emailGO.GetComponent<Button>().onClick.AddListener(() => OpenEmail(email)); 
            }
        }

        private void OpenEmail(Email email)
        {
            var emailBodyGO = Phone.Instance.SwitchScreen("EmailBody");
            var fields = emailBodyGO.GetComponentsInChildren<TMP_Text>();
            fields[0].text = "From: " + email.Sender + "<" + email.Sender.Replace(' ', '.') + "@keywave.net>";
            fields[1].text = "To: " + email.Recipient + "<" + email.Recipient.Replace(' ', '.') + "@keywave.net>";
            fields[2].text = email.Subject;
            fields[3].text = email.BodyText;
            email.Read = true;
            
            // if there are no images, we are done here!
            if (email.BodyImagePaths.Length <= 0) return;
            
            var images = EmailManager.LoadEmailImages(email.BodyImagePaths);
            foreach (var image in images)
            {
                var imageGO = new GameObject("image", typeof(RawImage));
                imageGO.transform.parent = emailBodyGO.GetComponentInChildren<ContentSizeFitter>().transform;
                imageGO.GetComponent<RawImage>().texture = image;
            }
        }
    }
}