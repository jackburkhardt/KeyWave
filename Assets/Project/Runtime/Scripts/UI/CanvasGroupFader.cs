using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using Project;

namespace Project.Runtime.Scripts.UI
{
    public class CanvasGroupFader : MonoBehaviour
    {
        public UnityEvent OnFadeIn;
        public UnityEvent OnFadedIn;
        public UnityEvent OnFadeOut;
        public UnityEvent OnFadedOut;

        [SerializeField] private float duration;
        [Label("Target Alpha")]
        [SerializeField] private float alpha;
        
        [GetComponent]
        [SerializeField] private CanvasGroup _canvasGroup;

        private void Awake()
        {
            
            _canvasGroup ??= GetComponent<CanvasGroup>();
            
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void Start()
        {
            
            //_canvasGroup.alpha = 0;
            _canvasGroup.blocksRaycasts = false;
        
        }

        private void OnEnable()
        {
            
        }

        public void FadeIn()
        {
            OnFadeIn.Invoke();
            StopAllCoroutines();
            
            StartCoroutine(FadeIn(duration, alpha));
        }

        public void FadeOut()
        {
            OnFadeOut.Invoke();
            StopAllCoroutines();
            StartCoroutine(FadeOut(duration, alpha));
        }

        private IEnumerator FadeIn(float duration, float alpha)
        {
           // _canvasGroup.gameObject.SetActive(true);
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 0;
            while (_canvasGroup.alpha < alpha)
            {
                _canvasGroup.alpha += Time.deltaTime / duration;
                yield return null;
            }
            OnFadedIn.Invoke();
        }

        private IEnumerator FadeOut(float duration, float alpha)
        {
            _canvasGroup.alpha = alpha;
            while (_canvasGroup.alpha > 0)
            {
                _canvasGroup.alpha -= Time.deltaTime / duration;
                yield return null;
            }
           // _canvasGroup.gameObject.SetActive(false);
            _canvasGroup.blocksRaycasts = false;
            OnFadedOut.Invoke();
            //gameObject.SetActive(false);
        }
    }
}