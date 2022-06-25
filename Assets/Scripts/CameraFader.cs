using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class CameraFader : MonoBehaviour
{
    public static CameraFader Instance = null;
    [SerializeField] private SpriteRenderer _fadeImage;
    [SerializeField] private Color _defaultFadeColor = Color.black;
    [SerializeField] private float _defaultFadeSpeed = 1;

    // TODO: adjust if need more cameras
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        } else if (Instance != this)
        {
            Destroy(gameObject);
        }
        _fadeImage.color = _defaultFadeColor;
        //DontDestroyOnLoad(this.transform.parent.gameObject); //lmao
        StartCoroutine(FadeFromColor(_defaultFadeSpeed));
    }

    public IEnumerator FadeToColor(Color color, float speed)
    {
        color.a = 0;
        _fadeImage.color = color;

        while (_fadeImage.color.a < 1)
        {
            float fadeAmount = color.a + (speed * Time.deltaTime);
            color = new Color(color.r, color.g, color.b, fadeAmount);
            _fadeImage.color = color;

            yield return null;
        }
    }

    public IEnumerator FadeToColor(float speed) => FadeToColor(_defaultFadeColor, speed);

    public IEnumerator FadeToColor() => FadeToColor(_defaultFadeColor, _defaultFadeSpeed);

    public IEnumerator FadeFromColor(float speed)
    {
        var color = _fadeImage.color;
        while (_fadeImage.color.a > 0)
        {
            float fadeAmount = color.a - (speed * Time.deltaTime);
            color = new Color(color.r, color.g, color.b, fadeAmount);
            _fadeImage.color = color;

            yield return null;
        }
        
    }

    public IEnumerator FadeFromColor() => FadeFromColor(_defaultFadeSpeed);
    
}
