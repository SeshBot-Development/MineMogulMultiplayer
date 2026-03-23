using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DisplacementMeshGenerator : MonoBehaviour
{
	[Tooltip("Allowed values: 1, 2, 4, 8, 16")]
	[Range(1f, 16f)]
	public int subdivisions = 4;

	public int CellSize = 1;

	private readonly int[] allowedValues = new int[5] { 1, 2, 4, 8, 16 };

	[SerializeField]
	public HashSet<Vector2Int> activeTiles = new HashSet<Vector2Int>
	{
		new Vector2Int(0, 0)
	};

	private Dictionary<Vector2Int, Vector3[]> tileVertices = new Dictionary<Vector2Int, Vector3[]>();

	public void AddTile(Vector2Int gridPos)
	{
		if (activeTiles.Add(gridPos))
		{
			GeneratePlane();
		}
	}

	public void RemoveTile(Vector2Int gridPos)
	{
		if (activeTiles.Remove(gridPos))
		{
			GeneratePlane();
		}
	}

	public void GeneratePlane()
	{
		float num = (float)CellSize / (float)subdivisions;
		int num2 = subdivisions + 1;
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		List<Vector2> list3 = new List<Vector2>();
		tileVertices.Clear();
		foreach (Vector2Int activeTile in activeTiles)
		{
			Vector3[] array = new Vector3[num2 * num2];
			int count = list.Count;
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					int num3 = j + i * num2;
					float x = (float)j * num + (float)(activeTile.x * CellSize);
					float z = (float)i * num + (float)(activeTile.y * CellSize);
					list.Add(array[num3] = new Vector3(x, 0f, z));
					list3.Add(new Vector2((float)j / (float)subdivisions, (float)i / (float)subdivisions));
				}
			}
			for (int k = 0; k < subdivisions; k++)
			{
				for (int l = 0; l < subdivisions; l++)
				{
					int num4 = l + k * num2;
					list2.Add(count + num4);
					list2.Add(count + num4 + num2);
					list2.Add(count + num4 + 1);
					list2.Add(count + num4 + 1);
					list2.Add(count + num4 + num2);
					list2.Add(count + num4 + num2 + 1);
				}
			}
			tileVertices[activeTile] = array;
		}
		Mesh mesh = new Mesh();
		mesh.name = "DisplacementMesh";
		mesh.vertices = list.ToArray();
		mesh.triangles = list2.ToArray();
		mesh.uv = list3.ToArray();
		mesh.RecalculateNormals();
		GetComponent<MeshFilter>().sharedMesh = mesh;
	}

	private void OnValidate()
	{
		int num = allowedValues[0];
		int num2 = Mathf.Abs(subdivisions - num);
		int[] array = allowedValues;
		foreach (int num3 in array)
		{
			int num4 = Mathf.Abs(subdivisions - num3);
			if (num4 < num2)
			{
				num = num3;
				num2 = num4;
			}
		}
		subdivisions = num;
		GeneratePlane();
	}

	public IEnumerable<Vector2Int> GetAllTilePositions()
	{
		return activeTiles;
	}

	public bool HasTile(Vector2Int pos)
	{
		return activeTiles.Contains(pos);
	}

	public Vector3 GetTileCenter(int tileX, int tileZ)
	{
		float num = CellSize;
		Vector3 position = new Vector3((float)tileX * num + num * 0.5f, 0f, (float)tileZ * num + num * 0.5f);
		return base.transform.TransformPoint(position);
	}

	public void Expand(int tileX, int tileZ)
	{
		Vector2Int vector2Int = new Vector2Int(tileX, tileZ);
		if (!activeTiles.Contains(vector2Int))
		{
			AddTile(vector2Int);
		}
	}

	public void RemoveTile(int tileX, int tileZ)
	{
		Vector2Int vector2Int = new Vector2Int(tileX, tileZ);
		if (activeTiles.Contains(vector2Int))
		{
			RemoveTile(vector2Int);
		}
	}
}
