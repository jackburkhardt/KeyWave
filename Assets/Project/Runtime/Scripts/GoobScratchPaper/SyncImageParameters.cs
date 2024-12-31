using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[ExecuteAlways]
public class SyncImageParameters : MonoBehaviour
{
    
    
    private bool isImage => imageToSyncWith != null;
    private bool isGraphic => graphicToSyncWith != null;
    
    [HideIf("isGraphic")]
    public Image imageToSyncWith;
    [HideIf("isImage")]
    public Graphic graphicToSyncWith;
    
    [HideIf("isGraphic")]
    public bool syncFillAmount = true;
    public bool syncColor = true;
    public bool syncAlpha = true;
    [HideIf("isGraphic")]
    public bool syncIcon = false;

    private Image _image;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

  
    // Update is called once per frame
    void Update()
    {
        _image ??= GetComponent<Image>();
        
        if (!isImage && !isGraphic) return;
        
        if (syncFillAmount)
        {
            _image.fillAmount = isImage ? imageToSyncWith.fillAmount : _image.fillAmount;
        }

        if (syncColor)
        {
            var color = isImage ? imageToSyncWith.color : graphicToSyncWith.color;
            _image.color = new Color(color.r, color.g, color.b, _image.color.a);
        }
        
        if (syncAlpha)
        {
            var alpha = isImage ? imageToSyncWith.color.a : graphicToSyncWith.color.a;
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, alpha);
        }
        
        if (syncIcon)
        {
            _image.sprite = isImage ? imageToSyncWith.sprite : _image.sprite;
        }
        
    }
}
