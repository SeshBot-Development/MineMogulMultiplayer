using System.Collections.Generic;
using UnityEngine;

public class ShakerTable : MonoBehaviour
{
	public float SievingTime = 6f;

	private List<OrePiece> _sievingList = new List<OrePiece>();

	private void Update()
	{
		_sievingList.RemoveAll((OrePiece item) => item == null || !item.isActiveAndEnabled);
		foreach (OrePiece sieving in _sievingList)
		{
			sieving.AddSieveValue(Time.deltaTime / SievingTime);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			_sievingList.Add(componentInParent);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			_sievingList.Remove(componentInParent);
		}
	}
}
