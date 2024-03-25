using UnityEngine;
using UnityEngine.InputSystem;
using Image = UnityEngine.UI.Image;

public class PointOrb : MonoBehaviour
{
    [SerializeField] private TrackingOrb trackingOrbTemplate;

    private float elapsedTime, orbDiameter, targetRadius, angle, expirationTime;

    private int orbIndex;
    [SerializeField] private float distanceFromCenter, paddingBetweenOrbs, paddingBetweenLayers, defaultLifetime, timeBetweenOrbs, rotationSpeed;
 
    [SerializeField] private Color wellnessColor, localKnowledgeColor, businessResearchColor;
    [SerializeField] private Image image;
    
    private Points.Type _orbType;

    public void SetOrbProperties(Points.Type orbType)
    {
        _orbType = orbType;
        
        switch (orbType)
        {
            case Points.Type.Wellness:
                image.color = wellnessColor;
                break;
            case Points.Type.Savvy:
                image.color = localKnowledgeColor;
                break;
            case Points.Type.Business:
                image.color = businessResearchColor;
                break;
        }
    }
    
    
    // Start is called before the first frame update
    void Awake()
    {
        SetOrbIndexBasedOnPrevious();
        trackingOrbTemplate.gameObject.SetActive(false);
    }

    private void SetOrbIndexBasedOnPrevious()
    {
        var currentMaxIndex = 0;
        var orbCount = 0;
        
        foreach (PointOrb orb in transform.parent.GetComponentsInChildren<PointOrb>())
        {
            if (!orb.gameObject.activeSelf || orb == this) continue;
            orbCount++;
            if (orb.orbIndex > currentMaxIndex)
            {
                currentMaxIndex = orb.orbIndex;
                elapsedTime = orb.elapsedTime;
            }
        }

        orbIndex = orbCount == 0 ? 0 : currentMaxIndex + 1;
        
    }

 

    private void Start()
    {
        orbDiameter = GetComponent<RectTransform>().rect.width;
        targetRadius = (distanceFromCenter + orbDiameter) / 2;

        var maxOrbCountCumulative = MaxOrbCountInCurrentLayer;
        var maxOrbCountInPreviousLayer = 0;
        
        while (maxOrbCountCumulative < orbIndex + 1)
        {
            maxOrbCountInPreviousLayer = maxOrbCountCumulative;
            targetRadius += orbDiameter/2 + paddingBetweenLayers;
            maxOrbCountCumulative += MaxOrbCountInCurrentLayer;
        }
        
        var angleIndex = orbIndex - maxOrbCountInPreviousLayer;
        angle = GetOrbAngle(MaxOrbCountInCurrentLayer) * angleIndex;
        SetOrbPosition(angle, Mouse.current.position.ReadValue());
        expirationTime = defaultLifetime + orbIndex * timeBetweenOrbs;
    }
    
    private int MaxOrbCountInCurrentLayer => (int) (2 * Mathf.PI / Mathf.Asin((orbDiameter + paddingBetweenOrbs)/ (2 * targetRadius)));

    private float GetOrbAngle(int orbCount)
    {
        return 360f / orbCount;
    }

    private void SetOrbPosition(float angle, Vector3 center)
    {
        var x = center.x + targetRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
        var y = center.y + targetRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
        transform.position = new Vector3(x, y, 0);
    }
    

    // Update is called once per frame
    void LateUpdate()
    {
        elapsedTime += Time.deltaTime;
        angle += rotationSpeed * Time.deltaTime;
        angle %= 360f;
        SetOrbPosition(angle, Mouse.current.position.ReadValue());

       

        if (elapsedTime > expirationTime)
        {
            var obj = Instantiate(trackingOrbTemplate, transform.parent);
            obj.gameObject.SetActive(true);
            obj.GetComponent<TrackingOrb>().SetOrbProperties(_orbType);
            Destroy(gameObject);
        }
        
    }
}
