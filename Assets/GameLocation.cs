using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[CreateAssetMenu(fileName = "New Location", menuName = "Location")]
public class GameLocation : ScriptableObject
{

    public GameManager.Region region;
    public GameManager.Locations location;
    public string description;
    public Sprite pin;
    public Color buttonTint;
    public List<Scene> scenes;
    public List<string> objectives;
    public Vector2 coordinates;
    public bool isUnlocked;
    
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
