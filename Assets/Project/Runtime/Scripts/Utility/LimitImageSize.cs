using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(ContentSizeFitter))]
[RequireComponent(typeof(LayoutElement))]
public class LimitImageSize : MonoBehaviour
{
    public float maxWidth = 300;
    public float maxHeight = 300;
    public float minWidth = 100;
    public float minHeight = 100;
    
    private void OnValidate()
    {
       var image = GetComponent<Image>();
       var rectTransform = GetComponent<RectTransform>();
       var layoutElement = GetComponent<LayoutElement>();
       var width = image.sprite.rect.width;
       var height = image.sprite.rect.height;
       var aspectRatio = width / height;
       

       var rect = rectTransform.rect;
      
       if (rect.width > maxWidth)
       {
           layoutElement.preferredWidth = maxWidth;
           layoutElement.preferredHeight = maxWidth/aspectRatio;
       }
       
       else if (rect.height > maxHeight)
       {
              layoutElement.preferredHeight = maxHeight;
              layoutElement.preferredWidth = maxHeight * aspectRatio;
       }
       
       else if (rect.width < minWidth)
       {
           layoutElement.preferredWidth = minWidth;
           layoutElement.preferredHeight = minWidth/aspectRatio;
       }
       
       else if (rect.height < minHeight)
       {
           layoutElement.preferredHeight = minHeight;
           layoutElement.preferredWidth = minHeight * aspectRatio;
       }
    }

    [Button("Set to Max Size")]
    public void MaxSize()
    {
        var image = GetComponent<Image>();
        var rectTransform = GetComponent<RectTransform>();
      
        var layoutElement = GetComponent<LayoutElement>();
        var width = image.sprite.rect.width;
        var height = image.sprite.rect.height;
        var aspectRatio = width / height;
        
        if (maxWidth < maxHeight)
        {
            layoutElement.preferredWidth = maxWidth;
            layoutElement.preferredHeight = maxWidth/aspectRatio;
        }
       
        else
        {
            layoutElement.preferredHeight = maxHeight;
            layoutElement.preferredWidth = maxHeight * aspectRatio;
        }
    }
    
    [Button("Set to Min Size")]
    public void MinSize()
    {
        var image = GetComponent<Image>();
        var rectTransform = GetComponent<RectTransform>();
        
        var layoutElement = GetComponent<LayoutElement>();
        var width = image.sprite.rect.width;
        var height = image.sprite.rect.height;
        var aspectRatio = width / height;
        
        if (minWidth < minHeight)
        {
            layoutElement.preferredWidth = minWidth;
            layoutElement.preferredHeight = minWidth/aspectRatio;
        }
       
        else
        {
            layoutElement.preferredHeight = minHeight;
            layoutElement.preferredWidth = minHeight * aspectRatio;
        }
    }
}
