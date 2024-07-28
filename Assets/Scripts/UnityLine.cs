using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityLine : BaseLine
{
    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public override void AddPoint(Vector3 point)
    {
        line.positionCount++;
        line.SetPosition(line.positionCount - 1, point);
    }

    public override void SetClosed(bool closed)
    {
        line.loop = closed;
    }
}
