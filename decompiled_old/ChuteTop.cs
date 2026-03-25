using System.Collections;
using UnityEngine;

public class ChuteTop : MonoBehaviour
{
	public GameObject HopperChuteVersionPrefab;

	private void OnEnable()
	{
		StartCoroutine(WaitThenCheckForHopperAbove());
	}

	public IEnumerator WaitThenCheckForHopperAbove()
	{
		if (Singleton<BuildingManager>.Instance == null)
		{
			yield break;
		}
		yield return new WaitForEndOfFrame();
		if (base.enabled)
		{
			Vector3 vector = base.transform.position + Vector3.up * 0.25f;
			Vector3 up = Vector3.up;
			float num = 1f;
			Debug.DrawRay(vector, up * num, Color.yellow, 5f);
			if (Physics.Raycast(vector, up, out var hitInfo, num, Singleton<BuildingManager>.Instance.BuildingPlacementCollisionLayers) && hitInfo.collider.GetComponentInParent<Hopper>() != null)
			{
				ConvertToHopperVersion();
			}
		}
	}

	public void ConvertToHopperVersion()
	{
		Object.Instantiate(HopperChuteVersionPrefab, base.transform.position, base.transform.rotation);
		Object.Destroy(base.gameObject);
	}
}
