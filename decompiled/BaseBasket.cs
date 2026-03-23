using System.Collections.Generic;
using UnityEngine;

public class BaseBasket : MonoBehaviour
{
	protected List<OrePiece> _basketOreList = new List<OrePiece>();

	public List<OrePiece> GetOrePiecesInFilter()
	{
		return _basketOreList;
	}

	protected virtual void OnTriggerEnter(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null && !_basketOreList.Contains(componentInParent))
		{
			_basketOreList.Add(componentInParent);
			componentInParent.BasketsThisIsInside.Add(this);
		}
	}

	protected virtual void OnTriggerExit(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			_basketOreList.Remove(componentInParent);
			componentInParent.BasketsThisIsInside.Remove(this);
		}
	}

	protected virtual void OnDisable()
	{
		foreach (OrePiece basketOre in _basketOreList)
		{
			basketOre.BasketsThisIsInside.Remove(this);
		}
		_basketOreList.Clear();
	}
}
