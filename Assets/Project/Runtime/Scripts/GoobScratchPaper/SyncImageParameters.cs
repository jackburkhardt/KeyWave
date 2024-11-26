using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SyncImageParameters : MonoBehaviour
{
    public Image imageToSyncWith;
    public bool syncFillAmount = true;
    public bool syncColor = true;
    public bool syncAlpha = true;
    public bool syncIcon = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (imageToSyncWith == null) return;
        
        if (syncFillAmount)
        {
            GetComponent<Image>().fillAmount = imageToSyncWith.fillAmount;
        }

        if (syncColor)
        {
            var color = imageToSyncWith.color;
            GetComponent<Image>().color = new Color(color.r, color.g, color.b, GetComponent<Image>().color.a);
        }
        
        if (syncAlpha)
        {
            GetComponent<Image>().color = new Color(GetComponent<Image>().color.r, GetComponent<Image>().color.g, GetComponent<Image>().color.b, imageToSyncWith.color.a);
        }
        
        if (syncIcon)
        {
            GetComponent<Image>().sprite = imageToSyncWith.sprite;
        }
        
    }
}
