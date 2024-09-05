using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using NaughtyAttributes.Editor;
using Project.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.UIElements;

namespace Project.Editor
{
    

[CustomPropertyDrawer(typeof(GetComponentAttribute))]
public class GetComponentProperyDrawer : PropertyDrawerBase
{
    
    protected override float GetPropertyHeight_Internal(SerializedProperty property, GUIContent label)
    {
        return GetPropertyHeight(property);
    }

    protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(rect, label, property);

        Rect propertyRect = new Rect()
        {
            x = rect.x,
            y = rect.y,
            width = rect.width,
            height = EditorGUIUtility.singleLineHeight
        };
        
        property.serializedObject.Update();

        var trueLabel = label;
        
        if (TryGetNullComponent(property, out var component, out var gameObject))
        {
            TryInjectComponent(property, gameObject, component);
        }

        else
        {   
            if (ComponentMatchesGetComponent(component, gameObject))
            {
                label = new GUIContent( $"{label.text} (Auto-Injected)");
            }
        }
       
        EditorGUI.PropertyField(propertyRect, property, label);
        
        property.serializedObject.ApplyModifiedProperties();

        EditorGUI.EndProperty();
    }
    
    private bool TryGetNullComponent(SerializedProperty property, out object component, out GameObject gameObject)
    {
        
        component = fieldInfo.GetValue(property.serializedObject.targetObject);
        gameObject = component != null ? property.serializedObject.targetObject.GameObject() : null;
        return component.IsUnityNull();
    }
    
    private bool TryInjectComponent(SerializedProperty property, GameObject gameObject, object component)
    {
        var getComponent = gameObject.GetComponent(component.GetType());
        if (getComponent != null) fieldInfo.SetValue(property.serializedObject.targetObject, getComponent);
        return getComponent != null;
    }
    
    private bool ComponentMatchesGetComponent(object component, GameObject gameObject)
    {
        return ReferenceEquals(component, gameObject.GetComponent(component.GetType()));
    }
    
    
    
    
}
}


