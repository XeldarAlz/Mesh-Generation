using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
public class BasicMeshGeneration : MonoBehaviour
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
    private void GenerateMesh(Vector3[] vertices, int[] triangles)
    {
        // Clear any existing data in the mesh.
        _mesh.Clear();

        // Set the vertices and triangles for the mesh.
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;

        // Recalculate the normals for the mesh (used for lighting calculations).
        _mesh.RecalculateNormals();

        // Set the MeshFilter's mesh to the generated mesh and apply the material to the mesh.
        _meshFilter.mesh = _mesh;
        _meshRenderer.material = _material;
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

    // Creates a simple triangle mesh.
    private void GenerateTriangleMesh()
    {
        // The vertices of the triangle.
        Vector3[] vertices = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0)
        };

        // The indices of the triangle's vertices.
        int[] triangles = new int[] { 0, 1, 2 };

        // Call GenerateMesh to create the mesh.    
        GenerateMesh(vertices, triangles);
    }

    // Creates a simple quad mesh.
    private void GenerateQuadMesh()
    {
        // The vertices of the quad.
        Vector3[] vertices = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0),
            new Vector3(0, 1, 0)
        };

        // The indices of the quad's vertices.
        int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };

        // Call GenerateMesh to create the mesh.
        GenerateMesh(vertices, triangles);
    }

    // Creates a simple cube mesh.
    private void GenerateCubeMesh()
    {
        // The vertices of the cube.
        Vector3[] vertices = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 1),
            new Vector3(1, 1, 1),
            new Vector3(0, 1, 1)
        };

        // The indices of the cube's vertices.
        int[] triangles = new int[] {
            0, 2, 1, 0, 3, 2, // Front face
            1, 6, 5, 1, 2, 6, // Right face
            5, 7, 4, 5, 6, 7, // Back face
            4, 3, 0, 4, 7, 3, // Left face
            0, 5, 4, 0, 1, 5, // Top face
            3, 6, 2, 3, 7, 6  // Bottom face
        };
        
        // Call GenerateMesh to create the mesh.
        GenerateMesh(vertices, triangles);
    }

    // ClearMesh removes the mesh from the MeshFilter.
    public void ClearMesh()
    {
        _meshFilter.mesh = null;
    }
}
