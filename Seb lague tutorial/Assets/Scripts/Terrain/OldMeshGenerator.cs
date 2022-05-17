using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
public class OldMeshGenerator : MonoBehaviour
{

    [Header("General")]
    public int xSize = 64;
    public int zSize = 64;

    [Tooltip("Not in use")]
    [Range(1f, 10f)]
    public int resolution = 2;

    [Header("Noisemap")]

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


    [Header("Debug")]
    [Tooltip("toggle on to force mesh update")]
    public bool update = false;

    Mesh mesh;

    Vector3[] vertices;
    Vector2[] uv;
    int[] triangles;

    

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        CreateShape();
        UpdateMesh();
    }


    void Update()
    {
        if (update)
        {
            update = false;
            CreateShape();
            UpdateMesh();
        }
    }

    void CreateShape()
    {

        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        uv = new Vector2[(xSize + 1) * (zSize + 1)];


        for (int z = 0, i = 0; z < zSize + 1; z++)
        {
            for (int x = 0; x < xSize + 1; x++, i++)
            {
                float y = NoiseCalculation((float)x/xSize, (float)z/zSize); // Mathf.PerlinNoise((float)x/xSize * 5, (float)z /zSize * 5) * 3;
                Debug.Log(y);
                vertices[i] = new Vector3(x, y, z);
                uv[i] = new Vector2(z / (float)zSize, x / (float) xSize);
            }
        }

        triangles = new int[xSize * zSize * 6];


        for (int z = 0, vert = 0, tris = 0; z < zSize; z++, vert++)
        {
            for (int x = 0; x < xSize; x++, vert++, tris += 6)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;
            }
        }

    }

    public void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles; 
        mesh.uv = uv;

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
