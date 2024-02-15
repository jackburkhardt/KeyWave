using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelObjective : MonoBehaviour
{
    [SerializeField] private Text _objectiveText;
    
    public void SetObjectiveText(string text)
    {
        
        _objectiveText.text = text;
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
