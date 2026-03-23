using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000026 RID: 38
public class ChuteTop : MonoBehaviour
{
	// Token: 0x06000129 RID: 297 RVA: 0x000072C4 File Offset: 0x000054C4
	private void OnEnable()
	{
		base.StartCoroutine(this.WaitThenCheckForHopperAbove());
	}

	// Token: 0x0600012A RID: 298 RVA: 0x000072D3 File Offset: 0x000054D3
	public IEnumerator WaitThenCheckForHopperAbove()
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
		Vector3 vector = base.transform.position + Vector3.up * 0.25f;
		Vector3 up = Vector3.up;
		float num = 1f;
		Debug.DrawRay(vector, up * num, Color.yellow, 5f);
		RaycastHit raycastHit;
		if (Physics.Raycast(vector, up, out raycastHit, num, Singleton<BuildingManager>.Instance.BuildingPlacementCollisionLayers) && raycastHit.collider.GetComponentInParent<Hopper>() != null)
		{
			this.ConvertToHopperVersion();
		}
		yield break;
	}

	// Token: 0x0600012B RID: 299 RVA: 0x000072E2 File Offset: 0x000054E2
	public void ConvertToHopperVersion()
	{
		Object.Instantiate<GameObject>(this.HopperChuteVersionPrefab, base.transform.position, base.transform.rotation);
		Object.Destroy(base.gameObject);
	}

	// Token: 0x04000124 RID: 292
	public GameObject HopperChuteVersionPrefab;
}
