using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.UI
{
    [RequireComponent(typeof(Button))]
    [ExecuteInEditMode]
    public class CircularUIButton : MonoBehaviour
    {
        public enum Side
        {
            Right,
            Top,
            Left,
            Bottom
        };

        public enum Spacing
        {
            Spread,
            Clustered
        };

        public Image image;
        [SerializeField] private Transform pointerHand;
        [SerializeField] private Button button;
        [SerializeReference] private Side watchSide;
        [SerializeField] private bool syncWatchSides = true;
        [SerializeReference] private Spacing spacing;
        [SerializeField] private bool syncSpacingWithWatchSide = true;
        [SerializeField] private float _padding;
        [SerializeField] private bool _mouseEvents = true;
        

        [ShowIf(nameof(_mouseEvents))]
        public UnityEvent onHover, onClick, onMouseExit;


        // Start is called before the first frame update

        private float _interactAngleThreshold;
        [NonSerialized] public float angleIndex;

        [NonSerialized] public bool isButtonActive = false;
        [NonSerialized] public Spacing masterSpacing = Spacing.Spread;
        List<CircularUIButton> matchedSideInteractables = new List<CircularUIButton>();
        private float offset;

        private Dictionary<Side, int> SideIndex = new()
        {
            {Side.Right, 0},
            {Side.Top, 1},
            {Side.Left, 2},
            {Side.Bottom, 3}
        };

        public float Offset
        {
            get => offset;
            set => offset = value;
        }

        public float CircularUIDegreeSum
        {
            get
            {
                var degreeSum = 0f;
                foreach (var circularUIButton in transform.parent.GetComponentsInChildren<CircularUIButton>())
                {
                    degreeSum += circularUIButton.image.fillAmount * 360f;
                }

                return degreeSum;
            }
        }


        public Color ButtonColor
        {
            get => image.color;
            set => image.color = value;
        }

        private void Awake()
        {
            image ??= GetComponent<Image>();
            button ??= GetComponent<Button>();
            image.alphaHitTestMinimumThreshold = 2;
        }


        private void Update()
        {
        
            var sameSideInteractables = FindObjectsOfType<CircularUIButton>().ToList().Where(watchInteractable => watchInteractable.watchSide == watchSide).ToList();
            sameSideInteractables.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
        
            var currentIndex = 0;
            var degreeSum = 0f;
            var didSpacingChange = spacing != masterSpacing;

            // sync sides

            if (sameSideInteractables.Count < matchedSideInteractables.Count && syncWatchSides)
            {
                var matchedInteractable = matchedSideInteractables.Except(sameSideInteractables).ToList()[0];
                watchSide = matchedInteractable.syncWatchSides ? matchedInteractable.watchSide : watchSide;
                matchedSideInteractables = sameSideInteractables;
                return;
            }
        
            matchedSideInteractables = sameSideInteractables;
        
     
        
            // sync spacing
        
            foreach (var interactable in sameSideInteractables)
            {
                angleIndex = interactable == this ? degreeSum : angleIndex;
                degreeSum += interactable.image.fillAmount * 360;
          
                if (didSpacingChange && syncSpacingWithWatchSide) interactable.spacing = interactable.masterSpacing = spacing;
            }

            currentIndex = sameSideInteractables.IndexOf(this);

        
            image.fillOrigin = 1;
        
            switch (spacing)
            {
                case Spacing.Spread:
                
                    var zAngle = 90 * (SideIndex[watchSide] + 2f * ((currentIndex + 1f) / (sameSideInteractables.Count + 1f) - image.fillAmount) - 1);
                    var center = 90 * (SideIndex[watchSide] - image.fillAmount * 2);
                    zAngle = Mathf.Lerp(zAngle, center, _padding);
                    transform.localRotation = Quaternion.Euler(0, 0, zAngle);
                    break;
                case Spacing.Clustered:
                    transform.localRotation = Quaternion.Euler(0, 0, angleIndex + SideIndex[watchSide] * 90 - degreeSum / 2 + offset);
                    break;
            }

        
            _interactAngleThreshold = image.fillAmount * 360;
            var watchHandAngle = pointerHand.transform.localEulerAngles.z > 0 ? pointerHand.transform.localEulerAngles.z : pointerHand.transform.localEulerAngles.z + 360;
            var minAngle = image.transform.localEulerAngles.z;
            
            minAngle =  minAngle  > 0 ? minAngle : minAngle + 360;
            var maxAngle = minAngle + _interactAngleThreshold; maxAngle = maxAngle > 360 ? maxAngle - 360 : maxAngle;
        
        
        
            var pointer = new PointerEventData(EventSystem.current);
        
            //check if pointer is not already on top of another button


            var isArrowPointingAtButton = minAngle < maxAngle ? watchHandAngle < maxAngle && watchHandAngle > minAngle : watchHandAngle > 0 && watchHandAngle < maxAngle || watchHandAngle > minAngle && watchHandAngle < 360;
        
            if (!isArrowPointingAtButton && isButtonActive)
            {
                ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerExitHandler);
                if (_mouseEvents) onMouseExit.Invoke();
                isButtonActive = false;
            }

            if (isArrowPointingAtButton && button.interactable && !isButtonActive)
            {
                isButtonActive = true;
                ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
                if (_mouseEvents) onHover.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Mouse0) && isButtonActive && button.interactable && !WatchHandCursor.Frozen)
            {
                Debug.Log("Clicking button");
                Debug.Log(Time.timeScale);
                Debug.Log(WatchHandCursor.Frozen);
                ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
                if (_mouseEvents) onClick.Invoke();
            }
    
       
        
        }


        private void OnEnable()
        {
      
        }


        private void OnDisable()
        {
        
        }

        public void SetAngleIndex(float angle)
        {
            angleIndex = angle;
        }
    }
}