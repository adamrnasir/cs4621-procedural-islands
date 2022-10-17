using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int depth = 20;
    public int width = 256;
    public int height = 256;
    public int seed = 0;
    public int octaves = 5;
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public Vector2 offset = Vector2.zero;

    void Update()
    {
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);
        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[width, height];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }


        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y, octaveOffsets);
            }
        }

        return heights;
    }

    float CalculateHeight(int x, int y, Vector2[] octaveOffsets)
    {

        float sum_h = 0f;
        float amplitude = 1f;
        float frequency = 1f;

        for (int i = 0; i < octaves; i++)
        {
            float x_coord = (float)x / width * frequency + octaveOffsets[i].x;
            float y_coord = (float)y / height * frequency + octaveOffsets[i].y;
            float h = Mathf.PerlinNoise(x_coord, y_coord) * 2 - 1;
            sum_h += h * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return sum_h;
    }
}
