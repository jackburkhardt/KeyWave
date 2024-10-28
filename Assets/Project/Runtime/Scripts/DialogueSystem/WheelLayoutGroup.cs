using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    public class WheelLayoutGroup : LayoutGroup
    {
        [SerializeField] private float m_Radius = 200f;  // Radius of the circle layout
        [SerializeField] private float m_StartAngle = 0f; // Angle offset to start placing elements
        [SerializeField] private bool m_Clockwise = true; // Clockwise placement of elements

        /// <summary>
        /// Radius of the circle layout
        /// </summary>
        public float radius { get { return m_Radius; } set { SetProperty(ref m_Radius, value); } }

        /// <summary>
        /// Angle offset to start placing elements
        /// </summary>
        public float startAngle { get { return m_StartAngle; } set { SetProperty(ref m_StartAngle, value); } }

        /// <summary>
        /// Whether elements are placed in a clockwise direction
        /// </summary>
        public bool clockwise { get { return m_Clockwise; } set { SetProperty(ref m_Clockwise, value); } }

        protected WheelLayoutGroup() { }

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

        private void ArrangeElements()
        {
            float angleStep = 360f / rectChildren.Count;  // Equal angle distribution

            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform rect = rectChildren[i];

                // Calculate the angle for the current element
                float angle = m_StartAngle - 90f + (i * angleStep * (m_Clockwise ? 1 : -1));
                float radian = angle * Mathf.Deg2Rad;  // Convert to radians for sine and cosine

                // Calculate the position using polar coordinates
                float posX = Mathf.Cos(radian) * m_Radius;
                float posY = Mathf.Sin(radian) * m_Radius;

                // Set the position of the element
                SetChildAlongAxis(rect, 0, posX + rectTransform.rect.width / 2f - rect.rect.width / 2f); // Centering
                SetChildAlongAxis(rect, 1, posY + rectTransform.rect.height / 2f - rect.rect.height / 2f); // Centering
            }
        }
    }
}
