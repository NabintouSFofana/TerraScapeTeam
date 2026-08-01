using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TerrainGrid : MonoBehaviour
{
    [Header("Grid settings")]
    [Min(2)] public int width = 65;
    [Min(2)] public int depth = 65;
    [Min(0.1f)] public float cellSize = 1f;

    [Header("Starting appearance")]
    public Color startingColor = new Color(0.20f, 0.55f, 0.18f);

    private Mesh mesh;
    private MeshCollider meshCollider;

    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colors;

    private bool rebuildRequested;

    void OnEnable()
    {
        RequestRebuild();
    }

    void OnValidate()
    {
        width = Mathf.Max(2, width);
        depth = Mathf.Max(2, depth);
        cellSize = Mathf.Max(0.1f, cellSize);

        RequestRebuild();
    }

    void Update()
    {
        if (!rebuildRequested)
            return;

        rebuildRequested = false;
        BuildGrid();
    }

    void RequestRebuild()
    {
        if (!gameObject.activeInHierarchy)
            return;

        rebuildRequested = true;
    }

    public void BuildGrid()
    {
        width = Mathf.Max(2, width);
        depth = Mathf.Max(2, depth);

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        Mesh oldMesh = meshFilter.sharedMesh;

        mesh = new Mesh();
        mesh.name = "Editable Terrain Mesh";

        if (width * depth > 65535)
        {
            mesh.indexFormat =
                UnityEngine.Rendering.IndexFormat.UInt32;
        }

        CreateVertices();
        CreateTriangles();

        meshFilter.sharedMesh = mesh;

        UpdateMesh();

        if (oldMesh != null && oldMesh != mesh)
        {
            if (Application.isPlaying)
            {
                Destroy(oldMesh);
            }
            else
            {
                DestroyImmediate(oldMesh);
            }
        }
    }

    void CreateVertices()
    {
        vertices = new Vector3[width * depth];
        colors = new Color[vertices.Length];

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = Index(x, z);

                vertices[index] = new Vector3(
                    x * cellSize,
                    0f,
                    z * cellSize
                );

                colors[index] = startingColor;
            }
        }
    }

    void CreateTriangles()
    {
        triangles =
            new int[(width - 1) * (depth - 1) * 6];

        int triangleIndex = 0;

        for (int z = 0; z < depth - 1; z++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int bottomLeft = Index(x, z);
                int bottomRight = Index(x + 1, z);
                int topLeft = Index(x, z + 1);
                int topRight = Index(x + 1, z + 1);

                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = topRight;

                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topRight;
                triangles[triangleIndex++] = bottomRight;
            }
        }
    }

    public void RecalculateMesh()
    {
        UpdateMesh();
    }

    void UpdateMesh()
    {
        if (mesh == null ||
            vertices == null ||
            triangles == null ||
            colors == null)
        {
            return;
        }

        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (meshCollider == null)
        {
            meshCollider = GetComponent<MeshCollider>();
        }

        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    public float GetHeight(int x, int z)
    {
        if (!IsValidCell(x, z))
            return 0f;

        return vertices[Index(x, z)].y;
    }

    public void SetHeight(int x, int z, float height)
    {
        if (!IsValidCell(x, z))
            return;

        int index = Index(x, z);

        Vector3 vertex = vertices[index];
        vertex.y = height;
        vertices[index] = vertex;
    }

    public Color GetVertexColor(int x, int z)
    {
        if (!IsValidCell(x, z))
            return startingColor;

        return colors[Index(x, z)];
    }

    public void SetVertexColor(int x, int z, Color color)
    {
        if (!IsValidCell(x, z))
            return;

        colors[Index(x, z)] = color;
    }

    public bool WorldToGrid(
        Vector3 worldPoint,
        out float gridX,
        out float gridZ
    )
    {
        Vector3 localPoint =
            transform.InverseTransformPoint(worldPoint);

        gridX = localPoint.x / cellSize;
        gridZ = localPoint.z / cellSize;

        return
            gridX >= 0f &&
            gridX <= width - 1 &&
            gridZ >= 0f &&
            gridZ <= depth - 1;
    }

    // Flatten() removed from here - TerrainSculptBrush.FlattenTerrain() (F key)
    // does the same thing and also resets vertex colors back to grass, so this
    // was dead/duplicate code.

    bool IsValidCell(int x, int z)
    {
        return
            vertices != null &&
            x >= 0 &&
            x < width &&
            z >= 0 &&
            z < depth;
    }

    int Index(int x, int z)
    {
        return z * width + x;
    }
}
