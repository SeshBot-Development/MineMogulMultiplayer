using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200003C RID: 60
public class CrusherMachine : MonoBehaviour
{
	// Token: 0x0600019D RID: 413 RVA: 0x00008C54 File Offset: 0x00006E54
	private void Update()
	{
		Vector3 right = Vector3.right;
		float num = this.RotateSpeed * Time.deltaTime;
		this.GrindingPiece1.transform.Rotate(right, num);
		this.GrindingPiece2.transform.Rotate(right, -num);
	}

	// Token: 0x0600019E RID: 414 RVA: 0x00008C9C File Offset: 0x00006E9C
	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null && !this._crushing.Contains(component))
		{
			this._crushing.Add(component);
			base.StartCoroutine(this.CrushOre(component));
		}
	}

	// Token: 0x0600019F RID: 415 RVA: 0x00008CE2 File Offset: 0x00006EE2
	private IEnumerator CrushOre(OrePiece ore)
	{
		yield return new WaitForSeconds(1f);
		if (this._crushing.Contains(ore))
		{
			if (ore != null)
			{
				this._crushing.Remove(ore);
				ore.TryConvertToCrushed();
			}
			else
			{
				this._crushing.RemoveWhere((OrePiece ore) => ore == null);
			}
		}
		yield break;
	}

	// Token: 0x04000184 RID: 388
	public float RotateSpeed = 50f;

	// Token: 0x04000185 RID: 389
	public GameObject GrindingPiece1;

	// Token: 0x04000186 RID: 390
	public GameObject GrindingPiece2;

	// Token: 0x04000187 RID: 391
	private readonly HashSet<OrePiece> _crushing = new HashSet<OrePiece>();
}
