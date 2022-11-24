using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePlaneTerrain : MonoBehaviour
{


    // changing these seems to change how spiky this shit is 
    int heightScale = 20; 
    float detailScale = 5.0f; 
    float width = 50; 
    float height = 50; 

    public int seed = 7;

    public int octaves = 5;
    public float persistence = 0.5f;
    public float lacunarity = 3f;
    public Vector2 offset = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

         for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        Mesh mesh = this.GetComponent<MeshFilter>().mesh; 
        Vector3[] vertices = mesh.vertices; 
        for(int v = 0; v < vertices.Length; v++){
            vertices[v].y = CalculateHeight((int) ((vertices[v].x + this.transform.position.x)/2.0f), (int) ((vertices[v].z + this.transform.position.z)/2.0f), octaveOffsets) * heightScale;
        }

        mesh.vertices = vertices; 
        mesh.RecalculateBounds();
        mesh.RecalculateNormals(); 
        this.gameObject.AddComponent<MeshCollider>(); 
        
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
