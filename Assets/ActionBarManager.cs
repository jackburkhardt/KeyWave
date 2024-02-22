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
    
    public enum OrbType
    {
        Wellness,
        LocalKnowledge,
        BusinessResearch
    }

    [SerializeField] private RectTransform wellnessBar;
    [SerializeField] private RectTransform localKnowledgeBar;
    [SerializeField] private RectTransform businessResearchBar;

    private float maxBarWidth;

    public RectTransform orbTemplate;
    
    
    // Start is called before the first frame update
    void Start()
    {
        maxBarWidth = GetComponent<RectTransform>().rect.width;
        orbTemplate.gameObject.SetActive(false);
    }
    
    public void UpdateWellnessScore(int score)
    {
        wellnessScore += score;
        UpdateBar();
    }
    
    public void UpdateLocalKnowledgeScore(int score)
    {
        localKnowledgeScore += score;
        UpdateBar();
    }
    
    public void UpdateBusinessResearchScore(int score)
    {
        businessResearchScore += score;
        UpdateBar();
    }
    
    private void UpdateBar()
    {
        wellnessBar.sizeDelta = new Vector2(maxBarWidth * wellnessScore / maxScore, wellnessBar.rect.height);
        localKnowledgeBar.sizeDelta = new Vector2(maxBarWidth * localKnowledgeScore / maxScore, localKnowledgeBar.rect.height);
        businessResearchBar.sizeDelta = new Vector2(maxBarWidth * businessResearchScore / maxScore, businessResearchBar.rect.height);
    }

    private void Update()
    {
    }
    
    public void AddWellnessPoints(int points)
    {
        StartCoroutine(SpawnOrbHandler(points, OrbType.Wellness));
    }
    
    public void AddLocalKnowledgePoints(int points)
    {
        StartCoroutine(SpawnOrbHandler(points, OrbType.LocalKnowledge));
    }
    
    public void AddBusinessResearchPoints(int points)
    {
        StartCoroutine(SpawnOrbHandler(points, OrbType.BusinessResearch));
    }

    IEnumerator SpawnOrbHandler(int count, OrbType type)
    {
        for (int i = 0; i < count; i++)
        {
            var orb = Instantiate(orbTemplate, transform.parent);
            orb.gameObject.SetActive(true);
            orb.GetComponent<PointOrb>().SetOrbProperties(type);
            
            yield return new WaitForSeconds(0.02f);
        }
    }
   
}
