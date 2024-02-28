using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class MapLocationName : MonoBehaviour
{

    // Start is called before the first frame update
    TMP_Text m_TextComponent;


    private void OnEnable()
    {
        GameEvent.OnUIElementMouseHover += RevealLocationName;
        GameEvent.OnUIElementMouseExit += HideLocationName;
    }

    private void OnDisable()
    {
        GameEvent.OnUIElementMouseHover -= RevealLocationName;
        GameEvent.OnUIElementMouseExit -= HideLocationName;
    }

    private void Start()
    {
        m_TextComponent = GetComponent<TMP_Text>();
        m_TextComponent.enabled = false;
    }
    public void RevealLocationName(Transform icon)
    {
        m_TextComponent.enabled = true;
        m_TextComponent.text = icon.name;
       
    }

    public void HideLocationName(Transform icon)
    {

      m_TextComponent.enabled = false;
    }



}
