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


    TerrainData[] terrain; //if there will be more then 1 chunk, there will need to be more then 1;


    Mesh mesh;



    int[] triangles;

    void Start()
    {
        terrain = new TerrainData[1];
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
        terrain[0] = new TerrainData(mapSize, biomeData);

        terrain[0].biomeMap.GenerateBiomeMap(0, 0, mapSize + 1, 0);


        float[,] baseNoiseMap = Noise.GetNoiseMap(0, 0, mapSize + 1, noiseScale, new  Vector2(0,0));
        float[,] bumpMap = Noise.GetNoiseMap(0, 0, mapSize + 1, noiseScale * 10, new Vector2(0,0));

        
        

        for (int y = 0, i = 0; y < mapSize + 1; y++)
        {
            for (int x = 0; x < mapSize + 1; x++, i++)
            {

                float noiseVal;

                noiseVal = baseNoiseMap[x, y];

                float mapHeightMap = noiseVal * mapHeight;

                float upper = 0.5f;
                float lower = 0.35f;

                if (baseNoiseMap[x, y] >= upper)
                {
                    mapHeightMap += (bumpMap[x, y] * mapHeight / 2) * (baseNoiseMap[x, y] - upper);
                }
                else if (baseNoiseMap[x, y] <= lower)
                {
                    float wave = Mathf.Sin(x + y) * Mathf.Min((-baseNoiseMap[x, y] + lower), 0.1f) * 10;
                    mapHeightMap = lower * mapHeight + wave;
                }


                terrain[0].biomeMap.SetHeight(i, noiseVal);
   

                terrain[0].vertices[i] = new Vector3(x, mapHeightMap, y);
                terrain[0].uv[i] = new Vector2(x / (float)mapSize, y / (float)mapSize);
            }
        }

        terrain[0].biomeMapTexture = TextureFromColourMap(terrain[0].biomeMap.map, mapSize + 1);
        


        for (int z = 0, vert = 0, tris = 0; z < mapSize; z++, vert++)
        {
            for (int x = 0; x < mapSize; x++, vert++, tris += 6)
            {
                terrain[0].triangles[tris + 0] = vert + 0;
                terrain[0].triangles[tris + 1] = vert + mapSize + 1;
                terrain[0].triangles[tris + 2] = vert + 1;

                terrain[0].triangles[tris + 3] = vert + 1;
                terrain[0].triangles[tris + 4] = vert + mapSize + 1;
                terrain[0].triangles[tris + 5] = vert + mapSize + 2;
            }
        }

    }

    public void UpdateMesh()
    {
        terrain[0].Update(material);

        mesh.Clear();
        mesh.vertices = terrain[0].vertices;
        mesh.triangles = terrain[0].triangles;
        mesh.uv = terrain[0].uv;

        material.SetFloat("VertexHeightScale", mapHeight);

        material.SetTexture("BiomeDataMap", terrain[0].biomeMapTexture);

        material.SetFloat("BiomeCount", terrain[0].biomeMap.biomeCount);

        mesh.RecalculateNormals();
    }


    void SavePNG()
    {
        byte[] bytes = terrain[0].biomeMapTexture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/../Image.png", bytes);
    }


    public class TerrainData
    {
        public BiomeMap biomeMap;
        public Texture2D biomeMapTexture;
        public Mesh mesh;

        public Vector3[] vertices;
        public Vector2[] uv;

        public int[] triangles;
        int mapSize;


        public TerrainData(int mapSize, BiomeData biomeData)
        {
            vertices = new Vector3[(mapSize + 1) * (mapSize + 1)];
            uv = new Vector2[(mapSize + 1) * (mapSize + 1)];
            triangles = new int[mapSize * mapSize * 6];
            this.mapSize = mapSize;

            biomeMap = new BiomeMap(mapSize, biomeData);
            biomeMapTexture = new Texture2D(mapSize + 1, mapSize + 1);
            mesh = new Mesh();

    
        }

        public void Update(Material material)
        {
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;


            material.SetTexture("BiomeDataMap", biomeMapTexture);

            material.SetFloat("BiomeCount", biomeMap.biomeCount);

            mesh.RecalculateNormals();
        }

        float[,] GetVerticeHeight()
        {
            float[,] vertHeight = new float[mapSize + 1, mapSize + 1];

            for (int y = 0, i = 0; y < mapSize + 1; y++) {
                for (int x = 0; x < mapSize + 1; x++, i++)
                {
                    vertHeight[x, y] = vertices[i].y;
                }
            }

            return vertHeight;
        }

        void SetVertHeight(float[,] vertHeight)
        {
            for (int y = 0, i = 0; y < mapSize + 1; y++)
            {
                for (int x = 0; x < mapSize + 1; x++, i++)
                {
                    vertices[i].y = vertHeight[x, y];
                }
            }
        }
    }
}
