using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public static class TerrainMeshGenerator
{
    public static void Generate(in Mesh[] meshes)
    {
        for (int level = 0; level < meshes.Length; ++level)
        {
            meshes[level] = GenerateMesh(level + 1);
        }
    }

    private static Mesh GenerateMesh(int level)
    {
        Mesh mesh = new Mesh();

        int subdivisions = 1 << level;
        float texelSize = 1.0f / (subdivisions - 1);

        Vector3[] vertices;
        GenerateVertices(1.0f, subdivisions, texelSize, out vertices);

        int[] triangles = GenerateIndices(subdivisions);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        MeshUtility.Optimize(mesh);

        mesh.bounds = new Bounds(new Vector3(0, 0.5f, 0), Vector3.one);

        return mesh;
    }

    private static void GenerateVertices(float size, int subdivisions, float texelSize, out Vector3[] vertices)
    {
        vertices = new Vector3[subdivisions * subdivisions];
        for (int i = 0; i < subdivisions; ++i)
        {
            for (int j = 0; j < subdivisions; ++j)
            {
                vertices[subdivisions * i + j] = new Vector3(i, 0, j) * texelSize - new Vector3(1, 0, 1) / 2;
            }
        }
    }

    private static int[] GenerateIndices(int subdivisions)
    {
        int[] triangles = new int[(subdivisions - 1) * (subdivisions - 1) * 2 * 3];
        for (int i = 0; i < subdivisions - 1; ++i)
        {
            for (int j = 0; j < subdivisions - 1; ++j)
            {
                int bottomLeft = i * subdivisions + j;
                int topLeft = bottomLeft + 1;
                int bottomRight = (i + 1) * subdivisions + j;
                int topRight = bottomRight + 1;

                int index = (i * (subdivisions - 1) + j) * 6;

                triangles[index++] = topLeft;
                triangles[index++] = topRight;
                triangles[index++] = bottomRight;

                triangles[index++] = bottomRight;
                triangles[index++] = bottomLeft;
                triangles[index++] = topLeft;
            }
        }

        return triangles;
    }
}
