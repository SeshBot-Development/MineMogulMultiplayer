using System;
using System.Collections;
using UnityEngine;

// Token: 0x0200002E RID: 46
public class ContractSellTrigger : MonoBehaviour
{
	// Token: 0x0600014D RID: 333 RVA: 0x00007B70 File Offset: 0x00005D70
	private void OnTriggerEnter(Collider other)
	{
		if (Singleton<ContractsManager>.Instance.ActiveContract == null)
		{
			return;
		}
		if (other.CompareTag("MarkedForDestruction"))
		{
			return;
		}
		if (other.attachedRigidbody == null)
		{
			return;
		}
		BoxObject componentInParent = other.GetComponentInParent<BoxObject>();
		if (componentInParent != null)
		{
			Singleton<ContractsManager>.Instance.DepositBox(componentInParent);
		}
	}

	// Token: 0x0600014E RID: 334 RVA: 0x00007BC2 File Offset: 0x00005DC2
	private IEnumerator DelayThenSellBox(BoxObject box)
	{
		box.gameObject.tag = "MarkedForDestruction";
		Transform[] componentsInChildren = box.transform.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.tag = "MarkedForDestruction";
		}
		yield return new WaitForSeconds(2f);
		if (box == null)
		{
			yield break;
		}
		Singleton<ContractsManager>.Instance.DepositBox(box);
		yield break;
	}
}
