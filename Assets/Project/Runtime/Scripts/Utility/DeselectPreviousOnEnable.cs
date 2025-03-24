using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeselectPreviousOnEnable : MonoBehaviour
{
    private void OnEnable()
    {
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }
}
