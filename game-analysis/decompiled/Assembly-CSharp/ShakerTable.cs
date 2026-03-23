using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000D6 RID: 214
public class ShakerTable : MonoBehaviour
{
	// Token: 0x060005B1 RID: 1457 RVA: 0x0001DC60 File Offset: 0x0001BE60
	private void Update()
	{
		this._sievingList.RemoveAll((OrePiece item) => item == null || !item.isActiveAndEnabled);
		foreach (OrePiece orePiece in this._sievingList)
		{
			orePiece.AddSieveValue(Time.deltaTime / this.SievingTime);
		}
	}

	// Token: 0x060005B2 RID: 1458 RVA: 0x0001DCE8 File Offset: 0x0001BEE8
	private void OnTriggerEnter(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			this._sievingList.Add(componentInParent);
		}
	}

	// Token: 0x060005B3 RID: 1459 RVA: 0x0001DD14 File Offset: 0x0001BF14
	private void OnTriggerExit(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			this._sievingList.Remove(componentInParent);
		}
	}

	// Token: 0x040006EA RID: 1770
	public float SievingTime = 6f;

	// Token: 0x040006EB RID: 1771
	private List<OrePiece> _sievingList = new List<OrePiece>();
}
