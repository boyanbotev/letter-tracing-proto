using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[System.Serializable]
public class Stroke : MonoBehaviour
{
    public SplineContainer spline;
    public BaseLine line;
    public bool shouldLoopAtEnd = false;
}
