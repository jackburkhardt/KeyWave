using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseArcFromBezier : MonoBehaviour
{
    private BezierRenderer bezierRenderer;
    public Transform target;
    public float zDepth;

    
    // Start is called before the first frame update
    void Start()
    {
        bezierRenderer = GetComponent<BezierRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {
        float midY = bezierRenderer.p0.y + 0.5f;
    
        bezierRenderer.p0 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bezierRenderer.p2 = target.position;
        bezierRenderer.p0 = new Vector3(bezierRenderer.p0.x, bezierRenderer.p0.y, 0);
        bezierRenderer.p1 = new Vector3((bezierRenderer.p0.x + bezierRenderer.p2.x) * 0.5f, bezierRenderer.p0.y + 0.5f, 0);




        bezierRenderer.p0 = new Vector3(bezierRenderer.p0.x, bezierRenderer.p0.y, zDepth);
        bezierRenderer.p1 = new Vector3(bezierRenderer.p1.x, bezierRenderer.p1.y, zDepth);
        bezierRenderer.p2 = new Vector3(bezierRenderer.p2.x, bezierRenderer.p2.y, zDepth);




    }
}
