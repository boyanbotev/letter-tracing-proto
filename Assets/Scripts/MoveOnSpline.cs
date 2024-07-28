using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System;
using System.Linq;

public class MoveOnSpline : MonoBehaviour
{
    [SerializeField] private SplineContainer[] splines;
    private SplineContainer currentSpline;
    public BaseLine[] lines;
    BaseLine currentLine;

    private int currentSplineIndex = 0;
    private bool isActive = false;
    private bool isCompleted = false;
    private float lastT = 0;
    private float lastEnactedT = 0;
    private float maxDifference = 0.02f;
    private float minDifference = 0.01f;
    private float finishPercentage = 0.90f;
    [SerializeField] private List<Vector3> traversedPoints;
    [SerializeField] bool shouldLoopAtEnd = false;

    private void Awake()
    {
        traversedPoints = new();
        currentSpline = splines[currentSplineIndex];
        currentLine = lines[currentSplineIndex];
        AddLinePosition(transform.position);
    }

    void Update() {

        if (isCompleted) return;

        AddTraversedKnots();

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

        SplineUtility.GetNearestPoint(currentSpline.Spline, transform.position, out float3 _, out float t);
        
        lastT = t;

        HandleCompletion(t);
    }

    // TODO: use touch
    // remove artifacts
    // refactor

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

        SplineUtility.GetNearestPoint(currentSpline.Spline, mousePos, out float3 nearest, out float t);

        float difference = GetDifference(t, lastT);

        if (t > lastT && difference < 0.5)
        {
            Vector3 newPos = new Vector3(nearest.x, nearest.y, 0);
            transform.position = newPos;

            DrawLine(newPos, difference, t);
        }

        if (difference > minDifference) lastEnactedT = t;
    }

    float GetDifference(float a, float b)
    {
        return Math.Abs(a - b);
    }

    void DrawLine(Vector3 newPos, float difference, float t)
    {
        if (difference < maxDifference && difference > minDifference)
        {
            AddLinePosition(newPos);
        }
        else if (difference > maxDifference)
        {
            float tempT = lastT;

            while (tempT < t)
            {
                tempT += maxDifference;

                Vector3 pos = SplineUtility.EvaluatePosition(currentSpline.Spline, tempT);
                Vector3 adaptedPos = new(pos.x, pos.y, 0);
                transform.position = adaptedPos;
                AddLinePosition(adaptedPos);
            }
        }
    }

    bool HasTraversedAllKnots()
    {
        foreach (var knot in currentSpline.Spline.Knots)
        {
            if (!traversedPoints.Contains(knot.Position)) return false;
        }
        return true;
    }

    void AddLinePosition(Vector3 pos)
    {
        Vector3 adaptedPos = new(pos.x, pos.y, 0);
        currentLine.AddPoint(adaptedPos);
    }

    void AddTraversedKnots()
    {
        var closestKnot = currentSpline.Spline.Knots
            .OrderBy(knot => Vector2.Distance(new Vector2(knot.Position.x, knot.Position.y), transform.position))
            .FirstOrDefault();

        if (!traversedPoints.Contains(closestKnot.Position))
        {
            traversedPoints.Add(closestKnot.Position);
        }
    }

    void HandleCompletion(float t)
    {
        if (t >= finishPercentage && HasTraversedAllKnots())
        {
            currentSplineIndex++;

            if (currentSplineIndex >= splines.Length)
            {
                isCompleted = true;
                if (shouldLoopAtEnd) currentLine.SetClosed(true);
                gameObject.SetActive(false);
                Debug.Log("IS COMPLETED");
            }
            else
            {
                lastT = 0;
                lastEnactedT = 0;
                currentSpline = splines[currentSplineIndex];
                currentLine = lines[currentSplineIndex];
                Vector3 pos = SplineUtility.EvaluatePosition(currentSpline.Spline, 0);
                transform.position = new Vector3(pos.x, pos.y, 0);
            }
        }
    }
}
