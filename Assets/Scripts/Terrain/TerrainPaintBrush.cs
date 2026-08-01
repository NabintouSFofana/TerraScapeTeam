using UnityEngine;

[RequireComponent(typeof(TerrainGrid))]
public class TerrainPaintBrush : MonoBehaviour
{
    public enum PaintMode
    {
        Grass,
        Rock,
        Sand
    }

    [Header("References")]
    public Camera cam;

    [Tooltip("Set this to a layer that only the terrain is on, so the brush doesn't hit trees/rocks placed on top and silently do nothing.")]
    public LayerMask terrainLayer = ~0;

    [Header("Paint Settings")]
    public PaintMode mode = PaintMode.Grass;

    [Min(0.5f)]
    public float brushRadius = 5f;

    [Range(0.01f, 1f)]
    public float paintStrength = 0.5f;

    [Header("Terrain Colors")]
    public Color grassColor =
        new Color(0.25f, 0.65f, 0.20f);

    public Color rockColor =
        new Color(0.45f, 0.45f, 0.45f);

    public Color sandColor =
        new Color(0.85f, 0.75f, 0.45f);

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
            TerrainToolState.ActiveTool.Paint
        )
        {
            return;
        }

        bool shiftPressed =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

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
            PaintAtMouse();
        }
    }

    void HandleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            mode = PaintMode.Grass;

            TerrainToolState.currentTool =
                TerrainToolState.ActiveTool.Paint;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            mode = PaintMode.Rock;

            TerrainToolState.currentTool =
                TerrainToolState.ActiveTool.Paint;
        }

        // Was KeyCode.S - that collided with FreeCameraController's WASD
        // backward movement, so every "walk backward" tap also switched you
        // into Sand paint mode. Moved to X, which nothing else uses.
        if (Input.GetKeyDown(KeyCode.X))
        {
            mode = PaintMode.Sand;

            TerrainToolState.currentTool =
                TerrainToolState.ActiveTool.Paint;
        }
    }

    void PaintAtMouse()
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

        Color targetColor =
            GetSelectedColor();

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

                Color currentColor =
                    grid.GetVertexColor(x, z);

                Color blendedColor =
                    Color.Lerp(
                        currentColor,
                        targetColor,
                        paintStrength * falloff
                    );

                grid.SetVertexColor(
                    x,
                    z,
                    blendedColor
                );
            }
        }

        grid.RecalculateMesh();
    }

    Color GetSelectedColor()
    {
        switch (mode)
        {
            case PaintMode.Rock:
                return rockColor;

            case PaintMode.Sand:
                return sandColor;

            default:
                return grassColor;
        }
    }

    public void SetGrassMode()
    {
        mode = PaintMode.Grass;

        TerrainToolState.currentTool =
            TerrainToolState.ActiveTool.Paint;
    }

    public void SetRockMode()
    {
        mode = PaintMode.Rock;

        TerrainToolState.currentTool =
            TerrainToolState.ActiveTool.Paint;
    }

    public void SetSandMode()
    {
        mode = PaintMode.Sand;

        TerrainToolState.currentTool =
            TerrainToolState.ActiveTool.Paint;
    }
}
