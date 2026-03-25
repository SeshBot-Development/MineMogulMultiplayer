using System.Collections.Generic;
using UnityEngine;

public class SorterMachine : MonoBehaviour
{
	public bool MaintainObjectYValue;

	public Transform PassTransform;

	public Transform PassTransformLarge;

	public Transform FailTransform;

	public Transform FailTransformLarge;

	public float BaseFlowRate = 2f;

	public SoundDefinition PassthroughSound;

	public SoundDefinition SortSound;

	public Transform AudioPosition;

	public SorterFilterBasket Filter;

	private Queue<Collider> _colliderQueue = new Queue<Collider>();

	private TimeUntil _timeUntilCanSort;

	private void FixedUpdate()
	{
		if (_colliderQueue.Count > 0 && _timeUntilCanSort < 0)
		{
			OrePiece component = _colliderQueue.Dequeue().GetComponent<OrePiece>();
			if (component != null)
			{
				ProcessOre(component);
			}
			if (_colliderQueue.Count > 0)
			{
				_timeUntilCanSort = GetCooldown(_colliderQueue.Peek());
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			if (_colliderQueue.Count == 0)
			{
				_timeUntilCanSort = GetCooldown(component.PieceType);
			}
			_colliderQueue.Enqueue(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.GetComponent<OrePiece>() != null)
		{
			_colliderQueue = new Queue<Collider>(new List<Collider>(_colliderQueue).FindAll((Collider o) => (object)o != other));
		}
	}

	private void OnDisable()
	{
	}

	private void ProcessOre(OrePiece ore)
	{
		if (ore == null)
		{
			return;
		}
		Rigidbody component = ore.GetComponent<Rigidbody>();
		if (component != null)
		{
			if (ore.CurrentMagnetTool != null)
			{
				ore.CurrentMagnetTool.DetachBody(component);
			}
			component.interpolation = RigidbodyInterpolation.None;
		}
		bool flag = ore.PieceType == PieceType.Plate || ore.PieceType == PieceType.Rod || ore.PieceType == PieceType.Pipe || ore.PieceType == PieceType.Gear || ore.PieceType == PieceType.Geode || ore.PieceType == PieceType.OreCluster;
		if (Filter.OreMatchesFilter(ore))
		{
			Vector3 position = (flag ? PassTransformLarge.position : PassTransform.position);
			if (MaintainObjectYValue)
			{
				position.y = ore.transform.position.y;
			}
			ore.transform.position = position;
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(PassthroughSound, AudioPosition.position);
		}
		else
		{
			Vector3 position2 = (flag ? FailTransformLarge.position : FailTransform.position);
			if (MaintainObjectYValue)
			{
				position2.y = ore.transform.position.y;
			}
			ore.transform.position = position2;
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(SortSound, AudioPosition.position);
		}
	}

	private float GetCooldown(Collider collider)
	{
		OrePiece component = collider.GetComponent<OrePiece>();
		if (component != null)
		{
			return GetCooldown(component.PieceType);
		}
		return GetCooldown(PieceType.INVALID);
	}

	private float GetCooldown(PieceType pieceType)
	{
		return pieceType switch
		{
			PieceType.Ore => 1f / BaseFlowRate, 
			PieceType.Crushed => 0.5f / BaseFlowRate, 
			PieceType.Ingot => 1.25f / BaseFlowRate, 
			PieceType.Gear => 1.25f / BaseFlowRate, 
			PieceType.Plate => 1.5f / BaseFlowRate, 
			PieceType.Pipe => 1.5f / BaseFlowRate, 
			PieceType.Rod => 1.5f / BaseFlowRate, 
			PieceType.OreCluster => 1.5f / BaseFlowRate, 
			_ => 1f / BaseFlowRate, 
		};
	}
}
