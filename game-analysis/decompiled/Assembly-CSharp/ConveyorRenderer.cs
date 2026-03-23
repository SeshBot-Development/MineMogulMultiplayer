using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000039 RID: 57
public class ConveyorRenderer : MonoBehaviour
{
	// Token: 0x06000194 RID: 404 RVA: 0x00008A30 File Offset: 0x00006C30
	private void LateUpdate()
	{
		if (ConveyorBatchRenderingComponent.NeedsUpdate)
		{
			this._meshBatches.Clear();
			foreach (ConveyorBatchRenderingComponent conveyorBatchRenderingComponent in ConveyorBatchRenderingComponent.AllConveyors)
			{
				List<Matrix4x4> list;
				if (!this._meshBatches.TryGetValue(conveyorBatchRenderingComponent.MeshIndex, out list))
				{
					list = new List<Matrix4x4>();
					this._meshBatches[conveyorBatchRenderingComponent.MeshIndex] = list;
				}
				list.Add(conveyorBatchRenderingComponent.CachedMatrix);
			}
			ConveyorBatchRenderingComponent.NeedsUpdate = false;
		}
		foreach (KeyValuePair<int, List<Matrix4x4>> keyValuePair in this._meshBatches)
		{
			int key = keyValuePair.Key;
			List<Matrix4x4> value = keyValuePair.Value;
			ConveyorRenderer.ConveyorMeshSet conveyorMeshSet = this.ConveyorMeshSets[key];
			for (int i = 0; i < conveyorMeshSet.Materials.Length; i++)
			{
				this.DrawBatched(value, conveyorMeshSet.Mesh, i, conveyorMeshSet.Materials[i]);
			}
		}
	}

	// Token: 0x06000195 RID: 405 RVA: 0x00008B58 File Offset: 0x00006D58
	private void DrawBatched(List<Matrix4x4> matrices, Mesh mesh, int submeshIndex, Material material)
	{
		for (int i = 0; i < matrices.Count; i += 1023)
		{
			int num = Mathf.Min(1023, matrices.Count - i);
			Graphics.DrawMeshInstanced(mesh, submeshIndex, material, matrices.GetRange(i, num));
		}
	}

	// Token: 0x04000179 RID: 377
	public ConveyorRenderer.ConveyorMeshSet[] ConveyorMeshSets;

	// Token: 0x0400017A RID: 378
	private readonly Dictionary<int, List<Matrix4x4>> _meshBatches = new Dictionary<int, List<Matrix4x4>>();

	// Token: 0x0400017B RID: 379
	private const int BatchSize = 1023;

	// Token: 0x02000128 RID: 296
	[Serializable]
	public class ConveyorMeshSet
	{
		// Token: 0x04000872 RID: 2162
		public Mesh Mesh;

		// Token: 0x04000873 RID: 2163
		public Material[] Materials;
	}
}
