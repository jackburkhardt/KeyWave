using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraPan : MonoBehaviour
{
    public static CameraPan Instance = null;
    [SerializeField] private Vector3 _defaultCameraPosition; // 10.8
    [SerializeField] private float _defaultPanLerp = 1;
    [SerializeField] private float _defaultPanAcceleration = 0.5f;


    // TODO: adjust if need more cameras
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        _defaultCameraPosition = Camera.main.transform.position;
    }

    //  public IEnumerator FadeToColor(float speed) => FadeToColor(_defaultFadeColor, speed);

    public void SetCameraPosition() => SetCameraPosition(_defaultCameraPosition);

    public void SetCameraPosition(Vector2 pos)
    {
        Camera.main.transform.position = new Vector2(pos.x, pos.y);
    }

    public void SetRelativeCameraPosition(Vector2 pos)
    {
        Camera.main.transform.position = new Vector2(Camera.main.transform.position.x + pos.x, Camera.main.transform.position.y + pos.y);
    }

    public IEnumerator PanToTarget() => PanToTarget(_defaultCameraPosition, _defaultPanAcceleration, _defaultPanLerp);

    public IEnumerator PanToTarget(Vector2 target) => PanToTarget(target, _defaultPanAcceleration, _defaultPanLerp);


    public IEnumerator PanToTarget(Vector2 target, float acceleration, float lerp)
    {
        float progress = 0;
        float progressSpeed = 0;
        Vector3 initialPos = Camera.main.transform.position;
        Vector3 newTarget = Vector3.Lerp(initialPos, new Vector3(target.x, target.y, -10f), _defaultPanLerp);


        while (progress <= 1)
        {
            progress = CalculateProgress(Camera.main.transform.position.x, initialPos.x, newTarget.x);
            progressSpeed -= (acceleration * Time.deltaTime * Math.Sign(progress - 0.5));
            progress += Math.Abs(progressSpeed) * Time.deltaTime;
            Camera.main.transform.position = Vector3.Lerp(initialPos, newTarget, progress);

            yield return null;
        }
    }

 
   

    float CalculateProgress(float currentValue, float initialValue, float targetValue)
    {
        return Math.Abs((currentValue - initialValue) / (initialValue - targetValue));
    }
}
