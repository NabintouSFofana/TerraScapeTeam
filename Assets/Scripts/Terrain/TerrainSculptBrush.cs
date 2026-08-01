using UnityEngine;

[RequireComponent(typeof(TerrainGrid))]
public class TerrainSculptBrush : MonoBehaviour
{
    public enum BrushMode
    {
        Raise,
        Lower,
        Smooth
    }

    [Header("References")]
    public Camera cam;

    [Tooltip("Set this to a layer that only the terrain is on, so the brush doesn't hit trees/rocks placed on top and silently do nothing.")]
    public LayerMask terrainLayer = ~0;

    [Header("Brush Settings")]
    public BrushMode mode = BrushMode.Raise;

    [Min(0.5f)]
    public float brushRadius = 5f;

    [Min(0.1f)]
    public float brushStrength = 4f;

    [Header("Random Terrain Settings")]
    [Min(0.1f)]
    public float randomTerrainHeight = 12f;

    [Range(0.1f, 1f)]
    public float randomTerrainRoughness = 0.55f;

    public int randomSeed = -1;

    [Header("Default Terrain Color")]
    public Color defaultGrassColor =
    new Color(0.20f, 0.55f, 0.18f);

    private TerrainGrid grid;

    void Awake()
    {
        grid = GetComponent<TerrainGrid>();

        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        HandleHotkeys();

        if (
            TerrainToolState.currentTool !=
            TerrainToolState.ActiveTool.Sculpt
        )
        {
            return;
        }

        bool shiftPressed =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        // Shift + mouse wheel changes the brush size.
        if (shiftPressed)
        {
            float scroll =
                Input.GetAxis("Mouse ScrollWheel");

            if (Mathf.Abs(scroll) > 0.001f)
            {
                brushRadius = Mathf.Clamp(
                    brushRadius + scroll * 5f,
                    0.5f,
                    30f
                );
            }
        }

        if (Input.GetMouseButton(0))
        {
            ApplyBrushAtMouse();
        }
    }

    void HandleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            mode = BrushMode.Raise;

            TerrainToolState.currentTool =
                TerrainToolState.ActiveTool.Sculpt;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            mode = BrushMode.Lower;

            TerrainToolState.currentTool =
                TerrainToolState.ActiveTool.Sculpt;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            mode = BrushMode.Smooth;

            TerrainToolState.currentTool =
                TerrainToolState.ActiveTool.Sculpt;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateRandomTerrain();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            FlattenTerrain();
        }
    }

    void ApplyBrushAtMouse()
    {
        if (cam == null || grid == null)
            return;

        Ray ray =
            cam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            1000f,
            terrainLayer
        ))
        {
            return;
        }

        TerrainGrid hitGrid =
            hit.collider.GetComponentInParent<TerrainGrid>();

        if (hitGrid != grid)
            return;

        if (!grid.WorldToGrid(
            hit.point,
            out float gridX,
            out float gridZ
        ))
        {
            return;
        }

        int centerX = Mathf.RoundToInt(gridX);
        int centerZ = Mathf.RoundToInt(gridZ);

        int radiusInCells =
            Mathf.CeilToInt(
                brushRadius / grid.cellSize
            );

        for (
            int z = centerZ - radiusInCells;
            z <= centerZ + radiusInCells;
            z++
        )
        {
            for (
                int x = centerX - radiusInCells;
                x <= centerX + radiusInCells;
                x++
            )
            {
                if (
                    x < 0 ||
                    x >= grid.width ||
                    z < 0 ||
                    z >= grid.depth
                )
                {
                    continue;
                }

                float distance =
                    Vector2.Distance(
                        new Vector2(x, z),
                        new Vector2(gridX, gridZ)
                    ) * grid.cellSize;

                if (distance > brushRadius)
                    continue;

                float falloff =
                    1f - distance / brushRadius;

                falloff *= falloff;

                ApplyToCell(x, z, falloff);
            }
        }

        grid.RecalculateMesh();
    }

    void ApplyToCell(
        int x,
        int z,
        float falloff
    )
    {
        float currentHeight =
            grid.GetHeight(x, z);

        switch (mode)
        {
            case BrushMode.Raise:
                grid.SetHeight(
                    x,
                    z,
                    currentHeight +
                    brushStrength *
                    falloff *
                    Time.deltaTime
                );
                break;

            case BrushMode.Lower:
                grid.SetHeight(
                    x,
                    z,
                    currentHeight -
                    brushStrength *
                    falloff *
                    Time.deltaTime
                );
                break;

            case BrushMode.Smooth:
                float average =
                    AverageNeighborHeight(x, z);

                grid.SetHeight(
                    x,
                    z,
                    Mathf.Lerp(
                        currentHeight,
                        average,
                        falloff *
                        Time.deltaTime *
                        brushStrength
                    )
                );
                break;
        }
    }

    float AverageNeighborHeight(int x, int z)
    {
        float total = 0f;
        int count = 0;

        for (int dz = -1; dz <= 1; dz++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int neighborX = x + dx;
                int neighborZ = z + dz;

                if (
                    neighborX < 0 ||
                    neighborX >= grid.width ||
                    neighborZ < 0 ||
                    neighborZ >= grid.depth
                )
                {
                    continue;
                }

                total +=
                    grid.GetHeight(
                        neighborX,
                        neighborZ
                    );

                count++;
            }
        }

        if (count > 0)
            return total / count;

        return grid.GetHeight(x, z);
    }

    public void GenerateRandomTerrain()
    {
        if (grid == null)
            grid = GetComponent<TerrainGrid>();

        if (grid.width != grid.depth)
        {
            Debug.LogError(
                "Random terrain requires Width and Depth to be the same."
            );

            return;
        }

        int size = grid.width;

        if (!IsValidDiamondSquareSize(size))
        {
            Debug.LogError(
                "Terrain Width and Depth must be 33, 65, 129, or 257."
            );

            return;
        }

        float[,] heightMap =
            DiamondSquareGenerator.Generate(
                size,
                randomTerrainRoughness,
                randomSeed
            );

        for (int z = 0; z < grid.depth; z++)
        {
            for (int x = 0; x < grid.width; x++)
            {
                float normalizedHeight =
                    heightMap[x, z];

                float worldHeight =
                    normalizedHeight *
                    randomTerrainHeight;

                grid.SetHeight(
                    x,
                    z,
                    worldHeight
                );

                grid.SetVertexColor(
                x,
                z,
                defaultGrassColor
                );
            }
        }

        grid.RecalculateMesh();

        Debug.Log(
            "Random terrain generated with automatic terrain colors."
        );
    }


    public void FlattenTerrain()
    {
        if (grid == null)
            grid = GetComponent<TerrainGrid>();

        for (int z = 0; z < grid.depth; z++)
        {
            for (int x = 0; x < grid.width; x++)
            {
                grid.SetHeight(x, z, 0f);

                grid.SetVertexColor(
                x,
                z,
                defaultGrassColor
                );
            }
        }

        grid.RecalculateMesh();

        Debug.Log(
            "Terrain flattened and reset to grass."
        );
    }

    bool IsValidDiamondSquareSize(int size)
    {
        if (size < 3)
            return false;

        int value = size - 1;

        return (value & (value - 1)) == 0;
    }

    public void SetMode(int modeIndex)
    {
        if (modeIndex < 0 || modeIndex > 2)
            return;

        mode = (BrushMode)modeIndex;

        TerrainToolState.currentTool =
            TerrainToolState.ActiveTool.Sculpt;
    }

    public void SetRaiseMode()
    {
        mode = BrushMode.Raise;

        TerrainToolState.currentTool =
            TerrainToolState.ActiveTool.Sculpt;
    }

    public void SetLowerMode()
    {
        mode = BrushMode.Lower;

        TerrainToolState.currentTool =
            TerrainToolState.ActiveTool.Sculpt;
    }

    public void SetSmoothMode()
    {
        mode = BrushMode.Smooth;

        TerrainToolState.currentTool =
            TerrainToolState.ActiveTool.Sculpt;
    }
}
