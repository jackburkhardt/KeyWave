using System;
using System.Collections;
using System.Collections.Generic;
using Gilzoide.RoundedCorners;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;


public class AddressableInjector : MonoBehaviour
{
    public enum AddressableType
    {
        Sprite
    }
    
    public enum SpriteComponent
    {
        Image,
        RoundedImage
    }
    
    public string addressableName;
    
    private string _currentAddressableName;
    
    public AddressableType addressableType;
    
    [ShowIf ("addressableType", AddressableType.Sprite)]
    public Sprite sprite;
    [ShowIf ("addressableType", AddressableType.Sprite)]
    public SpriteComponent spriteComponent;
   
    [ShowIf("spriteComponent", SpriteComponent.Image)]
    public Image image;
    
    [ShowIf("spriteComponent", SpriteComponent.RoundedImage)]
    public RoundedImage roundedImage;


    
    private void OnValidate()
    {
        Inject();
    }
    
    private void Inject()
    {
        if (addressableName != _currentAddressableName)
        {
            _currentAddressableName = addressableName;
            sprite = null;
        }
        
        switch (addressableType)
        {
            case AddressableType.Sprite:
                
                if (sprite == null)
                {
                    Project.Runtime.Scripts.AssetLoading.AddressableLoader.RequestLoad<Sprite>(addressableName, (s) =>
                    {
                        sprite = s;
                        SetSprite();
                        
                    });
                }
                
                else  SetSprite();
                
                void SetSprite()
                {
                    switch (spriteComponent)
                    {
                        case SpriteComponent.Image:
                            image ??= GetComponent<Image>();
                        
                            if (image != null) image.sprite = sprite;
                        
                            break;
                        case SpriteComponent.RoundedImage:
                            roundedImage ??= GetComponent<RoundedImage>();
                        
                            if (roundedImage != null) roundedImage.Sprite = sprite;
                        
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
               
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

   
   
}
