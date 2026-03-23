using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrusherMachine : MonoBehaviour
{
	public float RotateSpeed = 50f;

	public GameObject GrindingPiece1;

	public GameObject GrindingPiece2;

	private readonly HashSet<OrePiece> _crushing = new HashSet<OrePiece>();

	private void Update()
	{
		Vector3 right = Vector3.right;
		float num = RotateSpeed * Time.deltaTime;
		GrindingPiece1.transform.Rotate(right, num);
		GrindingPiece2.transform.Rotate(right, 0f - num);
	}

	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null && !_crushing.Contains(component))
		{
			_crushing.Add(component);
			StartCoroutine(CrushOre(component));
		}
	}

	private IEnumerator CrushOre(OrePiece ore)
	{
		yield return new WaitForSeconds(1f);
		if (!_crushing.Contains(ore))
		{
			yield break;
		}
		if (ore != null)
		{
			_crushing.Remove(ore);
			ore.TryConvertToCrushed();
		}
		else
		{
			_crushing.RemoveWhere((OrePiece orePiece) => orePiece == null);
		}
	}
}
