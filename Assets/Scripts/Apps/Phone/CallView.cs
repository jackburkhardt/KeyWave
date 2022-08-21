using System;
using System.Collections;
using KeyWave;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

namespace Apps.Phone
{
    public class CallView : MonoBehaviour
    {
        [SerializeField] private GameObject pickUpButton;
        [SerializeField] private GameObject hangUpButton;

        [SerializeField] private Image callerImage;
        [SerializeField] private TMP_Text callTopText;
        [SerializeField] private TMP_Text callBottomText;

        private string callNode;
        private CallBackend.PhoneContact callContact;
        private bool callActive;

        public void ReceiveCall(CallBackend.PhoneContact caller, string node)
        {
            callContact = caller;
            callNode = node;
            
            callTopText.text = caller.ContactName;
            callBottomText.text = "CALL INCOMING";
        }

        public void AcceptCall()
        {
            Dialogue.Run(callNode);
            pickUpButton.SetActive(false);
            
            var endCallPos = hangUpButton.transform.position;
            // just move the button to the middle of the screen when the pick up button is gone
            endCallPos = new Vector3(endCallPos.x - (endCallPos.x - pickUpButton.transform.position.x) / 2, 
                endCallPos.y, endCallPos.z);
            hangUpButton.transform.position = endCallPos;

            StartCoroutine(TrackCallTime());
        }

        public void EndCall()
        {
            if (callActive)
            {
                Dialogue.Stop();
                StopCoroutine(TrackCallTime());
            }
            // todo: some kind of callback so the caller knows the call was declined
            Destroy(this.gameObject);
        }

        private IEnumerator TrackCallTime()
        {
            int seconds = 0;
            while (seconds < 1000) // juuust in case for some reason the call never ends
            {
                callBottomText.text = $"{TimeSpan.FromSeconds(seconds):mm\\:ss}";
                seconds++;
                yield return new WaitForSeconds(1);
            }
            
            throw new Exception("CallView: A call has been going for 1000 seconds and was forcefully ended. Is this intended"); 
        }
        
        
    }
}