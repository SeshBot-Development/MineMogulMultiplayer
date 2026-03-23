using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000041 RID: 65
public class DepositBoxCrusher : MonoBehaviour
{
	// Token: 0x060001BB RID: 443 RVA: 0x000099C0 File Offset: 0x00007BC0
	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			base.StartCoroutine(this.CrushOre(component));
		}
	}

	// Token: 0x060001BC RID: 444 RVA: 0x000099EB File Offset: 0x00007BEB
	private IEnumerator CrushOre(OrePiece ore)
	{
		yield return new WaitForSeconds(0.3f);
		if (ore != null && ore.TryConvertToCrushed())
		{
			yield return new WaitForFixedUpdate();
			yield return new WaitForFixedUpdate();
			PhysicsUtils.SimpleExplosion(base.transform.position, 0.5f, 6f, 0.5f);
		}
		yield break;
	}
}
