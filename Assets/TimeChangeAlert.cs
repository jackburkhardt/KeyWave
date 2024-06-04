using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using UnityEngine;

public class TimeChangeAlert : MonoBehaviour
{
    [SerializeField] private UITextField _textField;
    [SerializeField] Animator _animator;
    
    public void OnTimeChange((int, int) timeSpan)
    {
        
        var hourDifference = string.Empty;
        var minuteDifference = string.Empty;
        var secondDifference = string.Empty;
        
        var oldTimeInMinutes = timeSpan.Item1 / 60;
        var newTimeInMinutes = timeSpan.Item2 / 60;
        
        var difference = newTimeInMinutes - oldTimeInMinutes;
        
      

        if (difference <= 0) return;

        _textField.text = $"+{difference}";
        _animator.SetTrigger("Show");
        

    }
    
    
    
    public void OnTimeChanged()
    {
        _animator.SetTrigger("Hide");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
