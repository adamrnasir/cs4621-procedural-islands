using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int depth = 20;
    public int width = 256;
    public int height = 256;

    public float[] scales = { 5f, 6f, 20f, 40f, 400f };
    public float[] multipliers = { -1f, 0.8f, 0.4f, 0.3f, 0.05f };

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

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }

        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        float sum_h = 0f;
        for (int i = 0; i < scales.Length; i++)
        {
            float s = scales[i];
            float xCoord = (float)x / width * (s);
            float yCoord = (float)y / height * (s);
            sum_h += multipliers[i] * Mathf.PerlinNoise(xCoord, yCoord);
        }

        return sum_h;
    }
}
