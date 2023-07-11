using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
public class JobMeshGeneration : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Mesh _mesh;
    private Material _material;
    
    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _mesh = new Mesh();
        _material = new Material(Shader.Find("Standard"));
    }

    // Generates a mesh using a given vertices and triangles
    private void GenerateMesh(NativeArray<Vector3> vertices, NativeArray<int> triangles, MeshType meshType)
    {
        JobHandle jobHandle = default;

        // Create different jobs based on vertices and triangles count
        switch (meshType)
        {
            case MeshType.Triangle:
                var triangleJob = new GenerateTriangleMeshJob
                {
                    Vertices = vertices,
                    Triangles = triangles
                };
                jobHandle = triangleJob.Schedule();
                break;

            case MeshType.Quad:
                var quadJob = new GenerateQuadMeshJob
                {
                    Vertices = vertices,
                    Triangles = triangles
                };
                jobHandle = quadJob.Schedule();
                break;

            case MeshType.Cube:
                var cubeJob = new GenerateCubeMeshJob
                {
                    Vertices = vertices,
                    Triangles = triangles
                };
                jobHandle = cubeJob.Schedule();
                break;
        }

        // Wait until the job is done
        jobHandle.Complete();

        // Clear any existing data in the mesh.
        _mesh.Clear();
        
        // Set the vertices and triangles for the mesh.
        _mesh.vertices = vertices.ToArray();
        _mesh.triangles = triangles.ToArray();
        
        // Recalculate the normals for the mesh (used for lighting calculations).
        _mesh.RecalculateNormals();

        // Set the mesh to our MeshFilter and assign a new Material to our MeshRenderer
        _meshFilter.mesh = _mesh;
        _meshRenderer.material = _material;

        // Dispose the native arrays when we are done
        vertices.Dispose();
        triangles.Dispose();
    }
    
    // Creates a mesh of the specified type.
    public void GenerateMeshOfType(MeshType meshType)
    {
        switch (meshType)
        {
            case MeshType.Triangle:
                GenerateTriangleMesh();
                break;
            case MeshType.Quad:
                GenerateQuadMesh();
                break;
            case MeshType.Cube:
                GenerateCubeMesh();
                break;
        }
    }

    private void GenerateTriangleMesh()
    {
        var vertices = new NativeArray<Vector3>(3, Allocator.TempJob);
        var triangles = new NativeArray<int>(3, Allocator.TempJob);
        GenerateMesh(vertices, triangles, MeshType.Triangle);
    }

    private void GenerateQuadMesh()
    {
        var vertices = new NativeArray<Vector3>(4, Allocator.TempJob);
        var triangles = new NativeArray<int>(6, Allocator.TempJob);
        GenerateMesh(vertices, triangles, MeshType.Quad);
    }

    private void GenerateCubeMesh()
    {
        var vertices = new NativeArray<Vector3>(8, Allocator.TempJob);
        var triangles = new NativeArray<int>(36, Allocator.TempJob);
        GenerateMesh(vertices, triangles, MeshType.Cube);
    }

    // Clears the current mesh
    public void ClearMesh()
    {
        _meshFilter.mesh = null;
    }

    // These structs are jobs used to generate specific meshes. BurstCompile for optimization
    [BurstCompile]
    private struct GenerateTriangleMeshJob : IJob
    {
        public NativeArray<Vector3> Vertices;
        public NativeArray<int> Triangles;

        public void Execute()
        {
            // Create vertices and triangles for a triangle mesh
            Vertices[0] = new Vector3(0, 0, 0);
            Vertices[1] = new Vector3(1, 0, 0);
            Vertices[2] = new Vector3(0, 1, 0);

            Triangles[0] = 0;
            Triangles[1] = 1;
            Triangles[2] = 2;
        }
    }

    [BurstCompile]
    private struct GenerateQuadMeshJob : IJob
    {
        public NativeArray<Vector3> Vertices;
        public NativeArray<int> Triangles;

        public void Execute()
        {
            // Create vertices and triangles for a quad mesh
            Vertices[0] = new Vector3(0, 0, 0);
            Vertices[1] = new Vector3(1, 0, 0);
            Vertices[2] = new Vector3(1, 1, 0);
            Vertices[3] = new Vector3(0, 1, 0);

            Triangles[0] = 0;
            Triangles[1] = 1;
            Triangles[2] = 2;
            Triangles[3] = 0;
            Triangles[4] = 2;
            Triangles[5] = 3;
        }
    }

    [BurstCompile]
    private struct GenerateCubeMeshJob : IJob
    {
        public NativeArray<Vector3> Vertices;
        public NativeArray<int> Triangles;

        public void Execute()
        {
            // Create vertices and triangles for a cube mesh
            Vertices[0] = new Vector3(0, 0, 0);
            Vertices[1] = new Vector3(1, 0, 0);
            Vertices[2] = new Vector3(1, 1, 0);
            Vertices[3] = new Vector3(0, 1, 0);
            Vertices[4] = new Vector3(0, 0, 1);
            Vertices[5] = new Vector3(1, 0, 1);
            Vertices[6] = new Vector3(1, 1, 1);
            Vertices[7] = new Vector3(0, 1, 1);

            // Manually create triangles for a cube mesh
            Triangles[0] = 0; Triangles[1] = 2; Triangles[2] = 1;
            Triangles[3] = 0; Triangles[4] = 3; Triangles[5] = 2;
            Triangles[6] = 1; Triangles[7] = 6; Triangles[8] = 5;
            Triangles[9] = 1; Triangles[10] = 2; Triangles[11] = 6;
            Triangles[12] = 5; Triangles[13] = 7; Triangles[14] = 4;
            Triangles[15] = 5; Triangles[16] = 6; Triangles[17] = 7;
            Triangles[18] = 4; Triangles[19] = 3; Triangles[20] = 0;
            Triangles[21] = 4; Triangles[22] = 7; Triangles[23] = 3;
            Triangles[24] = 0; Triangles[25] = 5; Triangles[26] = 4;
            Triangles[27] = 0; Triangles[28] = 1; Triangles[29] = 5;
            Triangles[30] = 3; Triangles[31] = 6; Triangles[32] = 2;
            Triangles[33] = 3; Triangles[34] = 7; Triangles[35] = 6;
        }
    }
}
