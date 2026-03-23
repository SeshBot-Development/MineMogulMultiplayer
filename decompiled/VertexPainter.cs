using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class VertexPainter : MonoBehaviour
{
	public Color paintColor = Color.red;

	public float brushRadius = 1f;

	[Range(0f, 1f)]
	public float brushStrength = 1f;

	public bool editOriginalMesh;

	[HideInInspector]
	public Vector3 BrushPosition;

	private Mesh _meshInstance;

	private Mesh _originalMesh;

	private void OnEnable()
	{
		MeshFilter component = GetComponent<MeshFilter>();
		if (!(component == null) && !(component.sharedMesh == null))
		{
			if (_originalMesh == null)
			{
				_originalMesh = component.sharedMesh;
			}
			if (editOriginalMesh)
			{
				_meshInstance = _originalMesh;
			}
			else if (component.sharedMesh == _originalMesh)
			{
				_meshInstance = Object.Instantiate(_originalMesh);
				component.sharedMesh = _meshInstance;
			}
			else
			{
				_meshInstance = component.sharedMesh;
			}
		}
	}

	public void PaintAtPosition(Vector3 worldPos, Color color)
	{
		MeshFilter component = GetComponent<MeshFilter>();
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
			if (Vector3.Distance(base.transform.TransformPoint(vertices[i]), worldPos) <= brushRadius)
			{
				array[i] = Color.Lerp(array[i], color, brushStrength);
			}
		}
		sharedMesh.colors = array;
	}

	public void FillMesh(Color color)
	{
		MeshFilter component = GetComponent<MeshFilter>();
		if (!(component == null) && !(component.sharedMesh == null))
		{
			Mesh sharedMesh = component.sharedMesh;
			Color[] array = new Color[sharedMesh.vertices.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = color;
			}
			sharedMesh.colors = array;
		}
	}
}
