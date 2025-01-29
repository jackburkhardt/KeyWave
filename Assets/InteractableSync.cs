using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[ExecuteAlways]
[RequireComponent(typeof(Selectable))]

public class InteractableSync : MonoBehaviour
{
    public Selectable master;
    private Selectable slave;
    
    public void OnValidate()
    {
        slave ??= GetComponent<Selectable>();
    }

    private void Awake()
    {
        slave ??= GetComponent<Selectable>();
    }

    private void Update()
    {
        if (master != null)  slave.interactable = master.interactable;
    }
}
