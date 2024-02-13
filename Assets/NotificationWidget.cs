using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class NotificationWidget : MonoBehaviour
{


    [SerializeField] private TMPro.TMP_Text _taskText, _etaText;
    public GameManager.Hour hour;
    public GameManager.Minute minute;

    public string task;
    public GameManager.Locations location;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _taskText.text = $"{task} at the {location} at {hour}:{minute}";
        _etaText.text = $"Earliest ETA: {GameManager.instance.GetEtaToLocation(location)}";
    }
}
