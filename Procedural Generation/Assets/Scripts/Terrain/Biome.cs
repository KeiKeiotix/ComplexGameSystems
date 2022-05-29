using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biome
{

    public Color[] GetBiomeMap(int mapX, int mapY, int chunkSize, int seed)
    {

        //initialise the stuffs
        BiomeData biomeData = new BiomeData();
        int biomeCount = biomeData.biomes.Length;
        //float[,,] biomeNoiseMaps = new float[biomeCount,biomeCount,biomeCount];
        //float[][,] otherWay = new float[biomeCount][biomeCount, biomeCount];
        NoiseMap[] noiseMaps = new NoiseMap[biomeCount];
        Vector2[] biomeOffset = new Vector2[biomeCount];

        Color[] biomeMap = new Color[(chunkSize + 1) * (chunkSize + 1)];

        System.Random rand = new System.Random(seed);
        rand.Next();

        //Get offsets for each biome noisemap
        for (int i = 0; i < biomeCount; i++)
        {
            biomeOffset[i].x = rand.Next(-1000000, 1000000);
            biomeOffset[i].y = rand.Next(-1000000, 1000000);
        }



        if (biomeData.forceSingleBiome == true)
        {

        }
        else
        {
            //get noisemap for each biome
            for (int i = 0; i < biomeCount; i++)
            {
                noiseMaps[i].noiseMap = Noise.GetNoiseMap(mapX, mapY, chunkSize, biomeData.biomes[i].noiseScale, biomeOffset[i]);
            }
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    //block data
                    int biomeTop = 0;
                    float biomeRatio = 1; //1 will be entirely the top, 0.5f is a equal mix;
                    int biomeSub = 0;

                    float topBiomeHeight = 0;
                    float biomeSubHeight = 0;

                    for (int i = 0; i < biomeCount; i++)
                    {
                        float thisNoiseVal = noiseMaps[i].noiseMap[x, y];

                        if ()
                        if (thisNoiseVal > topBiomeHeight)
                        {
                            topBiomeHeight = thisNoiseVal;
                            biomeTop = i;
                        }
                    }


                }
            }

        }



        return biomeMap;
    }

    class NoiseMap
    {
        public float[,] noiseMap;
    }
    
}
