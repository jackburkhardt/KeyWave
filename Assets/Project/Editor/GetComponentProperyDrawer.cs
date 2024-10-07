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
        
       

        EditorGUI.EndProperty();
        
        property.serializedObject.ApplyModifiedProperties();
        property.serializedObject.Update();
    }
    
    private bool TryGetNullComponent(SerializedProperty property, out object component, out GameObject gameObject)
    {
        gameObject = property.serializedObject.targetObject.GameObject();
        component = fieldInfo.GetValue(property.serializedObject.targetObject);
        return component.IsUnityNull();
    }
    
    private bool TryInjectComponent(SerializedProperty property, GameObject gameObject, object component)
    {
        Component getComponent = null;
        if (component != null) getComponent = gameObject.GetComponent(component.GetType());
        getComponent ??= gameObject.GetComponent(fieldInfo.FieldType);
        if (getComponent != null) fieldInfo.SetValue(property.serializedObject.targetObject, getComponent);
        return getComponent != null;
    }
    
    private bool ComponentMatchesGetComponent(object component, GameObject gameObject)
    {
        return component != null && !component.IsUnityNull() && ReferenceEquals(component, gameObject.GetComponent(component.GetType()));
    }
    
    
    
    
}
}


