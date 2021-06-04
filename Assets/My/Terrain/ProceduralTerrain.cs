using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ProceduralTerrain : MonoBehaviour
{
    TerrainChunk[] chunks;

    Mesh[] meshes;
    [SerializeField] Dictionary<Vector3Int, RenderTexture> heightmaps;
    Dictionary<Vector3Int, RenderTexture> heightmapsToRemove;
    TerrainHeightmapGenerator heightmapGenerator;
    
    [SerializeField] int chunkSizePower;
    [SerializeField] int viewDistance;
    [SerializeField] Camera targetCamera;
    [SerializeField] Material terrainMaterial;
    [SerializeField] float distanceFalloffExponent;
    [SerializeField] float terrainScale;

    public int ChunkSize
    {
        get;
        private set;
    }

    public Camera TargetCamera
    {
        get => targetCamera;
        private set => targetCamera = value;
    }

    public Material TerrainMaterial
    {
        get => terrainMaterial;
        private set => terrainMaterial = value;
    }

    public int ViewDistance
    {
        get => viewDistance;
        set => viewDistance = value;
    }
    public int MaterialHeightmapPropertyID
    {
        get;
        private set;
    }

    public float DistanceFalloffExponent
    {
        get => distanceFalloffExponent;
        private set => distanceFalloffExponent = value;
    }

    private void Awake()
    {
        MaterialHeightmapPropertyID = Shader.PropertyToID("_Heightmap");
        heightmapGenerator = GetComponent<TerrainHeightmapGenerator>();
    }

    private void Start()
    {
        CreateChunks();
    }

    public void CreateChunks()
    {
        Cleanup();

        ChunkSize = 1 << chunkSizePower;

        meshes = new Mesh[chunkSizePower];
        TerrainMeshGenerator.Generate(in meshes);

        chunks = new TerrainChunk[(ViewDistance * 2 + 1) * (ViewDistance * 2 + 1)];

        for (int x = -ViewDistance, i = 0; x <= ViewDistance; ++x)
        {
            for (int y = -ViewDistance; y <= ViewDistance; ++y)
            {
                Vector3 position = new Vector3(x, 0, y) * ChunkSize;

                GameObject chunk = new GameObject(x + " " + y);
                chunk.transform.parent = transform;
                chunk.transform.position = position;
                TerrainChunk chunkComponent = chunk.AddComponent<TerrainChunk>();
                chunkComponent.terrain = this;

                chunks[i++] = chunkComponent;
            }
        }

        heightmaps = new Dictionary<Vector3Int, RenderTexture>(chunks.Length);
        heightmapsToRemove = new Dictionary<Vector3Int, RenderTexture>(chunks.Length);

        UpdateChunks();

        void Cleanup()
        {
            Transform child;
            while (transform.childCount > 0)
            {
                child = transform.GetChild(0);
                DestroyImmediate(child.gameObject);
            }
        }
    }

    internal void ReturnHeightmap(Vector3Int chunkPosition)
    {
        if (heightmaps.ContainsKey(chunkPosition))
        {
            heightmapsToRemove.Add(chunkPosition, heightmaps[chunkPosition]);
            heightmaps.Remove(chunkPosition);
        }
    }

    internal RenderTexture RequestHeightmap(Vector3Int position)
    {
        RenderTexture texture;
        if (heightmaps.TryGetValue(position, out texture))
        {
            return texture;
        }
        else if (heightmapsToRemove.TryGetValue(position, out texture))
        {
            heightmapsToRemove.Remove(position);
            heightmaps.Add(position, texture);
            return texture;
        }
        else
        {
            texture = new RenderTexture(ChunkSize, ChunkSize, 0, RenderTextureFormat.R16);
            texture.enableRandomWrite = true;
            texture.Create();
            heightmaps.Add(position, texture);

            Vector3 rectSize = new Vector3(ChunkSize, 0, ChunkSize);
            Vector3 rectPos = position - rectSize / 2;
            rectPos = new Vector3(rectPos.x, rectPos.z, 0);
            rectSize = new Vector3(rectSize.x, rectSize.z, 0);

            rectPos *= terrainScale;
            rectSize *= terrainScale;

            Rect rect = new Rect(rectPos, rectSize);
            heightmapGenerator.Generate(texture, rect);
            
            return texture;
        }
    }

    private void Update()
    {
        Vector3 newPosition = RoundToChunkCoordinates(targetCamera.transform.position + targetCamera.transform.forward * ChunkSize * ViewDistance / 2);
        newPosition = new Vector3(newPosition.x, 0, newPosition.z);
        
        if (newPosition != RoundToChunkCoordinates(transform.position))
        {
            transform.position = newPosition;
            UpdateChunks();
        }

    }

    private void UpdateChunks()
    {
        foreach (TerrainChunk chunk in chunks)
        {
            chunk.TryReturnHeightmap();
        }
        foreach (TerrainChunk chunk in chunks)
        {
            chunk.RequestHeightmap();
        }
        heightmapsToRemove.Clear();
    }

    internal Mesh GetMeshByResolution(int resolution)
    {
        int index = meshes.Length - 1 - resolution;
        if (index < 0)
            return meshes[0];
        else
            return meshes[index];
    }

    public Vector3Int RoundToChunkCoordinates(Vector3 position)
    {
        return new Vector3Int(
            Mathf.RoundToInt(position.x / ChunkSize) * ChunkSize,
            Mathf.RoundToInt(position.y / ChunkSize) * ChunkSize,
            Mathf.RoundToInt(position.z / ChunkSize) * ChunkSize
        );
    }
}
