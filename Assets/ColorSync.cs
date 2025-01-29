using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[ExecuteAlways]
[RequireComponent(typeof(Graphic))]
public class ColorSync : MonoBehaviour
{
    public Graphic master;
    private Graphic slave;
    
    public void OnValidate()
    {
        slave ??= GetComponent<Graphic>();
    }

    private void Awake()
    {
        slave ??= GetComponent<Graphic>();
    }

    private void Update()
    {
        if (master != null)  slave.color = master.color;
    }
}
