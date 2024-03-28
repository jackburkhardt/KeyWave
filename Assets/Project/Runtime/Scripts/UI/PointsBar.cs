using System.Collections;
using UnityEngine;

public class PointsBar : PlayerEventHandler
{
    private int visualizedWellnessScore, visualizedLocalKnowledgeScore, visualizedBusinessResearchScore;
    

    private int maxScore = 1000;

    private int spawnedOrbCount;

    [SerializeField] private RectTransform wellnessBar;
    [SerializeField] private RectTransform localKnowledgeBar;
    [SerializeField] private RectTransform businessResearchBar;

    private float maxBarWidth;

    public RectTransform orbTemplate;


    private new void OnEnable()
    {
        base.OnEnable();
        visualizedWellnessScore = Points.Score(Points.Type.Wellness);
        visualizedLocalKnowledgeScore = Points.Score(Points.Type.Savvy);
        visualizedBusinessResearchScore = Points.Score(Points.Type.Business);
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

        UpdateBar(amount);
    }

    protected override void OnPlayerEvent(PlayerEvent playerEvent)
    {
        switch (playerEvent.Type)
        {
            case "points":
                var type = Points.TypeFromString(playerEvent.Receiver);
                SpawnOrbs(type, int.Parse(playerEvent.Value));
                break;
        }
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
    
    
    public void UpdateBar(int amount)
    {
        SetRectWidth(wellnessBar, maxBarWidth * visualizedWellnessScore / maxScore);
        SetRectWidth(localKnowledgeBar, maxBarWidth * visualizedLocalKnowledgeScore / maxScore);
        SetRectWidth(businessResearchBar, maxBarWidth * visualizedBusinessResearchScore / maxScore);

        spawnedOrbCount -= amount;
        
        if (spawnedOrbCount <= 0)
        {
            StartCoroutine(AnimationCompleteHandler());
        }
    }

    IEnumerator AnimationCompleteHandler()
    {
        yield return new WaitForSeconds(1);
        Points.AnimationComplete();
        OnEnable();
    }
    public void SpawnOrbs(Points.Type type, int points)
    {
        StartCoroutine(SpawnOrbHandler(type, points));
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
