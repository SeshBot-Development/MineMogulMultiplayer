using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000008 RID: 8
public class BaseBasket : MonoBehaviour
{
	// Token: 0x06000030 RID: 48 RVA: 0x00002C3C File Offset: 0x00000E3C
	public List<OrePiece> GetOrePiecesInFilter()
	{
		return this._basketOreList;
	}

	// Token: 0x06000031 RID: 49 RVA: 0x00002C44 File Offset: 0x00000E44
	protected virtual void OnTriggerEnter(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null && !this._basketOreList.Contains(componentInParent))
		{
			this._basketOreList.Add(componentInParent);
			componentInParent.BasketsThisIsInside.Add(this);
		}
	}

	// Token: 0x06000032 RID: 50 RVA: 0x00002C88 File Offset: 0x00000E88
	protected virtual void OnTriggerExit(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			this._basketOreList.Remove(componentInParent);
			componentInParent.BasketsThisIsInside.Remove(this);
		}
	}

	// Token: 0x06000033 RID: 51 RVA: 0x00002CC0 File Offset: 0x00000EC0
	protected virtual void OnDisable()
	{
		foreach (OrePiece orePiece in this._basketOreList)
		{
			orePiece.BasketsThisIsInside.Remove(this);
		}
		this._basketOreList.Clear();
	}

	// Token: 0x0400003A RID: 58
	protected List<OrePiece> _basketOreList = new List<OrePiece>();
}
