using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TrackingOrb : MonoBehaviour
{
    [SerializeField] private PointsBar pointsBar;
    [SerializeField] private Image image;
    
    [SerializeField] private Color wellnessColor, localKnowledgeColor, businessResearchColor;

    private Points.Type _orbType;
    
    public void SetOrbProperties(Points.Type orbType)
    {
        _orbType = orbType;
        image.color = Points.Color(orbType);

    }
    
    // Start is called before the first frame update
    void Start()
    {
        transform.position = Points.SpawnPosition;

        StartCoroutine(AnimateOrb());
    }
    
    
    IEnumerator AnimateOrb()
    {
        var firstAnimTarget = Vector3.Lerp(transform.position, pointsBar.transform.position, 0.15f);
        
        
        
        
        LeanTween.move(gameObject, firstAnimTarget, 0.75f).setEaseOutCirc();

        yield return new WaitForSeconds(0.75f);
        
        LeanTween.move(gameObject, pointsBar.transform.position, 1.5f).setEaseInCubic().setOnComplete(() =>
        {
            pointsBar.OnHit(_orbType);
            Destroy(gameObject);
        });

    }

}
