using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public enum DrawMode { NoiseMap, ColourMap, Mesh};
    public DrawMode drawMode;


    public const int mapChunkSize = 129; //1 is added later to make it work correctly

    [Range(0, 7)]
    public int levelOfDetail;


    [Range(1f, 500f)]
    public float noiseScale = 1;

    [Range(1, 10)]
    public int octaves;
    [Range(0f, 1f)]
    public float persistance;
    [Range(1f, 10f)]
    public float lacunarity;

    public int seed = 0;
    public Vector2 offset;

    [Range(1f, 1000f)]
    public float MeshHeightMultiplier = 1;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate = false;

    public TerrainType[] regions;
    

    public void GenerateMap()
    {
        
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);


        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int z = 0; z < mapChunkSize; z++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, z];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colourMap[z * mapChunkSize + x] = regions[i].colour;

                        break;
                    }
                }  
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        } else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
        } else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, MeshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
        }
    }

    private void OnValidate()
    {

    }


}

[System.Serializable]
public struct TerrainType
{
    public string type;
    public float height;
    public Color colour;
}
