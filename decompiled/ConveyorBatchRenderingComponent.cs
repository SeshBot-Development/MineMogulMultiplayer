using System.Collections.Generic;
using UnityEngine;

public class ConveyorBatchRenderingComponent : MonoBehaviour
{
	public static readonly List<ConveyorBatchRenderingComponent> AllConveyors = new List<ConveyorBatchRenderingComponent>();

	public static bool NeedsUpdate = true;

	public Matrix4x4 CachedMatrix;

	public int MeshIndex;

	private void OnEnable()
	{
		CachedMatrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.lossyScale);
		AllConveyors.Add(this);
		NeedsUpdate = true;
		GetComponent<Renderer>().enabled = false;
	}

	private void OnDisable()
	{
		AllConveyors.Remove(this);
		NeedsUpdate = true;
	}

	public void RefreshMatrix()
	{
		CachedMatrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.lossyScale);
		NeedsUpdate = true;
	}
}
