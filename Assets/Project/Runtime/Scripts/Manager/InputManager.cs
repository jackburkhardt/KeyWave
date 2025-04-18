using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ DisallowMultipleComponent]
public class InputManager : MonoBehaviour
{
    
        
    private Vector2 m_mousePosition;
    private bool cursorModeChangedLastFrame = false;
    
    InputAction clickAction;
    InputAction moveAction;
    InputAction submitAction;
    InputAction cancelAction;

    private CustomDialogueUI _customDialogueUI;
    private List<ItemUIPanel> _itemUIPanels;
    private SmartWatchPanel _smartWatchPanel;
    
    public StandardUIPauseButton pauseButton;
    
    
    private const float autoPauseCooldownTime = 0.5f;
    private float _autoPauseCooldownRemaining;
    
    public static InputManager instance;


    private void OnValidate()
    {
        pauseButton ??= FindObjectOfType<StandardUIPauseButton>();
    }

    public void OnGameSceneStart()
    {
        ResetDefaultSelectable();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(this);
        }
        
       
    }

    private void Start()
    {
        var inputSystemUIInputModule = FindObjectOfType<InputSystemUIInputModule>();
        _itemUIPanels = FindObjectsByType<ItemUIPanel>( FindObjectsInactive.Include,  FindObjectsSortMode.None ).ToList();
        _smartWatchPanel = FindObjectOfType<SmartWatchPanel>();
        clickAction = inputSystemUIInputModule.leftClick;
        moveAction = inputSystemUIInputModule.move;
        submitAction = inputSystemUIInputModule.submit;
        cancelAction = inputSystemUIInputModule.cancel;
        _customDialogueUI = FindObjectOfType<CustomDialogueUI>();
    }
    
    
    
    private void Update()
    {
        
        // auto-pausing gameplay on focus lost
        
        if (GameManager.settings.autoPauseOnFocusLost)
        {
            if (_autoPauseCooldownRemaining > 0)
            {
                _autoPauseCooldownRemaining -= Time.deltaTime;
            }

            if (!Application.isFocused && !SceneManager.GetSceneByName("PauseMenu").isLoaded &&
                _autoPauseCooldownRemaining <= 0)
            {
                pauseButton.TogglePause();
                _autoPauseCooldownRemaining = autoPauseCooldownTime;
            }
        }
        
        // show cursor when using mouse if it was hidden
        
          if (!cursorModeChangedLastFrame)
          {
              // cursor handling
              var newMousePosition = Mouse.current.position.ReadValue();

              if (submitAction.WasPressedThisFrame() || moveAction.WasPressedThisFrame() ||
                  cancelAction.WasPressedThisFrame())
              {
                  ShowCursor(false);
              }
            
              else if (newMousePosition != m_mousePosition)
              {
                  ShowCursor(true);
              }

              else
              {
                  m_mousePosition = Mouse.current.position.ReadValue();
              }
          }

          else
          {
              m_mousePosition = Mouse.current.position.ReadValue();
              cursorModeChangedLastFrame = false;
          }
            
          var currentlyOpenMenu = CustomUIMenuPanel.latestInstance;
            
          // input and selection handling
          if (clickAction.WasPressedThisFrame() || submitAction.WasPressedThisFrame())
          {
              var openSubtitlePanel = CustomUISubtitlePanel.latestInstance;
              if (openSubtitlePanel != null && openSubtitlePanel.isOpen && (currentlyOpenMenu == null || !currentlyOpenMenu.isOpen))
              {
                  if (openSubtitlePanel.subtitleText.maxVisibleCharacters > 0 && (openSubtitlePanel.continueButton != null && openSubtitlePanel.continueButton.enabled))
                  {
                      openSubtitlePanel.continueButton.OnFastForward();
                  }
              }
          }

          if (moveAction.WasPressedThisFrame())
          {
              var anyMenuOpen = currentlyOpenMenu != null && currentlyOpenMenu.isOpen; 
              var anyItemPanelOpen = _itemUIPanels.Any(p => p.isOpen);

              if ( _defaultSelectable != null && EventSystem.current.currentSelectedGameObject == null)
              {
                  EventSystem.current.SetSelectedGameObject(_defaultSelectable.gameObject);
              }

              if (anyMenuOpen|| anyItemPanelOpen)
              {
                  var panel = anyMenuOpen?  (UIPanel) currentlyOpenMenu: _itemUIPanels.FirstOrDefault(p => p.isOpen);
                  var selected = EventSystem.current.currentSelectedGameObject;

                  if (panel != null)
                  {
                      var selectedIsValid = selected != null && (selected.transform.IsChildOf(panel.transform) || (selected.transform == _smartWatchPanel.homeButton.transform && _smartWatchPanel.homeButton.isOpen));

                      if (!selectedIsValid)
                      {
                          var firstButton = anyMenuOpen? panel.GetComponentInChildren< StandardUIResponseButton >().gameObject : panel.GetComponentInChildren<ItemUIButton>().gameObject; 
                          var firstValidSelectable = firstButton.GetComponentsInChildren<Selectable>()
                              .First(p => p.navigation.mode != Navigation.Mode.None).gameObject;
                          if (firstValidSelectable == null) firstValidSelectable = firstButton;
                          EventSystem.current.SetSelectedGameObject(firstValidSelectable);
                      }
                        
                  }
              }
          }
            
          if (cancelAction.WasPressedThisFrame())
          {
              if (_smartWatchPanel.homeButton.isOpen) _smartWatchPanel.homeButton.OnClick();
              else if (pauseButton.gameObject.activeSelf) pauseButton.TogglePause();
          }
    }
    
    
        
    private Selectable _defaultSelectable;

    public void OverrideDefaultSelectable(Selectable selectable)
    {
        EventSystem.current.SetSelectedGameObject(null);
        _defaultSelectable = selectable;
    }
        
    public void ResetDefaultSelectable()
    {
        _defaultSelectable = null;
    }
        
    private void ShowCursor(bool value)
    {
        cursorModeChangedLastFrame = true;
        if (Cursor.visible == value) return;
        EventSystem.current.SetSelectedGameObject( null);
        Cursor.visible = value;
        Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public static void PauseGame()
    {
        instance.pauseButton.TogglePause();
    }
}
