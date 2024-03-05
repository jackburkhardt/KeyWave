using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierRenderer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public Vector3 p0;
    public Vector3 p1;
    public Vector3 p2;
    public Material lineMAT;

    public float lineWidth = 0.15f;

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = lineMAT;
        lineRenderer.widthMultiplier = lineWidth;
        lineRenderer.rendererPriority = 2;
        
    }

    void Update()
    {
        DrawQuadraticBezierCurve(p0, p1, p2);
    }

    void DrawQuadraticBezierCurve(Vector3 point0, Vector3 point1, Vector3 point2)
    {
        lineRenderer.positionCount = 200;
        float t = 0f;
        Vector3 B = new Vector3(0, 0, 0);
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            B = (1 - t) * (1 - t) * point0 + 2 * (1 - t) * t * point1 + t * t * point2;
            lineRenderer.SetPosition(i, B);
            t += (1 / (float)lineRenderer.positionCount);
        }
    }
}