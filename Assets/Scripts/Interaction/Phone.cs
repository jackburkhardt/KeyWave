using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interaction
{
    public class Phone : MonoBehaviour
    {
        [SerializeField] private float openPhoneY;
        [SerializeField] private float closePhoneY;
        [SerializeField] private float openCloseDuration;
        private bool transitioning;
        [SerializeField] private List<Object> screens;


        public void SwitchScreen(string screen)
        {
            
        }
        
        public void StartOpenPhone() => StartCoroutine(OpenPhone());
        private IEnumerator OpenPhone()
        {
            if (transitioning) yield break;
            Vector3 startPos = transform.position;
            float t = 0f;
            transitioning = true;
            while (t < openCloseDuration)
            {
                t += Time.deltaTime;
                // the choice between slerp and lerp was simply based on desired visual look
                transform.position = Vector3.Lerp(startPos, new Vector3(startPos.x, openPhoneY), t / openCloseDuration);
                yield return null;
            }
            transitioning = false;
        }

        public void StartClosePhone() => StartCoroutine(ClosePhone());
        private IEnumerator ClosePhone()
        {
            if (transitioning) yield break;
            Vector3 startPos = transform.position;
            float t = 0f;
            transitioning = true;
            while (t < openCloseDuration)
            {
                t += Time.deltaTime;
                // the choice between slerp and lerp was simply based on desired visual look
                transform.position = Vector3.Lerp(startPos, new Vector3(startPos.x, closePhoneY), t / openCloseDuration);
                yield return null;
            }
            transitioning = false;
        }
    }
    
}