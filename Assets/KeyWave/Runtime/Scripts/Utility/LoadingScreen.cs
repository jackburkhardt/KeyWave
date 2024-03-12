using System.Collections;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    bool _loading = false;
    private float fadeInSpeed = 3;
    private float fadeOutSpeed = 1.5f;
    private float timeSinceLoadStart = 0;
    [SerializeField] private CanvasGroup _canvasGroup;
    // Start is called before the first frame update
    void Start()
    {
        _canvasGroup.alpha = 0;
    }
    
    public IEnumerator FadeCanvasIn()
    {
        _loading = true;
        timeSinceLoadStart = 0;
        
        while (_canvasGroup.alpha < 1)
        {
            _canvasGroup.alpha += Time.deltaTime * fadeInSpeed;
            yield return null;
        }
    }

    public IEnumerator FadeCanvasOut()
    {
        while (timeSinceLoadStart < 1.5f)
        {
            yield return null;
        }
        
        while (_canvasGroup.alpha > 0)
        {
            _canvasGroup.alpha -= Time.deltaTime * fadeOutSpeed;
            yield return null;
        }
        
        _loading = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_loading) timeSinceLoadStart += Time.deltaTime;
    }
}
