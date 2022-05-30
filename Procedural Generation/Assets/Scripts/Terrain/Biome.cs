using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Biome
{


    public Color[] GetBiomeMap(int mapX, int mapY, int chunkSize, int seed)
    {
        //dist between biomes to blend them
        float distToBlend = 0.2f;


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

            //loop through each X/Y pos on the chunk
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    //block data
                    int topBiome = 0;
                    float biomeRatio = 1; //1 will be entirely the top, 0.5f is a equal mix;
                    int subBiome = 0;

                    float topBiomeHeight = 0;
                    float subBiomeHeight = 0;

                    //if the top biome is forced, it need
                    bool topBiomeForced = false;

                    for (int i = 0; i < biomeCount; i++)
                    {
                        float thisNoiseVal = noiseMaps[i].noiseMap[x, y];

                        //If this biomes height req to be active is lower then the current height, do the rest of the checks (if it isn't, next biome)
                        if (biomeData.biomes[i].activeAboveValue < thisNoiseVal)
                        {
                            if (topBiomeForced)
                            {
                                if (thisNoiseVal > subBiomeHeight)
                                {
                                    subBiomeHeight = thisNoiseVal;
                                    subBiome = i;
                                }

                            }
                            else
                            {

                                if (biomeData.biomes[i].forceIfAboveValue)
                                {
                                    topBiomeForced = true;

                                    subBiome = topBiome;
                                    subBiomeHeight = topBiomeHeight;
                                    topBiomeHeight = thisNoiseVal;
                                    topBiome = i;
                                } else
                                {

                                    //if this biome is higher then the previous
                                    if (thisNoiseVal > topBiomeHeight)
                                    {
                                        subBiomeHeight = topBiomeHeight;
                                        subBiome = topBiome;
                                        topBiomeHeight = thisNoiseVal;
                                        topBiome = i;
                                    }
                                    else if (thisNoiseVal > subBiomeHeight)
                                    {
                                        subBiomeHeight = thisNoiseVal;
                                        subBiome = i;
                                    }
                                }
                            }                           
                        }
                    }

                    //Get the % (0 -> 1) of how much the sub-biome should be shown
                    //Note: Generally wont go higher then 0.5 as then the "top" and "sub" should switch

                    float heightDiff = topBiomeHeight - subBiomeHeight;


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
