using UnityEngine;

// A flat water surface. Moving it up floods low parts of the terrain into lakes.
// Owner: Syed (environment).

public class WaterPlane : MonoBehaviour
{
    public float level = 3f;

    void Start()
    {
        SetLevel(level);
    }

    // Set the water height
    public void SetLevel(float y)
    {
        level = y;
        Vector3 p = transform.position;
        p.y = y;
        transform.position = p;
    }
}