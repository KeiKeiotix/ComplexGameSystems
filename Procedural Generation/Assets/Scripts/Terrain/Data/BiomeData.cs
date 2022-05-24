using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BiomeData : ScriptableObject
{

    public Biomes[] biomes;

    public bool forceSingleBiome = false;
    public int biomeToForce = 0;


    [System.Serializable]
    public class Biomes
    {



        [Tooltip("How much 'noise' a tile uses, 10 will make a tile use a range of 10 reading from noise.")]
        public float noiseScale = 4;

        [Tooltip("A base biome, will always be the active biome if it has the highest noise value.")]
        public bool baseBiome = true;

        [Tooltip("If this is not a base biome, it will only be the active biome if it has the highest value and is above this value.")]
        public float activeAboveValue = 0;

        [Tooltip("If noise value is higher then 'activeAboveValue', and there is no higher up 'forceVal' active, this biome will be forced.")]
        public bool forceIfAboveValue = false;

    }

}

