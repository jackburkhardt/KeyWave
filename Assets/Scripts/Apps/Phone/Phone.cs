using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Apps.Phone
{
    public class Phone : MonoBehaviour
    {
        public static Phone Instance;
        [SerializeField] private float openPhoneY;
        [SerializeField] private float closePhoneY;
        [SerializeField] private float openCloseDuration;
        private bool transitioning;
        [SerializeField] private List<Object> screens = new List<Object>();
        [SerializeField] private Transform phoneTransform;
        [SerializeField] private Transform appTransform;
        [SerializeField] private GameObject openPhoneButton;
        private List<GameObject> screenHistory = new List<GameObject>();

        private void Awake()
        {
            Instance = this;
            SwitchScreen("HomeScreen");
        }

        public void UISwitchScreen(string screen) => SwitchScreen(screen);
        public GameObject SwitchScreen(string screen)
        {
            var screenPrefab = screens.Find(s => s.name == screen);
            if (!screenPrefab)
            {
                Debug.LogError("Attempted to switch to screen \"" + screen + "\" but one by that name was not found. Did you add the prefab to the screen list?");    
                return null;
            }

            //var screenGO = Instantiate(screenPrefab, phoneTransform.position, Quaternion.identity, phoneTransform) as GameObject;
            var screenGO = Instantiate(screenPrefab, appTransform) as GameObject;
            screenHistory.Add(screenGO);
            return screenGO;
        }

        /// <summary>
        /// Goes back to the previous screen, removing the current one in the process.
        /// </summary>
        public void GoBack()
        {
            if (screenHistory.Count <= 1)
            {
                StartClosePhone();
                return;
            }
            
            Destroy(screenHistory.Last());
            screenHistory.Remove(screenHistory.Last());
        }

        /// <summary>
        /// Removes all screens until it returns to the home screen.
        /// </summary>
        public void GoHome()
        {
            while (screenHistory.Count > 1)
            {
                Destroy(screenHistory.Last());
                screenHistory.Remove(screenHistory.Last());
            }
        }

        public void SendNotification()
        {
            
        }
        
        /// <summary>
        /// Starts a coroutine to bring the phone into view.
        /// </summary>
        public void StartOpenPhone() => StartCoroutine(OpenPhone());
        private IEnumerator OpenPhone()
        {
            if (transitioning) yield break;
            openPhoneButton.SetActive(false);
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

        /// <summary>
        /// Starts a coroutine to hide the phone from view.
        /// </summary>
        public void StartClosePhone() => StartCoroutine(ClosePhone());
        private IEnumerator ClosePhone()
        {
            if (transitioning) yield break;
            Vector3 startPos = transform.position;
            float t = 0f;
            transitioning = true;

            GoHome();
            
            while (t < openCloseDuration)
            {
                t += Time.deltaTime;
                // the choice between slerp and lerp was simply based on desired visual look
                transform.position = Vector3.Lerp(startPos, new Vector3(startPos.x, closePhoneY), t / openCloseDuration);
                yield return null;
            }
            openPhoneButton.SetActive(true);
            transitioning = false;
        }
    }
    
}