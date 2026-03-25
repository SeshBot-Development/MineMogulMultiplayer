using UnityEngine;

public class BaseModularSupports : MonoBehaviour
{
	protected BuildingObject _buildingObject;

	protected virtual void Start()
	{
		_buildingObject = GetComponentInParent<BuildingObject>();
		SpawnSupports();
	}

	public virtual void SpawnSupports()
	{
	}

	public virtual void RespawnSupports(bool RespawnNextFrame = false)
	{
	}
}
