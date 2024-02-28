using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles fading the camera in and out using a black image.
/// </summary>
public class CameraZoom : MonoBehaviour
{
    public static CameraZoom Instance = null;
    [SerializeField] private float _defaultCameraSize; // 10.8
    [SerializeField] private float _defaultZoomAmount = 1;
    [SerializeField] private float _defaultZoomAcceleration = 1.1f;


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

        _defaultCameraSize = Camera.main.orthographicSize;
    }

    //  public IEnumerator FadeToColor(float speed) => FadeToColor(_defaultFadeColor, speed);

    public void SetCameraSize() => SetCameraSize(_defaultCameraSize);

    public void SetCameraSize (float size)
    {
        Camera.main.orthographicSize = size;
    }

    public void SetRelativeCameraSize(float size)
    {
        Camera.main.orthographicSize = _defaultCameraSize + size;
    }


    public IEnumerator ZoomToTarget(float target, float acceleration)
    {
        float progress = 0;
        float speed = 0;
        float initial = Camera.main.orthographicSize;

        while (progress <= 1)
        {
            progress = CalculateProgress(Camera.main.orthographicSize, initial, target);
            Camera.main.orthographicSize += (speed * Time.deltaTime) * Math.Sign(target - initial);
            speed -= (acceleration * Time.deltaTime * Math.Sign(progress - 0.5));
            yield return null;
        }
    }

    public IEnumerator ZoomIn() => ZoomIn(_defaultZoomAmount, _defaultZoomAcceleration);

    public IEnumerator ZoomIn(float zoomAmount, float zoomAcceleration)
    {
        yield return ZoomToTarget(Camera.main.orthographicSize - zoomAmount, zoomAcceleration);
    }


    public IEnumerator ZoomOut() => ZoomOut(_defaultZoomAmount, _defaultZoomAcceleration);
    public IEnumerator ZoomOut(float zoomAmount, float zoomAcceleration)
    {
        yield return ZoomToTarget(Camera.main.orthographicSize + zoomAmount, zoomAcceleration);
    }

    float CalculateProgress(float currentValue, float initialValue, float targetValue)
    {
        return Math.Abs((currentValue - initialValue) / (initialValue - targetValue));
    }

}
