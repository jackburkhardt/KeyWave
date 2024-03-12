using UnityEngine;

public class MapIconManager : MonoBehaviour
{


    public GameObject[] icons;
    //if list is not 6 u die

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject icon in icons)
        {
            if (icon.GetComponent<BoxCollider2D>() == null) icon.AddComponent<BoxCollider2D>().size = new Vector2(17.56f, 19.48f);
            icon.AddComponent<MapIconMouseActions>();

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}



public class MapIconMouseActions : MonoBehaviour
{
    private void OnMouseOver()
    {
        GameEvent.UIElementMouseHover(transform);
    }

    private void OnMouseExit()
    {
        GameEvent.UIElementMouseExit(transform);
    }

    private void OnMouseDown()
    {
        GameEvent.UIElementMouseClick(transform);
    }
}
