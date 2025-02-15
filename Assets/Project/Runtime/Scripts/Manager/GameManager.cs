using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NaughtyAttributes;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Scripts.AssetLoading;
using Project.Runtime.Scripts.Audio;
using Project.Runtime.Scripts.DialogueSystem;
using Project.Runtime.Scripts.Events;
using Project.Runtime.Scripts.SaveSystem;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Transition = Project.Runtime.Scripts.AssetLoading.LoadingScreen.Transition;


namespace Project.Runtime.Scripts.Manager
{
    [Serializable]


    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public static GameStateManager gameStateManager;
        public static PlayerEventStack playerEventStack;
        
        

        public static Settings settings
        {
            get
            {
                #if UNITY_EDITOR
                if (instance == null) return AssetDatabase.LoadAssetAtPath<Settings>("Assets/Game Settings.asset");
                #else
                if (instance == null)
                {
                    // force instantiation of GM instance since there are dependants in Awake, OnEnable of other scripts
                    instance = FindAnyObjectByType<GameManager>();
                }
                #endif
                return instance.currentSettings;
            }
            set => instance.currentSettings = value;
        }
        
        public List<Location> locations;
        public bool capFramerate = false;
        public Canvas mainCanvas;
        [SerializeField] private Settings currentSettings;
        
        public SmartWatch smartWatchAsset;

        [ShowIf("capFramerate")] public int framerateLimit;

        private CustomLuaFunctions _customLuaFunctions;
        public DailyReport dailyReport;

        private string last_convo = string.Empty;

        public static GameState gameState => GameStateManager.instance.gameState;

        public static Action OnGameManagerAwake;
    
        public UnityEvent OnGameSceneStart;
        public UnityEvent OnGameSceneEnd;
        
        
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

        private void Start()
        {
            GameEvent.OnPlayerEvent += OnPlayerEvent;
            //OnSaveDataApplied();
        }
        
        private float autoPauseCooldown = 0.5f;

        private void Update()
        {
            
            if (autoPauseCooldown > 0)
            {
                autoPauseCooldown -= Time.deltaTime;
            }

            if (!Application.isFocused && !SceneManager.GetSceneByName("PauseMenu").isLoaded && autoPauseCooldown <= 0)
            {
                TogglePause();
                autoPauseCooldown = 0.5f;
            }
            
            if (capFramerate) Application.targetFrameRate = framerateLimit;
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
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

        private void OnPlayerEvent(PlayerEvent e)
        {
            if (gameState.Clock > Clock.DayEndTime)
            {
                GameEvent.OnDayEnd();
                DialogueManager.StopAllConversations();
                DialogueManager.StartConversation("EndOfDay");
            }
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
                SetLocation(gameState.PlayerLocation().Name, Transition.Black);

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


        public void EndOfDay() => App.App.Instance.ChangeScene("EndOfDay", gameStateManager.gameState.current_scene, Transition.Black);

        public void StartOfDay() => App.App.Instance.ChangeScene("StartOfDay", gameStateManager.gameState.current_scene);
       
        
        private const float DistanceToCafé = 300f;


        public static float DistanceToLocation(int locationID)
        {
            var location = DialogueManager.masterDatabase.GetLocation(locationID);
            if (gameState.PlayerLocation() == location) return 0;
            // if cafe, distance is relative to current location
            if (location.Name == "Café")
            {
                return DistanceToCafé;
            }
                
            var locationCoordinates = location.LookupVector2("Coordinates");
            var playerCoordinates = gameState.PlayerLocation().LookupVector2("Coordinates");

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
        
            GameEvent.OnMove(newLocation, gameState.PlayerLocation().Name, (int)DistanceToLocation(location.id));
            
            
            BroadcastMessage( "OnTravel");
            DialogueManager.StopConversation();
            OnGameSceneEnd?.Invoke();

            var currentScene = string.Empty;
            
            if (SceneManager.GetSceneByName("StartMenu").isLoaded) currentScene = "StartMenu";
            else currentScene = gameState.current_scene;

            SmartWatch.ResetCurrentApp();
            
            
            StartCoroutine(TravelToHandler());

            DialogueManager.PlaySequence("ChannelFade(Music, out, 1);");
            DialogueManager.PlaySequence("ChannelFade(Environment, out, 1);");

            
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
                
                OnGameSceneStart?.Invoke();
                
                
                BroadcastMessage( "OnGameSceneStart");
                
                location.AssignedField("Visit Count").value = (location.LookupInt("Visit Count") + 1).ToString();
                
            }
        }
        
        public void StartBaseOrPreBaseConversation()
        {
            var visitCount = gameState.PlayerLocation(true).LookupInt("Visit Count");
            var loopConversation = gameState.PlayerLocation(true).LookupBool("Loop Conversation");


            if (!gameState.PlayerLocation(true).FieldExists("Conversation"))
            {
                DialogueManager.StartConversation("Base");
                return;
            }

            if (visitCount == 0 || loopConversation)
            {
                if (gameState.PlayerLocation(true).IsFieldAssigned("Conversation"))
                    DialogueManager.StartConversation(
                        gameState.PlayerLocation().LookupValue("Conversation"));

                else
                {
                    var generatedConversation =
                        GameManager.GenerateConversation(gameState.PlayerLocation(true));
                    // SequencerCommandGoToConversatio
                    Debug.Log("Generated entries: " + generatedConversation.dialogueEntries.Count);
                    DialogueManager.StartConversation(generatedConversation.Title);
                }
            }

            else DialogueManager.StartConversation("Base");

        }

        public void Wait(int duration)
        {
            GameEvent.OnWait(duration);
        }
        
        
        public void TogglePause()
        {
            if (SceneManager.GetSceneByName("PauseMenu").isLoaded != PauseMenu.active) return;
            
            if (PauseMenu.active)
            {
                PauseMenu.instance.UnpauseGame();
            }
            else
            {
                App.App.Instance.LoadScene("PauseMenu", transition: Transition.None);
            }
        }
        
        public static Conversation GenerateConversation(Asset asset)
            {
                var template = Template.FromDefault();
                
                var dialogueEntries = new List<DialogueEntry>();
                var conversation = template.CreateConversation( Template.FromDefault().GetNextConversationID(DialogueManager.masterDatabase), $"/GENERATED/{asset.Name}");
                var entryActorID = asset.IsFieldAssigned("Entry Actor")
                    ? int.Parse(asset.LookupValue("Entry Actor"))
                    : -1;
                
                var visitCount = asset.LookupInt("Visit Count");
                var entryFieldLabelStart = visitCount > 0  && asset.FieldExists("Repeat Entry Count") ? "Repeat Entry" : "Entry";
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
                    
                    Debug.Log($"{entryFieldLabelStart} {i} Menu Text: {menuText}");
                    var dialogueText = asset.LookupValue($"{entryFieldLabelStart} {i} Dialogue Text");
                
                    var newDialogueEntry = template.CreateDialogueEntry( i, conversation.id, string.Empty);
                    newDialogueEntry.MenuText = menuText;
                    newDialogueEntry.DialogueText = dialogueText;
                
                    newDialogueEntry.ActorID = entryActorID;

                    if (i == musicEntry)
                    {
                        var musicPath = asset.LookupValue("Music");
                        newDialogueEntry.userScript += $"PlayClipLooped({musicPath})";
                    }
                
                    if (i < entryCount) newDialogueEntry.outgoingLinks = new List<Link>() { new Link( conversation.id, i, conversation.id, i + 1) };
                
                    dialogueEntries.Add(newDialogueEntry);
                };
                
                
                conversation.dialogueEntries = dialogueEntries;

                if (asset is Location)
                {
                    conversation.fields.Add( new Field( "Location", asset.id.ToString(), FieldType.Location));
                }
                
                else if (asset is Item item)
                {
                    if (item.IsAction)
                    {
                        conversation.fields.Add( new Field( "Action", item.id.ToString(), FieldType.Item));
                    }
                }
                
                DialogueManager.masterDatabase.conversations.Add(conversation);
                return conversation;
            }

        public UnityEvent OnGameClose;
        public void CloseGame()
        {
            AudioEngine.Instance.StopAllAudio();
            
            OnGameClose?.Invoke();
        }

        public void SetSublocation(PixelCrushers.DialogueSystem.Location location)
        {
            if (location == gameState.PlayerLocation(true)) return;
            StartCoroutine(SwitchSublocation(location));
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
            Debug.Log(DialogueManager.masterDatabase.GetLocation(location.id).Name);
            Debug.Log(DialogueManager.masterDatabase.GetLocation(location.RootID).Name);
            var locationScene = SceneManager.GetSceneByName(DialogueManager.masterDatabase.GetLocation(location.RootID).Name);
            DialogueManager.Pause();
                
            yield return Fade( "stay", transitionDuration/4);
            yield return new WaitForSeconds(transitionDuration/4);
                
            var destinationSublocationGameObject = locationScene.FindGameObject(location.Name);
            if (destinationSublocationGameObject != null) destinationSublocationGameObject.SetActive(true);
                
            if (gameState.PlayerLocation(true).IsSublocation)
            {
                var currentSublocationGameObject = locationScene.FindGameObject(gameState.PlayerLocation(true).Name);
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