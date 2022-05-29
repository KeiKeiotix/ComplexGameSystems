using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{

    public static float[,] GetNoiseMap(int mapX, int mapY, int mapSize, float scale, Vector2 offset)
    {
        float[,] noiseMap = new float[mapSize, mapSize];



        float xOffset = offset.x;
        float yOffset = offset.y;

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
