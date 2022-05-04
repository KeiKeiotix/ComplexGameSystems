using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{

    

    public static int maxViewDistance = 384;
    public Transform player;
    public static Vector2 playerPosition;

    public int chunkSize = MapGenerator.mapChunkSize - 1;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDistance = maxViewDistance / chunkSize;
    }

    void Update()
    {
        playerPosition = new Vector2(player.position.x, player.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        int currentChunkCoordX = Mathf.RoundToInt(playerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(playerPosition.y / chunkSize);

        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        TerrainChunk terrainChunk;
        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; ++xOffset)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunk = terrainChunkDictionary[viewedChunkCoord];
                    terrainChunk.UpdateTerrainChunk();
                } else
                {
                    terrainChunk = new TerrainChunk(viewedChunkCoord, chunkSize, this.transform);
                    terrainChunkDictionary.Add(viewedChunkCoord, terrainChunk);
                }
                if (terrainChunk.IsVisible())
                {
                    terrainChunksVisibleLastUpdate.Add(terrainChunk);
                }

            }
        }

    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;

        Bounds bounds;
        public TerrainChunk(Vector2 coord, int size, Transform parent)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);

            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * size / 10.0f;
            meshObject.transform.parent = parent;

            SetVisible(false);
        }



        public void UpdateTerrainChunk() 
        {
            float viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(playerPosition));
            bool visible = (viewerDistFromNearestEdge <= maxViewDistance);
            
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }
        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }


    }


}
