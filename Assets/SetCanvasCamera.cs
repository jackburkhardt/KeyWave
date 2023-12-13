using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SortingLayerManager : MonoBehaviour
{
    public SortingLayer sortingLayer;
}

public class SetCanvasCamera : SortingLayerManager
{

  //  public SortingLayerManager sortingLayerManager;
     [SerializeField] string sortingLayer;

    // Start is called before the first frame update
    private void Awake()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
      //  GetComponent<Canvas>().sortingLayerName = sortingLayer;
    }
}
