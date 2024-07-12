using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System;

public class MoveOnSpline : MonoBehaviour
{
    [SerializeField] private SplineContainer spline;

    private bool isActive = false;
    private bool isCompleted = false;
    private float lastT = 0;
    private Vector3 initialPos;

    private void Awake()
    {
        initialPos = transform.position = SplineUtility.EvaluatePosition(spline.Spline, 0);
    }

    void Update() {

        if (isCompleted) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }
        else if (Input.GetMouseButton(0) && isActive)
        {
            HandleMouseHold();
        } 
        else if (isActive)
        {
            isActive = false;
            transform.position = initialPos;
        }

        SplineUtility.GetNearestPoint(spline.Spline, transform.position, out float3 _, out float t);

        if (t >= 0.99)
        {
            isCompleted = true;
            Debug.Log("IS COMPLETED");
        }

        lastT = t;
    }

    // TODO: use touch
    // (animate) back to starting position if is not completed
    // complete if reach end of spline
    // draw line filling in shape
    // handle multiple shapes

    /// <summary>
    /// Check if clicking on node object to determine whether we are active.
    /// </summary>
    void HandleMouseDown()
    {
        var rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
        if (rayHit.collider && rayHit.collider.gameObject == gameObject)
        {
            isActive = true;
        }
    }

    /// <summary>
    /// Move node nearest knot in spine if we are going the right direction around the spine.
    /// </summary>
    void HandleMouseHold()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        SplineUtility.GetNearestPoint(spline.Spline, mousePos, out float3 nearest, out float t);

        if (t > lastT && GetDifference(t, lastT) < 0.5)
        {
            Debug.Log("greater than last time" + t + lastT);
            transform.position = nearest;
        }
    }

    float GetDifference(float a, float b)
    {
        return Math.Abs(a - b);
    }
}
