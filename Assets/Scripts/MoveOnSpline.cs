using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System;
using UnityEngine.UIElements;

public class MoveOnSpline : MonoBehaviour
{
    [SerializeField] private SplineContainer spline;
    [SerializeField] private LineRenderer lineRenderer;

    private bool isActive = false;
    private bool isCompleted = false;
    private float lastT = 0;
    private float maxDifference = 0.02f;
    private float finishPerecentage = 0.98f;
    [SerializeField] bool shouldLoopAtEnd = false;

    private void Awake()
    {
        Vector3 pos = SplineUtility.EvaluatePosition(spline.Spline, 0);
        transform.position = new Vector3(pos.x, pos.y, 0);
        AddLinePosition(transform.position);
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
        }

        SplineUtility.GetNearestPoint(spline.Spline, transform.position, out float3 _, out float t);

        if (t >= finishPerecentage)
        {
            isCompleted = true;
            if (shouldLoopAtEnd) lineRenderer.loop = true;
            gameObject.SetActive(false);
            Debug.Log("IS COMPLETED");
        }

        lastT = t;
    }

    // TODO: use touch
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
    /// Move node to nearest point in spine if we are going the right direction around the spine.
    /// </summary>
    void HandleMouseHold()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        SplineUtility.GetNearestPoint(spline.Spline, mousePos, out float3 nearest, out float t);

        float difference = GetDifference(t, lastT);

        if (t > lastT && difference < 0.5)
        {
            Vector3 newPos = new Vector3(nearest.x, nearest.y, 0);
            transform.position = newPos;

            DrawLine(newPos, difference, t);
        }
    }

    float GetDifference(float a, float b)
    {
        return Math.Abs(a - b);
    }

    void DrawLine(Vector3 newPos, float difference, float t)
    {
        if (difference < maxDifference)
        {
            AddLinePosition(newPos);
        }
        else if (difference > maxDifference)
        {
            float tempT = lastT;

            while (tempT < t)
            {
                tempT += maxDifference;

                Vector3 pos = SplineUtility.EvaluatePosition(spline.Spline, tempT);
                Vector3 adaptedPos = new Vector3(pos.x, pos.y, 0);
                transform.position = adaptedPos;
                AddLinePosition(adaptedPos);
            }
        }
    }

    void AddLinePosition(Vector3 pos)
    {
        lineRenderer.positionCount++;
        Vector3 adaptedPos = new Vector3(pos.x, pos.y, 0);
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, adaptedPos);
    }
}
