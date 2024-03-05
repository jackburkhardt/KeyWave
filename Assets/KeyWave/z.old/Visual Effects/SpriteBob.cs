/*

using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBob : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float _defaultY;
    [SerializeField] private float _defaultYDelta = 0.25f;
    [SerializeField] private float _defaultUpAcceleration = 1.5f;
    [SerializeField] private float _defaultDownAcceleration = 0.5f;
    private void Awake()
    {
        _defaultY = transform.position.y;
    }

    private void Start()
    {
        StartCoroutine(BobUpAndDown());
    }

    public IEnumerator BobUpAndDown()
    {
        while (GameManager.playerActor.location = transform.parent) {
        yield return BobToTarget(_defaultY + _defaultYDelta, _defaultUpAcceleration);
        yield return BobToTarget(_defaultY, _defaultDownAcceleration);
        }
    }

    public IEnumerator BobToTarget(float target, float acceleration)
    {
        float progress = 0;
        float speed = 0;
        float initial = transform.position.y;

        while (progress <= 1)
        {
            progress = CalculateProgress(transform.position.y, initial, target);
            transform.position += Vector3.up * (speed * Time.deltaTime) * Math.Sign(target - initial);
            speed -= (acceleration * Time.deltaTime * Math.Sign(progress - 0.5));
            yield return null;
        }

    }


    float CalculateProgress(float currentValue, float initialValue, float targetValue)
    {
        return Math.Abs((currentValue - initialValue) / (initialValue - targetValue));
    }
}

*/
