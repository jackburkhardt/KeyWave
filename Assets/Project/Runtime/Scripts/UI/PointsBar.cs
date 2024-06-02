using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PointsBar : MonoBehaviour
{
   
   
    
    [SerializeField] private HorizontalOrVerticalLayoutGroup layoutGroup;
    [SerializeField] private RectTransform pointsBarUnitTemplate;

    private List<RectTransform> PointsBarUnits => layoutGroup.GetComponentsInChildren<RectTransform>().Where(p => p.transform != layoutGroup.transform && p.gameObject.activeSelf).ToList();
    private float UnitMinWidth =>  layoutGroup.GetComponent<RectTransform>().rect.width / Points.MaxScore;
    private float LayoutGroupWidth => layoutGroup.GetComponent<RectTransform>().rect.width;

    private float TotalUnitWidth
    {
        get
        {
            var currentWidth = 0f;
            foreach (var barUnit in PointsBarUnits)
            {
                currentWidth += barUnit.rect.width;

            }

            return currentWidth;
        }
    }


    public LinkedList<(Points.Type type, int amt)> pointQueue = new();

    public void OnParticleDeath(ParticleSystem.Particle particle)
    {
        AddOrExpandUnit(particle.startColor);
        
        void AddOrExpandUnit(Color color)
        {
            var unit = PointsBarUnits.FirstOrDefault(p => p.GetComponent<Image>().color == color);
        
            if (unit == null)
            {
                unit = Instantiate(pointsBarUnitTemplate, layoutGroup.transform);
                unit.gameObject.SetActive(true);
                unit.gameObject.name = color.ToString();
                unit.GetComponent<Image>().color = color;
                unit.GetComponent<RectTransform>().sizeDelta = new Vector2(0, unit.GetComponent<RectTransform>().rect.height);
            }
            
            var unitRectTransform = unit.GetComponent<RectTransform>();
            var rect = unitRectTransform.rect;
            unitRectTransform.sizeDelta = new Vector2(rect.width + UnitMinWidth,rect.height);

            if (TotalUnitWidth > LayoutGroupWidth)
            {
                foreach (var barUnit in PointsBarUnits)
                {
                    barUnit.sizeDelta = new Vector2(barUnit.rect.width * (LayoutGroupWidth) / (TotalUnitWidth), barUnit.rect.height);
                }
            }
        }
    }

    

    private void OnEnable()
    {
       
        
        GameEvent.OnPlayerEvent += OnPlayerEvent;
    }
    
    private void OnPlayerEvent(PlayerEvent playerEvent)
    {
        if (playerEvent.EventType == "points")
        {
            Debug.Log("got point event");
            Points.PointsField pointsInfo = (Points.PointsField)playerEvent.Data;
            pointQueue.AddLast((pointsInfo.Type, pointsInfo.Points));
        }
    }

   

    

    // Start is called before the first frame update
    void Start()
    {
        pointsBarUnitTemplate.gameObject.SetActive(false);
       
        
    }




  
    
    /*

    public void OnParticleAttracted()
    {
        // reduce whatever is in the queue by 1 and then update the bar
        // if queue entry has no more points, remove it
        // if queue is empty, call AnimationCompleteHandler
        
        if (pointQueue.Count > 0)
        {
            var first = pointQueue.First.Value;
            if (first.amt > 0)
            {
                pointQueue.First.Value = (first.type, first.amt - 1);
                Debug.Log("points left: " + first.amt + " for " + first.type + " type");
                OnHit(first.type);
            }
            else
            {
                pointQueue.RemoveFirst();
            }
        }
        else
        {
            StartCoroutine(AnimationCompleteHandler());
        }
        
    }
    
    */
    
    

  
}
