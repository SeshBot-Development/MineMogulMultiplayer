using System;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorRenderer : MonoBehaviour
{
	[Serializable]
	public class ConveyorMeshSet
	{
		public Mesh Mesh;

		public Material[] Materials;
	}

	public ConveyorMeshSet[] ConveyorMeshSets;

	private readonly Dictionary<int, List<Matrix4x4>> _meshBatches = new Dictionary<int, List<Matrix4x4>>();

	private const int BatchSize = 1023;

	private void LateUpdate()
	{
		if (ConveyorBatchRenderingComponent.NeedsUpdate)
		{
			_meshBatches.Clear();
			foreach (ConveyorBatchRenderingComponent allConveyor in ConveyorBatchRenderingComponent.AllConveyors)
			{
				if (!_meshBatches.TryGetValue(allConveyor.MeshIndex, out var value))
				{
					value = new List<Matrix4x4>();
					_meshBatches[allConveyor.MeshIndex] = value;
				}
				value.Add(allConveyor.CachedMatrix);
			}
			ConveyorBatchRenderingComponent.NeedsUpdate = false;
		}
		foreach (KeyValuePair<int, List<Matrix4x4>> meshBatch in _meshBatches)
		{
			int key = meshBatch.Key;
			List<Matrix4x4> value2 = meshBatch.Value;
			ConveyorMeshSet conveyorMeshSet = ConveyorMeshSets[key];
			for (int i = 0; i < conveyorMeshSet.Materials.Length; i++)
			{
				DrawBatched(value2, conveyorMeshSet.Mesh, i, conveyorMeshSet.Materials[i]);
			}
		}
	}

	private void DrawBatched(List<Matrix4x4> matrices, Mesh mesh, int submeshIndex, Material material)
	{
		for (int i = 0; i < matrices.Count; i += 1023)
		{
			int count = Mathf.Min(1023, matrices.Count - i);
			Graphics.DrawMeshInstanced(mesh, submeshIndex, material, matrices.GetRange(i, count));
		}
	}
}
