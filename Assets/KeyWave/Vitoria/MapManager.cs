using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{

    public GameObject DriveMode;
    public GameObject MapMode;

    public void TurnOnDriveMode()
    {
        DriveMode.SetActive(!DriveMode.active);
        MapMode.SetActive(!DriveMode.active);
    }

}
