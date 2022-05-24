using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{

    public static float[,] GetNoiseMap(int mapX, int mapY, int mapSize, float scale, int seed)
    {
        float[,] noiseMap = new float[mapSize, mapSize];


        System.Random rand = new System.Random(seed);
        rand.Next();
        float xOffset = rand.Next(-100000, 100000);
        float yOffset = rand.Next(-100000, 100000);

        float pointDist =  scale / mapSize;

        float topLeftX = (mapX * scale) + xOffset;
        float topLeftY = (mapY * scale) + yOffset;


        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                float xPos = topLeftX + (x * pointDist);
                float yPos = topLeftY + (y * pointDist);
                float perlinVal = PerlinNoise(xPos, yPos);

                noiseMap[x, y] = perlinVal;
            }
        }



        return noiseMap;
    }

    static float PerlinNoise(float x, float y)
    {
        float perlinValue = Mathf.PerlinNoise(x, y);



        return perlinValue;
    }


}
