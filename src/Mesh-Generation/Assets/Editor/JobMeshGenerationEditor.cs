using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(JobMeshGeneration))]
public class JobMeshGenerationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        JobMeshGeneration generator = (JobMeshGeneration)target;

        if (GUILayout.Button("Generate Triangle Mesh"))
            generator.GenerateMeshOfType(MeshType.Triangle);

        if (GUILayout.Button("Generate Quad Mesh"))
            generator.GenerateMeshOfType(MeshType.Quad);

        if (GUILayout.Button("Generate Cube Mesh"))
            generator.GenerateMeshOfType(MeshType.Cube);
        
        if (GUILayout.Button("Clear Mesh"))
            generator.ClearMesh();
    }
}