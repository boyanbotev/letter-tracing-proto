using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapesLine : BaseLine
{
    private Polyline line;

    private void Awake()
    {
        line = GetComponent<Polyline>();
    }

    public override void AddPoint(Vector3 point)
    {
        line.AddPoint(point);
    }

    public override void SetClosed(bool closed)
    {
        line.Closed = closed;
    }
}
