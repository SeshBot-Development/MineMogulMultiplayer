using System;
using UnityEngine;

// Token: 0x0200010B RID: 267
[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class VertexPainter : MonoBehaviour
{
	// Token: 0x06000707 RID: 1799 RVA: 0x00023BA8 File Offset: 0x00021DA8
	private void OnEnable()
	{
		MeshFilter component = base.GetComponent<MeshFilter>();
		if (component == null || component.sharedMesh == null)
		{
			return;
		}
		if (this._originalMesh == null)
		{
			this._originalMesh = component.sharedMesh;
		}
		if (this.editOriginalMesh)
		{
			this._meshInstance = this._originalMesh;
			return;
		}
		if (component.sharedMesh == this._originalMesh)
		{
			this._meshInstance = Object.Instantiate<Mesh>(this._originalMesh);
			component.sharedMesh = this._meshInstance;
			return;
		}
		this._meshInstance = component.sharedMesh;
	}

	// Token: 0x06000708 RID: 1800 RVA: 0x00023C40 File Offset: 0x00021E40
	public void PaintAtPosition(Vector3 worldPos, Color color)
	{
		MeshFilter component = base.GetComponent<MeshFilter>();
		if (component == null || component.sharedMesh == null)
		{
			return;
		}
		Mesh sharedMesh = component.sharedMesh;
		Vector3[] vertices = sharedMesh.vertices;
		Color[] array = sharedMesh.colors;
		if (array == null || array.Length != vertices.Length)
		{
			array = new Color[vertices.Length];
		}
		for (int i = 0; i < vertices.Length; i++)
		{
			if (Vector3.Distance(base.transform.TransformPoint(vertices[i]), worldPos) <= this.brushRadius)
			{
				array[i] = Color.Lerp(array[i], color, this.brushStrength);
			}
		}
		sharedMesh.colors = array;
	}

	// Token: 0x06000709 RID: 1801 RVA: 0x00023CEC File Offset: 0x00021EEC
	public void FillMesh(Color color)
	{
		MeshFilter component = base.GetComponent<MeshFilter>();
		if (component == null || component.sharedMesh == null)
		{
			return;
		}
		Mesh sharedMesh = component.sharedMesh;
		Color[] array = new Color[sharedMesh.vertices.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = color;
		}
		sharedMesh.colors = array;
	}

	// Token: 0x0400081C RID: 2076
	public Color paintColor = Color.red;

	// Token: 0x0400081D RID: 2077
	public float brushRadius = 1f;

	// Token: 0x0400081E RID: 2078
	[Range(0f, 1f)]
	public float brushStrength = 1f;

	// Token: 0x0400081F RID: 2079
	public bool editOriginalMesh;

	// Token: 0x04000820 RID: 2080
	[HideInInspector]
	public Vector3 BrushPosition;

	// Token: 0x04000821 RID: 2081
	private Mesh _meshInstance;

	// Token: 0x04000822 RID: 2082
	private Mesh _originalMesh;
}
