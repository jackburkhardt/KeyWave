using PixelCrushers;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasicTextPanel : MonoBehaviour
{
    public UITextField textField;
    public AbstractTypewriterEffect typewriterEffect;
    public Animator animator;
    
    public string showTrigger = "Show";
    public string hideTrigger = "Hide";
    
    public void ShowText(string text)
    {
        animator.SetTrigger(showTrigger);
        if (typewriterEffect != null)
        {
            typewriterEffect.StartTyping(text);
        }
        else
        {
            textField.text = text;
        }
    }
    
    public void ShowText(TextMeshProUGUI text)
    {
        ShowText(text.text);
    }
    
    public void ShowText(Text text)
    {
        ShowText(text.text);
    }
    
    public void Hide()
    {
        animator.SetTrigger(hideTrigger);
    }
    
    
}
