using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PixelCrushers.DialogueSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(RectTransform))]
public class MapAnimator : MonoBehaviour
{
    public Image pinPrefab;
    // Start is called before the first frame update
    private float _animationSpeed = 20f; // animation speed
    private bool _standby = true;
    private RectTransform _rectTransform, _parentRectTransform;
    private Vector3 _direction;
    
    
    [SerializeField] private RectTransform _infoPanel, _confirmButton, _cancelButton, _objectivePrefab;
    [SerializeField] private TMPro.TMP_Text _locationName, _etaText, _descriptionText;
    private RectTransform _objectivePanel;
    
    private void Start()
    {
        // Initialize the origin position (you can set this to your starting point)
        _rectTransform = GetComponent<RectTransform>();
        _parentRectTransform = transform.parent.GetComponent<RectTransform>();
        ResetAnimation();
        SpawnPins();
        _objectivePanel = _objectivePrefab.parent.GetComponent<RectTransform>();
        _objectivePrefab.gameObject.SetActive(false);

        // Start the coroutine to move the object
       // StartCoroutine(MoveObject());
    }

    private void SpawnPins()
    {
        foreach (var location in GameManager.instance.locations)
        {
            var coordinates = new Vector3(location.coordinates.x, location.coordinates.y, 0);
            var pin = Instantiate(pinPrefab.gameObject, transform, false);
            pin.transform.localPosition = coordinates;
        }
        
        pinPrefab.gameObject.SetActive(false);
        
    }
    
    /*

    public void ShowInfoPanelHandler(Transform locationTransform, GameManager.Locations location, string descriptionText,
        int distanceInSeconds, bool onClick = false)
    {
        if (!_confirmButton.gameObject.activeSelf && !onClick || _confirmButton.gameObject.activeSelf && onClick) 
            ShowInfoPanel(locationTransform, location, descriptionText, distanceInSeconds);
    }
    
    */
    
    public void ShowFleetingInfoPanel(string location)
    {
        if (!_confirmButton.gameObject.activeSelf) ShowInfoPanel(Location.FromString(location));
    }
    
    public void ShowFleetingInfoPanel(Location location)
    {
        if (!_confirmButton.gameObject.activeSelf) ShowInfoPanel(location);
    }
    
    

    public void ShowPersistentInfoPanel(Location location)
    {
        ShowInfoPanel(location);
        ShowConfirmationButtons();
        ZoomInOnCoordinates(location.coordinates);
    }
    
    public void ShowPersistentInfoPanel(string location)
    {

         ShowPersistentInfoPanel(Location.FromString(location));
    }
    
    public void HideFleetingInfoPanel()
    {
        if (!_confirmButton.gameObject.activeSelf) HideInfoPanel();
       
    }

  
    

   

    
    
    
    
    private void ShowInfoPanel(Location location)  
    {
        _locationName.text = location.name;
        _confirmButton.GetComponent<InvokeMovePlayer>().SetDestination(location);
        _etaText.text = $"ETA: {Clock.EstimatedTimeOfArrival(location)}";
        _descriptionText.text = location.description;
        _infoPanel.gameObject.SetActive(true);

        foreach (var objective in _objectivePanel.GetComponentsInChildren<InfoPanelObjective>())
        {
            if (objective == _objectivePrefab.GetComponent<InfoPanelObjective>()) continue;
            
            Destroy(objective.gameObject);
        }

        foreach (var objective in location.objectives)
        {
            var objectiveItem = Instantiate(_objectivePrefab.gameObject, _objectivePanel);
            objectiveItem.SetActive(true);
            objectiveItem.GetComponent<InfoPanelObjective>().SetObjectiveText(objective.ToString());
        }
        
        RefreshLayoutGroups.Refresh(_objectivePanel.gameObject);
        
        
    }

    private void HideInfoPanel()
    {
        _infoPanel.gameObject.SetActive(false);
    }

    private void ShowConfirmationButtons() {
        _confirmButton.gameObject.SetActive(true);
        _cancelButton.gameObject.SetActive(true);
    }

    private void HideConfirmationButtons()
    {
        _confirmButton.gameObject.SetActive(false);
        _cancelButton.gameObject.SetActive(false);
    }
    
    

    public void ZoomInOnCoordinates(Vector2 coordinates)
    {
        CancelAnimations();
        LeanTween.moveLocal(gameObject, -coordinates * transform.localScale.x, 1f).setEaseInOutSine();
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
        transform.localPosition += _direction * (_animationSpeed * Time.deltaTime);
        
        
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
