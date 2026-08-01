using UnityEngine;

// Builds and maintains a square grid terrain mesh from a heightmap.
// Owner: Loi (terrain system).

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TerrainMesh : MonoBehaviour
{
    [Tooltip("Number of quads per side. Higher = smoother but heavier.")]
    public int resolution = 128;

    [Tooltip("World size of the terrain in meters (square).")]
    public float size = 100f;

    // Heightmap, indexed [x, z], sized (resolution+1) x (resolution+1).
    public float[,] heights;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    MeshCollider meshCollider;

    public float StepSize => size / resolution;

    void Awake()
    {
        int n = resolution + 1;
        if (heights == null) heights = new float[n, n];
        Build();
    }

    // Creates the mesh from scratch based on the current heightmap.
    public void Build()
    {
        int n = resolution + 1;

        mesh = new Mesh { name = "TerrainMesh" };
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // support >65k verts
        GetComponent<MeshFilter>().mesh = mesh;
        meshCollider = GetComponent<MeshCollider>();

        vertices = new Vector3[n * n];
        uvs = new Vector2[n * n];
        triangles = new int[resolution * resolution * 6];

        float step = StepSize;
        for (int z = 0; z < n; z++)
        {
            for (int x = 0; x < n; x++)
            {
                int i = z * n + x;
                vertices[i] = new Vector3(x * step, heights[x, z], z * step);
                uvs[i] = new Vector2((float)x / resolution, (float)z / resolution);
            }
        }

        int t = 0;
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = z * n + x;
                triangles[t++] = i;
                triangles[t++] = i + n;
                triangles[t++] = i + 1;
                triangles[t++] = i + 1;
                triangles[t++] = i + n;
                triangles[t++] = i + n + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshCollider.sharedMesh = mesh;
    }

    // Push the current heightmap into the mesh and refresh normals + collider.
    // Call this after any edit to the heights array.
    public void ApplyHeights()
    {
        if (vertices == null) { Build(); return; }

        int n = resolution + 1;
        for (int z = 0; z < n; z++)
            for (int x = 0; x < n; x++)
                vertices[z * n + x].y = heights[x, z];

        mesh.vertices = vertices;
        // keeps lighting correct while sculpting 
        mesh.RecalculateNormals();   
        mesh.RecalculateBounds();
        // force the collider to rebuild
        meshCollider.sharedMesh = null;   
        meshCollider.sharedMesh = mesh;
    }

    // Convert a world-space point to the nearest grid cell.
    public bool WorldToGrid(Vector3 world, out int gx, out int gz)
    {
        Vector3 local = transform.InverseTransformPoint(world);
        gx = Mathf.RoundToInt(local.x / StepSize);
        gz = Mathf.RoundToInt(local.z / StepSize);
        return gx >= 0 && gx <= resolution && gz >= 0 && gz <= resolution;
    }

    // Height of the terrain at a grid cell (used by water, objects, texturing).
    public float HeightAt(int gx, int gz)
    {
        gx = Mathf.Clamp(gx, 0, resolution);
        gz = Mathf.Clamp(gz, 0, resolution);
        return heights[gx, gz];
    }
}
