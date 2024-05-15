using System;
using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class Portrait : MonoBehaviour
{
   private Image image;
   private Image currentImage => GetComponent<Image>();

   [SerializeField]
   private Animator animator;
   
   
   private void OnConversationLine()
   {
      if (currentImage.sprite == null)
      {
         if (image != null) currentImage.sprite = image.sprite;
      }
      
      else image = currentImage;
   }

   private void OnDisable()
   {
      animator.SetTrigger("HidePortrait");
   }

   private void OnEnable()
   {
      animator.SetTrigger("ShowPortrait");
   }
}
