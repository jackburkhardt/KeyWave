using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteMouseOver : MonoBehaviour
{
    [SerializeField] private Color _defaultColor;
    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultColor = spriteRenderer.color;
    }

    void Start()
    {
        if (GetComponent<BoxCollider2D>() == null) gameObject.AddComponent<BoxCollider2D>();
    }

    

    private void OnMouseOver()
    {
        spriteRenderer.color = Color.white;
    }

    private void OnMouseExit()
    {
        spriteRenderer.color = _defaultColor;
    }

    private void OnMouseDown()
    {
        if (!transform.GetChild(0).gameObject.active) {
            StartCoroutine(CameraPan.Instance.PanToTarget(transform.position));
            StartCoroutine(CameraZoom.Instance.ZoomIn(1f, 3f));

        }

        else
        {
            StartCoroutine(CameraPan.Instance.PanToTarget());
            StartCoroutine(CameraZoom.Instance.ZoomOut());
        }

        transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.active);

    }

}
