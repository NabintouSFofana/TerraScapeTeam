using System.Collections.Generic;
using UnityEngine;

// Cubic Bezier curve math.
// Owner: Anisha (camera controls).

public static class Bezier
{
    public static Vector3 Point(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        float tuTriple = 3f * t * u;
        float c0 = u * u * u;
        float c1 = tuTriple * u;
        float c2 = tuTriple * t;
        float c3 = t * t * t;
        return c0 * p0 + c1 * p1 + c2 * p2 + c3 * p3;
    }

    public static Vector3 Velocity(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float a = 3f * (t - 1f) * (t - 1f);
        float b = 3f * (3f * t - 1f) * (t - 1f);
        float c = -3f * t * (3f * t - 2f);
        float d = 3f * t * t;
        return a * p0 + b * p1 + c * p2 + d * p3;
    }

    static Vector3 Middle(Vector3 a, Vector3 b) => (a + b) * 0.5f;

    public static void Subdivide(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
                                 List<Vector3> outPoints, float tolerance = 0.25f,
                                 int depth = 0)
    {
        if (Vector3.Distance(p0, p3) <= tolerance || depth > 16)
        {
            outPoints.Add(p3);
            return;
        }

        Vector3 a  = Middle(p0, p1);
        Vector3 b  = Middle(p3, p2);
        Vector3 c  = Middle(p1, p2);
        Vector3 a1 = Middle(a, c);
        Vector3 b1 = Middle(b, c);
        Vector3 c1 = Middle(a1, b1);

        Subdivide(p0, a, a1, c1, outPoints, tolerance, depth + 1);
        Subdivide(c1, b1, b, p3, outPoints, tolerance, depth + 1);
    }
}