using System;
using System.Collections.Generic;
using UnityEngine;

static class DebugTools
{
    public static void DrawCircle(Vector3 center, float radius, int time)
    {
        Vector3 start = center + new Vector3(Mathf.Sin(0) * radius, 0, Mathf.Cos(0) * radius);
        Vector3 end;

        for(int i = 1; i < 360; i++)
        {
            end = center + new Vector3(Mathf.Sin(i) * radius, 0, Mathf.Cos(i) * radius);
            Debug.DrawLine(start, end, Color.red, time);
            start = end;
        }
    }
}
