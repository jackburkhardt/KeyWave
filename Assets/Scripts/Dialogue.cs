using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interaction;
using TMPro;
using UnityEngine.UI;
public class Dialogue : MonoBehaviour
{
    public static Dialogue Instance = null;
    [SerializeField] private TMP_Text _displayText;
    [SerializeField] private TMP_Text _npcNameText;
    
    // the background of the text box
    [SerializeField] private Image _dialogueBackgroundImage;

    // graphical way to show to player that they can press button to show next part of text
    [SerializeField] private Image _continueImage;
    
    // the delay between characters in the dialogue text scrolling (smaller = faster)
    // 0.01 fast, 0.05 medium, 0.1 slow
    [SerializeField] private float _textScrollDelay;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        } else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Displays lines of dialogue to the player.
    /// </summary>
    /// <param name="dialogue">Lines of dialogue to be displayed.</param>
    /// <param name="interacObj">The interactive object this is related to (usually the caller)</param>
    /// <returns></returns>
    public IEnumerator Run(List<string> dialogue, IInteractable interacObj)
    {
        // enable visible canvas elements, set blank
        _dialogueBackgroundImage.enabled = true;
        _displayText.text = "";
        _npcNameText.text = interacObj.GetType() == typeof(Character) ? interacObj.name : "";
        _npcNameText.enabled = true;
        _displayText.enabled = true;

        /*
        // if this convo has a specific text style, set it up
        if (interacObj.Conversation.enableCustomStyle)
        {
            SetStyle(interacObj.Conversation.Style);
        }
        */

        foreach (var line in dialogue)
        {
            // this coroutine chops each line up into a char array and then
            // displays each char in order to simulate scroll effect
            var characters = line.ToCharArray();
            foreach (var c in characters)
            {
                // using the | character will add pauses mid-line!
                if (c == '|')
                {
                    while (!Input.GetMouseButton(0))
                    {
                        _continueImage.enabled = true;
                        yield return null;
                    }
                    _continueImage.enabled = false;
                    continue;
                }

                /*if (Input.GetMouseButton(1))
                {
                    _displayText.text = line;
                    break;
                }*/
                _displayText.text += c;
                yield return new WaitForSeconds(_textScrollDelay);
            }
            
            // wait for player to press button to continue
            while (!Input.GetMouseButton(0))
            {
                _continueImage.enabled = true;
                yield return null;
            }
            
            _continueImage.enabled = false;

        }
        
        // disable visible canvas elements 
        _dialogueBackgroundImage.enabled = false;
        _npcNameText.enabled = false;
        _displayText.enabled = false;
        
        /*// reset text style to default if need be
        if (interacObj.Conversation.enableCustomStyle)
        {
            SetStyle(_defaultStyle);
        }*/
        
        if (interacObj.GetType() != typeof(PopupItem)) interacObj.EndInteraction();
    }

    public IEnumerator Run(string dialogue, IInteractable interacObj)
        => Run(new List<string> {dialogue}, interacObj);

    /// <summary>
    /// Sets the dialogue text styling (font, style, color, size).
    /// </summary>
    /// <param name="style"></param>
    private void SetStyle(DialogueStyle style)
    {
        _displayText.font = style.Font;
        _displayText.fontStyle = style.FontStyle;
        _displayText.fontSize = style.Size;
        _displayText.color = style.TextColor;
    }
}

/// <summary>
/// Contains information about text styling for use with NPCs or other special text.
/// </summary>
public struct DialogueStyle
{
    public TMP_FontAsset Font;
    public FontStyles FontStyle;
    public float Size;
    public Color TextColor;

    public DialogueStyle(TMP_FontAsset font, FontStyles fontStyle, float size, Color textColor)
    {
        Font = font;
        FontStyle = fontStyle;
        Size = size;
        TextColor = textColor;
    }
}
