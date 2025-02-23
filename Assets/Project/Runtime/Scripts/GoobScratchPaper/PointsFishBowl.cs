using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using UnityEngine;
using UnityEngine.UI;

public class PointsFishBowl : MonoBehaviour
{

    [NaughtyAttributes.Dropdown("pointTypes")]
    [SerializeField] private string type;
 
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
    
    private Item pointItem => Points.GetDatabaseItem( type);


    private float Fill
    {
        get => fillAmount;
        set
        {
            
            if (fill) fill.fillAmount = value;
            if (inverseFill) inverseFill.fillAmount = value;
        } 
    }

    
    private void OnValidate()
    {
        SetFishBowl();
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

    private void SetFishBowl()
    {
        if (ring) ring.color = pointItem.LookupColor("Ring Color");
        if (icon)
        {
            icon.color = pointItem.LookupColor("Color");
            icon.sprite = Sprite.Create( pointItem.icon, new Rect(0, 0, pointItem.icon.width, pointItem.icon.height), Vector2.zero);
        }

        if (useFill)
        {
            Fill = fillAmount;
            if (inverseIcon)
            {
                inverseIcon.color = pointItem.LookupColor("Color");
                inverseIcon.sprite = Sprite.Create( pointItem.icon, new Rect(0, 0,  pointItem.icon.width,  pointItem.icon.height), Vector2.zero);
            }
        
            if (fill) fill.color = pointItem.LookupColor("Color");
            if (inverseFill) inverseFill.color = pointItem.LookupColor("Inverse Color");
        }


        var panel = GetComponentInChildren<PointsPanel>();
        if (panel) panel.SetPanel(pointItem);

    }

    private void OnEnable()
    {
        Points.OnPointsChange += SetPoints;
    }

    private void OnDisable()
    {
        Points.OnPointsChange -= SetPoints;
    }

    private void SetPoints(string pointType, int amount)
    {
        if (pointType != type)
        {
            GetComponent<Selectable>().interactable = false;
            return;
        }
        
        animator.SetTrigger(animationTrigger);
        if (Points.MaxScore(pointType) != 0) 
            DOTween.To(() => Fill, x => Fill = x, Points.Score(pointType) / (float) Points.MaxScore(pointType), timeToFill);
    }


}
