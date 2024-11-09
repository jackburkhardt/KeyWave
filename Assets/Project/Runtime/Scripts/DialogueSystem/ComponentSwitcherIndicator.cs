using System.Linq;
using UnityEngine;

[ExecuteInEditMode]

public class ComponentSwitcherIndicator : MonoBehaviour
{
    public bool preview;
    public ComponentSwitcher componentSwitcher;

    public GameObject activeIndicatorTemplate;
    public GameObject inactiveIndicatorTemplate;
    public int IndicatorCount => transform.GetComponentsInChildren<Transform>().Where(p =>
            p.transform != activeIndicatorTemplate.transform
            && p.transform != inactiveIndicatorTemplate.transform)
        .ToList()
        .Count;

    private void OnValidate()
    {
        if (!componentSwitcher.IsFirstComponentSwitcher && componentSwitcher.sync) componentSwitcher = componentSwitcher.FirstComponentSwitcher;
        if (preview) UpdateIndicator(false);
        else if (!Application.isPlaying)
        {
            DestroyNonTemplateChildren(false);
            activeIndicatorTemplate.SetActive(true);
            inactiveIndicatorTemplate.SetActive(true);
        }
    }

    private void DestroyNonTemplateChildren(bool destroy = true)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).transform != activeIndicatorTemplate.transform
                && transform.GetChild(i).transform != inactiveIndicatorTemplate.transform)
            {
                if (!destroy)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
                else
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
        }
    }
    
    public void Update()
    {
        if (!Application.isPlaying && preview) UpdateIndicator(true);
    }
    public void UpdateIndicator(bool destroy = true)
    {

        if (IndicatorCount != componentSwitcher.ComponentCount)
        {
            activeIndicatorTemplate.SetActive(false);
            inactiveIndicatorTemplate.SetActive(false);

            DestroyNonTemplateChildren(destroy);
            
            
            
        }
        
        for (int i = 0; i < componentSwitcher.ComponentCount; i++)
        {
            if (i == componentSwitcher.ActiveIndex)
            {
                Instantiate(activeIndicatorTemplate, transform).SetActive(true);
            }
            else
            {
                Instantiate(inactiveIndicatorTemplate, transform).SetActive(true);
            }
        }
        
       
    }
}
