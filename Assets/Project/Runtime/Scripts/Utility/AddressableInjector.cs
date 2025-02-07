using System;
using System.Collections.Generic;
using Gilzoide.RoundedCorners;
using NaughtyAttributes;
using Project.Runtime.Scripts.AssetLoading;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
    
    [Dropdown("addressableNames")]
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

    public List<string> addressableNames
    {
        get
        {
            var names = new List<string>();
            foreach (var resourceLocator in Addressables.ResourceLocators)
            {
              //  if (resourceLocator.LocatorId == "AddressableAssetSettings") continue;
              //  if (resourceLocator.LocatorId == "DynamicResourceLocator") continue;
                foreach (var key in resourceLocator.Keys)
                {
                    if (!key.ToString().Contains("/")) continue;
                    names.Add(key.ToString());
                }
            }

            return names;
        }
    }
    
    private void OnValidate()
    {
        image ??= GetComponent<Image>();
        roundedImage ??= GetComponent<RoundedImage>();
        
        
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
                    AddressableLoader.RequestLoad<Sprite>(addressableName, (s) =>
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
