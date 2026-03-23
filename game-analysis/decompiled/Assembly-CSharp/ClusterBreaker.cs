using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000027 RID: 39
public class ClusterBreaker : MonoBehaviour
{
	// Token: 0x0600012D RID: 301 RVA: 0x00007319 File Offset: 0x00005519
	private void Awake()
	{
		this._movingPieceStartingPos = this.MovingPiece.transform.localPosition;
	}

	// Token: 0x0600012E RID: 302 RVA: 0x00007334 File Offset: 0x00005534
	private void Update()
	{
		float num = Mathf.Sin(Time.time * this.speed) * this.amplitude;
		this.MovingPiece.transform.localPosition = this._movingPieceStartingPos + Vector3.up * num;
	}

	// Token: 0x0600012F RID: 303 RVA: 0x00007380 File Offset: 0x00005580
	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null && !this._crushing.Contains(component))
		{
			this._crushing.Add(component);
			base.StartCoroutine(this.CrushOre(component));
		}
	}

	// Token: 0x06000130 RID: 304 RVA: 0x000073C6 File Offset: 0x000055C6
	private IEnumerator CrushOre(OrePiece ore)
	{
		float num = (float)Random.Range(1, 2);
		yield return new WaitForSeconds(num);
		if (this._crushing.Contains(ore))
		{
			if (ore != null)
			{
				this._crushing.Remove(ore);
				ore.CompleteClusterBreaking();
			}
			else
			{
				this._crushing.RemoveWhere((OrePiece ore) => ore == null);
			}
		}
		yield break;
	}

	// Token: 0x04000125 RID: 293
	public GameObject MovingPiece;

	// Token: 0x04000126 RID: 294
	public float amplitude = 0.03f;

	// Token: 0x04000127 RID: 295
	public float speed = 15f;

	// Token: 0x04000128 RID: 296
	private readonly HashSet<OrePiece> _crushing = new HashSet<OrePiece>();

	// Token: 0x04000129 RID: 297
	private Vector3 _movingPieceStartingPos;
}
