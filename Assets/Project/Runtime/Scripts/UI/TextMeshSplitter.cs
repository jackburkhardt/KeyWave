using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEditor.Experimental;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(TMP_Text))]
public class TextMeshSplitter : MonoBehaviour
{
    private TMP_Text _textMesh => GetComponent<TMP_Text>();
    
    [SerializeReference] private List<TMP_Text> _textMeshes = new List<TMP_Text>();
    
    [SerializeField] private string splitter = "~~~";


    
    public enum SplitNullAction
    {
        doNothing,
        setAllTextToMeshIndex
    }
    
    public SplitNullAction splitNullAction;
    
    [ShowIf("splitNullAction", SplitNullAction.setAllTextToMeshIndex)]
    public int meshIndex;
    


    private string currentText;
    
    
    // Start is called before the first frame update
    void Start()
    {
        currentText = _textMesh.text;
    }

    private void OnEnable()
    {
        StartCoroutine(SplitTextHandler());
    }

    IEnumerator SplitTextHandler()
    {
        yield return new WaitForEndOfFrame();
        SplitText();
    }

    void SplitText()
    {
        var text = _textMesh.text;
        var strings = _textMesh.text.Split(splitter);

        if (!_textMesh.text.Contains(splitter))
        {
            if (splitNullAction == SplitNullAction.setAllTextToMeshIndex)
            {
                for (int i = 0; i < _textMeshes.Count; i++)
                {
                    if (i == meshIndex)
                    {
                        _textMeshes[i].text = text;
                    }
                    else
                    {
                        _textMeshes[i].text = "";
                    }
                }

                return;
            }
        }
        
        

        for (int i = 0; i < _textMeshes.Count; i++)
        {
            if (i >= strings.Length)
            {
                _textMeshes[i].text = "";
                continue;
            }

            if (_textMeshes[i] == this._textMesh)
            {
                var firstVisibleCharacter = 0;
                for (int j = 0; j < i; j++)
                {
                    firstVisibleCharacter += strings[j].Length + splitter.Length;
                }

                _textMesh.firstVisibleCharacter = firstVisibleCharacter;
                _textMesh.maxVisibleCharacters = firstVisibleCharacter + strings[i].Length;
             }
            
            else if (i == _textMeshes.Count - 1 && strings.Length > _textMeshes.Count) 
            {
                for (int j = i; j < strings.Length; j++)
                {
                    _textMeshes[i].text += strings[j] + splitter;
                   
                }
            }
            else if (i < strings.Length)
            {
                _textMeshes[i].text = strings[i];
                
            }
            else
            {
                _textMeshes[i].text = "";
            }
            
            
        }
        
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_textMesh.text != currentText)
        {
            currentText = _textMesh.text;
            StartCoroutine(SplitTextHandler());
        }
    }
}
