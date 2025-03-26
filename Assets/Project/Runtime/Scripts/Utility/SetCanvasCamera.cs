using UnityEngine;

namespace Project.Runtime.Scripts.Utility
{
    public class SortingLayerManager : MonoBehaviour
    {
        public SortingLayer sortingLayer;
    }

    public class SetCanvasCamera : SortingLayerManager
    {

        // Start is called before the first frame update
        private void Awake()
        {
            GetComponent<Canvas>().worldCamera = Camera.main;
        }
    }
}