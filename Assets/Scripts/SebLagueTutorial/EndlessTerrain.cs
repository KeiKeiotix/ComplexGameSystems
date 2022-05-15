using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{


    public LODInfo[] detailLevels;
    public static int maxViewDistance;

    public Transform player;
    public static Vector2 playerPosition;
    Vector2 playerPositionOld = playerPosition;

    public const float playerPosDeltaForChunkUpdate = 25;
    float sqrPlayerPosDeltaForChunkUpdate = playerPosDeltaForChunkUpdate * playerPosDeltaForChunkUpdate;


    static MapGenerator mapGenerator;
    public Material mapMaterial;


    int chunkSize = MapGenerator.mapChunkSize - 1;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDistance = maxViewDistance / chunkSize;
        mapGenerator = FindObjectOfType<MapGenerator>();


        UpdateVisibleChunks();
    }

    void Update()
    {
        playerPosition = new Vector2(player.position.x, player.position.z) / mapGenerator.terrainData.uniformScale;

        if ((playerPositionOld - playerPosition).sqrMagnitude > sqrPlayerPosDeltaForChunkUpdate)
        {
            UpdateVisibleChunks();
            playerPositionOld = playerPosition;
        }
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
                    terrainChunk = new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial);
                    terrainChunkDictionary.Add(viewedChunkCoord, terrainChunk);
                }
                //if (terrainChunk.IsVisible())
                //{
                //    terrainChunksVisibleLastUpdate.Add(terrainChunk);
                //}

            }
        }

    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;

        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODMesh[] lodMeshes;
        LODMesh collisionLODMesh;
        LODInfo[] detailLevels;
        int previousLodIndex = -1;

        MapData mapData;
        bool mapDataRecieved = false;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);

            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3 * mapGenerator.terrainData.uniformScale;
            meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;

            //meshObject.transform.localScale = Vector3.one * size / 10.0f;
            meshObject.transform.parent = parent;

            SetVisible(false);

            this.detailLevels = detailLevels;
            lodMeshes = new LODMesh[detailLevels.Length];
            
            for (int i = 0; i < detailLevels.Length; ++i)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
                if (detailLevels[i].useForCollider)
                {
                    collisionLODMesh = lodMeshes[i] ;
                }
            }

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataRecieved = true;

            Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;


            UpdateTerrainChunk();
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            meshFilter.mesh = meshData.CreateMesh();
        }

        public void UpdateTerrainChunk() 
        {
            if (mapDataRecieved)
            {
                float viewerDistFromNearestEdge = bounds.SqrDistance(playerPosition); 
                // Mathf.Sqrt(bounds.SqrDistance(playerPosition)); -- reason for below being MVD * MVD, more efficient then sqrt.
                //As well as detialLevels * detailLevels.

                bool visible = (viewerDistFromNearestEdge <= maxViewDistance * maxViewDistance);

                if (visible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDistFromNearestEdge > detailLevels[i].visibleDistThreshold * detailLevels[i].visibleDistThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (lodIndex != previousLodIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLodIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                            
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                    if (lodIndex == 0)
                    {
                        if (collisionLODMesh.hasMesh)
                        {
                            meshCollider.sharedMesh = collisionLODMesh.mesh;
                        } else if (!collisionLODMesh.hasRequestedMesh)
                        {
                            collisionLODMesh.RequestMesh(mapData);
                        }


                    }

                    terrainChunksVisibleLastUpdate.Add(this);

                }

                SetVisible(visible);
            }
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

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;

        System.Action updateCallback;
        
        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        public void RequestMesh(MapData mapdata)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapdata, lod, onMeshDataRecieved);
        }

        public void onMeshDataRecieved(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        [Range(0, 7)]
        public int lod;
        public int visibleDistThreshold;
        public bool useForCollider;
    }

}
