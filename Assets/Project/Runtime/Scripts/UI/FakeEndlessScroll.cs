using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class FakeEndlessScroll : MonoBehaviour
{
    [ValidateInput("ContentIsNotThisTransform", "Content cannot be this GameObject")]
    public Transform content;
    
    public Transform anchorOne;
    public Transform anchorTwo;
    
    public bool flipDirection;
    
    private bool ContentIsNotThisTransform => content != this.transform;
    
    public float scrollSpeed = 1f;

    private void Update()
    {
        if (content == null && ContentIsNotThisTransform || anchorOne == null || anchorTwo == null) return;
        
        var distance = Vector3.Distance(anchorOne.position, anchorTwo.position);
        var direction = (anchorTwo.position - anchorOne.position).normalized * (flipDirection ? 1 : -1);
        
        
        content.position += direction * scrollSpeed * Time.deltaTime * distance;
        
        if (Vector3.Distance(content.position, transform.position) > distance)
        {
            content.position = transform.position;
        }
    }

   
}
