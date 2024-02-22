using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TrackingOrb : MonoBehaviour
{
    [SerializeField] private ActionBarManager pointsBar;
    [SerializeField] private Image image;

    private ActionBarManager.OrbType _orbType;
    
    public void SetOrbProperties(ActionBarManager.OrbType orbType, Color color)
    {
        _orbType = orbType;
        image.color = color;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        transform.position = Mouse.current.position.ReadValue();
        LeanTween.move(gameObject, pointsBar.transform.position, 1.25f).setEaseInExpo().setOnComplete(() =>
        {
            switch ( _orbType)
            {
                case ActionBarManager.OrbType.Wellness:
                    pointsBar.UpdateWellnessScore(1);
                    break;
                case ActionBarManager.OrbType.LocalKnowledge:
                    pointsBar.UpdateLocalKnowledgeScore(1);
                    break;
                case ActionBarManager.OrbType.BusinessResearch:
                    pointsBar.UpdateBusinessResearchScore(1);
                    break;
            }
           
            Destroy(gameObject);
        });
        
    }

}
