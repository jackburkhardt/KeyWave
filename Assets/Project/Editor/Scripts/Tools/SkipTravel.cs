using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PixelCrushers.DialogueSystem;

public class SkipTravel
{

    [MenuItem("Tools/Perils and Pitfalls/Game/SkipTravel/Hotel")]
    private static void Hotel()
    {
         Location.FromString("Hotel").MoveHere();
    }
    
    [MenuItem("Tools/Perils and Pitfalls/Game/SkipTravel/Café")]
    private static void Café()
    {
        Location.FromString("Café").MoveHere();
    }
    
    [MenuItem("Tools/Perils and Pitfalls/Game/SkipTravel/Store")]
    private static void Store()
    {
        Location.FromString("Store").MoveHere();
    }
    
    [MenuItem("Tools/Perils and Pitfalls/Game/SkipTravel/Mall")]
    private static void Mall()
    {
        Location.FromString("Mall").MoveHere();
    }
    [MenuItem("Tools/Perils and Pitfalls/Game/SkipTravel/Island")]
    private static void Island()
    {
        Location.FromString("Island").MoveHere();
    }
    
    [MenuItem("Tools/Perils and Pitfalls/Game/SkipTravel/Park")]
    private static void Park()
    {
        Location.FromString("Park").MoveHere();
    }
    
    [MenuItem("Tools/Perils and Pitfalls/Game/SkipTravel/Neighborhood")]
    private static void Neighborhood()
    {
        Location.FromString("Neighborhood").MoveHere();
    }
    
    [MenuItem("Tools/Perils and Pitfalls/Game/SkipTravel/Bar")]
    private static void Bar()
    {
        Location.FromString("Bar").MoveHere();
    }
    
    
    
    
}
    
