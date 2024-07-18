using System.Collections.Generic;
using System.Linq;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using Project.Runtime.Assets.Tutorials;
using Project.Runtime.Scripts.UI;
using Project.Runtime.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Runtime.Scripts.Manager
{
    public class TutorialPanel : MonoBehaviour
    {
        public static TutorialPanel instance;
        private static readonly int Hide = Animator.StringToHash("Hide");

        [SerializeField] private UITextField _tutorialText;
        [SerializeField] private Image _tutorialImage;
        [SerializeReference] private List<Tutorial> _tutorials;
        [SerializeField] private Image _dotTemplate;
        [SerializeField] private Button _forwardButton;
        [SerializeField] private Button _backButton;

        [SerializeField] private Transform _content;


        private int _currentTutorialIndex = 0;
        private List<Image> _dots = new List<Image>();
        private Tutorial _tutorial = null;
        private List<Image> Dots => _dotTemplate.transform.parent.GetComponentsInChildren<Image>().Where(p => p.transform != _dotTemplate.transform && p.transform !=  _dotTemplate.transform.parent).ToList();

        // Start is called before the first frame update
        void Start()
        {
            _dotTemplate.gameObject.SetActive(false);
            _content.gameObject.SetActive(false);
        
        
        }

        private void OnEnable()
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

        private void OnDisable()
        {
       
        }

        void SetupTutorial()
        {
            _currentTutorialIndex = 0;
        

            _tutorial ??= _tutorials[0];

            _dots = new List<Image>();
        
            for (int i = 0; i < _tutorial.tutorialText.Count; i++)
            {
                var dot = Instantiate(_dotTemplate, _dotTemplate.transform.parent);
                dot.gameObject.SetActive(true);
                _backButton.gameObject.SetActive(false);
                _dots.Add(dot.GetComponent<Image>());
            }
        
            RefreshPanel();
        
        
        }

        void RefreshPanel()
        {
            _tutorialText.text = _tutorial.tutorialText[_currentTutorialIndex];
            SetTutorialImage(_tutorial.tutorialSprites[_currentTutorialIndex]);
        
            for (int i = 0; i< _dots.Count; i++)
            {
                _dots[i].color = i == _currentTutorialIndex ? Color.white : new Color(1f, 1f, 1f, 0.5f);
            }
        
            RefreshLayoutGroups.Refresh(gameObject);
        }

        public void OnShow()
        {
            DialogueManager.Pause(); 
            //WatchHandCursor.GlobalFreeze();
        
            _content.gameObject.SetActive(true);
       
            SetupTutorial();
        
     
       
        }


        public void OnHide()
        {
            DialogueManager.Unpause();
           // WatchHandCursor.GlobalUnfreeze();
            _content.gameObject.SetActive(false);
        
            foreach (var dot in Dots)
            {
                Destroy(dot.gameObject);
            }
        }

        void SetTutorialImage(Sprite sprite)
        {
            if (sprite == null)
            {
                _tutorialImage.gameObject.SetActive(false);
                return;
            }
        
            _tutorialImage.gameObject.SetActive(true);
            _tutorialImage.sprite = sprite;
        }

        public void OnNextTutorialIndex()
        {
            _currentTutorialIndex++;
            if (_tutorial.tutorialText.Count > 1) _backButton.gameObject.SetActive(true);
            if (_currentTutorialIndex >= _tutorial.tutorialText.Count)
            {
                DialogueLua.SetVariable($"Tutorial/{_tutorial.name}", true);
                GetComponent<Animator>().SetTrigger(Hide);
            
                return;
            }
        
       
            RefreshPanel();
        }

        public void OnPreviousTutorialIndex()
        {
            _currentTutorialIndex--;
            if (_currentTutorialIndex <= 0)
            {
                _backButton.gameObject.SetActive(false);
            }
        
            RefreshPanel();
        }

        public void PlayTutorial(string tutorialName)
        {
            _tutorial = _tutorials.FirstOrDefault(p => p.name == tutorialName);
            _tutorial ??= _tutorials.FirstOrDefault(p => p.name.Contains(tutorialName));


            if (DialogueLua.GetVariable($"Tutorial/{_tutorial.name}").asBool) return;
            if (DialogueLua.GetVariable("debug.disable_tutorials").asBool) return;
            if (_tutorial == null)
            {
                Debug.LogError($"Tutorial {tutorialName} not found");
                return;
            }
            
            
        
       
            GetComponent<Animator>().SetTrigger("Show");
          
        }

        public static void Play(string tutorialName)
        {
            instance.PlayTutorial(tutorialName);
        }

        // Update is called once per frame
    }
}