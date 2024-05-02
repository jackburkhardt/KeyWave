using System;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using StandardUIResponseButton = PixelCrushers.DialogueSystem.Wrappers.StandardUIResponseButton;

[RequireComponent(typeof(Button))]
[ExecuteInEditMode]
public class CircularUIButton : MonoBehaviour
{
    public Image image;
    [SerializeField] private Transform pointerHand;
    [SerializeField] private Button button;
    [SerializeReference] private Side watchSide;
    [SerializeField] private bool syncWatchSides = true;
    [SerializeReference] private Spacing spacing;
    [SerializeField] private bool syncSpacingWithWatchSide = true;
    [NonSerialized] public float angleIndex;
    [NonSerialized] public Spacing masterSpacing = Spacing.Spread;
    List<CircularUIButton> matchedSideInteractables = new List<CircularUIButton>();
    [SerializeField] private float _padding;
    public UnityEvent onHover, onClick, onMouseExit;
    public static float globalOffset;
  
    
    public static float CircularUIDegreeSum
    {
        get
        {
            var degreeSum = 0f;
            foreach (var circularUIButton in FindObjectsOfType<CircularUIButton>())
            {
                degreeSum += circularUIButton.image.fillAmount * 360f;
            }

            return degreeSum;
        }
    }
    
   
    public enum Side
    {
        Right,
        Top,
        Left,
        Bottom
    };
    
    private Dictionary<Side, int> SideIndex = new()
    {
        {Side.Right, 0},
        {Side.Top, 1},
        {Side.Left, 2},
        {Side.Bottom, 3}
    };
    
    public enum Spacing
    {
        Spread,
        Clustered
    };

    

    public Color ButtonColor
    {
        get => image.color;
        set => image.color = value;
    }

    public void SetAngleIndex(float angle)
    {
        angleIndex = angle;
    }

  
    
    // Start is called before the first frame update

    private float _interactAngleThreshold;

    private void Start()
    {
        image ??= GetComponent<Image>();
        button ??= GetComponent<Button>();
        image.alphaHitTestMinimumThreshold = 2;
    }
    

    private void OnDisable()
    {
        if (GetComponent<StandardUIResponseButton>() == null || CustomUIMenuPanel.Instance == null) return;
        if (GetComponent<StandardUIResponseButton>() != CustomUIMenuPanel.Instance.buttonTemplate) Destroy(gameObject);
    }


    private void OnEnable()
    {
      
    }

    [NonSerialized] public bool isButtonActive = false;

    
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


        //spacing = masterSpacing;
        
        // get the index of the current button
        
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
                transform.localRotation = Quaternion.Euler(0, 0, angleIndex + SideIndex[watchSide] * 90 - degreeSum / 2 + globalOffset);
                break;
        }

        
        _interactAngleThreshold = image.fillAmount * 360;
        var watchHandAngle = pointerHand.transform.localEulerAngles.z > 0 ? pointerHand.transform.localEulerAngles.z : pointerHand.transform.localEulerAngles.z + 360;
        var minAngle = image.transform.localEulerAngles.z;
            
        minAngle =  minAngle  > 0 ? minAngle : minAngle + 360;
        var maxAngle = minAngle + _interactAngleThreshold;
        
        
        
        var pointer = new PointerEventData(EventSystem.current);
        
        //check if pointer is not already on top of another button


        var isArrowPointingAtButton = watchHandAngle < maxAngle && watchHandAngle > minAngle;

        if (isArrowPointingAtButton && button.interactable && !isButtonActive)
        {
            isButtonActive = true;
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
            onHover.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && isButtonActive && button.interactable)
        {
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
            onClick.Invoke();
           }
    
        if (!isArrowPointingAtButton && isButtonActive)
        {
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerExitHandler);
            onMouseExit.Invoke();
            isButtonActive = false;
        }
        
    }
}
