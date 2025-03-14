using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Editor.Scripts.Attributes.DrawerAttributes;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.UI;

public class PointsFishBowl : MonoBehaviour
{
    [ShowIf("gameManagerIsNull")]
    [SerializeField] private DialogueDatabase dialogueDatabase;
    private bool gameManagerIsNull => GameManager.instance == null;
    
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

    [Range(0, 1)] [SerializeField] private float fillAmount;

    [SerializeField] private float timeToFill = 1f;
    

    [SerializeField] Animator animator;
    [SerializeField] private string animationTrigger = "Fill";

    public Graphic ring;
    public Image icon;
    [ShowIf("useFill")]
    public Image inverseIcon;

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
        get => fillAmount;
        set
        {
            fillAmount = value;
            if (fill) fill.fillAmount = value;
            if (inverseFill) inverseFill.fillAmount = value;
        } 
    }

    private void OnAwake()
    {
         SetPoints( type, 1);
    }

    
    private void OnValidate()
    {
        if (!Application.isPlaying) SetFishBowl();
    }

    public void SetPointType(Item item)
    {
        type = item.Name;
        SetFishBowl();
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

    private void SetFishBowl()
    {
        if (pointItem == null) return;
        
        if (icon)
        { 
           
            icon.sprite = Sprite.Create( pointItem.icon, new Rect(0, 0, pointItem.icon.width, pointItem.icon.height), Vector2.zero);
        }

        if (useFill)
        {
            Fill = fillAmount;
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
        SetFishBowl();
        Points.OnPointsChange += SetPoints;
    }

    private void OnDisable()
    {
        Points.OnPointsChange -= SetPoints;
    }

    private void SetPoints(string pointType, int newScore)
    {
        if (pointType != type)  return;
        animator.SetTrigger(animationTrigger);
        
        if (Points.MaxScore(pointType) == 0) return;
        
        isPointsDecreasing = newScore < 0;
        
        
        DOTween.To(() => Fill, x => Fill = x, (float) newScore / Points.MaxScore(pointType), timeToFill).SetEase(Ease.InOutSine);
            
        DOTween.To(() => Shine, x => Shine = x, 1, timeToFill / 2).SetEase(Ease.InOutSine).OnComplete( () =>
        DOTween.To( () => Shine, x => Shine = x, 0,  timeToFill / 2).SetEase(Ease.InOutSine));
        
     
        
    }


}
