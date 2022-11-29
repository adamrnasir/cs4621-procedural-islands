using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class Tile { 
    public GameObject theTile; 
    public float creationTime; 
    public GameObject waterplane;

    public Tile(GameObject t, GameObject wp, float ct) { 
        theTile = t; 
        creationTime = ct;
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



    int planeSize = 256; 
    int halfTilesX = 2; 
    int halfTilesZ = 2; 

    Vector3 startPos; 

    Hashtable tiles = new Hashtable();

    TerrainData GenerateTerrain(TerrainData terrainData, float x_offset, float y_offset)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);
        terrainData.SetHeights(0, 0, GenerateHeights(x_offset, y_offset));
        return terrainData;
    }

    float[,] GenerateHeights(float x_offset, float y_offset)
    {
        float[,] heights = new float[width + 1, height + 1];

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
            }
        }

        return heights;
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
            float y_coord = (float)(y + x_offset) / height * frequency+ octaveOffsets[i].y;
            // float x_coord = (float)(x ) / width * frequency + octaveOffsets[i].x;
            // float y_coord = (float)(y ) / height * frequency+ octaveOffsets[i].y;
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

        for (int x = -halfTilesX; x < halfTilesX; x++) 
        {
            for (int z = -halfTilesZ; z < halfTilesZ; z++) 
            { 
                Vector3 pos = new Vector3((x * planeSize + startPos.x), 0, (z * planeSize + startPos.z));
                Vector3 waterpos = new Vector3((x * planeSize + startPos.x + planeSize / 2), 2, (z * planeSize + startPos.z + planeSize / 2));
                TerrainData _terraindata = GenerateTerrain(new TerrainData(), pos.x, pos.z);

                // sand texture
                TerrainLayer tl = new TerrainLayer();
                tl.diffuseTexture = sandTexture; 
                _terraindata.terrainLayers = new TerrainLayer[] {tl};

                GameObject terrain = Terrain.CreateTerrainGameObject(_terraindata);
                GameObject water = (GameObject) Instantiate(plane, waterpos, Quaternion.identity);
                GameObject t = (GameObject) Instantiate(terrain, pos, Quaternion.identity);
                Destroy(terrain);

                string tilename = "Tile_" + ((int)(pos.x)).ToString() + "_" + ((int)(pos.z)).ToString();
                t.name = tilename; 
                Tile tile = new Tile(t, water, updateTime);
                tiles.Add(tilename, tile);
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {

        int xMove = (int)(player.transform.position.x - startPos.x); 
        int zMove = (int)(player.transform.position.z - startPos.z); 

        if (Mathf.Abs(xMove) >= planeSize || Mathf.Abs(zMove) >= planeSize) { 

            float updateTime = Time.realtimeSinceStartup; 

            //force integer position and round to nearest tile 
            int playerX = (int)(Mathf.Floor(player.transform.position.x/planeSize)*planeSize);
            int playerZ = (int)(Mathf.Floor(player.transform.position.z/planeSize)*planeSize);

            for (int x = -halfTilesX; x < halfTilesX; x++) { 
                for (int z = -halfTilesZ; z < halfTilesZ; z++) { 
                    Vector3 pos = new Vector3((x * planeSize + playerX), 0, (z * planeSize + playerZ));
                    Vector3 waterpos = new Vector3((x * planeSize + playerX + planeSize / 2), 2, (z * planeSize + playerZ + planeSize / 2));
                    string tilename = "Tile_" + ((int)(pos.x)).ToString() + "_" + ((int)(pos.z)).ToString();

                    if(!tiles.ContainsKey(tilename)) { 
                        TerrainData _terraindata = GenerateTerrain(new TerrainData(), pos.x, pos.z);
                         // sand texture
                        TerrainLayer tl = new TerrainLayer();
                        tl.diffuseTexture = sandTexture; 
                        _terraindata.terrainLayers = new TerrainLayer[] {tl};

                        GameObject terrain = Terrain.CreateTerrainGameObject(_terraindata);



                        GameObject t = (GameObject) Instantiate(terrain, pos, Quaternion.identity);
                        GameObject water = (GameObject) Instantiate(plane, waterpos, Quaternion.identity);
                        Destroy(terrain);
                        t.name = tilename; 
                        Tile tile = new Tile(t, water, updateTime);

                        tiles.Add(tilename, tile);
                    }
                    else { 
                        (tiles[tilename] as Tile).creationTime = updateTime;
                    }
                }
            }

            Hashtable newTerrain = new Hashtable(); 
            foreach(Tile tls in tiles.Values){ 
                if (tls.creationTime != updateTime) { 
                    Destroy(tls.theTile);
                    Destroy(tls.waterplane);
                }
                else { 
                    newTerrain.Add(tls.theTile.name, tls);
                }
            }

            tiles = newTerrain; 

            startPos = player.transform.position; 



        }
        
    }
}
