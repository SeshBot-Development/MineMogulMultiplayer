using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000053 RID: 83
public class Hopper : MonoBehaviour
{
	// Token: 0x0600022A RID: 554 RVA: 0x0000AF87 File Offset: 0x00009187
	private void OnEnable()
	{
		base.StartCoroutine(this.WaitThenCheckForChuteTopBelow());
	}

	// Token: 0x0600022B RID: 555 RVA: 0x0000AF96 File Offset: 0x00009196
	public IEnumerator WaitThenCheckForChuteTopBelow()
	{
		if (Singleton<BuildingManager>.Instance == null)
		{
			yield break;
		}
		yield return new WaitForEndOfFrame();
		if (!base.enabled)
		{
			yield break;
		}
		Vector3 vector = base.transform.position + Vector3.up * 2f;
		Vector3 down = Vector3.down;
		float num = 1f;
		Debug.DrawRay(vector, down * num, Color.yellow, 5f);
		RaycastHit raycastHit;
		if (Physics.Raycast(vector, down, out raycastHit, num, Singleton<BuildingManager>.Instance.BuildingPlacementCollisionLayers))
		{
			ChuteTop componentInParent = raycastHit.collider.GetComponentInParent<ChuteTop>();
			if (componentInParent != null)
			{
				componentInParent.ConvertToHopperVersion();
			}
		}
		yield break;
	}
}
