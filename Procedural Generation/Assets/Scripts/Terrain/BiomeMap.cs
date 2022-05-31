using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeMap
{
    BiomeData biomeData;
    public Color[] map;
    public int biomeCount;

    public BiomeMap(int mapSize, BiomeData biomeData)
    {
        map = new Color[(mapSize + 1) * (mapSize + 1)];

        biomeCount = biomeData.biomes.Length;
    }



    public void GenerateBiomeMap(int mapX, int mapY, int mapSize, int seed)
    {

        MakeBiomeMap(mapX, mapY, mapSize, seed);
    }


    void MakeBiomeMap(int mapX, int mapY, int mapSize, int seed)
    {
        //dist between biomes to blend them
        float distToBlend = 0.1f;


        //initialise the stuffs
        int biomeCount = biomeData.biomes.Length;
        //float[,,] biomeNoiseMaps = new float[biomeCount,biomeCount,biomeCount];
        //float[][,] otherWay = new float[biomeCount][biomeCount, biomeCount];
        NoiseMap[] noiseMaps = new NoiseMap[biomeCount];
        Vector2[] biomeOffset = new Vector2[biomeCount];

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
            for (int y = 0, i = 0; y < mapSize; y++)
            {
                for (int x = 0; x < mapSize; x++, i++)
                {
                    SetBiomeData(i, biomeData.biomeToForce, 1f, 0, 0, 0);
                }
            }

        }
        else
        {
            //get noisemap for each biome
            for (int i = 0; i < biomeCount; i++)
            {
                noiseMaps[i].noiseMap = Noise.GetNoiseMap(mapX, mapY, mapSize, biomeData.biomes[i].noiseScale, biomeOffset[i]);
            }

            //loop through each X/Y pos on the chunk
            for (int y = 0, i = 0; y < mapSize; y++)
            {


                for (int x = 0; x < mapSize; x++, i++)
                {
                    //block data
                    int topBiome = 0;

                    int subBiome = 0;

                    float topBiomeHeight = 0;
                    float subBiomeHeight = 0;

                    //if the top biome is forced, it need
                    bool topBiomeForced = false;

                    for (int b = 0; b < biomeCount; b++)
                    {
                        float thisNoiseVal = noiseMaps[b].noiseMap[x, y];

                        //If this biomes height req to be active is lower then the current height, do the rest of the checks (if it isn't, next biome)
                        if (biomeData.biomes[b].activeAboveValue < thisNoiseVal)
                        {
                            if (topBiomeForced)
                            {
                                if (thisNoiseVal > subBiomeHeight)
                                {
                                    subBiomeHeight = thisNoiseVal;
                                    subBiome = b;
                                }

                            }
                            else
                            {

                                if (biomeData.biomes[b].forceIfAboveValue)
                                {
                                    topBiomeForced = true;

                                    subBiome = topBiome;
                                    subBiomeHeight = topBiomeHeight;
                                    topBiomeHeight = thisNoiseVal;
                                    topBiome = b;
                                }
                                else
                                {

                                    //if this biome is higher then the previous
                                    if (thisNoiseVal > topBiomeHeight)
                                    {
                                        subBiomeHeight = topBiomeHeight;
                                        subBiome = topBiome;
                                        topBiomeHeight = thisNoiseVal;
                                        topBiome = b;
                                    }
                                    else if (thisNoiseVal > subBiomeHeight)
                                    {
                                        subBiomeHeight = thisNoiseVal;
                                        subBiome = b;
                                    }
                                }
                            }
                        }
                    }

                    SetBiomeData(i, topBiome, topBiomeHeight, subBiome, subBiomeHeight, distToBlend);

                }
            }
        }

    }





    void SetBiomeData(int dataPos, int topBiome, float topBiomeHeight, int subBiome, float subBiomeHeight, float distanceToBlend)
    {
        //Get the % (0 -> 1) of how much the sub-biome should be shown
        //Note: Generally wont go higher then 0.5 as then the "top" and "sub" should switch
        //distToBlend = 0.1, if heightDiff is 0.1, percent is 40% of bottom, of height is 0.4 percent is 0.1
        float biomeRatio = Mathf.Clamp(distanceToBlend - (topBiomeHeight - subBiomeHeight), 0f, 1f);

        map[dataPos].r = topBiome / (float)biomeCount;
        map[dataPos].g = biomeRatio;
        map[dataPos].b = subBiome / (float)biomeCount;
    }

    public void SetHeight(int dataPos, float height)
    {
        map[dataPos].a = height;
    }


    class NoiseMap
    {
        public float[,] noiseMap;
    }
}
