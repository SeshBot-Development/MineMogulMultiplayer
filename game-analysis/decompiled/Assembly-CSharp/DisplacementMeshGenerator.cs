using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200010A RID: 266
[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DisplacementMeshGenerator : MonoBehaviour
{
	// Token: 0x060006FD RID: 1789 RVA: 0x000237D4 File Offset: 0x000219D4
	public void AddTile(Vector2Int gridPos)
	{
		if (this.activeTiles.Add(gridPos))
		{
			this.GeneratePlane();
		}
	}

	// Token: 0x060006FE RID: 1790 RVA: 0x000237EA File Offset: 0x000219EA
	public void RemoveTile(Vector2Int gridPos)
	{
		if (this.activeTiles.Remove(gridPos))
		{
			this.GeneratePlane();
		}
	}

	// Token: 0x060006FF RID: 1791 RVA: 0x00023800 File Offset: 0x00021A00
	public void GeneratePlane()
	{
		float num = (float)this.CellSize / (float)this.subdivisions;
		int num2 = this.subdivisions + 1;
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		List<Vector2> list3 = new List<Vector2>();
		this.tileVertices.Clear();
		foreach (Vector2Int vector2Int in this.activeTiles)
		{
			Vector3[] array = new Vector3[num2 * num2];
			int count = list.Count;
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					int num3 = j + i * num2;
					float num4 = (float)j * num + (float)(vector2Int.x * this.CellSize);
					float num5 = (float)i * num + (float)(vector2Int.y * this.CellSize);
					Vector3 vector = new Vector3(num4, 0f, num5);
					array[num3] = vector;
					list.Add(vector);
					list3.Add(new Vector2((float)j / (float)this.subdivisions, (float)i / (float)this.subdivisions));
				}
			}
			for (int k = 0; k < this.subdivisions; k++)
			{
				for (int l = 0; l < this.subdivisions; l++)
				{
					int num6 = l + k * num2;
					list2.Add(count + num6);
					list2.Add(count + num6 + num2);
					list2.Add(count + num6 + 1);
					list2.Add(count + num6 + 1);
					list2.Add(count + num6 + num2);
					list2.Add(count + num6 + num2 + 1);
				}
			}
			this.tileVertices[vector2Int] = array;
		}
		Mesh mesh = new Mesh();
		mesh.name = "DisplacementMesh";
		mesh.vertices = list.ToArray();
		mesh.triangles = list2.ToArray();
		mesh.uv = list3.ToArray();
		mesh.RecalculateNormals();
		base.GetComponent<MeshFilter>().sharedMesh = mesh;
	}

	// Token: 0x06000700 RID: 1792 RVA: 0x00023A30 File Offset: 0x00021C30
	private void OnValidate()
	{
		int num = this.allowedValues[0];
		int num2 = Mathf.Abs(this.subdivisions - num);
		foreach (int num3 in this.allowedValues)
		{
			int num4 = Mathf.Abs(this.subdivisions - num3);
			if (num4 < num2)
			{
				num = num3;
				num2 = num4;
			}
		}
		this.subdivisions = num;
		this.GeneratePlane();
	}

	// Token: 0x06000701 RID: 1793 RVA: 0x00023A96 File Offset: 0x00021C96
	public IEnumerable<Vector2Int> GetAllTilePositions()
	{
		return this.activeTiles;
	}

	// Token: 0x06000702 RID: 1794 RVA: 0x00023A9E File Offset: 0x00021C9E
	public bool HasTile(Vector2Int pos)
	{
		return this.activeTiles.Contains(pos);
	}

	// Token: 0x06000703 RID: 1795 RVA: 0x00023AAC File Offset: 0x00021CAC
	public Vector3 GetTileCenter(int tileX, int tileZ)
	{
		float num = (float)this.CellSize;
		Vector3 vector = new Vector3((float)tileX * num + num * 0.5f, 0f, (float)tileZ * num + num * 0.5f);
		return base.transform.TransformPoint(vector);
	}

	// Token: 0x06000704 RID: 1796 RVA: 0x00023AF4 File Offset: 0x00021CF4
	public void Expand(int tileX, int tileZ)
	{
		Vector2Int vector2Int = new Vector2Int(tileX, tileZ);
		if (this.activeTiles.Contains(vector2Int))
		{
			return;
		}
		this.AddTile(vector2Int);
	}

	// Token: 0x06000705 RID: 1797 RVA: 0x00023B20 File Offset: 0x00021D20
	public void RemoveTile(int tileX, int tileZ)
	{
		Vector2Int vector2Int = new Vector2Int(tileX, tileZ);
		if (!this.activeTiles.Contains(vector2Int))
		{
			return;
		}
		this.RemoveTile(vector2Int);
	}

	// Token: 0x04000817 RID: 2071
	[Tooltip("Allowed values: 1, 2, 4, 8, 16")]
	[Range(1f, 16f)]
	public int subdivisions = 4;

	// Token: 0x04000818 RID: 2072
	public int CellSize = 1;

	// Token: 0x04000819 RID: 2073
	private readonly int[] allowedValues = new int[] { 1, 2, 4, 8, 16 };

	// Token: 0x0400081A RID: 2074
	[SerializeField]
	public HashSet<Vector2Int> activeTiles = new HashSet<Vector2Int>
	{
		new Vector2Int(0, 0)
	};

	// Token: 0x0400081B RID: 2075
	private Dictionary<Vector2Int, Vector3[]> tileVertices = new Dictionary<Vector2Int, Vector3[]>();
}
