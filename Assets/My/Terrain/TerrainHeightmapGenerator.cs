using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[ExecuteInEditMode]
public class TerrainHeightmapGenerator : MonoBehaviour
{
    [SerializeField] ComputeShader shader;
    [SerializeField] string[] kernelNames;
    [SerializeField] string outputName;
    [SerializeField] string rectName;

    int[] kernelIndices;
    Vector3Int[] localWorkGroupSizes;
    int outputID;
    int rectID;

    private void OnValidate()
    {
        kernelIndices = new int[kernelNames.Length];
        localWorkGroupSizes = new Vector3Int[kernelNames.Length];
        for (int i = 0; i < kernelNames.Length; ++i)
        {
            kernelIndices[i] = shader.FindKernel(kernelNames[i]);
            localWorkGroupSizes[i] = GetLocalWorkGroupSize(kernelIndices[i]);
        }

        outputID = Shader.PropertyToID(outputName);
        rectID = Shader.PropertyToID(rectName);
    }

    private Vector3Int GetLocalWorkGroupSize(int kernelIndex)
    {
        uint x, y, z;
        shader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        return new Vector3Int((int)x, (int)y, (int)z);
    }

    public void Generate(in RenderTexture heightmap, Rect rect)
    {
        shader.SetVector(rectID, new Vector4(rect.xMin, rect.xMax, rect.yMin, rect.yMax));
        for (int i = 0; i < kernelIndices.Length; ++i)
        {
            shader.SetTexture(kernelIndices[i], outputID, heightmap);
            shader.Dispatch(kernelIndices[i], heightmap.width / localWorkGroupSizes[i].x, heightmap.height / localWorkGroupSizes[i].y, 1);
        }
    }
}