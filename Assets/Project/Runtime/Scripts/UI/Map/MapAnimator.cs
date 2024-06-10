using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Runtime.Scripts.App;
using Project.Runtime.Scripts.UI.Map;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Button = UnityEngine.UIElements.Button;

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
    [SerializeField] private TMP_Text _locationName, _etaText, _descriptionText;
    [SerializeField] private GameObjectSwitcher _bannerSwitcher;
    private RectTransform _objectivePanel;
    
    private List<CircularUIButton> WatchEdgeButtons => FindObjectsOfType<CircularUIButton>().ToList();
    
    private void Start()
    {
        // Initialize the origin position (you can set this to your starting point)
        _rectTransform = GetComponent<RectTransform>();
        _parentRectTransform = transform.parent.GetComponent<RectTransform>();
        ResetAnimation();
        SpawnPins();
        _objectivePanel = _objectivePrefab.parent.GetComponent<RectTransform>();
        _objectivePrefab.gameObject.SetActive(false);
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
    
    public void ShowFleetingInfoPanel(MapLocationInfo info)
    {
        if (!_confirmButton.gameObject.activeSelf) ShowInfoPanel(info);
    }
    

    public void ShowPersistentInfoPanel(MapLocationInfo info)
    {
        var location = info.location;
        if (!location.unlocked) return;
        
        ShowInfoPanel(info);
        ShowConfirmationButtons();
        ZoomInOnCoordinates(location.coordinates);
    }
    
    public void HidePersistentInfoPanel()
    {
        HideInfoPanel();
        HideConfirmationButtons();
        ResetAnimation();
    }
    
    public void HideFleetingInfoPanel()
    {
        if (!_confirmButton.gameObject.activeSelf) HideInfoPanel();
       
    }
    
    private void ShowInfoPanel(MapLocationInfo info)
    {
        var location = info.location;
        _infoPanel.gameObject.SetActive(true);
        
        if (location.unlocked)
        {
            _locationName.text = location.Name;
            _confirmButton.GetComponent<InvokeMovePlayer>().SetDestination(location);
            _etaText.text = $"ETA: {Clock.EstimatedTimeOfArrival(location)}";
            _descriptionText.text = location.description;
            _bannerSwitcher.ShowObject(info.banner.gameObject);
        }
        else
        {
            _locationName.text = "???";
            _etaText.text = $"ETA: Unknown";
            _descriptionText.text = "You don't know anything about this location yet...";
            _bannerSwitcher.HideAll();
        }
        
        

        var infoColor = info.GetComponent<Image>().color;
        var infoPanelColor = _infoPanel.GetComponent<Image>().color;
        
        _infoPanel.GetComponent<Image>().color = new Color(infoColor.r, infoColor.g, infoColor.b, infoPanelColor.a);
        

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
    }

    private void HideConfirmationButtons()
    {
        _confirmButton.gameObject.SetActive(false);
    }

    public void DisableCircularButtonsAndCursor()
    {
        foreach (var button in WatchEdgeButtons)
        {
            button.GetComponent<UnityEngine.UI.Button>().interactable = false;
        }
        
        WatchHandCursor.GlobalFreeze();
    }
    
    public void EnableCircularButtonsAndCursor()
    {
        foreach (var button in WatchEdgeButtons)
        {
            button.GetComponent<UnityEngine.UI.Button>().interactable = true;
        }
        WatchHandCursor.GlobalUnfreeze();
    }

    public void ZoomInOnCoordinates(Vector2 coordinates)
    {
        CancelAnimations();
        LeanTween.moveLocal(gameObject, -coordinates * transform.localScale.x, 1f).setEaseInOutSine();
        LeanTween.scale(transform.parent.gameObject, new Vector3(2, 2, 1), 1f).setEaseInOutSine();
        _standby = false;
    }

    public void CancelAnimations()
    {
        LeanTween.cancel(gameObject);
        LeanTween.cancel(transform.parent.gameObject);
    }

    public void CancelButton()
    {
        if (_infoPanel.gameObject.activeSelf)
        {
            HidePersistentInfoPanel();
            
        }
        else
        {
            GameManager.instance.CloseMap(true);
        }
    }
    
    public void ResetAnimation()
    {
        CancelAnimations();
        LeanTween.scale(transform.parent.gameObject, new Vector3(1, 1, 1), 0.5f).setEaseInOutSine();
        _standby = true;
        _direction = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0f).normalized;
        HideConfirmationButtons();
        HideInfoPanel();
    }

    private void Update()
    {
        if (!_standby) return;

        //pick a random direction
        
        var distanceToHorizontalEdge = (_rectTransform.rect.width - _parentRectTransform.rect.width) / 2f - Mathf.Abs(_rectTransform.localPosition.x);
        var distanceToVerticalEdge = (_rectTransform.rect.height - _parentRectTransform.rect.height) / 2f - Mathf.Abs(_rectTransform.localPosition.y);
        
        transform.localPosition += _direction * (_animationSpeed * Time.deltaTime);
        
        
        if (distanceToHorizontalEdge < 30 || distanceToVerticalEdge < 30)
        {
            var newXdirection = _rectTransform.localPosition.x > 0
                ? Random.Range(-10f, 0)
                : Random.Range(0, 10f);
            
            var newYdirection = _rectTransform.localPosition.y > 0
                ? Random.Range(-10f, 0)
                : Random.Range(0, 10f);
            
            _direction = new Vector3(newXdirection, newYdirection, 0f).normalized;
        }
    }
    
}
