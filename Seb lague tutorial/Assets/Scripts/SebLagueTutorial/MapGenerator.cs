using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class MapGenerator : MonoBehaviour
{
    public Noise.NormalizeMode normalizeMode;

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

    public float uniformScale = 2f;

    public bool useFlatShading = false;
    public bool useFalloff = false;

    [Range(1f, 1000f)]
    public float MeshHeightMultiplier = 1;
    public AnimationCurve meshHeightCurve;

    public enum DrawMode { NoiseMap, ColourMap, Mesh, FalloffMap};
    public DrawMode drawMode;

    public TerrainData terrainData;
    public NoiseData noiseData;

    [Range(0, 7)]
    public int editorPreviewLOD;

    public bool autoUpdate = false;

    public TerrainType[] regions;
    static MapGenerator instance;

    float[,] falloffMap;
    
    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue;
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue;


    //bool uuseFlatShading = false;
    //public bool useFlatShadin;

    public static int mapChunkSize
    {
        get
        {
            //maybe problematic

            //if (instance == null)
            //{
            //    instance = FindObjectOfType<MapGenerator>();              
            //}
            //bool aBool = instance.useFlatShading;

            //end maybe problematic

           { return 127; }
        }
    }


    private void Awake()
    {
        instance = this;
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
        instance = FindObjectOfType<MapGenerator>();
    }

    private void Start()
    {
        mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
        meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(offset);
        MapDisplay display = FindObjectOfType<MapDisplay>();

  
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, MeshHeightMultiplier, meshHeightCurve, editorPreviewLOD,  useFlatShading), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        } else if (drawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };
        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }

    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, MeshHeightMultiplier, meshHeightCurve, lod, useFlatShading);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> mapThreadInfo = mapDataThreadInfoQueue.Dequeue();
                mapThreadInfo.callback(mapThreadInfo.parameter);
            }
        }
        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> mapThreadInfo = meshDataThreadInfoQueue.Dequeue();
                mapThreadInfo.callback(mapThreadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData(Vector2 centre)
    {
        
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, noiseScale, octaves, persistance, lacunarity, centre+ offset, normalizeMode);

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (useFalloff)
                {
                    noiseMap[x,y] -= falloffMap[x, y];
                }
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colourMap[y * mapChunkSize + x] = regions[i].colour;

                        break;
                    }
                }  
            }
        }

        return new MapData(noiseMap, colourMap);

    }

    private void OnValidate()
    {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo (Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }


}

[System.Serializable]
public struct TerrainType
{
    public string type;
    public float height;
    public Color colour;
}

public struct MapData
{
    public float[,] heightMap;
    public Color[] colourMap;

    public MapData(float[,] heightMap, Color[] colourMap)
    {
        this.heightMap = heightMap;
        this.colourMap = colourMap;
    }
}