using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CircularLayoutSelectionNav : MonoBehaviour
{
    public enum Direction
    {
        Clockwise,
        CounterClockwise
    }
    
    public Direction direction = Direction.Clockwise;
    
    private void OnEnable()
    {
        var selectables = GetComponentsInChildren<UnityEngine.UI.Selectable>().Where(p => p.transform.parent == transform).ToList();
        
        for (int i = 0; i < selectables.Count; i++)
        {
            var selectable = selectables[i];
            var nextIndex = i + 1;
            var prevIndex = i - 1;
            if (nextIndex >= selectables.Count) nextIndex = 0;
            if (prevIndex < 0) prevIndex = selectables.Count - 1;
            var next = selectables[nextIndex];
            var prev = selectables[prevIndex];
            switch (direction)
            {
                case Direction.Clockwise:
                    selectable.navigation = new UnityEngine.UI.Navigation
                    {
                        mode = UnityEngine.UI.Navigation.Mode.Explicit,
                        selectOnUp = prev,
                        selectOnDown = next
                        
                    };
                    break;
                case Direction.CounterClockwise:
                    selectable.navigation = new UnityEngine.UI.Navigation
                    {
                        mode = UnityEngine.UI.Navigation.Mode.Explicit,
                        selectOnUp = next,
                        selectOnDown = prev
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
