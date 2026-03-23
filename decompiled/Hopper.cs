using System.Collections;
using UnityEngine;

public class Hopper : MonoBehaviour
{
	private void OnEnable()
	{
		StartCoroutine(WaitThenCheckForChuteTopBelow());
	}

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
		if (Physics.Raycast(vector, down, out var hitInfo, num, Singleton<BuildingManager>.Instance.BuildingPlacementCollisionLayers))
		{
			ChuteTop componentInParent = hitInfo.collider.GetComponentInParent<ChuteTop>();
			if (componentInParent != null)
			{
				componentInParent.ConvertToHopperVersion();
			}
		}
	}
}
