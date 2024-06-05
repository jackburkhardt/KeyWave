using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AllInOneMaterialController : MonoBehaviour
{
   private Material _material;
    
    public float _leftClipping;
    public float _rightClipping;
    public float _topClipping;
    public float _bottomClipping;


    private void Awake()
    {
        
    }
    
    public void SetClipping()
    {
        
    }
    // Start is called before the first frame update
    
    // Update is called once per frame
    private void OnUpdate()
    {
        _material = GetComponent<Image>().material;
        _material.SetFloat("_LeftClipping", _leftClipping);
        _material.SetFloat("_RightClipping", _rightClipping);
        _material.SetFloat("_TopClipping", _topClipping);
        _material.SetFloat("_BottomClipping", _bottomClipping);
    }
}
