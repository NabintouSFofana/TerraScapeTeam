using UnityEngine;

// Raise / lower / smooth the terrain with the mouse. Left-click and drag on the
// ground. Brush size and strength are adjustable 
// force the collider to rebuild

[RequireComponent(typeof(TerrainMesh))]
public class TerrainSculptor : MonoBehaviour
{
    public enum Tool { Raise, Lower, Smooth }
    public Tool tool = Tool.Raise;

    [Tooltip("Brush radius in world meters.")]
    public float brushRadius = 5f;

    [Tooltip("How fast the ground moves at the brush center (meters/second).")]
    public float strength = 15f;

    public Camera cam;

    TerrainMesh terrain;

    void Awake()
    {
        terrain = GetComponent<TerrainMesh>();
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (!Input.GetMouseButton(0)) return;

        // Ignore clicks that land on a UI element (so buttons don't sculpt).
        var es = UnityEngine.EventSystems.EventSystem.current;
        if (es != null && es.IsPointerOverGameObject()) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            ApplyBrush(hit.point);
    }

    void ApplyBrush(Vector3 center)
    {
        if (!terrain.WorldToGrid(center, out int cx, out int cz)) return;

        int res = terrain.resolution;
        int r = Mathf.CeilToInt(brushRadius / terrain.StepSize);
        float amount = strength * Time.deltaTime;

        for (int z = Mathf.Max(0, cz - r); z <= Mathf.Min(res, cz + r); z++)
        {
            for (int x = Mathf.Max(0, cx - r); x <= Mathf.Min(res, cx + r); x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (z - cz) * (z - cz)) * terrain.StepSize;
                if (dist > brushRadius) continue;

                // Smooth cosine falloff: full strength at center, 0 at the edge.
                float falloff = Mathf.Cos(dist / brushRadius * Mathf.PI * 0.5f);

                if (tool == Tool.Raise)
                    terrain.heights[x, z] += amount * falloff;
                else if (tool == Tool.Lower)
                    terrain.heights[x, z] -= amount * falloff;
                else // Smooth: pull each vertex toward the average of its neighbors.
                {
                    float avg = 0f; int c = 0;
                    if (x > 0)   { avg += terrain.heights[x - 1, z]; c++; }
                    if (x < res) { avg += terrain.heights[x + 1, z]; c++; }
                    if (z > 0)   { avg += terrain.heights[x, z - 1]; c++; }
                    if (z < res) { avg += terrain.heights[x, z + 1]; c++; }
                    avg /= c;
                    terrain.heights[x, z] = Mathf.Lerp(terrain.heights[x, z], avg, falloff);
                }
            }
        }

        terrain.ApplyHeights();
    }

    // Called by the UI later to switch tools.
    public void SetTool(int t) => tool = (Tool)t;
    public void SetBrushRadius(float v) => brushRadius = v;
    public void SetStrength(float v) => strength = v;
}
