using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class Tile { 
    public GameObject theTile; 
    public float creationTime; 
    public GameObject waterplane;
    public GameObject[] treeArr;
    public ParticleSystem[] windArr;
    public Vector2 coord;

    public Tile(GameObject t, GameObject wp, GameObject[] trees, ParticleSystem[] wind, float ct, Vector2 c) { 
        theTile = t; 
        creationTime = ct;
        treeArr = trees; 
        coord = c;
        windArr = wind;
        waterplane = wp;
    }
}

public class GenerateInfinite : MonoBehaviour
{

    public GameObject player; 

    public int depth = 20;
    public int width = 256;
    public int height = 256;
    public int seed = 7;
    public int octaves = 5;
    public float persistence = 0.5f;
    public float lacunarity = 3f;
    public Vector2 offset = Vector2.zero;
    public Texture2D sandTexture;
    public GameObject plane;
    public GameObject tree;
    public ParticleSystem wind;



    int planeSize = 256; 
    int halfTilesX = 4; 
    int halfTilesZ = 4; 

    Vector3 startPos; 

    Hashtable tiles = new Hashtable();

    ParticleSystem[] GenerateWind(float x_min, float x_max, float y_min, float y_max) { 
        List<ParticleSystem> windArr =  new List<ParticleSystem>();

        if (x_min > x_max) { 
            float temp = x_min; 
            x_min = x_max; 
            x_max = temp;
        }

        if (y_min > y_max) { 
            float temp = y_min; 
            y_min = y_max; 
            y_max = temp;
        }

        for (float x = x_min; x <= x_max; x++) { 
            for (float y = y_min; y <= y_max; y++) { 
                float windSeed = Random.Range(0, 1f); 
                if (windSeed > 0.99990) { 
                    windArr.Add(Instantiate (wind, new Vector3(x + Random.Range(0, 3f), Random.Range(4, 8), y + Random.Range(0, 3f)), Quaternion.identity));

                }
            }
        }

        return windArr.ToArray();

    }

    (TerrainData terrainData, GameObject[] trees) GenerateTerrain(TerrainData terrainData, float x_offset, float y_offset)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);
        var heights_and_trees = GenerateHeightsAndTrees(x_offset, y_offset);
        terrainData.SetHeights(0, 0, heights_and_trees.heights);
        return (terrainData, heights_and_trees.trees);
    }

    (float[,] heights, GameObject[] trees) GenerateHeightsAndTrees(float x_offset, float y_offset)
    {
        float[,] heights = new float[width + 1, height + 1];
        List<GameObject> trees = new List<GameObject>();

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }


        for (int x = 0; x <= width; x++)
        {
            for (int y = 0; y <= height; y++)
            {
                heights[x, y] = CalculateHeight(x, y, x_offset, y_offset, octaveOffsets);
                if (heights[x, y] > 0.3) { 
                    float treeSeed = Random.Range(0, 1f);
                    if (treeSeed > 0.9995) {
                        trees.Add(Instantiate(tree, new Vector3(y + x_offset, heights[x, y] * depth - 1, x + y_offset), Quaternion.identity));
                    }
                }
            }
        }

        return (heights, trees.ToArray());
    }

    float CalculateHeight(int x, int y, float x_offset, float y_offset, Vector2[] octaveOffsets)
    {

        // Debug.Log(this.transform.position.x);
        float sum_h = 0f;
        float amplitude = 1f;
        float frequency = 1f;

        for (int i = 0; i < octaves; i++)
        {
            float x_coord = (float)(x + y_offset) / width * frequency + octaveOffsets[i].x;
            float y_coord = (float)(y + x_offset) / height * frequency + octaveOffsets[i].y;
            float h = Mathf.PerlinNoise(x_coord, y_coord) * 2 - 1;
            sum_h += h * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return sum_h;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.transform.position = Vector3.zero;
        startPos = Vector3.zero; 

        float updateTime = Time.realtimeSinceStartup; 

        Regenerate(startPos.x, startPos.z, updateTime);
    }

    // // Update is called once per frame
    void Update()
    {
        int xMove = (int)(player.transform.position.x - startPos.x); 
        int zMove = (int)(player.transform.position.z - startPos.z); 


        if (Mathf.Abs(xMove) >= planeSize || Mathf.Abs(zMove) >= planeSize) { 

            float updateTime = Time.realtimeSinceStartup; 

            //force integer position and round to nearest tile 
            int playerX = (int)(Mathf.Floor(player.transform.position.x/planeSize)*planeSize);
            int playerZ = (int)(Mathf.Floor(player.transform.position.z/planeSize)*planeSize);

            Regenerate(playerX, playerZ, updateTime);

            Hashtable newTerrain = new Hashtable(); 
            foreach(Tile tls in tiles.Values){ 
                if (tls.creationTime != updateTime) { 
                    Destroy(tls.theTile);
                    Destroy(tls.waterplane);
                    foreach (GameObject tree in tls.treeArr) {
                        Destroy(tree);
                    }
                    foreach(ParticleSystem wind in tls.windArr) { 
                        Destroy(wind);
                    }
                }
                else { 
                    newTerrain.Add(tls.theTile.name, tls);
                }
            }

            tiles = newTerrain; 

            startPos = player.transform.position; 
        }
    }

    void Regenerate(float playerX, float playerZ, float updateTime)
    {
        for (int x = -halfTilesX; x < halfTilesX; x++) { 
            for (int z = -halfTilesZ; z < halfTilesZ; z++) { 
                Vector3 pos = new Vector3((x * planeSize + playerX), 0, (z * planeSize + playerZ));
                Vector3 waterpos = new Vector3((x * planeSize + playerX + planeSize / 2), 2, (z * planeSize + playerZ + planeSize / 2));
                string tilename = "Tile_" + ((int)(pos.x)).ToString() + "_" + ((int)(pos.z)).ToString();

                if(!tiles.ContainsKey(tilename)) {
                    (TerrainData _terraindata, GameObject[] tree_arr) = GenerateTerrain(new TerrainData(), pos.x, pos.z);
                    // sand texture
                    TerrainLayer tl = new TerrainLayer();
                    tl.diffuseTexture = sandTexture; 
                    _terraindata.terrainLayers = new TerrainLayer[] {tl};

                    GameObject terrain = Terrain.CreateTerrainGameObject(_terraindata);

                    ParticleSystem[] windArr = GenerateWind(x * planeSize + playerX, (x + x) * planeSize + playerX, z * planeSize + playerZ, (z + z) * planeSize + playerZ);

                    GameObject t = (GameObject) Instantiate(terrain, pos, Quaternion.identity);
                    t.layer = 6;
                    GameObject water = (GameObject) Instantiate(plane, waterpos, Quaternion.identity);

                    Destroy(terrain);
                    t.name = tilename; 

                    Vector2 coord = new Vector2(x, z);

                    Tile tile = new Tile(t, water, tree_arr, windArr, updateTime, coord);

                    tiles.Add(tilename, tile);
                }
                else { 
                    (tiles[tilename] as Tile).creationTime = updateTime;
                }
            }
        }
    }
}
