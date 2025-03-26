using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Editor.Scripts.Attributes.DrawerAttributes;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

public class PointsFishBowl : MonoBehaviour
{
    [ShowIf("gameManagerIsNull")]
    [SerializeField] private DialogueDatabase dialogueDatabase;
    private bool gameManagerIsNull => GameManager.instance == null;
    
    public CanvasGroup canvasGroup;

    public bool isAnimated = true;

    
    private DialogueDatabase Database
    {
        get
        {
            if (gameManagerIsNull) return dialogueDatabase;
            return GameManager.settings.dialogueDatabase;
        }
    }

    [PointsPopup]
    [SerializeField] private string type;
    
    public UITextField scoreText;
    
    [Label("Use Score as Fill")]
    public bool useFill;

    private List<string> pointTypes
    {
        get
        {
            var types = Points.GetAllPointsTypes().Select( p => p.Name).ToList();
            types.Insert(0, "(All)");
            return types;
        }
    }

    [SerializeField] private float timeToFill = 1f;
    

    public Graphic ring;
    public Image icon;
    [ShowIf("useFill")]
    public Image inverseIcon;

    [ShowIf("useFill")]
    public Image fill;
    [ShowIf("useFill")]
    public Image inverseFill;
   
   
    
    private Item pointItem => Points.GetDatabaseItem( type, Database);
    
    
    [Range(0, 1)] [SerializeField] private float shine;

    private Color RingColor
    {
        get
        {
            if (DialogueManager.instance != null && pointItem != null)
            {
                return pointItem.LookupColor("Ring Color");
            }
            
            return Color.white;
            
        }
        
    }
    private Color IconColor
    {
        get
        {
            if (DialogueManager.instance != null && pointItem != null)
            {
                return pointItem.LookupColor("Color");
            }
            
            return Color.white;
            
        }
    }

    private Color InverseIconColor
    {
        get
        {
            if (DialogueManager.instance != null && pointItem != null)
            {
                return pointItem.LookupColor("Inverse Color");
            }
            
            return Color.white;
        }
    }

    public float Shine
    {
        get => shine;
        set
        {
            shine = value;
            SetColors(shine);
        }
    }

    private float Fill
    {
        get
        {
            if (fill) return fill.fillAmount;
            else
            {
                return 0;
            }
        }
        set
        {
            if (fill) fill.fillAmount = value;
            if (inverseFill) inverseFill.fillAmount = value;
        } 
    }

    
    private void OnValidate()
    {
        if (!Application.isPlaying) SetFishBowlProperties();
        canvasGroup ??= GetComponent<CanvasGroup>();
    }

    public void SetPointType(Item item)
    {
        type = item.Name;
        SetFishBowlProperties();
    }
    
    public Item GetPointType()
    {
        return pointItem;
    }

    private void SetColors()
    {
        if (ring) ring.color = RingColor;
        if (icon) icon.color = IconColor;
        if (inverseIcon) inverseIcon.color = IconColor;
        if (fill) fill.color = IconColor;
        if (inverseFill) inverseFill.color = InverseIconColor;
    }

    private bool isPointsDecreasing = false;

    private void SetColors(float shine, float amount = 1)
    {
     
        var blendColor = !isPointsDecreasing ? IconColor : Color.red;
        var backgroundColor = !isPointsDecreasing ? IconColor : Color.red;

        if (ring) ring.color = ColorBlend(RingColor, blendColor, "Multiply", shine / 2);
        if (icon) icon.color = ColorBlend(IconColor, blendColor, "Multiply", shine / 2);
        if (inverseIcon) inverseIcon.color = ColorBlend(IconColor, blendColor, "Multiply", shine/ 2);
        if (fill) fill.color = ColorBlend(IconColor, blendColor, "Multiply", shine / 2);
        if (inverseFill) inverseFill.color = ColorBlend(InverseIconColor, blendColor, "Multiply", shine / 2);
        
        if (TryGetComponent<Graphic>(out  var graphic)) graphic.color = Color.Lerp( new Color(0, 0, 0, 1f/255f), backgroundColor, shine);

    }

    public static Color ColorBlend(Color baseColor, Color blendColor, string blendMode, float lerp = 1f)
    {
        float BlendChannel(float baseChannel, float blendChannel)
        {
            switch (blendMode)
            {
                case "Overlay":
                    return baseChannel < 0.5f 
                        ? 2 * baseChannel * blendChannel 
                        : 1 - 2 * (1 - baseChannel) * (1 - blendChannel);
                case "SoftLight":
                    return (1 - 2 * blendChannel) * baseChannel * baseChannel + (2 * blendChannel * baseChannel);
                case "Multiply":
                    return baseChannel * blendChannel;
                default:
                    return 0;
            }
        }
            
        float r = BlendChannel(baseColor.r, blendColor.r);
        float g = BlendChannel(baseColor.g, blendColor.g);
        float b = BlendChannel(baseColor.b, blendColor.b);
        float a = Mathf.Lerp(baseColor.a, blendColor.a, 0.5f); // Optional alpha blending

        return Color.Lerp( baseColor, new Color(r, g, b, a), lerp);
    }

    private void SetFishBowlProperties()
    {
        if (pointItem == null) return;
        
        if (icon)
        { 
           
            icon.sprite = Sprite.Create( pointItem.icon, new Rect(0, 0, pointItem.icon.width, pointItem.icon.height), Vector2.zero);
        }

        if (useFill)
        {
            if (inverseIcon)
            {
                inverseIcon.sprite = Sprite.Create( pointItem.icon, new Rect(0, 0,  pointItem.icon.width,  pointItem.icon.height), Vector2.zero);
            }
        }
        
        SetColors(shine);


        var panel = GetComponentInChildren<PointsPanel>();
        if (panel) panel.SetPanel(pointItem);
        
        if (scoreText.gameObject != null) scoreText.text = Points.Score(type, Database).ToString();

    }

    private void OnEnable()
    {
        SetFishBowlProperties();
        Points.OnPointsChange += OnPointsChange;
        TravelUIResponseButton.OnLocationSelected += OnLocationSelected;
        LocationPanel.onLocationPanelClose += OnLocationPanelClose;
       SetPointsFillImmediate();
        
    }

    private void OnDisable()
    {
        DOTween.Kill( this);
        Points.OnPointsChange -= OnPointsChange;
        TravelUIResponseButton.OnLocationSelected -= OnLocationSelected;
        LocationPanel.onLocationPanelClose -= OnLocationPanelClose;
        
    }

    private void OnPointsChange(string pointType, int newScore, Points.PointsChangeExpression expression)
    {
        if (!isAnimated) return;
        if (pointType != type)  return;
        if (Points.MaxScore(pointType) == 0) return;

        if (expression == Points.PointsChangeExpression.Explicit)
        {
            
            AnimatePointsFill(newScore);
        }

        else
        {
            SetPointsFillImmediate();
        }
        
    }
    
    private void AnimatePointsFill( int newScore)
    {
        
        isPointsDecreasing = newScore < 0;

        if (newScore == 0) return;
        
        DOTween.Kill( this);
        
        var maxScore = Points.MaxScore(type);
        
        if (maxScore == 0) return;
        
        var fillAmount = (float) newScore / maxScore;
        
        DOTween.To(() => Fill, x => Fill = x, fillAmount, timeToFill).SetEase(Ease.InOutSine);
            
        DOTween.To(() => Shine, x => Shine = x, 1, timeToFill / 2).SetEase(Ease.InOutSine).OnComplete( () =>
            DOTween.To( () => Shine, x => Shine = x, 0,  timeToFill / 2).SetEase(Ease.InOutSine));
    }
    
    private void SetPointsFillImmediate()
    {
        
        DOTween.Kill( this);
        
        var score = Points.Score(type, Database);
        var maxScore = Points.MaxScore(type);
        
        if (score < 0) return;
        if (maxScore <= 0) return;
        
        var scoreToFill = (float) score / maxScore;
        
        DOTween.To( () => Fill, x => Fill = x, scoreToFill, 0.35f).SetEase(Ease.InOutSine);
    }
    
    private void OnLocationSelected(Location location)
    {
        if (!isAnimated) return;
        if (location.LookupInt($"{type} Affinity") > 0)
        {
            DOTween.To(() => Shine, x => Shine = x, 0.75f, timeToFill / 2).SetEase(Ease.InOutSine);
            if (canvasGroup != null) DOTween.To( () => canvasGroup.alpha, x => canvasGroup.alpha = x, 1, timeToFill / 2).SetEase(Ease.InOutSine);
        }
        else
        {
            DOTween.To(() => Shine, x => Shine = x, 0, timeToFill / 2).SetEase(Ease.InOutSine);
            if (canvasGroup != null) DOTween.To( () => canvasGroup.alpha, x => canvasGroup.alpha = x, 0.25f, timeToFill / 2).SetEase(Ease.InOutSine);
        }
    }
    
    private void OnLocationPanelClose()
    {
        if (!isAnimated) return;
        if (canvasGroup != null) DOTween.To( () => canvasGroup.alpha, x => canvasGroup.alpha = x, 1, timeToFill / 2).SetEase(Ease.InOutSine);
        DOTween.To(() => Shine, x => Shine = x, 0, timeToFill / 2).SetEase(Ease.InOutSine);
    }

    public void OnTimeChange()
    {
        if (useFill)
        {
            DOTween.To(() => Shine, x => Shine = x, 0, timeToFill / 2).SetEase(Ease.InOutSine);
        }
    }

    
    


}
