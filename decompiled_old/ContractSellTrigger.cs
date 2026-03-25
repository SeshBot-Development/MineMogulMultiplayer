using System.Collections;
using UnityEngine;

public class ContractSellTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (Singleton<ContractsManager>.Instance.ActiveContract != null && !other.CompareTag("MarkedForDestruction") && !(other.attachedRigidbody == null))
		{
			BoxObject componentInParent = other.GetComponentInParent<BoxObject>();
			if (componentInParent != null)
			{
				Singleton<ContractsManager>.Instance.DepositBox(componentInParent);
			}
		}
	}

	private IEnumerator DelayThenSellBox(BoxObject box)
	{
		box.gameObject.tag = "MarkedForDestruction";
		Transform[] componentsInChildren = box.transform.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.tag = "MarkedForDestruction";
		}
		yield return new WaitForSeconds(2f);
		if (!(box == null))
		{
			Singleton<ContractsManager>.Instance.DepositBox(box);
		}
	}
}
