using Assignments;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Apps.Phone
{
    public class InboxView : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private Object readEmailPrefab;
        [SerializeField] private Object unreadEmailPrefab;
        
        private void OnEnable()
        {
            foreach (var email in EmailBackend.PlayerInbox)
            {
                var emailGO = Instantiate(email.Read ? readEmailPrefab : unreadEmailPrefab, content) as GameObject;
                var fields = emailGO.GetComponentsInChildren<TMP_Text>();
                fields[0].text = email.Sender;
                fields[1].text = email.Subject;
                emailGO.GetComponent<Button>().onClick.AddListener(() => OpenEmail(email)); 
            }
        }

        private void OpenEmail(EmailBackend.Email email)
        {
            // jank level on this is a solid 7/10. todo: unjankify
            var emailBodyGO = Phone.Instance.SwitchScreen("EmailBody");
            var fields = emailBodyGO.GetComponentsInChildren<TMP_Text>();
            fields[0].text = "From: " + email.Sender + " <" + email.Sender.Replace(' ', '.') + "@keywave.net>";
            fields[1].text = "To: " + email.Recipient + " <" + email.Recipient.Replace(' ', '.') + "@keywave.net>";
            fields[2].text = email.Subject;
            fields[3].text = email.BodyText;
            email.Read = true;
            
            if (email.CompletesAssignments.Length > 0) AssignmentManager.CompleteAssignment(email.CompletesAssignments);
            if (email.ActivatesAssignments.Length > 0) AssignmentManager.ActivateAssignment(email.ActivatesAssignments);
            
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
    }
}