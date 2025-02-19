using System;
using DG.Tweening;
using NaughtyAttributes;
using Unity.Mathematics;

namespace UnityEngine.UI
{
    public class ArcLayoutGroup : LayoutGroup
    {

        [HideIf("m_alignWithRadialFill")]
        [SerializeField] private float m_Radius = 200f;  // Radius of the circle layout

        [ShowIf("m_alignWithRadialFill")] [Label("Radius Offset")]
        [SerializeField] private float m_radialFillRadius = 0;  // Radius of the circle layout
        [DisableIf("m_alignWithRadialFill")]
        [SerializeField] private float m_StartAngle = 0f; // Angle offset to start placing elements
        [DisableIf("m_alignWithRadialFill")]
        [SerializeField] private float m_EndAngle = 360f; // Angle offset to start placing elements
        [DisableIf("m_alignWithRadialFill")]
        [SerializeField] private bool m_Clockwise = true; // Clockwise placement of elements
        [SerializeField] private bool reverseOrientation;
        
        
        private bool m_imageIsRadialFill => GetComponent<Image>()?.type == Image.Type.Filled;
        [ShowIf("m_imageIsRadialFill")] [SerializeField]
        private bool m_alignWithRadialFill;

        [ShowIf("m_imageIsRadialFill")] [Range(-180,180)]
        [SerializeField] private float m_radialFillPadding;


        public float StartAngle => m_alignWithRadialFill ? GetComponent<Image>().fillOrigin * 90f - m_radialFillPadding / 2f : m_StartAngle;
        public float EndAngle => m_alignWithRadialFill ? GetComponent<Image>().fillAmount * 360f + m_radialFillPadding : m_EndAngle;
        public bool Clockwise => m_alignWithRadialFill ? GetComponent<Image>().fillClockwise : m_Clockwise;
        
        public float Radius => m_alignWithRadialFill ? GetComponent<RectTransform>().rect.width / 2 + m_radialFillRadius : m_Radius;


        protected ArcLayoutGroup() { }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
        }
#endif

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
        }

        public override void CalculateLayoutInputVertical()
        {
            base.CalculateLayoutInputHorizontal();
        }

        public override void SetLayoutHorizontal()
        {
            ArrangeElements();
        }

        public override void SetLayoutVertical()
        {
            ArrangeElements();
        }

        public void ArrangeElements()
        {
            
            
            float angleStep = EndAngle / Mathf.Max( rectChildren.Count - 1, 1) ;  // Equal angle distribution
            
            for (int i = 0; i < rectChildren.Count; i++)
            {
                
                RectTransform rect = rectChildren[i];

                // Calculate the angle for the current element
                
                
                float angle = StartAngle - 90f + (i * angleStep * (Clockwise ? 1 : -1));
                
                float radian = angle * Mathf.Deg2Rad;
                
                // Convert to radians for sine and cosine

                // Calculate the position using polar coordinates
                float posX = Mathf.Cos(radian) * Radius;
                float posY = Mathf.Sin(radian) * Radius;
                
                
                if (reverseOrientation)
                {
                    rect = rectChildren[rectChildren.Count - 1 - i];
                }
                
                

                // Set the position of the element
                SetChildAlongAxis(rect, 0, posX + rectTransform.rect.width / 2f - rect.rect.width / 2f); // Centering
                SetChildAlongAxis(rect, 1, posY + rectTransform.rect.width / 2f - rect.rect.width / 2f); // Centering
            }
            
        }
        
        public void DestroyAllElements()
        {
            for (int i = 0; i < rectChildren.Count; i++)
            {
                Destroy(rectChildren[i].gameObject);
            }
        }
        
    }
}
