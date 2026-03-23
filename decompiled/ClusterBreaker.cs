using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterBreaker : MonoBehaviour
{
	public GameObject MovingPiece;

	public float amplitude = 0.03f;

	public float speed = 15f;

	private readonly HashSet<OrePiece> _crushing = new HashSet<OrePiece>();

	private Vector3 _movingPieceStartingPos;

	private void Awake()
	{
		_movingPieceStartingPos = MovingPiece.transform.localPosition;
	}

	private void Update()
	{
		float num = Mathf.Sin(Time.time * speed) * amplitude;
		MovingPiece.transform.localPosition = _movingPieceStartingPos + Vector3.up * num;
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
		float seconds = Random.Range(1, 2);
		yield return new WaitForSeconds(seconds);
		if (!_crushing.Contains(ore))
		{
			yield break;
		}
		if (ore != null)
		{
			_crushing.Remove(ore);
			ore.CompleteClusterBreaking();
		}
		else
		{
			_crushing.RemoveWhere((OrePiece orePiece) => orePiece == null);
		}
	}
}
