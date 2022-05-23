using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SebNoise 
{

    public enum NormalizeMode { Local, Global };

    public static float[,] GenerateNoiseMap(int mapX, int mapY, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {
        if (scale <= 0)
        {
            scale = 0.00001f;
        }

        float[,] noiseMap = new float[mapX, mapY];

        System.Random rand = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = rand.Next(-10000, 10000) + offset.x;
            float offsetY = rand.Next(-10000, 10000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }



        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfX = mapX / 2.0f;
        float halfY = mapY / 2.0f;

        for (int y = 0; y < mapY; y++)
        {
            for (int x = 0; x < mapX; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfX + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfY + octaveOffsets[i].y) / scale * frequency;

                    float perlinVal = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinVal * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;




                }
                
                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                } else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;

            }
        }

        //Debug.Log("noiseval pre norm: " + noiseMap[(int)halfX, (int)halfY]);
        for (int y = 0; y < mapY; y++)
        {
            for (int x = 0; x < mapX; x++)
            {

                if (normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                } else
                {
                    noiseMap[x, y] = (noiseMap[x, y] + 1) / (2 * (maxPossibleHeight));

                }

            }
        }
        //Debug.Log("noiseval post norm: " + noiseMap[(int)halfX, (int)halfY]);
        //Debug.Log("Local:\nMin - " + minLocalNoiseHeight + ", Max - " + maxLocalNoiseHeight);
        //Debug.Log("Global:\nMin - " + 0 + ", Max - " + maxPossibleHeight);

        return noiseMap;
    }
}
