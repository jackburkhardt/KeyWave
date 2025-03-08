using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreenUIResponseButton : StandardUIResponseButton
{
    public Image appIcon;
    public Graphic appColor;
    public float timeBetweenButtonShow = 0.1f;
    public Animator animator;
    public string showTrigger = "Show";
    private string _app;
    private bool _fakeResponseButton = false;
    public override Response response
    {
        get { return base.response; }
        set
        {
            base.response = value;
            var appField = response.destinationEntry.fields.Find(p => p.title == "App");
            if (appField != null)
            {
                var appID = int.Parse(appField.value);
                var app = DialogueManager.masterDatabase.GetItem(appID);
                if (app != null)
                {
                    appIcon.sprite = Sprite.Create( app.icon, new Rect(0, 0, app.icon.width, app.icon.height), new Vector2(0.5f, 0.5f));
                    appColor.color = app.LookupColor("Color");
                    _fakeResponseButton = !app.LookupBool("Force Response Menu");
                    _app = app.Name;
                }
                
            }
           
            
        }
    }

    public void OnEnable()
    {
        DOTween.Sequence().AppendInterval(transform.GetSiblingIndex() * timeBetweenButtonShow ).AppendCallback(() =>
        {
            animator.SetTrigger(showTrigger);
        });
    }

    public override void OnClick()
    {
        var smartWatchPanel = FindObjectOfType<SmartWatchPanel>();
        if (smartWatchPanel != null)
        {
            smartWatchPanel.OpenApp(_app);
        }
        
        if (!_fakeResponseButton)
        {
            base.OnClick();
        }
    }

}
