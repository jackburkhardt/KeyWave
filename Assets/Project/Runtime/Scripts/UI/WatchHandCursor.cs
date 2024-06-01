using UnityEngine;

public class WatchHandCursor : MonoBehaviour
{

    [SerializeField] private float minimumDistance = 750;
    public bool isMouseOver = false;
    private static bool isFrozen = false;
    public bool stickToWatchTicks = false;
    
    
    public float AngleCenteredSouth => transform.rotation.eulerAngles.z - 270;

    public void OnEnable()
    {
       Unfreeze();
    }


    public static void Freeze()
    {
        isFrozen = true;
    }

    public static void Unfreeze()
    {
       isFrozen = false;
    }

    // Update is called once per frame
    void Update()
    {
       if (isFrozen) return;
       // rotate self to point at the mouse cursor
        
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 0;
        
        Vector3 screenPos = transform.position;
        
        float distance = Vector3.Distance(screenPos, mousePos);
        
        if (distance > minimumDistance && minimumDistance != 0)
        {
            isMouseOver = false;
            return;
        }

        isMouseOver = true;

        float angle = Mathf.Atan((mousePos.y - screenPos.y) / (mousePos.x - screenPos.x));
        
        if (mousePos.x < screenPos.x) angle += Mathf.PI;
        
       
        angle *= Mathf.Rad2Deg;
        
       // if (mousePos.y > screenPos.y) angle 

       if (stickToWatchTicks)
       {
           var minAngle = angle - (angle % 6);
           var maxAngle = (minAngle + 6);
           angle = Mathf.RoundToInt((angle - minAngle < maxAngle - angle) ? minAngle : maxAngle);
       }

        

        
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(0, 0, (angle))), Time.deltaTime * 20);
        
       
       
    }
}
