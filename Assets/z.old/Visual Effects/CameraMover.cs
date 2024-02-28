using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{

    public static CameraMover Instance = null;

     private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        } else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void CameraTeleport(Transform destination)
    {
        Camera main = Camera.allCameras[0]; // bit faster than camera.main methinks
        Vector3 newPos = new Vector3(destination.position.x, destination.position.y, main.transform.position.z);
        main.transform.position = newPos;
    }

}
