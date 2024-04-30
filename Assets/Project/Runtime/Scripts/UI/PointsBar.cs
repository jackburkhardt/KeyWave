using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsBar : MonoBehaviour
{
    private int visualizedWellnessScore, visualizedLocalKnowledgeScore, visualizedBusinessResearchScore;
    

    private int maxScore = 1000;

    [SerializeField] private RectTransform wellnessBar;
    [SerializeField] private RectTransform localKnowledgeBar;
    [SerializeField] private RectTransform businessResearchBar;

    private float maxBarWidth;

    public RectTransform orbTemplate;

    public LinkedList<(Points.Type type, int amt)> pointQueue = new();

    private void OnEnable()
    {
        visualizedWellnessScore = Points.Score(Points.Type.Wellness);
        visualizedLocalKnowledgeScore = Points.Score(Points.Type.Savvy);
        visualizedBusinessResearchScore = Points.Score(Points.Type.Business);
        
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

    public void OnHit(Points.Type type, int amount = 1)
    {
        switch (type)
                {
                    case Points.Type.Wellness:
                        visualizedWellnessScore += amount;
                        break;
                    case Points.Type.Savvy:
                        visualizedLocalKnowledgeScore += amount;
                        break;
                    case Points.Type.Business:
                        visualizedBusinessResearchScore += amount;
                        break;
                }

        UpdateBar();
    }
    

    // Start is called before the first frame update
    void Start()
    {
        maxBarWidth = GetComponent<RectTransform>().rect.width;
        orbTemplate.gameObject.SetActive(false);
    }


    private void SetRectWidth(RectTransform rectTransform, float width)
    {
        rectTransform.sizeDelta = new Vector2(width, rectTransform.rect.height);
    }

    private void ClearBar()
    {
        SetRectWidth(wellnessBar, 0);
        SetRectWidth(localKnowledgeBar, 0);
        SetRectWidth(businessResearchBar, 0);
    }

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
    
    
    private void UpdateBar()
    {
        SetRectWidth(wellnessBar, maxBarWidth * visualizedWellnessScore / maxScore);
        SetRectWidth(localKnowledgeBar, maxBarWidth * visualizedLocalKnowledgeScore / maxScore);
        SetRectWidth(businessResearchBar, maxBarWidth * visualizedBusinessResearchScore / maxScore);
    }

    IEnumerator AnimationCompleteHandler()
    {
        yield return new WaitForSeconds(1);
        Points.AnimationComplete();
        OnEnable();
    }
}
