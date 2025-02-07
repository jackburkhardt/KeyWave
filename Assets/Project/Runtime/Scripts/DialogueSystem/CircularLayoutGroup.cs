using DG.Tweening;
using NaughtyAttributes;

namespace UnityEngine.UI
{
    public class CircularLayoutGroup : LayoutGroup
    {
        public enum LayoutMethod
        {
            GroupAsCircle,
            RestrictToCircle
        }

        public LayoutMethod layoutMethod = LayoutMethod.GroupAsCircle;
        
        [SerializeField] private bool m_useProportionalRadius;
        [HideIf("m_useProportionalRadius")]
        [SerializeField] private float m_Radius = 200f;  // Radius of the circle layout
        [ShowIf("m_useProportionalRadius")] [Label("Proportional to:")]
        [SerializeField] private ProportionalRadiusType m_proportionalRadiusType;
        [ShowIf("m_useProportionalRadius")]
        [Range(0,1)] [SerializeField] private float m_proportionalRadius = 0.5f;
        [SerializeField] private float m_StartAngle = 0f; // Angle offset to start placing elements
        [SerializeField] private bool m_Clockwise = true; // Clockwise placement of elements

      

        public enum ProportionalRadiusType
        {
            Width,
            Height
        }
        
        public float proportionalRadius
        {
            get
            {
                switch (m_proportionalRadiusType)
                {
                    case ProportionalRadiusType.Width:
                        return rectTransform.rect.width * m_proportionalRadius;
                    case ProportionalRadiusType.Height:
                        return rectTransform.rect.height * m_proportionalRadius;
                    default:
                        return m_proportionalRadius;
                }
            }
        }
        
        /// <summary>
        /// Radius of the circle layout
        /// </summary>
        public float radius { get { return m_useProportionalRadius ? proportionalRadius : m_Radius; } set { SetProperty(ref m_Radius, value); } }

        /// <summary>
        /// Angle offset to start placing elements
        /// </summary>
        public float startAngle { get { return m_StartAngle; } set { SetProperty(ref m_StartAngle, value); } }

        /// <summary>
        /// Whether elements are placed in a clockwise direction
        /// </summary>
        public bool clockwise { get { return m_Clockwise; } set { SetProperty(ref m_Clockwise, value); } }

        protected CircularLayoutGroup() { }

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
            
            switch (layoutMethod)
            {
                case LayoutMethod.GroupAsCircle:
                    GroupElementsIntoCircle();
                    break;
                case LayoutMethod.RestrictToCircle:
                    RestrictElementsIntoCircle();
                    break;
                    
            }
            
            
            
            
            void GroupElementsIntoCircle()
            {
                float angleStep = 360f / rectChildren.Count;  // Equal angle distribution
            
                for (int i = 0; i < rectChildren.Count; i++)
                {
                
                    RectTransform rect = rectChildren[i];

                    var outputRadius = rect.GetComponent<CircularLayoutGroupElement>() != null
                        ? rect.GetComponent<CircularLayoutGroupElement>().Radius * radius
                        : radius;

                    // Calculate the angle for the current element
                    float angle = m_StartAngle - 90f + (i * angleStep * (m_Clockwise ? 1 : -1));
                
                    angle += rect.GetComponent<CircularLayoutGroupElement>() != null
                        ? rect.GetComponent<CircularLayoutGroupElement>().DegreeOffset
                        : 0;
                
                    float radian = angle * Mathf.Deg2Rad;  // Convert to radians for sine and cosine

                    // Calculate the position using polar coordinates
                    float posX = Mathf.Cos(radian) * outputRadius;
                    float posY = Mathf.Sin(radian) * outputRadius;

                    // Set the position of the element
                    SetChildAlongAxis(rect, 0, posX + rectTransform.rect.width / 2f - rect.rect.width / 2f); // Centering
                    SetChildAlongAxis(rect, 1, posY + rectTransform.rect.height / 2f - rect.rect.height / 2f); // Centering
                }
            }


            void RestrictElementsIntoCircle()
            {
                for (int i = 0; i < rectChildren.Count; i++)
                {
                
                    RectTransform rect = rectChildren[i];
                    
                    var targetPosition = rect.GetComponent<CircularLayoutGroupElement>() != null
                        ? rect.GetComponent<CircularLayoutGroupElement>().TargetPosition
                        : Vector3.zero;

                    // Calculate the angle for the current element
                    var normalized = (targetPosition - transform.position).normalized;
                        
                
                    var posX = normalized.x * radius;
                    
                    float posY = - normalized.y * radius;

                    // Set the position of the element
                    SetChildAlongAxis(rect, 0, posX + rectTransform.rect.width / 2f - rect.rect.width / 2f); // Centering
                    SetChildAlongAxis(rect, 1, posY + rectTransform.rect.height / 2f - rect.rect.height / 2f); // Centering
                }
            }
            
            
            
        }


        public void AnimateElementsIndividuallyWithDelay(float delay)
        {
            GetComponent<Animator>().SetBool("Dirty", true);
            for (int i = 0; i < rectChildren.Count; i++)
            {
                var animator = rectChildren[i].GetComponent<Animator>();
                DOTween.Sequence()
                    .AppendInterval(i * delay + delay)
                    .AppendCallback(() => animator.SetTrigger("Show"));
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
