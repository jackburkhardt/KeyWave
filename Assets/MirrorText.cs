using System.Collections;
using System.Collections.Generic;
using PixelCrushers;
using UnityEngine;

public class MirrorText : MonoBehaviour
{
    [SerializeField] private UITextField text;
    [SerializeField] private UITextField mirrorText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = mirrorText.text;
    }
}
