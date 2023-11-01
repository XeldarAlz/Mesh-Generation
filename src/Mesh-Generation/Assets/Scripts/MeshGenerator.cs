using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class MeshGenerator : MonoBehaviour
{
	[Serializable]
	public struct Field
	{
		public int X;
		public int Z;

		public Field(int x, int z)
		{
			X = x;
			Z = z;
		}
	}

	public struct SquareRect
	{
		public float XMin;
		public float XMax;
		public float ZMin;
		public float ZMax;

		public SquareRect(float xMin, float xMax, float zMin, float zMax)
		{
			XMin = xMin;
			XMax = xMax;
			ZMin = zMin;
			ZMax = zMax;
		}

		public bool IsContainsOtherSquare(SquareRect rect)
		{
			return XMin <= rect.XMin && ZMin <= rect.ZMin && XMax >= rect.XMax && ZMax >= rect.ZMax;
		}

		public bool IsPointIncluded(Vector3 point)
		{
			return point.x >= XMin && point.z >= ZMin && point.x <= XMax && point.z <= ZMax;
		}

		public bool IsPointInside(Vector3 point)
		{
			return point.x > XMin && point.z > ZMin && point.x < XMax && point.z < ZMax;
		}
	}

	[Header("Cube")]
	[SerializeField] Vector3 _cubePosition = new Vector3(0, 0, 0);

	[SerializeField] float _cubeSize = 1;

	[Header("Plane from texture")]
	[SerializeField] Texture2D _texture;

	[Header("Plane from texture with custom size")]
	[SerializeField] Field _size = new Field(100, 100);

	[SerializeField] float _heightModifier = 1;

	List<Vector3> _globalVertices = new List<Vector3>();
	List<Vector3> result = new List<Vector3>();

	public void DrawCube()
	{
		List<Vector3> vertices = new List<Vector3>();
		for (int y = 0; y < 2; y++)
		{
			vertices.Add(_cubePosition + new Vector3(0, y, 0) * _cubeSize);
			vertices.Add(_cubePosition + new Vector3(0, y, 1) * _cubeSize);
			vertices.Add(_cubePosition + new Vector3(1, y, 0) * _cubeSize);
			vertices.Add(_cubePosition + new Vector3(1, y, 1) * _cubeSize);
		}

		int[] tris = new int[]
		{
			//bottom
			0, 2, 1,
			1, 2, 3,
			//top
			4, 5, 6,
			5, 7, 6,
			//left
			0, 1, 4,
			4, 1, 5,
			//right
			2, 6, 3,
			3, 6, 7,
			//face
			0, 4, 2,
			4, 6, 2,
			//back
			1, 3, 5,
			5, 3, 7
		};
		Mesh mesh = new Mesh
		{
			vertices = vertices.ToArray(),
			triangles = tris
		};
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		GetComponent<MeshFilter>().mesh = mesh;
	}

	public void GenerateMeshFromTexture()
	{
		// support large vertex count
		Mesh mesh = new Mesh
		{
			indexFormat = IndexFormat.UInt32
		};
		// create list
		List<Vector3> vertices = new List<Vector3>();
		int textureWidth = _texture.width;
		int textureHeight = _texture.height;
		// move through texture pixels to create vertex with height that depends on pixel grayscale
		for (int x = 0; x < textureWidth; x++)
		{
			for (int z = 0; z < textureHeight; z++)
			{
				Color color = _texture.GetPixel(x, z);
				float intensity = color.grayscale;
				float height = Mathf.Max(intensity, 0);

				Vector3 vertex = new Vector3(x, height * _heightModifier, z);
				vertices.Add(vertex);
			}
		}

		// create triangles
		List<int> triangles = CreateTriangles(vertices, textureWidth, textureHeight);

		// apply mesh data
		ApplyMesh(mesh, vertices, triangles);
	}
	
	public void GenerateMeshWithDefinedSize()
	{
		Mesh mesh = new Mesh
		{
			indexFormat = IndexFormat.UInt32
		};

		List<Vector3> vertices = new List<Vector3>();

		int texWidth = _texture.width;
		int texHeight = _texture.height;

		float[,] heights = GetTextureHeights(texWidth, texHeight);

		// move through texture pixels to create vertex with height that depends on pixel grayscale
		// but normalize data base on defined size
		for (int x = 0; x < _size.X; x++)
		{
			for (int z = 0; z < _size.Z; z++)
			{
				float normalizedX = (float)x / _size.X * heights.GetLength(0);
				float normalizedY = (float)z / _size.Z * heights.GetLength(1);
				float height = heights[(int)normalizedX, (int)normalizedY];
				Vector3 vertex = new Vector3(x, height * _heightModifier, z);
				vertices.Add(vertex);
			}
		}

		List<int> triangles = CreateTriangles(vertices, _size.X, _size.Z);

		ApplyMesh(mesh, vertices, triangles);
	}

	public void DrawOptimizedMesh()
	{
		Mesh mesh = new Mesh
		{
			indexFormat = IndexFormat.UInt32
		};
		_globalVertices.Clear();
		int textureWidth = _texture.width;
		int textureHeight = _texture.height;
		for (int x = 0; x < textureWidth; x++)
		{
			for (int z = 0; z < textureHeight; z++)
			{
				Color color = _texture.GetPixel(x, z);
				float intensity = color.grayscale;
				float height = Mathf.Max(intensity, 0);

				Vector3 vertex = new Vector3(x, height * _heightModifier, z);
				_globalVertices.Add(vertex);
			}
		}

		List<int> triangles = new List<int>();
		Optimizer(triangles, textureHeight, textureWidth);
		ApplyMesh(mesh, result, triangles);
	}

	public void Optimizer(List<int> triangles, int height, int width)
	{
		result.Clear();
		int lastSquareIndex = 0;
		HashSet<SquareRect> savedSquares = new HashSet<SquareRect>();
		int currentX = 0;
		SquareRect fullRect = new SquareRect(xMin: 0, xMax: width, zMin: 0, zMax: height);
		while (lastSquareIndex < height * width)
		{
			if (savedSquares.Any(x => x.IsPointInside(_globalVertices[lastSquareIndex])))
			{
				lastSquareIndex++;
				continue;
			}

			// define if there is a square for current point
			(int zSize, int xSize) = GetRectSize(height, width, savedSquares, lastSquareIndex, currentX);

			// is there is a square 
			if (zSize > 0 & xSize > 0)
			{
				// define corner vertices
				Vector3 point1 = _globalVertices[lastSquareIndex];
				Vector3 point2 = _globalVertices[lastSquareIndex + zSize];
				Vector3 point3 = _globalVertices[lastSquareIndex + height * xSize];
				Vector3 point4 = _globalVertices[lastSquareIndex + height * xSize + zSize];
				// create square struct
				SquareRect square = new SquareRect(xMin: point1.x, xMax: point3.x, zMin: point1.z, zMax: point4.z);
				// is square is a part of another square - skip it
				if (!savedSquares.Any(x => x.IsContainsOtherSquare(square)))
				{
					// add vertices and save index for triangle list
					int index1 = result.Count;
					result.Add(point1);
					result.Add(point2);
					result.Add(point3);
					result.Add(point4);
					// add square to saved squares for futures checks
					savedSquares.Add(square);

					int index2 = index1 + 1;
					int index3 = index1 + 2;
					int index4 = index1 + 3;
					// create triangles for current square
					triangles.AddRange(new int[]
					{
						index1, index2, index4,
						index1, index4, index3
					});
					// set next index
					lastSquareIndex += zSize < height - 1 ? zSize : (zSize + 1) * xSize;
				}
				else
				{
					lastSquareIndex++;
				}
			}
			// is there is no square (means bigger that 1x1) 
			else
			{
				// add points only up to pre last row of vertices except last one
				if (lastSquareIndex < height * (width - 1) && lastSquareIndex % height < height - 1)
				{
					// define corner vertices
					Vector3 point1 = _globalVertices[lastSquareIndex];
					Vector3 point2 = _globalVertices[lastSquareIndex + 1];
					Vector3 point3 = _globalVertices[lastSquareIndex + height];
					Vector3 point4 = _globalVertices[lastSquareIndex + height + 1];
					// create square struct
					SquareRect square = new SquareRect(xMin: point1.x, xMax: point3.x, zMin: point1.z, zMax: point2.z);
					result.Add(point1);
					// is square is a part of another square - skip it
					if (!savedSquares.Any(x => x.IsContainsOtherSquare(square)))
					{
						int index1 = result.Count - 1;
						result.Add(point2);
						result.Add(point3);
						result.Add(point4);
						int index2 = index1 + 1;
						int index3 = index1 + 2;
						int index4 = index1 + 3;
						// create triangles for current square
						triangles.AddRange(new int[]
						{
							index1, index2, index4,
							index1, index4, index3
						});
					}
				}
				// for other vertices and only if they are not included in any of the saved squares
				else if (lastSquareIndex > height * (width - 1) || lastSquareIndex % height == 0)
				{
					if (!savedSquares.Any(x => x.IsPointIncluded(_globalVertices[lastSquareIndex])))
					{
						result.Add(_globalVertices[lastSquareIndex]);
					}
				}

				// set next index
				lastSquareIndex++;
			}

			currentX = lastSquareIndex / height;
		}
	}

	(int, int) GetRectSize(int height, int width, ICollection<SquareRect> excludeSquares,
		int index, int currentX)
	{
		int zSize = 0;
		int xSize = 0;

		// go up to highest vertex to calculate max Z of possible square
		float lastHeight = _globalVertices[index].y;
		for (int z = index + 1; z < height + (currentX * height); z++)
		{
			if (z < height * (width - 1) && Math.Abs(_globalVertices[z].y - lastHeight) < float.Epsilon)
			{
				zSize++;
			}
			else
			{
				break;
			}
		}

		if (zSize > 0)
		{
			// is zSize is greater than 0 try to create square with right x step
			for (int x = currentX + 1; x < width; x++)
			{
				int possibleZ = 0;
				bool isSquare = true;
				int currentIndex = index % height;
				for (int z = currentIndex; z < currentIndex + zSize + 1; z++)
				{
					bool outOfBoundaries = x * height + z >= (height * width);
					// if any of the vertex have another y height this is not a square 
					if (outOfBoundaries || Math.Abs(_globalVertices[x * height + z].y - lastHeight) > float.Epsilon)
					{
						isSquare = false;
						break;
					}
					// anyway save possible square size
					possibleZ++;
				}

				// if it is square try to do the same for next X
				if (isSquare)
				{
					xSize++;
				}
				else
				{
					// if it's not a square set square size to possible Z and xSize
					if (possibleZ > 0)
					{
						xSize++;
						zSize = possibleZ - 1;
					}

					break;
				}
			}
		}

		return (zSize, xSize);
	}

	float[,] GetTextureHeights(int texWidth, int texHeight)
	{
		float[,] heights = new float[texWidth, texHeight];
		for (int x = 0; x < texWidth; x++)
		{
			for (int y = 0; y < texHeight; y++)
			{
				Color color = _texture.GetPixel(x, y);
				float intensity = color.grayscale;
				heights[x, y] = Mathf.Max(intensity, 0);
			}
		}

		return heights;
	}

	List<int> CreateTriangles(List<Vector3> vertices, int width, int height)
	{
		List<int> triangles = new List<int>();
		for (int index = 0; index < vertices.Count; index++)
		{
			//int index = vertices.IndexOf(vertex);
			if (index >= height * (width - 1))
				break;
			int heightIndex = (index + 1) % height;
			if (heightIndex < height && heightIndex != 0)
			{
				int[] tris = new int[]
				{
					index, index + 1, index + width + 1,
					index + width + 1, index + width, index
				};
				triangles.AddRange(tris);
			}
		}

		return triangles;
	}

	void ApplyMesh(Mesh mesh, List<Vector3> verticesList, List<int> triangles)
	{
		mesh.SetVertices(verticesList);
		mesh.SetTriangles(triangles, 0);
		mesh.Optimize();
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		meshFilter.mesh = mesh;
	}

	void OnDrawGizmos()
	{
		foreach (Vector3 vertex in result)
		{
			Gizmos.DrawSphere(vertex, 0.1f);
		}
	}
}
