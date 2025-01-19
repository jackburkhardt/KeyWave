using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CircularLayoutGroupElement : MonoBehaviour
{
    public bool overrideElement = true;
    
    [ShowIf("LayoutMethod", CircularLayoutGroup.LayoutMethod.GroupAsCircle)]
    [Range(0, 2)] [SerializeField] private float _slideRadius = 1;
    [ShowIf("LayoutMethod", CircularLayoutGroup.LayoutMethod.GroupAsCircle)]
    [Range(-180, 180)] [SerializeField] private float _degreeOffset;

    [Serializable]
    public class TargetPositionOrTransform
    {
        
        [AllowNesting][ShowIf("showTargetPosition")]
        [SerializeField] internal Vector2 targetPosition;
        [AllowNesting][Label("Target Position")]
        [SerializeField] internal Transform targetTransform;
        
        private bool showTargetPosition => targetTransform == null;
        
        public Vector2 TargetPosition => targetTransform == null ? targetPosition : targetTransform.position;
        
    }
    
    [ShowIf("LayoutMethod", CircularLayoutGroup.LayoutMethod.RestrictToCircle)]
    public TargetPositionOrTransform targetPositionOrTransform;
    
    public float Radius => overrideElement ? _slideRadius : 1;
    
    public float DegreeOffset => overrideElement ? _degreeOffset : 0;

    private float _currentRadius;
    
    public Vector3 TargetPosition => overrideElement ? targetPositionOrTransform.TargetPosition : Vector2.zero;
    
    private CircularLayoutGroup _circularLayoutGroup;


    private CircularLayoutGroup.LayoutMethod LayoutMethod => _circularLayoutGroup.layoutMethod;
    
    private void Update()
    {
        _circularLayoutGroup ??= GetComponentInParent<CircularLayoutGroup>();
        
        if (_circularLayoutGroup == null) return;
        
        _circularLayoutGroup.ArrangeElements();
    }


    private void OnValidate()
    {
        
        _circularLayoutGroup ??= GetComponentInParent<CircularLayoutGroup>();
        
        if (_circularLayoutGroup == null) return;

        if (_circularLayoutGroup.layoutMethod == CircularLayoutGroup.LayoutMethod.GroupAsCircle)
        {
            _circularLayoutGroup.ArrangeElements();
        }
        
        if (targetPositionOrTransform.targetTransform == this.transform ||
            targetPositionOrTransform.targetTransform == _circularLayoutGroup.transform) targetPositionOrTransform.targetTransform = null;
        
    }

    public void OnInstantiate(Transform instantiator)
    {
        targetPositionOrTransform.targetTransform = instantiator;
    }
    
}
