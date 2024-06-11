using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.Utility
{
    [ExecuteInEditMode]
    public class FitTextToCircularPanel : MonoBehaviour
    {
        private RectTransform _rectTransform;

        private Image parentImage;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            _rectTransform = GetComponent<RectTransform>();
            var width = _rectTransform.rect.width;
            //w_rectTransform.localPosition = new Vector3(width/2, _rectTransform.localPosition.y, 0);



        }
    }
}