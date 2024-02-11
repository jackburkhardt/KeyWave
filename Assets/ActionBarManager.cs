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

    [SerializeField] private RectTransform wellnessBar;
    [SerializeField] private RectTransform localKnowledgeBar;
    [SerializeField] private RectTransform businessResearchBar;

    private float maxBarWidth;
    
    
    // Start is called before the first frame update
    void Start()
    {
        maxBarWidth = GetComponent<RectTransform>().rect.width;
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
        if (Input.GetKeyDown(KeyCode.U))
        {
            UpdateWellnessScore(30);
        }
        
        if (Input.GetKeyDown(KeyCode.I))
        {
            UpdateLocalKnowledgeScore(30);
            // local savvy
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            UpdateBusinessResearchScore(30);
            // a.k.a. preparedness
        }
    }
}
