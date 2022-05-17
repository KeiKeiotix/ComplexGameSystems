using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : ScriptableObject
{
    public float uniformScale = 2f;

    public bool useFlatShading = false;
    public bool useFalloff = false;

    [Range(1f, 1000f)]
    public float MeshHeightMultiplier = 1;
    public AnimationCurve meshHeightCurve;
}
