using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000E0 RID: 224
public class SorterMachine : MonoBehaviour
{
	// Token: 0x06000602 RID: 1538 RVA: 0x0001F53C File Offset: 0x0001D73C
	private void FixedUpdate()
	{
		if (this._colliderQueue.Count > 0 && (in this._timeUntilCanSort) < 0)
		{
			OrePiece component = this._colliderQueue.Dequeue().GetComponent<OrePiece>();
			if (component != null)
			{
				this.ProcessOre(component);
			}
			if (this._colliderQueue.Count > 0)
			{
				this._timeUntilCanSort = this.GetCooldown(this._colliderQueue.Peek());
			}
		}
	}

	// Token: 0x06000603 RID: 1539 RVA: 0x0001F5B0 File Offset: 0x0001D7B0
	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			if (this._colliderQueue.Count == 0)
			{
				this._timeUntilCanSort = this.GetCooldown(component.PieceType);
			}
			this._colliderQueue.Enqueue(other);
		}
	}

	// Token: 0x06000604 RID: 1540 RVA: 0x0001F600 File Offset: 0x0001D800
	private void OnTriggerExit(Collider other)
	{
		if (other.GetComponent<OrePiece>() != null)
		{
			this._colliderQueue = new Queue<Collider>(new List<Collider>(this._colliderQueue).FindAll((Collider o) => o != other));
		}
	}

	// Token: 0x06000605 RID: 1541 RVA: 0x0001F654 File Offset: 0x0001D854
	private void OnDisable()
	{
	}

	// Token: 0x06000606 RID: 1542 RVA: 0x0001F658 File Offset: 0x0001D858
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
		if (this.Filter.OreMatchesFilter(ore))
		{
			Vector3 vector = (flag ? this.PassTransformLarge.position : this.PassTransform.position);
			if (this.MaintainObjectYValue)
			{
				vector.y = ore.transform.position.y;
			}
			ore.transform.position = vector;
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.PassthroughSound, this.AudioPosition.position, 1f, 1f, true, false);
			return;
		}
		Vector3 vector2 = (flag ? this.FailTransformLarge.position : this.FailTransform.position);
		if (this.MaintainObjectYValue)
		{
			vector2.y = ore.transform.position.y;
		}
		ore.transform.position = vector2;
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.SortSound, this.AudioPosition.position, 1f, 1f, true, false);
	}

	// Token: 0x06000607 RID: 1543 RVA: 0x0001F7C8 File Offset: 0x0001D9C8
	private float GetCooldown(Collider collider)
	{
		OrePiece component = collider.GetComponent<OrePiece>();
		if (component != null)
		{
			return this.GetCooldown(component.PieceType);
		}
		return this.GetCooldown(PieceType.INVALID);
	}

	// Token: 0x06000608 RID: 1544 RVA: 0x0001F7FC File Offset: 0x0001D9FC
	private float GetCooldown(PieceType pieceType)
	{
		switch (pieceType)
		{
		case PieceType.Ore:
			return 1f / this.BaseFlowRate;
		case PieceType.Crushed:
			return 0.5f / this.BaseFlowRate;
		case PieceType.Ingot:
			return 1.25f / this.BaseFlowRate;
		case PieceType.Plate:
			return 1.5f / this.BaseFlowRate;
		case PieceType.Pipe:
			return 1.5f / this.BaseFlowRate;
		case PieceType.Rod:
			return 1.5f / this.BaseFlowRate;
		case PieceType.Gear:
			return 1.25f / this.BaseFlowRate;
		case PieceType.OreCluster:
			return 1.5f / this.BaseFlowRate;
		}
		return 1f / this.BaseFlowRate;
	}

	// Token: 0x04000735 RID: 1845
	public bool MaintainObjectYValue;

	// Token: 0x04000736 RID: 1846
	public Transform PassTransform;

	// Token: 0x04000737 RID: 1847
	public Transform PassTransformLarge;

	// Token: 0x04000738 RID: 1848
	public Transform FailTransform;

	// Token: 0x04000739 RID: 1849
	public Transform FailTransformLarge;

	// Token: 0x0400073A RID: 1850
	public float BaseFlowRate = 2f;

	// Token: 0x0400073B RID: 1851
	public SoundDefinition PassthroughSound;

	// Token: 0x0400073C RID: 1852
	public SoundDefinition SortSound;

	// Token: 0x0400073D RID: 1853
	public Transform AudioPosition;

	// Token: 0x0400073E RID: 1854
	public SorterFilterBasket Filter;

	// Token: 0x0400073F RID: 1855
	private Queue<Collider> _colliderQueue = new Queue<Collider>();

	// Token: 0x04000740 RID: 1856
	private TimeUntil _timeUntilCanSort;
}
