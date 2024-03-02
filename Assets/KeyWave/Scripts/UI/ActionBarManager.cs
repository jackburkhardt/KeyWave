using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBarManager : MonoBehaviour
{
    private int wellnessScore = 0;

    private int localKnowledgeScore = 0;

    private int businessResearchScore = 0;

    private int maxScore = 1000;

    private int spawnedOrbCount;

    [SerializeField] private RectTransform wellnessBar;
    [SerializeField] private RectTransform localKnowledgeBar;
    [SerializeField] private RectTransform businessResearchBar;

    private float maxBarWidth;

    public RectTransform orbTemplate;

    private void OnEnable()
    {
        Points.OnPointsIncrease += AddPoints;
    }

    private void OnDisable()
    {
        Points.OnPointsIncrease -= AddPoints;
    }

    // Start is called before the first frame update
    void Start()
    {
        maxBarWidth = GetComponent<RectTransform>().rect.width;
        orbTemplate.gameObject.SetActive(false);
    }

    public void AddToBar(Points.Type type, int points)
    {
        switch (type)
        {
            case Points.Type.Wellness:
                wellnessScore += points;
                break;
            case Points.Type.LocalSavvy:
                localKnowledgeScore += points;
                break;
            case Points.Type.Business:
                businessResearchScore += points;
                break;
        }

        UpdateBar();

    }
    
    
    private void UpdateBar()
    {
        wellnessBar.sizeDelta = new Vector2(maxBarWidth * wellnessScore / maxScore, wellnessBar.rect.height);
        localKnowledgeBar.sizeDelta = new Vector2(maxBarWidth * localKnowledgeScore / maxScore, localKnowledgeBar.rect.height);
        businessResearchBar.sizeDelta = new Vector2(maxBarWidth * businessResearchScore / maxScore, businessResearchBar.rect.height);

        spawnedOrbCount--;
        
        if (spawnedOrbCount == 0)
        {

            StartCoroutine(AnimationCompleteHandler());
        }
    }

    IEnumerator AnimationCompleteHandler()
    {
        yield return new WaitForSeconds(1);
        Points.AnimationComplete();
    }

    private void Update()
    {
    }

    public void AddPoints(Points.Type type, int points)
    {
        StartCoroutine(SpawnOrbHandler(type, points));
    }
    
    public void AddWellnessPoints(int points)
    {
       AddPoints(Points.Type.Wellness, points);
    }
    
    public void AddLocalKnowledgePoints(int points)
    {
       AddPoints( Points.Type.LocalSavvy, points);
    }
    
    public void AddBusinessResearchPoints(int points)
    {
        AddPoints(Points.Type.Business, points);
    }

    IEnumerator SpawnOrbHandler(Points.Type type, int count)
    {
        
        for (int i = 0; i < count; i++)
        {
            spawnedOrbCount++;
            var orb = Instantiate(orbTemplate, transform.parent);
            orb.gameObject.SetActive(true);
            orb.GetComponent<TrackingOrb>().SetOrbProperties(type);
            
            yield return new WaitForSeconds(0.02f);
        }
    }
   
}
