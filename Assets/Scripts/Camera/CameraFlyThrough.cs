using System.Collections.Generic;
using UnityEngine;

// Cinematic camera flythrough along a cubic Bezier curve.
// Owner: Anisha (camera controls).

public class CameraFlythrough : MonoBehaviour
{
    [Tooltip("The four control points. P0 and P3 are the endpoints; P1 and P2 shape the curve.")]
    public Transform p0, p1, p2, p3;

    [Tooltip("Seconds to travel the whole curve.")]
    public float duration = 12f;

    [Tooltip("Play the flythrough automatically on start.")]
    public bool playOnStart = false;

    [Tooltip("Disable this while flying (so WASD doesn't fight the animation).")]
    public FlyCamera manualCamera;

    bool playing;
    float t;

    void Start()
    {
        if (playOnStart) Play();
    }

    public void Play()
    {
        if (!HasPoints()) { Debug.LogWarning("[Flythrough] Assign all four control points."); return; }
        t = 0f;
        playing = true;
        if (manualCamera != null) manualCamera.enabled = false;
    }

    public void Stop()
    {
        playing = false;
        if (manualCamera != null) manualCamera.enabled = true;
    }

    bool HasPoints() => p0 != null && p1 != null && p2 != null && p3 != null;

    void Update()
    {
        if (!playing) return;

        t += Time.deltaTime / Mathf.Max(0.01f, duration);
        if (t >= 1f) { t = 1f; playing = false; if (manualCamera != null) manualCamera.enabled = true; }

        Vector3 pos = Bezier.Point(p0.position, p1.position, p2.position, p3.position, t);
        Vector3 vel = Bezier.Velocity(p0.position, p1.position, p2.position, p3.position, t);

        transform.position = pos;
        if (vel.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(vel.normalized, Vector3.up);
    }

    // Draws the path in the Scene view
    void OnDrawGizmos()
    {
        if (!HasPoints()) return;

        var pts = new List<Vector3> { p0.position };
        Bezier.Subdivide(p0.position, p1.position, p2.position, p3.position, pts);

        Gizmos.color = Color.cyan;
        for (int i = 1; i < pts.Count; i++)
            Gizmos.DrawLine(pts[i - 1], pts[i]);

        // Control polygon
        Gizmos.color = Color.grey;
        Gizmos.DrawLine(p0.position, p1.position);
        Gizmos.DrawLine(p1.position, p2.position);
        Gizmos.DrawLine(p2.position, p3.position);
    }
}