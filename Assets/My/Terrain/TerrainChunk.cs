using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class TerrainChunk : MonoBehaviour
{
    public ProceduralTerrain terrain;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    int resolution;
    Vector3Int chunkPosition;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
    }

    private void Start()
    {
        resolution = -1;
        transform.localScale = new Vector3(terrain.ChunkSize, terrain.ChunkSize, terrain.ChunkSize);
        meshRenderer.material = terrain.TerrainMaterial;
    }

    private void OnBecameVisible()
    {
        if (Camera.current != terrain.TargetCamera)
            return;

        meshRenderer.enabled = true;
    }

    private void OnBecameInvisible()
    {
        if (Camera.current != terrain.TargetCamera)
            return;

        meshRenderer.enabled = false;
    }

    private void Update()
    {
        int newResolution = CalculateResolution();
        if (newResolution != resolution)
        {
            resolution = newResolution;
            meshFilter.sharedMesh = terrain.GetMeshByResolution(resolution);
        }
    }

    public void TryReturnHeightmap()
    {
        if (chunkPosition != terrain.RoundToChunkCoordinates(transform.position))
        {
            terrain.ReturnHeightmap(chunkPosition);
        }
    }
    public void RequestHeightmap()
    {
        if (chunkPosition != terrain.RoundToChunkCoordinates(transform.position))
        {
            chunkPosition = terrain.RoundToChunkCoordinates(transform.position);
            UpdatePropertyBlock();
        }
    }

    private void UpdatePropertyBlock()
    {
        meshRenderer.SetPropertyBlock(CreatePropertyBlock());
    }

    private MaterialPropertyBlock CreatePropertyBlock()
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetTexture(terrain.MaterialHeightmapPropertyID, terrain.RequestHeightmap(chunkPosition));
        return block;
    }

    int CalculateResolution()
    {
        float distance = Vector3.Distance(transform.position, terrain.TargetCamera.transform.position);
        distance = Mathf.Pow(distance, terrain.DistanceFalloffExponent);
        return Mathf.RoundToInt(distance / terrain.ChunkSize);
    }
}
