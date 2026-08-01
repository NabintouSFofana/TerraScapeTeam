using UnityEngine;

// Generates a random landscape using the diamond-square (midpoint displacement)
// fractal algorithm
// Owner: Loi (terrain system).

[RequireComponent(typeof(TerrainMesh))]
public class FractalTerrain : MonoBehaviour
{
    [Tooltip("0..1 — how quickly detail dies out. Higher = rougher, more jagged land.")]
    [Range(0.2f, 0.8f)]
    public float roughness = 0.5f;

    [Tooltip("Height range of the very first random displacement, in meters.")]
    public float initialAmplitude = 20f;

    TerrainMesh terrain;

    void Awake() => terrain = GetComponent<TerrainMesh>();

    // Build a fresh fractal heightmap and push it into the terrain.
    public void Generate()
    {
        int res = terrain.resolution;
        int n = res + 1;

        // Diamond-square needs a (2^k + 1) grid. Work on the smallest such grid
        // that covers our resolution, then copy the region we need into the terrain.
        int p = 1;
        while (p < res) p <<= 1;      
        int gn = p + 1;
        float[,] h = new float[gn, gn];

        float amp = initialAmplitude;

        // 1. Seed the four corners.
        h[0, 0] = Random.Range(-amp, amp);
        h[p, 0] = Random.Range(-amp, amp);
        h[0, p] = Random.Range(-amp, amp);
        h[p, p] = Random.Range(-amp, amp);

        // 2. Repeatedly do the diamond step then the square step, halving each time.
        for (int step = p; step > 1; step /= 2)
        {
            int half = step / 2;

            // Diamond: center of each square = avg of 4 corners + random.
            for (int z = half; z < gn; z += step)
                for (int x = half; x < gn; x += step)
                {
                    float a = (h[x - half, z - half] + h[x + half, z - half] +
                               h[x - half, z + half] + h[x + half, z + half]) * 0.25f;
                    h[x, z] = a + Random.Range(-amp, amp);
                }

            // Square: edge midpoints = avg of available neighbors + random.
            for (int z = 0; z < gn; z += half)
                for (int x = (z + half) % step; x < gn; x += step)
                {
                    float sum = 0f; int c = 0;
                    if (x - half >= 0) { sum += h[x - half, z]; c++; }
                    if (x + half < gn) { sum += h[x + half, z]; c++; }
                    if (z - half >= 0) { sum += h[x, z - half]; c++; }
                    if (z + half < gn) { sum += h[x, z + half]; c++; }
                    h[x, z] = sum / c + Random.Range(-amp, amp);
                }

            amp *= roughness;   // less displacement at finer detail = fractal self-similarity
        }

        // 3. Copy into the terrain heightmap and center it around y = 0.
        float mean = 0f;
        for (int z = 0; z < n; z++)
            for (int x = 0; x < n; x++)
                mean += h[Mathf.Min(x, gn - 1), Mathf.Min(z, gn - 1)];
        mean /= n * n;

        for (int z = 0; z < n; z++)
            for (int x = 0; x < n; x++)
                terrain.heights[x, z] = h[Mathf.Min(x, gn - 1), Mathf.Min(z, gn - 1)] - mean;

        terrain.ApplyHeights();
    }
}
