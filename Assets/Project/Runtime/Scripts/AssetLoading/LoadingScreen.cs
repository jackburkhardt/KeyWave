using System.Collections;
using UnityEngine;

namespace Project.Runtime.Scripts.AssetLoading
{
    public class LoadingScreen : MonoBehaviour
    {
        public enum Transition
        {
            Default,
            Black,
            None
        }

        public static bool loading = false;

        [SerializeField] private Transform _defaultLoadingScreen;
        [SerializeField] private Transform _blackLoadingScreen;

        public Transition transition = Transition.Default;
        private float fadeInSpeed = 3;
        private float fadeOutSpeed = 1.5f;
        private float timeSinceLoadStart = 0;

        private CanvasGroup CanvasGroup
        {
            get
            {
                switch (transition)
                {
                    case Transition.Default:
                        return _defaultLoadingScreen.GetComponent<CanvasGroup>();
                    case Transition.Black:
                        return _blackLoadingScreen.GetComponent<CanvasGroup>();
                    case Transition.None:
                        return null;
                    default:
                        return _defaultLoadingScreen.GetComponent<CanvasGroup>();
                }
            }
        }

        void Start()
        {
            _defaultLoadingScreen.GetComponent<CanvasGroup>().alpha = 0;
            _blackLoadingScreen.GetComponent<CanvasGroup>().alpha = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if (loading) timeSinceLoadStart += Time.deltaTime;
        }

        public IEnumerator Show(Transition? type = Transition.Default)
        {
            if (loading || type == Transition.None) yield break;
            loading = true;
            timeSinceLoadStart = 0;
        
            transition = type ?? Transition.Default;
        
            var canvasGroup = CanvasGroup;
        
            while (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha += Time.deltaTime * fadeInSpeed;
                yield return null;
            }
        }

        public IEnumerator Hide()
        {
        
            while (timeSinceLoadStart < 1.5f)
            {
                yield return null;
            }
        
            var canvasGroup = CanvasGroup;
        
            while (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha -= Time.deltaTime * fadeOutSpeed;
                yield return null;
            }
        
            loading = false;
        
        }
    }
}