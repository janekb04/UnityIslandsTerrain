using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;

[CustomEditor(typeof(ProceduralTerrain))]
public class ProceduralTerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Recreate Chunks"))
        {
            (target as ProceduralTerrain).CreateChunks();
        }
    }
}
