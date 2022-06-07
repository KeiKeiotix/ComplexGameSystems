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
    MeshCollider meshCollider;


    int[] triangles;

    void Start()
    {
        terrain = new TerrainData[1];
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;


        CreateMesh();
        UpdateMesh();

    }

    void Update()
    {
        if (update)
        {
            update = false;
            CreateMesh();
            //SavePNG is here so it is possible to check the texture as a .png to see if the values are right
            //SavePNG();
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
    
    /*
     * Goes through all the steps required to create the mesh
     * Currently the mesh is only applied to the gameobject this script is currently attached to (*1)
     * (*1) changeable in UpdateMesh(), more info @ TerrainData and/or UpdateMesh()
     */
    void CreateMesh()
    {
        terrain[0] = new TerrainData(mapSize, biomeData);

        terrain[0].biomeMap.GenerateBiomeMap(0, 0, mapSize + 1, 0);

        //main noisemap for the chunk
        float[,] baseNoiseMap = Noise.GetNoiseMap(0, 0, mapSize + 1, noiseScale, new Vector2(0, 0));
        //secondary noisemap that adds more texture to higher elevations
        float[,] bumpMap = Noise.GetNoiseMap(0, 0, mapSize + 1, noiseScale * 10, new Vector2(0,0));

        
        

        for (int y = 0, i = 0; y < mapSize + 1; y++)
        {
            for (int x = 0; x < mapSize + 1; x++, i++)
            {
                //noise val is used to colour the texture
                float noiseVal;

                noiseVal = baseNoiseMap[x, y];

                //changed to vertHeight here so the height can be editable without changing the resulting colour out
                float vertHeight = noiseVal * mapHeight;

                //above this val is where the extra 'rockiness' starts
                float upper = 0.5f;
                //below this value is where the wave effects start
                float lower = 0.35f;

                //both effects are changeable
                if (baseNoiseMap[x, y] >= upper)
                {
                    vertHeight += (bumpMap[x, y] * mapHeight / 2) * (baseNoiseMap[x, y] - upper);
                }
                else if (baseNoiseMap[x, y] <= lower)
                {
                    float wave = Mathf.Sin(x + y) * Mathf.Min((-baseNoiseMap[x, y] + lower), 0.1f) * 10;
                    vertHeight = lower * mapHeight + wave;
                }


                terrain[0].biomeMap.SetHeight(i, noiseVal);
   

                terrain[0].vertices[i] = new Vector3(x, vertHeight, y);
                terrain[0].uv[i] = new Vector2(x / (float)mapSize, y / (float)mapSize);
            }
        }

        terrain[0].biomeMapTexture = TextureFromColourMap(terrain[0].biomeMap.map, mapSize + 1);
        

        //probably should be in TerrainDate but here with direct access to the triangles array instead for now
        for (int y = 0, vert = 0, tris = 0; y < mapSize; y++, vert++)
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
        //this does update its own info correctly, but it does not create its own object 
        //terrain[0].Update(material);

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

        //Get the heights of just the verts to store then in a 2d float array to edit the terrain
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

        //To return a 2d float array that was edited
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
