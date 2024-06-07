using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanel : MonoBehaviour
{

    [SerializeField] private UITextField _tutorialText;
    [SerializeField] private Image _tutorialImage;
    [SerializeReference] private List<Tutorial> _tutorials;
    private List<string> _completedTutorials = new List<string>();
    private Tutorial _tutorial = null;
    [SerializeField] private Image _dotTemplate;
    private List<Image> _dots = new List<Image>();
    private List<Image> Dots => _dotTemplate.transform.parent.GetComponentsInChildren<Image>().Where(p => p.transform != _dotTemplate.transform && p.transform !=  _dotTemplate.transform.parent).ToList();
    [SerializeField] private Button _forwardButton;
    [SerializeField] private Button _backButton;

    [SerializeField] private Transform _content;
    
    public static TutorialPanel instance;
    
    
    private int _currentTutorialIndex = 0;
    private static readonly int Hide = Animator.StringToHash("Hide");

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
        WatchHandCursor.GlobalFreeze();
        
        _content.gameObject.SetActive(true);
       
        SetupTutorial();
        
        Debug.Log("Show");
       
    }



    public void OnHide()
    {
        DialogueManager.Unpause();
        WatchHandCursor.GlobalUnfreeze();
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
            _completedTutorials.Add(_tutorial.name);
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

            
        if (_completedTutorials.Count > 0 && _completedTutorials.Contains(_tutorial.name))
        {
            return;
        }
        
        if (_tutorial == null)
        {
            Debug.LogError($"Tutorial {tutorialName} not found");
            return;
        }
        
       
         GetComponent<Animator>().SetTrigger("Show");
          
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

    // Start is called before the first frame update
    void Start()
    {
        _dotTemplate.gameObject.SetActive(false);

        
        
    }

    // Update is called once per frame

}
