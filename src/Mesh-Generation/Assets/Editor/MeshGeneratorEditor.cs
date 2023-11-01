using UnityEditor;

using UnityEngine;

[CustomEditor(typeof(MeshGenerator))]
public class MeshGeneratorEditor : Editor
{
    MeshGenerator _target;

    private void OnEnable()
    {
        _target = target as MeshGenerator;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(25);

        if (GUILayout.Button("Draw cube mesh"))
            _target.DrawCube();


        if (GUILayout.Button("Generate From Texture"))
            _target.GenerateMeshFromTexture();


        if (GUILayout.Button("Generate With Size"))
            _target.GenerateMeshWithDefinedSize();


        if (GUILayout.Button("Optimized mesh"))
            _target.DrawOptimizedMesh();
    }
}
