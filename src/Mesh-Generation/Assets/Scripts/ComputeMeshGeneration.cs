using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
public class ComputeMeshGeneration : MonoBehaviour
{
    public ComputeShader shader;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private ComputeBuffer _vertexBuffer;
    private ComputeBuffer _triangleBuffer;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    // Generate a mesh of the specified type
    public void GenerateMeshOfType(MeshType meshType)
    {
        int vertexCount = 0;
        int triangleCount = 0;
        int kernelIndex = 0;

        // Determine the vertex and triangle count, and the kernel index based on the mesh type
        switch (meshType)
        {
            case MeshType.Triangle:
                vertexCount = 3;
                triangleCount = 3;
                kernelIndex = 0;
                break;

            case MeshType.Quad:
                vertexCount = 4;
                triangleCount = 6;
                kernelIndex = 1;
                break;

            case MeshType.Cube:
                vertexCount = 8;
                triangleCount = 36;
                kernelIndex = 2;
                break;
        }

        GenerateMesh(vertexCount, triangleCount, kernelIndex);
    }

    // Generate a mesh with the specified vertex and triangle count, using the specified compute shader kernel
    public void GenerateMesh(int vertexCount, int triangleCount, int kernelIndex)
    {
        _vertexBuffer = new ComputeBuffer(vertexCount, sizeof(float) * 3);
        _triangleBuffer = new ComputeBuffer(triangleCount, sizeof(int));

        shader.SetBuffer(kernelIndex, "vertices", _vertexBuffer);
        shader.SetBuffer(kernelIndex, "triangles", _triangleBuffer);

        shader.Dispatch(kernelIndex, 1, 1, 1);  // Execute the compute shader
        CreateMesh(vertexCount, triangleCount);  // Create the mesh using the generated vertex and triangle data
    }

    // Clear the mesh by setting the MeshFilter's mesh to null
    public void ClearMesh()
    {
        _meshFilter.mesh = null;
    }

    // Create a mesh using the given vertex and triangle count
    private void CreateMesh(int vertexCount, int triangleCount)
    {
        Vector3[] vertices = new Vector3[vertexCount];
        _vertexBuffer.GetData(vertices);

        int[] triangles = new int[triangleCount];
        _triangleBuffer.GetData(triangles);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        _meshFilter.mesh = mesh;
        _meshRenderer.material = new Material(Shader.Find("Standard"));

        _vertexBuffer.Release();
        _triangleBuffer.Release();
    }
}
