using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Runtime.Scripts.UI
{
    public class CanvasGroupFader : MonoBehaviour
    {
        public UnityEvent OnFadeIn;
        public UnityEvent OnFadedIn;
        public UnityEvent OnFadeOut;
        public UnityEvent OnFadedOut;

        [SerializeField] private float duration;
        [SerializeField] private float alpha;
        [SerializeField] private CanvasGroup _canvasGroup;

        private void Awake()
        {
            
            _canvasGroup ??= GetComponent<CanvasGroup>();
            
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void Start()
        {
            
            _canvasGroup.alpha = 0;
            if (_canvasGroup.gameObject != this.gameObject) _canvasGroup.gameObject.SetActive(false);
        
        }

        private void OnEnable()
        {
            
        }

        public void FadeIn()
        {
            OnFadeIn.Invoke();
            StopAllCoroutines();
            
            StartCoroutine(FadeIn(_canvasGroup, duration, alpha));
        }

        public void FadeOut()
        {
            OnFadeOut.Invoke();
            StopAllCoroutines();
            StartCoroutine(FadeOut(_canvasGroup, duration, alpha));
        }

        private IEnumerator FadeIn(CanvasGroup canvasGroup, float duration, float alpha)
        {
            _canvasGroup.gameObject.SetActive(true);
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
            _canvasGroup.gameObject.SetActive(false);
            OnFadedOut.Invoke();
            //gameObject.SetActive(false);
        }
    }
}