using UnityEngine;
using UnityEngine.UI;

public class RefreshLayoutGroups : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Refresh(this.gameObject);
    }
    
    public static void Refresh(GameObject root)

    {
        var componentsInChildren = root.GetComponentsInChildren<LayoutGroup>(true);
        foreach (var layoutGroup in componentsInChildren)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }

        var parent = root.GetComponent<LayoutGroup>();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());
    }
}
