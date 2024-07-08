using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupUtilities : MonoBehaviour
{
    public float alpha = 1;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<CanvasGroup>().alpha = alpha;
    }

   
}
