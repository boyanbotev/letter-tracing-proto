using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System;

public class MoveOnSpline : MonoBehaviour
{
    [SerializeField] TracingLetter currentLetter;
    private bool isActive = false;
    private float lastT = 0;
    private float lastEnactedT = 0;
    private float maxDifference = 0.02f;
    private float minDifference = 0.01f;
    private float finishPercentage = 0.90f;

    private void Awake()
    {
        currentLetter.AddLinePosition(transform.position);
    }

    void Update() {

        if (currentLetter.isCompleted) return;

        currentLetter.AddTraversedKnots(transform.position);

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

        SplineUtility.GetNearestPoint(currentLetter.currentStroke.spline.Spline, transform.position, out float3 _, out float t);
        
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

        SplineUtility.GetNearestPoint(currentLetter.currentStroke.spline.Spline, mousePos, out float3 nearest, out float t);

        float difference = GetDifference(t, lastT);

        if (t > lastT && difference < 0.5)
        {
            Vector3 newPos = new(nearest.x, nearest.y, 0);
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
            currentLetter.AddLinePosition(newPos);
        }
        else if (difference > maxDifference)
        {
            float tempT = lastT;

            while (tempT < t)
            {
                tempT += maxDifference;

                Vector3 pos = SplineUtility.EvaluatePosition(currentLetter.currentStroke.spline.Spline, tempT);
                Vector3 adaptedPos = new(pos.x, pos.y, 0);
                transform.position = adaptedPos;
                currentLetter.AddLinePosition(adaptedPos);
            }
        }
    }

    void HandleCompletion(float t)
    {
        if (t >= finishPercentage && currentLetter.HasTraversedAllKnots())
        {
            currentLetter.GoToNextStroke();

            if (currentLetter.isCompleted)
            {
                gameObject.SetActive(false);
                Debug.Log("IS COMPLETED");
            }
            else
            {
                lastT = 0;
                lastEnactedT = 0;
                Vector3 pos = SplineUtility.EvaluatePosition(currentLetter.currentStroke.spline.Spline, 0);
                transform.position = new Vector3(pos.x, pos.y, 0);
            }
        }
    }
}
