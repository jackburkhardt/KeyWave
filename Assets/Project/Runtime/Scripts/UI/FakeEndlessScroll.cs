using NaughtyAttributes;
using UnityEngine;

namespace Project.Runtime.Scripts.UI
{
    public class FakeEndlessScroll : MonoBehaviour
    {
        [ValidateInput("ContentIsNotThisTransform", "Content cannot be this GameObject")]
        public Transform content;
    
        public Transform anchorOne;
        public Transform anchorTwo;
    
        public bool flipScrollDirection;

        public bool flipContentRotation;
        private bool _contentIsFlipped;
    
        private bool ContentIsNotThisTransform => content != this.transform;
    
        public float scrollSpeed = 1f;

        private void Update()
        {
            if (flipContentRotation != _contentIsFlipped)
            {
                FlipContent(content);
                _contentIsFlipped = flipContentRotation;
            }
        
        
            if (content == null && ContentIsNotThisTransform || anchorOne == null || anchorTwo == null) return;
        
            var distance = Vector3.Distance(anchorOne.position, anchorTwo.position);
            var direction = (anchorTwo.position - anchorOne.position).normalized * (flipScrollDirection ? 1 : -1);
        
        
            content.position += direction * scrollSpeed * Time.deltaTime * distance;
        
            if (Vector3.Distance(content.position, transform.position) > distance)
            {
                content.position = transform.position;
            }
        }

        private void OnValidate()
        {
            if (flipContentRotation != _contentIsFlipped)
            {
                FlipContent(content);
                _contentIsFlipped = flipContentRotation;
            }
        }
    
        private void FlipContent(Transform container)
        {
            foreach (Transform t in container)
            {
                if (t.childCount == 0)
                {
                    t.rotation = Quaternion.Euler(0, 0, flipContentRotation ? 180 : 0);
                }
            
                FlipContent(t);
            
            }
        }
        
        public void FlipDirectionAndContent(bool flip) 
        {
            flipScrollDirection = flip;
            flipContentRotation = flip;
        }
    }
}
