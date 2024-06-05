using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "Tutorial", menuName = "Tutorial")]


public class Tutorial : ScriptableObject

{
    
    public List<string> tutorialText;
    public List<Sprite> tutorialSprites;
   
    
    private int _textCount = 0;

    // Update is called once per frame
    void OnValidate()
    {
        if (_textCount != tutorialText.Count())
        {
            _textCount = tutorialText.Count();
            
            var oldTutorialSprites = tutorialSprites;
            
            tutorialSprites = new List<Sprite>();
            
            for (int i = 0; i < _textCount; i++)
            {
                if (i < oldTutorialSprites.Count())
                {
                    tutorialSprites.Add(oldTutorialSprites[i]);
                }
                else
                {
                    tutorialSprites.Add(null);
                }
            }
            
        }
    }
}
