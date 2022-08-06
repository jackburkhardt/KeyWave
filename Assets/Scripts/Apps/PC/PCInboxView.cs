using Assignments;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Apps.PC
{
    public class PCInboxView : MonoBehaviour
    {        
        [SerializeField] private Transform inboxView;
        [SerializeField] private Transform emailView;
        [SerializeField] private Object readEmailPrefab;
        [SerializeField] private Object unreadEmailPrefab;
        [SerializeField] private Object emailViewPrefab;
    
        private void Start()
        {
            foreach (var email in EmailBackend.PlayerInbox)
            {
                var emailGO = Instantiate(email.Read ? readEmailPrefab : unreadEmailPrefab, inboxView) as GameObject;
                var fields = emailGO.GetComponentsInChildren<TMP_Text>();
                fields[0].text = email.Sender;
                fields[1].text = email.Subject;
                emailGO.GetComponent<Button>().onClick.AddListener(() => OpenEmail(email)); 
            }
        }

        private void OpenEmail(EmailBackend.Email email)
        {
            if (emailView.childCount > 0)
            {
                for (int i = emailView.childCount - 1; i >= 0; i--)
                {
                    Destroy(emailView.GetChild(i).gameObject);
                }
            }
            
            // jank level on this is a solid 7/10. todo: unjankify
            var emailBodyGO = Instantiate(emailViewPrefab, emailView) as GameObject;
            var fields = emailBodyGO.GetComponentsInChildren<TMP_Text>();
            fields[0].text = "From: " + email.Sender + " <" + email.Sender.Replace(' ', '.') + "@keywave.net>";
            fields[1].text = "To: " + email.Recipient + " <" + email.Recipient.Replace(' ', '.') + "@keywave.net>";
            fields[2].text = email.Subject;
            fields[3].text = email.BodyText;
            email.Read = true;
            
            if (email.CompletesAssignments is { Length: > 0 }) AssignmentManager.CompleteAssignment(email.CompletesAssignments);
            if (email.ActivatesAssignments is { Length: > 0 }) AssignmentManager.ActivateAssignment(email.ActivatesAssignments);
            
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