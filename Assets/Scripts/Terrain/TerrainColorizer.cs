using UnityEngine;

// Tints the terrain mesh with per-vertex colors based on height and slope:
// sand near/under water, grass on gentle mid ground, rock on steep slopes, snow up high.
// Owner: Loi (terrain system).

[RequireComponent(typeof(TerrainMesh), typeof(MeshFilter))]
public class TerrainColorizer : MonoBehaviour
{
    public Color sand  = new Color(0.76f, 0.70f, 0.50f);
    public Color grass = new Color(0.36f, 0.55f, 0.27f);
    public Color rock  = new Color(0.42f, 0.40f, 0.38f);
    public Color snow  = new Color(0.95f, 0.95f, 0.97f);

    [Tooltip("Height above the water line still counted as sandy shore.")]
    public float sandBand = 1.5f;
    [Tooltip("Slope (0 flat .. 1 vertical) above which we show rock.")]
    public float rockSlope = 0.5f;
    [Tooltip("Height above which we show snow.")]
    public float snowHeight = 22f;

    // Recolor the whole terrain. Pass the current water level so the shoreline lines up.
    public void Recolor(float waterLevel)
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null) return;

        Vector3[] verts = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Color[] colors = new Color[verts.Length];

        for (int i = 0; i < verts.Length; i++)
        {
            float h = verts[i].y;
            float slope = 1f - Mathf.Clamp01(normals[i].y);   // 0 = flat, 1 = vertical

            Color c;
            if (h < waterLevel + sandBand) c = sand;
            else if (slope > rockSlope)    c = rock;
            else if (h > snowHeight)        c = snow;
            else                            c = grass;

            colors[i] = c;
        }

        mesh.colors = colors;
    }
}
