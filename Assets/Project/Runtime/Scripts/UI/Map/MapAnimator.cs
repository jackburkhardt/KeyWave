using System;
using System.Collections.Generic;
using System.Linq;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Project.Runtime.Scripts.UI.Map
{
    [RequireComponent(typeof(RectTransform))]
    public class MapAnimator : MonoBehaviour
    {
        public Image pinPrefab;


        [SerializeField] private RectTransform _infoPanel, _confirmButton, _cancelButton, _returnButton, _objectivePrefab;
        [SerializeField] private TMP_Text _locationName, _etaText, _descriptionText;

        [SerializeField] private GameObjectSwitcher _infoPanelSwitcher;

        // Start is called before the first frame update
        private float _animationSpeed = 20f; // animation speed
        private Vector3 _direction;
        private bool _isFrozen;
        private RectTransform _objectivePanel;
        private RectTransform _rectTransform, _parentRectTransform;
        private bool _standby = true;

        private List<MapLocationInfo> WatchEdgeButtons => FindObjectsOfType<MapLocationInfo>().ToList();

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

        private void OnLoad()
        {
            if (Clock.CurrentTimeRaw >= Clock.ToSeconds("18:00"))
            {
                TutorialPanel.Play("SleepTutorial");
            }
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

        public void FreezeInfoPanel()
        {
            _isFrozen = true;
            ShowConfirmationButtons();
        }

        public void UnfreezeInfoPanel()
        {
            _isFrozen = false;
        }

        public void ShowInfoPanelAndFreeze(GameObject infoPanel)
        {
            _infoPanelSwitcher.ShowObject(infoPanel);
            FreezeInfoPanel();
        }

        public void ShowInfoPanelIfNotFrozen(GameObject infoPanel)
        {
            if (_isFrozen) return;
            _infoPanelSwitcher.ShowObject(infoPanel);
        }


        public void HideInfoPanelIfNotFrozen(GameObject infoPanel)
        {
            if (_isFrozen) return;
            _infoPanelSwitcher.HideObject(infoPanel);
            HideConfirmationButtons();
       
        }

        private void ShowConfirmationButtons() {
            _confirmButton.gameObject.SetActive(true);
            _cancelButton.gameObject.SetActive(true);
        }

        public void ReturnButton()
        {
            Debug.Log(Location.PlayerLocation.name);
            Location.PlayerLocation.MoveHere();
        }

        private void HideConfirmationButtons()
        {
            _confirmButton.gameObject.SetActive(false);
            _cancelButton.gameObject.SetActive(false);
            
        }

        public void DisableCircularButtonsAndCursor()
        {
            foreach (var button in WatchEdgeButtons)
            {
                button.GetComponent<Button>().interactable = false;
            }
        
            WatchHandCursor.GlobalFreeze();
        }

        public void EnableCircularButtonsAndCursor()
        {
            foreach (var button in WatchEdgeButtons)
            {
                button.GetComponent<Button>().interactable = button.location.unlocked;
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

        public void ZoomInOnLocation(Location location)
        {
            ZoomInOnCoordinates(location.coordinates);
        }

        public void ZoomInOnLocation(string locationName)
        {
            ZoomInOnLocation(Location.FromString(locationName));
        }

        public void CancelAnimations()
        {
            LeanTween.cancel(gameObject);
            LeanTween.cancel(transform.parent.gameObject);
        }

        public void CancelButton()
        {
            if (_isFrozen)
            {
                UnfreezeInfoPanel();
                _infoPanelSwitcher.HideAll();
                ResetAnimation();
            }
        }

        public void ResetAnimation()
        {
            CancelAnimations();
            LeanTween.scale(transform.parent.gameObject, new Vector3(1, 1, 1), 0.5f).setEaseInOutSine();
            _standby = true;
            _direction = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0f).normalized;
            HideConfirmationButtons();
        
        }
    }
}