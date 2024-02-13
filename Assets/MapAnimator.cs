using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(RectTransform))]
public class MapAnimator : MonoBehaviour
{
    
    // Start is called before the first frame update
    public float moveSpeed = 5f; // Adjust the movement speed as needed
    private bool _standby = true;
    private RectTransform _rectTransform, _parentRectTransform;
    private Vector3 _direction;
    
    
    [SerializeField] private RectTransform _infoPanel, _confirmButton, _cancelButton;
    [SerializeField] private TMPro.TMP_Text _locationName, _etaText, _descriptionText;
    private void Start()
    {
        // Initialize the origin position (you can set this to your starting point)
        _rectTransform = GetComponent<RectTransform>();
        _parentRectTransform = transform.parent.GetComponent<RectTransform>();
        ResetAnimation();
        

        // Start the coroutine to move the object
       // StartCoroutine(MoveObject());
    }

    public void ShowInfoPanelHandler(Transform locationTransform, GameManager.Locations location, string descriptionText,
        int distanceInSeconds, bool onClick = false)
    {
        if (!_confirmButton.gameObject.activeSelf && !onClick || _confirmButton.gameObject.activeSelf && onClick) 
            ShowInfoPanel(locationTransform, location, descriptionText, distanceInSeconds);
    }
    
    public void ShowInfoPanel(Transform locationTransform, GameManager.Locations location, string descriptionText, int distanceInSeconds)  
    {
        _locationName.text = location.ToString();
        _confirmButton.GetComponent<InvokeMovePlayer>().SetDestination(location);
        _etaText.text = $"ETA: {GameManager.instance.HoursMinutes(distanceInSeconds + DialogueLua.GetVariable("clock").asInt)}";
        _descriptionText.text = descriptionText;
        _infoPanel.gameObject.SetActive(true);
    }

    public void HideInfoPanel()
    {
        _infoPanel.gameObject.SetActive(false);
    }

    public void ShowConfirmationButtons() {
        _confirmButton.gameObject.SetActive(true);
        _cancelButton.gameObject.SetActive(true);
    }

    public void HideConfirmationButtons()
    {
        _confirmButton.gameObject.SetActive(false);
        _cancelButton.gameObject.SetActive(false);
    }
    
    public void HideInfoPanelHandler()
    {
       if (!_confirmButton.gameObject.activeSelf) HideInfoPanel();
       
    }

    public void ZoomInOnIcon(Transform locationTransform)
    {
        CancelAnimations();
        var location = locationTransform.localPosition;
        LeanTween.moveLocal(gameObject, -location * transform.localScale.x, 1f).setEaseInOutSine();
        LeanTween.scale(transform.parent.gameObject, new Vector3(2, 2, 1), 1f).setEaseInOutSine();
    }

    public void CancelAnimations()
    {
        LeanTween.cancel(gameObject);
        LeanTween.cancel(transform.parent.gameObject);
    }
   

    public void ResetAnimation()
    {
        CancelAnimations();
        LeanTween.scale(transform.parent.gameObject, new Vector3(1, 1, 1), 0.5f).setEaseInOutSine();
        _standby = true;
        _direction = new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f), 0f).normalized;
        HideConfirmationButtons();
        HideInfoPanel();
    }
    

    IEnumerator ShowLocationHandler(Vector3 location)
    {
        _standby = false;
        _infoPanel.gameObject.SetActive(false);
        //LeanTween.scale(transform.parent.gameObject, new Vector3(1, 1, 1), 0.5f).setEaseInOutSine();
        yield return new WaitForSeconds(0.75f);
    }


    private void Update()
    {
        if (!_standby) return;

        //pick a random direction
        
        var distanceToHorizontalEdge = (_rectTransform.rect.width - _parentRectTransform.rect.width) / 2f - Mathf.Abs(_rectTransform.localPosition.x);
        var distanceToVerticalEdge = (_rectTransform.rect.height - _parentRectTransform.rect.height) / 2f - Mathf.Abs(_rectTransform.localPosition.y);
        
      //  Debug.Log((_rectTransform.rect.width - _parentRectTransform.rect.width) / 2f);
        transform.localPosition += _direction * (moveSpeed * Time.deltaTime);
        
        
        if (distanceToHorizontalEdge < 30 || distanceToVerticalEdge < 30)
        {
            var newXdirection = _rectTransform.localPosition.x > 0
                ? UnityEngine.Random.Range(-10f, 0)
                : UnityEngine.Random.Range(0, 10f);
            
            var newYdirection = _rectTransform.localPosition.y > 0
                ? UnityEngine.Random.Range(-10f, 0)
                : UnityEngine.Random.Range(0, 10f);
            
            _direction = new Vector3(newXdirection, newYdirection, 0f).normalized;
        }
    }

    /*

    private IEnumerator MoveObject()
    {
        float totalMovementTime = 60f; // The amount of time you want the movement to take
        float currentMovementTime = 0f; // The amount of time that has passed

        while (true)
        {
            // Generate a new random destination position
            destination = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0f);

            // Move towards the destination using Lerp
            while (Vector3.Distance(transform.position, destination) > 0)
            {
                currentMovementTime += Time.deltaTime;
                transform.position = Vector3.Lerp(origin, destination, currentMovementTime / totalMovementTime);
                yield return null;
            }

            // Set the new origin for the next movement
            origin = destination;

            // Wait for a brief pause before picking the next random position
            yield return new WaitForSeconds(1f);
        }
    }
    
    
    
    
    */
}
