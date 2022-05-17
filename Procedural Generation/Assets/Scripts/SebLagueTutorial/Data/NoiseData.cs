using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : ScriptableObject
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
}
