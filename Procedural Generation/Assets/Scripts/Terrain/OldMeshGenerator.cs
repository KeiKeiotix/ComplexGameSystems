using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(MeshFilter))]
public class OldMeshGenerator : MonoBehaviour
{

    [Header("General")]
    public int mapSize = 64;

    [Tooltip("Height multiplier for the mesh")]
    public int mapHeight = 10;


    [Header("Noisemap")]

    [Tooltip("How much 'noise' a tile uses, 10 will make a tile use a range of 10 reading from noise.")]
    [Range(0.1f, 600f)]
    public float noiseScale = 5;

    [Tooltip("Frequency of overall mesh")]
    [Range(0.01f, 100f)]
    public float Frequency = 2f;

    [Tooltip("Amplitude of overall mesh")]
    [Range(0.01f, 100f)]
    public float Amplitude = 2f;

    [Tooltip("Layers of noise")]
    [Range(1, 10)]
    public int octaves = 1;

    [Tooltip("Frequency of each octave")]
    [Range(0f, 20f)]
    public float OctaveFrequency = 2f; //"Lacunarity"

    [Tooltip("Amplitude of each octave")]
    [Range(0f, 2f)]
    public float OctaveAmplititude = 0.5f; //"Persistance"

    public Material material;

    [Tooltip("Biome Data Asset")]
    public BiomeData biomeData;

    [Header("Debug")]
    [Tooltip("toggle on to force mesh update")]
    public bool update = false;

    Mesh mesh;
    Texture2D biomeDataTexture;
    BiomeMap biomeMap;

    Vector3[] vertices;
    Vector2[] uv;


    int[] triangles;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;

        CreateShape();
        UpdateMesh();

    }

    void Update()
    {
        if (update)
        {
            update = false;
            CreateShape();
            SavePNG();
            UpdateMesh();
        }
    }

    public static Texture2D TextureFromColourMap(Color[] colourmap, int mapSize)
    {
        Texture2D texture = new Texture2D(mapSize, mapSize);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourmap);
        texture.Apply();
        return texture;
    }

    void CreateShape()
    {

        vertices = new Vector3[(mapSize + 1) * (mapSize + 1)];
        uv = new Vector2[(mapSize + 1) * (mapSize + 1)];

        Color[] biomeDataColourMap = new Color[(mapSize + 1) * (mapSize + 1)];

        biomeMap = new BiomeMap(mapSize, biomeData);
        biomeMap.GenerateBiomeMap(0, 0, mapSize + 1, 0);


        float[,] baseNoiseMap = Noise.GetNoiseMap(0, 0, mapSize + 1, noiseScale, new  Vector2(0,0));
        float[,] bumpMap = Noise.GetNoiseMap(0, 0, mapSize + 1, noiseScale * 10, new Vector2(0,0));


        

        for (int y = 0, i = 0; y < mapSize + 1; y++)
        {
            for (int x = 0; x < mapSize + 1; x++, i++)
            {

                float noiseVal;

                noiseVal = baseNoiseMap[x, y];

                float finalHeight = noiseVal * mapHeight;

                float upper = 0.5f;
                float lower = 0.35f;

                if (baseNoiseMap[x, y] >= upper)
                {
                    finalHeight += (bumpMap[x, y] * mapHeight / 2) * (baseNoiseMap[x, y] - upper);
                }
                else if (baseNoiseMap[x, y] <= lower)
                {
                    float wave = Mathf.Sin(x + y) * Mathf.Min((-baseNoiseMap[x, y] + lower), 0.1f) * 10;
                    finalHeight = lower * mapHeight + wave;
                }


                //biomeMap.SetHeight(i, noiseVal);
                biomeMap.SetHeight(i, 1);
                vertices[i] = new Vector3(x, finalHeight, y);
                uv[i] = new Vector2(x / (float)mapSize, y / (float)mapSize);
            }
        }
       
        triangles = new int[mapSize * mapSize * 6];
        biomeDataTexture = TextureFromColourMap(biomeMap.map, mapSize + 1);
        


        for (int z = 0, vert = 0, tris = 0; z < mapSize; z++, vert++)
        {
            for (int x = 0; x < mapSize; x++, vert++, tris += 6)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + mapSize + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + mapSize + 1;
                triangles[tris + 5] = vert + mapSize + 2;
            }
        }

    }

    public void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        material.SetFloat("VertexHeightScale", mapHeight);

        material.SetTexture("BiomeDataMap", biomeDataTexture);

        material.SetFloat("BiomeCount", biomeMap.biomeCount);

        mesh.RecalculateNormals();
    }

    //private void OnDrawGizmos()
    //{
    //    if (vertices == null)
    //    {
    //        return;
    //    }
    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        Gizmos.DrawSphere(vertices[i], 0.1f);
    //    }
    //}

    void SavePNG()
    {
        byte[] bytes = biomeDataTexture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/../Image.png", bytes);
    }


    float NoiseCalculation(float x, float y)
    {

        float octaveFinal = 0.0f;



        for (var i = 0; i < octaves; i++)
        {
            octaveFinal += Mathf.PerlinNoise(x * Mathf.Pow(OctaveFrequency, i) * Frequency, y * Mathf.Pow(OctaveFrequency, i) * Frequency) * Mathf.Pow(OctaveAmplititude, i) * Amplitude;
        }

        return octaveFinal;
    }



}
