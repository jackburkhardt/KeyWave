using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapButton : MonoBehaviour
{
    private enum Side
    {
        Left,
        Right,
        Bottom,
        Top
    };

    [SerializeField] private Image image;

    [SerializeReference] private Side sideOnWatch;
    
    // Start is called before the first frame update
    void Start()
    {
        if (image == null) image = GetComponent<Image>();
    }


    // Update is called once per frame
    void Update()
    {
        // get all map buttons with the same sideOnWatch
        
        var sameSideButtons = new List<MapButton>();
        
        foreach (var mapButton in FindObjectsOfType<MapButton>())
        {
            if (mapButton.sideOnWatch == sideOnWatch) sameSideButtons.Add(mapButton);
        }
        
        sameSideButtons.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
        
        // get the index of the current button
        
        var currentIndex = sameSideButtons.IndexOf(this);

        var zAngle = (sideOnWatch == Side.Left ? 180f : 0f) + (currentIndex + 1) * 180f / (sameSideButtons.Count + 1);
        var angleOffset = image.fillAmount * 360 / 2f;
        
        transform.rotation = Quaternion.Euler(0, 0, zAngle - angleOffset);
        // sort all buttons on the same side by their child index
    }
}
