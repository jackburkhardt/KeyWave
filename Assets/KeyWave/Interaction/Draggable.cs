using UnityEngine;
using UnityEngine.EventSystems;

namespace Interaction
{
    /// <summary>
    /// Mouse drag feature for UI elements.
    /// </summary>
    public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerDownHandler, IPointerUpHandler
    {

        // Code adapted from: http://gyanendushekhar.com/2019/11/11/move-canvas-ui-mouse-drag-unity-3d-drag-drop-ui/

        private Vector2 lastMousePosition;
        [SerializeField] bool setToLastSiblingWhenClicked = true;
        [SerializeField] bool increaseSizeWhileDragging = true;

        public void OnBeginDrag(PointerEventData eventData)
        {
            lastMousePosition = eventData.position;
        }

        public void OnPointerDown(PointerEventData pointerEventData)
        {
            transform.SetAsLastSibling();
            if (increaseSizeWhileDragging) transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }

        public void OnPointerUp(PointerEventData pointerEventData)
        {
            if (increaseSizeWhileDragging) transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        public void OnDrag(PointerEventData eventData)
        {

            Vector2 currentMousePosition = eventData.position;
            Vector2 diff = currentMousePosition - lastMousePosition;
            RectTransform rect = GetComponent<RectTransform>();
            Vector3 oldPos = rect.position;
            Vector3 newPosition = rect.position +  new Vector3(diff.x, diff.y, transform.position.z);
            rect.position = newPosition;
            if(!IsRectTransformInsideSreen(rect))
            {
                rect.position = oldPos;
            }
            lastMousePosition = currentMousePosition;
        }
    
        private bool IsRectTransformInsideSreen(RectTransform rectTransform)
        {
            bool isInside = false;
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            int visibleCorners = 0;
            Rect rect = new Rect(0,0,Screen.width, Screen.height);
            foreach(Vector3 corner in corners)
            {
                if(rect.Contains(corner))
                {
                    visibleCorners++;
                }
            }
            if(visibleCorners == 4)
            {
                isInside = true;
            }
            return isInside;
        }
    }
}
