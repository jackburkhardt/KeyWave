using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Runtime.Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupFader : MonoBehaviour
    {
        public UnityEvent OnFadeIn;
        public UnityEvent OnFadedIn;
        public UnityEvent OnFadeOut;
        public UnityEvent OnFadedOut;

        [SerializeField] private float duration;
        [SerializeField] private float alpha;

        private void Awake()
        {
            //   gameObject.SetActive(false);
            GetComponent<CanvasGroup>().interactable = false;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        private void Start()
        {
            GetComponent<CanvasGroup>().alpha = 0;
        
        }

        private void OnEnable()
        {
            FadeIn();
        }

        public void FadeIn()
        {
            OnFadeIn.Invoke();
            var canvasGroup = GetComponent<CanvasGroup>();
            StopAllCoroutines();
            StartCoroutine(FadeIn(canvasGroup, duration, alpha));
        }

        public void FadeOut()
        {
            OnFadeOut.Invoke();
            var canvasGroup = GetComponent<CanvasGroup>();
            StopAllCoroutines();
            StartCoroutine(FadeOut(canvasGroup, duration, alpha));
        }

        private IEnumerator FadeIn(CanvasGroup canvasGroup, float duration, float alpha)
        {
            canvasGroup.alpha = 0;
            while (canvasGroup.alpha < alpha)
            {
                canvasGroup.alpha += Time.deltaTime / duration;
                yield return null;
            }
            OnFadedIn.Invoke();
        }

        private IEnumerator FadeOut(CanvasGroup canvasGroup, float duration, float alpha)
        {
            canvasGroup.alpha = alpha;
            while (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha -= Time.deltaTime / duration;
                yield return null;
            }
            OnFadedOut.Invoke();
            //gameObject.SetActive(false);
        }
    }
}