using System.Collections;
using UnityEngine;

namespace Project.Runtime.Scripts.AssetLoading
{
    public class LoadingScreen : MonoBehaviour
    {
        public enum LoadingScreenType
        {
            Default,
            Black,
        }

        public static bool loading = false;

        [SerializeField] private Transform _defaultLoadingScreen;
        [SerializeField] private Transform _blackLoadingScreen;

        public LoadingScreenType loadingScreenType = LoadingScreenType.Default;
        private float fadeInSpeed = 3;
        private float fadeOutSpeed = 1.5f;
        private float timeSinceLoadStart = 0;

        private CanvasGroup CanvasGroup
        {
            get
            {
                switch (loadingScreenType)
                {
                    case LoadingScreenType.Default:
                        return _defaultLoadingScreen.GetComponent<CanvasGroup>();
                    case LoadingScreenType.Black:
                        return _blackLoadingScreen.GetComponent<CanvasGroup>();
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

        public IEnumerator FadeCanvasIn(LoadingScreenType? type = LoadingScreenType.Default)
        {
            if (loading) yield break;
            loading = true;
            timeSinceLoadStart = 0;
        
            loadingScreenType = type ?? LoadingScreenType.Default;
        
            var canvasGroup = CanvasGroup;
        
            while (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha += Time.deltaTime * fadeInSpeed;
                yield return null;
            }
        }

        public IEnumerator FadeCanvasOut()
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