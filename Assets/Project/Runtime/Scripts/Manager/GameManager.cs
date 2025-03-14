using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NaughtyAttributes;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.Audio;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.Manager;
using Project.Runtime.Scripts.SaveSystem;
using Project.Runtime.Scripts.Utility;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Transition = Project.Runtime.Scripts.AssetLoading.LoadingScreen.Transition;


namespace Project.Runtime.Scripts.Manager
{
    [Serializable]


    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public static GameStateManager gameStateManager;

        public static int CurrentTime => settings != null && settings.Clock != null ? settings.Clock.CurrentTime : 0;

        public static PlayerEventStack playerEventStack;
        public static DialogueDatabase dialogueDatabase => settings != null ? settings.dialogueDatabase : null;

        public static Settings settings
        {
            get
            {
                instance ??= FindObjectOfType<GameManager>();
               
                if (instance == null) return null;
                return instance.currentSettings;
            }
            set => instance.currentSettings = value;
        }
        
        public List<Location> locations;
        public bool capFramerate = false;
        public Canvas mainCanvas;
        [SerializeField] private Settings currentSettings;
        

        [ShowIf("capFramerate")] public int framerateLimit;

        private CustomLuaFunctions _customLuaFunctions;
        public DailyReport dailyReport;

        private string last_convo = string.Empty;

        public static GameState gameState => GameStateManager.instance.gameState;

        public static Action OnGameManagerAwake;
    
        public UnityEvent OnGameSceneStart;
        public UnityEvent OnGameSceneEnd;
        
        public StandardUIPauseButton pauseButton;
        
        public Actor PlayerActor =>  DialogueManager.masterDatabase.actors.First(p => p.IsPlayer && p.IsFieldAssigned("Location"));
        
        public static bool TryGetSettings( out Settings settings)
        {
            instance ??= FindObjectOfType<GameManager>();
            if (instance == null)
            {
                settings = null;
                return false;
            }
            settings = instance.currentSettings;
            if (instance.currentSettings == null)
            {
                return false;
            }
            return true;
        }

        
        private static string _mostRecentApp = "SmartWatch/Home";
        
        public static string MostRecentApp
        {
            get
            {
                var convo = _mostRecentApp;
                _mostRecentApp = "SmartWatch/Home";
                return convo;
            }
            
            set => _mostRecentApp = $"SmartWatch/{value}";
        }


       private void Awake()
        {
            OnGameManagerAwake?.Invoke();

            //if instance of GameManager already exists, destroy it

            if (instance == null)
            {
                instance = this;
            }

            else if (instance != this)
            {
                Destroy(this);
            }

            if (gameStateManager == null)
            {
                gameStateManager = this.AddComponent<GameStateManager>();
            }

            if (playerEventStack == null)
            {
                playerEventStack = ScriptableObject.CreateInstance<PlayerEventStack>();
            }

            _customLuaFunctions = GetComponent<CustomLuaFunctions>() ?? gameObject.AddComponent<CustomLuaFunctions>();

        }
       
        InputAction clickAction;
        InputAction moveAction;
        InputAction submitAction;
        InputAction cancelAction;
        
        private List<StandardUIMenuPanel> _menuPanels;
        private List<StandardUISubtitlePanel> _subtitlePanels;
        private List<ItemUIPanel> _itemUIPanels;
        private SmartWatchPanel _smartWatchPanel;
        
        private Vector2 m_mousePosition;
        private bool cursorModeChangedLastFrame = false;


        private void Start()
        {
            var inputSystemUIInputModule = FindObjectOfType<InputSystemUIInputModule>();
            clickAction = inputSystemUIInputModule.leftClick;
            moveAction = inputSystemUIInputModule.move;
            submitAction = inputSystemUIInputModule.submit;
            cancelAction = inputSystemUIInputModule.cancel;
            _menuPanels = FindObjectsByType<StandardUIMenuPanel>( FindObjectsInactive.Include,  FindObjectsSortMode.None ).ToList();
            _subtitlePanels = FindObjectsByType<StandardUISubtitlePanel>( FindObjectsInactive.Include,  FindObjectsSortMode.None ).ToList();
            _itemUIPanels = FindObjectsByType<ItemUIPanel>( FindObjectsInactive.Include,  FindObjectsSortMode.None ).ToList();
            _smartWatchPanel = FindObjectOfType<SmartWatchPanel>();
        }
        
        private float autoPauseCooldown = 0.5f;
        
     
        private void Update()
        {
            if (settings.autoPauseOnFocusLost)
            {
                if (autoPauseCooldown > 0)
                {
                    autoPauseCooldown -= Time.deltaTime;
                }

                if (!Application.isFocused && !SceneManager.GetSceneByName("PauseMenu").isLoaded &&
                    autoPauseCooldown <= 0)
                {
                    pauseButton.TogglePause();
                    autoPauseCooldown = 0.5f;
                }
            }

            if (capFramerate) Application.targetFrameRate = framerateLimit;



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
            
          
            
            // input and selection handling
            if (clickAction.WasPressedThisFrame() || submitAction.WasPressedThisFrame())
            {
                var openSubtitlePanel = _subtitlePanels.FirstOrDefault(p => p.isOpen);
                if (openSubtitlePanel != null && !_menuPanels.Any(p => p.isOpen))
                {
                    if (openSubtitlePanel.subtitleText.maxVisibleCharacters > 0 && (openSubtitlePanel.continueButton != null && openSubtitlePanel.continueButton.enabled))
                    {
                        openSubtitlePanel.continueButton.OnFastForward();
                    }
                }
            }

            if (moveAction.WasPressedThisFrame())
            {
                var anyMenuPanelOpen = _menuPanels.Any(p => p.isOpen);
                var anyItemPanelOpen = _itemUIPanels.Any(p => p.isOpen);

                if ( _defaultSelectable != null && EventSystem.current.currentSelectedGameObject == null)
                {
                    EventSystem.current.SetSelectedGameObject(_defaultSelectable.gameObject);
                }

                if (anyMenuPanelOpen || anyItemPanelOpen)
                {
                    var panel = anyMenuPanelOpen ?  (UIPanel) _menuPanels.FirstOrDefault(p => p.isOpen) : _itemUIPanels.FirstOrDefault(p => p.isOpen);
                    var selected = EventSystem.current.currentSelectedGameObject;

                    if (panel != null)
                    {
                        var selectedIsValid = selected != null && (selected.transform.IsChildOf(panel.transform) || (selected.transform == _smartWatchPanel.homeButton.transform && _smartWatchPanel.homeButton.isOpen));

                        if (!selectedIsValid)
                        {
                            var firstButton = anyMenuPanelOpen ? panel.GetComponentInChildren< StandardUIResponseButton >().gameObject : panel.GetComponentInChildren<ItemUIButton>().gameObject; 
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
        
        

        private void OnDestroy()
        {
            Destroy(playerEventStack);
            Destroy(gameStateManager);
        }

        public void PauseDialogueSystem()
        {
            DialogueManager.instance.Pause();
        }

        public void UnpauseDialogueSystem()
        {
            DialogueManager.instance.Unpause();
        }

        public void OnSaveDataApplied()
        {
            dailyReport ??= new DailyReport(gameState.day);
        }

        public void DoEndOfDay()
        {
            GameEvent.OnDayEnd();
            DialogueManager.instance.gameObject.BroadcastMessageExt( "OnEndOfDay");
            App.App.Instance.ChangeScene("EndOfDay", gameStateManager.gameState.current_scene, Transition.Black);
        }

        public IEnumerator StartNewSave()
        {
            dailyReport = new DailyReport(gameState.day);
            
            while (App.App.isLoading)
            {
                yield return new WaitForEndOfFrame();
            }
            
            yield return new WaitForSeconds(0.25f);
            while (!DialogueManager.hasInstance)
            {
                yield return null;
            }

            if (DialogueLua.GetVariable("skip_content").asBool)
                SetLocation(gameState.GetPlayerLocation().Name, Transition.Black);

            else DialogueManager.instance.StartConversation("Intro");
        }

        public void OnConversationStart()
        {
            var activeConversation = DialogueManager.instance.activeConversation;
            var conversation = DialogueManager.masterDatabase.GetConversation(activeConversation.conversationTitle);
            if (Field.FieldExists(conversation.fields, "Base") &&
                !DialogueLua.GetConversationField(conversation.id, "Base").asBool) return;
            instance.mainCanvas.BroadcastMessage("SetTrigger", "Show", SendMessageOptions.DontRequireReceiver);
            
        }
        
        
        public static void DoLocalSave()
        {
            PixelCrushers.SaveSystem.SaveToSlot(1);
        }
        
        public void StartOfDay() => App.App.Instance.ChangeScene("StartOfDay", gameStateManager.gameState.current_scene);
       
        
        private const float DistanceToCafé = 300f;


        public static float DistanceToLocation(int locationID)
        {
            var location = DialogueManager.masterDatabase.GetLocation(locationID);
            if (gameState.GetPlayerLocation() == location) return 0;
            // if cafe, distance is relative to current location
            if (location.Name == "Café")
            {
                return DistanceToCafé;
            }
                
            var locationCoordinates = location.LookupVector2("Coordinates");
            var playerCoordinates = gameState.GetPlayerLocation().LookupVector2("Coordinates");

            return Vector2.Distance(playerCoordinates, locationCoordinates) * Traffic.CurrentTrafficMultiplier;
        }

        public static float DistanceToLocation(string locationName)
        {
            var location = DialogueManager.masterDatabase.GetLocation(locationName);
            return DistanceToLocation(location.id);
        }
        
        
        public void SetLocation(string locationName)
        {
            SetLocation(locationName, Transition.Default);
        }
        
        public void SetLocation(int locationID, Transition type =  Transition.Default)
        {
            var location = DialogueManager.masterDatabase.GetLocation(locationID);
            SetLocation(location.Name, type);
        }
        
        

        public void SetLocation(string newLocation,  Transition transition)
        {
            var location = DialogueManager.masterDatabase.GetLocation(newLocation);
        
            GameEvent.OnMove(newLocation, gameState.GetPlayerLocation().Name, (int)DistanceToLocation(location.id));
            
            BroadcastMessage( "OnTravel");
          
            EndGameScene();

            var currentScene = string.Empty;
            
            if (SceneManager.GetSceneByName("StartMenu").isLoaded) currentScene = "StartMenu";
            else currentScene = gameState.current_scene;
            
            StartCoroutine(TravelToHandler());
            
            
            IEnumerator TravelToHandler()
            {
                
                yield return App.App.Instance.ChangeScene(newLocation, currentScene, transition);
                
                AudioEngine.Instance.StopAllAudioOnChannel("Music");


                
                
                while (App.App.isLoading)
                {
                    
                    yield return null;
                }

                yield return new WaitForSeconds(1f);
                
                gameState.current_scene = newLocation;
                StartGameScene(gameState.GetPlayerLocation(true));
                
            }
        }
        
        public void StartBaseOrPreBaseConversation()
        {

            var visitCount = DialogueLua.GetLocationField(gameState.GetPlayerLocation(true).Name, "Visit Count").asInt;
                 var loopConversation = gameState.GetPlayerLocation(true).LookupBool("Loop Conversation");
            
            if (DialogueLua.GetLocationField( gameState.GetPlayerLocation(true).Name, "Dirty").asBool)
            {
                DialogueManager.StartConversation("Base");
                return;
            }


            if (!gameState.GetPlayerLocation(true).FieldExists("Conversation"))
            {
                DialogueManager.StartConversation("Base");
                return;
            }

            if (visitCount == 0)
            {
                if (gameState.GetPlayerLocation(true).IsFieldAssigned("Conversation"))
                    DialogueManager.StartConversation(
                        gameState.GetPlayerLocation(true).LookupValue("Conversation"));

                else
                {
                    var generatedConversation =
                        GameManager.GenerateConversation(gameState.GetPlayerLocation(true));
                    // SequencerCommandGoToConversatio
                    DialogueManager.StartConversation(generatedConversation.Title);
                }
            }
            
            else if (visitCount > 0 && loopConversation)
                if (gameState.GetPlayerLocation(true).IsFieldAssigned("Conversation"))
                    DialogueManager.StartConversation(
                        gameState.GetPlayerLocation(true).LookupValue("Conversation"));

                else
                {
                    var generatedConversation =
                        GameManager.GenerateConversation(gameState.GetPlayerLocation(true), true);
                    // SequencerCommandGoToConversatio
                    DialogueManager.StartConversation(generatedConversation.Title);
                }

            else DialogueManager.StartConversation("Base");
            
            MarkLocationAsDirty( gameState.GetPlayerLocation(true));
        }

        public void MarkLocationAsDirty(Location location)
        {
            DialogueLua.SetLocationField(gameState.GetPlayerLocation(true).Name, "Dirty", true);
            var visitCount = DialogueLua.GetLocationField(gameState.GetPlayerLocation(true).Name, "Visit Count").asInt;
            visitCount += 1;
            DialogueLua.SetLocationField( gameState.GetPlayerLocation(true).Name, "Visit Count", visitCount);
            gameState.GetPlayerLocation(true).AssignedField("Visit Count").value = (visitCount).ToString();
        }
        
        public void UnmarkLocationAsDirty(Location location)
        {
            DialogueLua.SetLocationField(gameState.GetPlayerLocation(true).Name, "Dirty", false);
        }

        public void Wait(int duration)
        {
            GameEvent.OnWait(duration);
        }

        public void StartGameScene( Location location)
        {
            GameManager.gameState.SetPlayerLocation(location);
            GameManager.instance.OnGameSceneStart?.Invoke();
            DialogueManager.instance.gameObject.BroadcastMessageExt( "OnGameSceneStart");
            ResetDefaultSelectable();
        }

        public void StartGameScene(int locationID)
        {
            var location = DialogueManager.masterDatabase.GetLocation(locationID);
            StartGameScene(location);
        }
        
        public void StartGameScene(string locationName)
        {
            var location = DialogueManager.masterDatabase.GetLocation(locationName);
            StartGameScene(location);
        }

        public void EndGameScene()
        {
            DialogueManager.StopConversation();
            OnGameSceneEnd?.Invoke();
            DialogueManager.instance.gameObject.BroadcastMessageExt( "OnGameSceneEnd");
            FindObjectOfType<SmartWatchPanel>().ResetCurrentApp();
            DialogueManager.PlaySequence("ChannelFade(Music, out, 1);");
            DialogueManager.PlaySequence("ChannelFade(Environment, out, 1);");
            UnmarkLocationAsDirty( gameState.GetPlayerLocation(true));
        }
        
        public static Conversation GenerateConversation(Asset asset, bool repeatEntries = false)
            {
                var template = Template.FromDefault();
                
                var dialogueEntries = new List<DialogueEntry>();
                var conversation = template.CreateConversation( Template.FromDefault().GetNextConversationID(DialogueManager.masterDatabase), $"/GENERATED/{asset.Name}");
                var entryActorID = asset.IsFieldAssigned("Entry Actor")
                    ? int.Parse(asset.LookupValue("Entry Actor"))
                    : -1;
                
                var entryFieldLabelStart = repeatEntries && asset.FieldExists("Repeat Entry Count") ? "Repeat Entry" : "Entry";
                var entryCount = asset.LookupInt($"{entryFieldLabelStart} Count");


                var startNode = template.CreateDialogueEntry( 0, conversation.id, "START");
            
                startNode.ActorID = entryActorID;
                startNode.Sequence = "None()";
                startNode.outgoingLinks = new List<Link> { new Link( conversation.id, 0, conversation.id, 1) };
                dialogueEntries.Add(startNode);
                
                int musicEntry = Field.FieldExists( asset.fields, $"Music {entryFieldLabelStart}") ? asset.LookupInt($"Music {entryFieldLabelStart}") : 0;
            

                for (int i = 1; i < entryCount + 1; i++)
                {
                    var menuText = asset.LookupValue($"{entryFieldLabelStart} {i} Menu Text");
                    var dialogueText = asset.LookupValue($"{entryFieldLabelStart} {i} Dialogue Text");
                    var duration = asset.LookupInt($"{entryFieldLabelStart} {i} Duration");
                    
                    var newDialogueEntry = template.CreateDialogueEntry( i, conversation.id, string.Empty);
                    newDialogueEntry.MenuText = menuText;
                    newDialogueEntry.DialogueText = dialogueText;
                
                    newDialogueEntry.ActorID = entryActorID;
                    
                    newDialogueEntry.fields.Add(new Field("Duration", duration.ToString(CultureInfo.InvariantCulture), FieldType.Number));

                    if (i == musicEntry)
                    {
                        var musicPath = asset.LookupValue("Music");
                        newDialogueEntry.userScript += $"PlayClipLooped(\"{musicPath}\")";
                    }
                
                    if (i < entryCount) newDialogueEntry.outgoingLinks = new List<Link>() { new Link( conversation.id, i, conversation.id, i + 1) };
                
                    dialogueEntries.Add(newDialogueEntry);
                };
                
                
                conversation.dialogueEntries = dialogueEntries;
                
                DialogueManager.masterDatabase.conversations.Add(conversation);
                return conversation;
            }

        public UnityEvent OnGameClose;
        public void CloseGame()
        {
            AudioEngine.Instance.StopAllAudio();
            
            OnGameClose?.Invoke();
        }

        public void ForceConversation(string conversation)
        {
            var conversationState = DialogueManager.currentConversationState;

            if (conversationState == null)
            {
                DialogueManager.instance.StartConversation(conversation);
            }

            else
            {
                var dialogueEntry = DialogueManager.currentConversationState.subtitle.dialogueEntry;
                var currentConversation = DialogueManager.masterDatabase.GetConversation(dialogueEntry.conversationID);
                
                if (currentConversation.Title != conversation)
                {
                    DialogueManager.instance.StopConversation();
                    DialogueManager.instance.StartConversation(conversation);
                }
            }
        }

        public void SetSublocation(PixelCrushers.DialogueSystem.Location location)
        {
            if (location == gameState.GetPlayerLocation(true)) return;
            DialogueManager.instance.gameObject.BroadcastMessageExt( "OnSublocationChange");
            StartCoroutine(SwitchSublocation(location));
        }

        public void SetSublocation(string locationName)
        {
            var location = DialogueManager.masterDatabase.GetLocation(locationName);
            if (location == null)
            {
                locationName = locationName.Replace($"{gameState.GetPlayerLocation(false).Name}/", "");
            }

            location = DialogueManager.masterDatabase.GetLocation(locationName);
            
            if (location == null)
            {
                Debug.LogError($"Location {locationName} not found.");
                return;
            }
            
            SetSublocation(DialogueManager.masterDatabase.GetLocation(locationName));
            
        }


        static Canvas faderCanvas = null;
        static UnityEngine.UI.Image faderImage = null;
        
        IEnumerator Fade( string direction, float duration = 1f, Color color = default)
        {
            
            bool stay;
            bool unstay;
            bool fadeIn;
            
            stay = string.Equals(direction, "stay", System.StringComparison.OrdinalIgnoreCase);
            unstay = string.Equals(direction, "unstay", System.StringComparison.OrdinalIgnoreCase);
            fadeIn = unstay || string.Equals(direction, "in", System.StringComparison.OrdinalIgnoreCase);
            
            if (color == default) color = Color.black;
            
            IEnumerator FadeHandler()
            {
                float SmoothMoveCutoff = 0.05f;
                int FaderCanvasSortOrder = 32760;
                
               
                float startTime;
                float endTime;
                
                
                if (faderCanvas == null)
                {
                    faderCanvas = new GameObject("Canvas (Fader)", typeof(Canvas)).GetComponent<Canvas>();
                    faderCanvas.transform.SetParent(DialogueManager.instance.transform);
                    faderCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    faderCanvas.sortingOrder = FaderCanvasSortOrder;
                }
                if (faderImage == null)
                {
                    faderImage = new GameObject("Fader Image", typeof(UnityEngine.UI.Image)).GetComponent<UnityEngine.UI.Image>();
                    faderImage.transform.SetParent(faderCanvas.transform, false);
                    faderImage.rectTransform.anchorMin = Vector2.zero;
                    faderImage.rectTransform.anchorMax = Vector2.one;
                    faderImage.sprite = null;
                    var initializeAlpha = (fadeIn || unstay) ? 1 : 0;
                    faderImage.color = new Color(color.r, color.g, color.b, initializeAlpha);
                }

                if (unstay && faderImage != null && Mathf.Approximately(0, faderImage.color.a))
                {
                    yield break;
                }
                else if (duration > SmoothMoveCutoff)
                {
                    faderCanvas.gameObject.SetActive(true);
                    faderImage.gameObject.SetActive(true);

                    // Set up duration:
                    startTime = Time.time;
                    endTime = startTime + duration;

                    // If fade in or out, start from 1 or 0. Otherwise start from current alpha.
                    var startingAlpha = fadeIn ? 1
                        : (stay || unstay) ? faderImage.color.a
                        : 0;
                    faderImage.color = new Color(color.r, color.g, color.b, startingAlpha);

                }
                else
                {

                    yield break;
                }
                
                while (Time.time < endTime)
                {
                    var t = (Time.time - startTime) / duration;
                    var a = fadeIn ? 1 - t : t;
                    faderImage.color = new Color(color.r, color.g, color.b, a);
                    yield return null;
                }
            }
            
            
            yield return FadeHandler();
            if (fadeIn) Destroy(faderCanvas.gameObject);
            if (fadeIn) Destroy(faderImage.gameObject);
                
        }
        
        IEnumerator SwitchSublocation(PixelCrushers.DialogueSystem.Location location, float transitionDuration = 3f)
        {
            var locationScene = SceneManager.GetSceneByName(DialogueManager.masterDatabase.GetLocation(location.RootID).Name);
            DialogueManager.Pause();
                
            yield return Fade( "stay", transitionDuration/4);
            yield return new WaitForSeconds(transitionDuration/4);
                
            var destinationSublocationGameObject = locationScene.FindGameObject(location.Name);
            if (destinationSublocationGameObject != null) destinationSublocationGameObject.SetActive(true);
                
            if (gameState.GetPlayerLocation(true).IsSublocation)
            {
                var currentSublocationGameObject = locationScene.FindGameObject(gameState.GetPlayerLocation(true).Name);
                if (currentSublocationGameObject != null) currentSublocationGameObject.SetActive(false);
            }
            
            
                
            yield return Fade( "unstay", transitionDuration/4);
            yield return new WaitForSeconds(transitionDuration/4);
            DialogueManager.Unpause();
                
            gameState.SetPlayerLocation(location);
            yield return null;
        }
    }
}


public class SequencerCommandSetLocationImmediate : SequencerCommand
{
    IEnumerator Start()
    {
        var location = parameters[0];
        var currentScene = string.Empty;
            
        if (SceneManager.GetSceneByName("StartMenu").isLoaded) currentScene = "StartMenu";
        else currentScene = GameManager.gameState.current_scene;
        yield return Project.Runtime.Scripts.App.App.Instance.ChangeScene(location, currentScene, Transition.None);
        GameManager.instance.StartGameScene(location); 
        Stop();
    }

}

public class SequencerCommandOnGameSceneStart : SequencerCommand
{
    public void Awake()
    {
        return;
        GameManager.instance.OnGameSceneStart?.Invoke();
        DialogueManager.instance.BroadcastMessage("OnGameSceneStart");
    }
}