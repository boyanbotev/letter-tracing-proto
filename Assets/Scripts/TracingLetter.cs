using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TracingLetter : MonoBehaviour
{
    public Stroke[] strokes;
    public Stroke currentStroke;
    public int currentStrokeIndex = 0;
    public bool isCompleted = false;

    private List<Vector3> traversedPoints;

    private void Awake()
    {
        currentStroke = strokes[currentStrokeIndex];
        traversedPoints = new();
    }

    public void GoToNextStroke()
    {
        currentStrokeIndex++;

        if (currentStrokeIndex >= strokes.Length)
        {
            isCompleted = true;
            if (currentStroke.shouldLoopAtEnd) currentStroke.line.SetClosed(true);
            return;
        }

        currentStroke = strokes[currentStrokeIndex];
    }

    public bool HasTraversedAllKnots()
    {
        foreach (var knot in currentStroke.spline.Spline.Knots)
        {
            if (!traversedPoints.Contains(knot.Position)) return false;
        }
        return true;
    }

    public void AddTraversedKnots(Vector3 pos)
    {
        var closestKnot = currentStroke.spline.Spline.Knots
            .OrderBy(knot => Vector2.Distance(new Vector2(knot.Position.x, knot.Position.y), pos))
            .FirstOrDefault();

        if (!traversedPoints.Contains(closestKnot.Position))
        {
            traversedPoints.Add(closestKnot.Position);
        }
    }

    public void AddLinePosition(Vector3 pos)
    {
        Vector3 adaptedPos = new(pos.x, pos.y, 0);
        currentStroke.line.AddPoint(adaptedPos);
    }

}
