using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Apps.PC
{
    /*
     * This class handles the email inbox screen for the PC. It is almost identical to the
     * phone inbox screen, except that it has a slightly different layout. This should be attached
     * to the Emails app prefab for the PC.
     */
    public class PCInboxView : MonoBehaviour
    {        
        [SerializeField] private Transform inboxView;
        [SerializeField] private Transform emailView;
        [SerializeField] private Object readEmailPrefab;
        [SerializeField] private Object unreadEmailPrefab;
        [SerializeField] private Object emailViewPrefab;
        
        // just keeps track of the inbox email listings and their gameobjects so they can easily be found
        private Dictionary<EmailBackend.Email, GameObject> emailToGameObject = new Dictionary<EmailBackend.Email, GameObject>();
    
        private void OnEnable()
        {
            GameEvent.OnEmailDeliver += AddEmailToInbox;
            GameEvent.OnEmailOpen += OpenEmail;

            EmailBackend.Load();

            foreach (var email in EmailBackend.PlayerInbox)
            {
                AddEmailToInbox(email);
            }
        }

        private void AddEmailToInbox(EmailBackend.Email email)
        {
            if (!email.Available) { Debug.Log("frick"); return; }
            
            var emailGO = Instantiate(email.Read ? readEmailPrefab : unreadEmailPrefab, inboxView) as GameObject;
            var fields = emailGO.GetComponentsInChildren<TMP_Text>();
            fields[0].text = email.Sender;
            fields[1].text = email.Subject;
            emailGO.GetComponent<Button>().onClick.AddListener(() => GameEvent.OpenEmail(email));
            
            emailToGameObject.Add(key: email, value: emailGO);
        }
        
        private void OpenEmail(EmailBackend.Email email)
        {
            // destroy all children of current emailView
            foreach (Transform child in emailView)
            {
                Destroy(child.gameObject);
            }
            
            // jank level on this is a solid 7/10. todo: unjankify
            var emailBodyGO = Instantiate(emailViewPrefab, emailView) as GameObject;
            var fields = emailBodyGO.GetComponentsInChildren<TMP_Text>();
            fields[0].text = "From: " + email.Sender + " <" + email.Sender.Replace(' ', '.') + "@keywave.net>";
            fields[1].text = "To: " + email.Recipient + " <" + email.Recipient.Replace(' ', '.') + "@keywave.net>";
            fields[2].text = email.Subject;
            fields[3].text = email.BodyText;
            email.Read = true;

            // this is kind of a hack to swap the inbox listing image for an email from "unread" to "read"
            // essentially just gets rid of the unread listing and makes a new one for read
            Destroy(emailToGameObject[email]);
            emailToGameObject.Remove(email);
            AddEmailToInbox(email);
            
            // if there are no images, we are done here!
            if (email.BodyImagePaths.Length <= 0) return;
            
            var images = EmailBackend.LoadEmailImages(email.BodyImagePaths);
            foreach (var image in images)
            {
                var imageGO = new GameObject("image", typeof(RawImage));
                imageGO.transform.parent = emailBodyGO.GetComponentInChildren<ContentSizeFitter>().transform;
                imageGO.GetComponent<RawImage>().texture = image;
            }
        }

        private void OnDestroy()
        {
            GameEvent.OnEmailDeliver -= AddEmailToInbox;
            GameEvent.OnEmailOpen -= OpenEmail;
        }
    }
}