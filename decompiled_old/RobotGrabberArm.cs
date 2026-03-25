using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RobotGrabberArm : MonoBehaviour
{
	private HashSet<OrePiece> _orePiecesInRange = new HashSet<OrePiece>();

	public ResourceType FilterResourceType;

	public PieceType FilterPieceType;

	public Transform Origin;

	public Transform TargetOrePiece;

	public Transform IKTarget;

	public Transform TargetPosition;

	public float moveSpeed = 5f;

	public float rotateSpeed = 10f;

	public float grabDistance = 0.2f;

	public float releaseDistance = 0.1f;

	public float arcHeight = 0.5f;

	private bool _isGrabbing;

	private Rigidbody _grabbedRigidbody;

	private Vector3 _grabPosition;

	private float _grabProgress;

	private void Update()
	{
		if (TargetOrePiece == null)
		{
			return;
		}
		Vector3 vector = TargetOrePiece.position - Origin.position;
		if (!_isGrabbing && vector.magnitude > 3f)
		{
			DropObject();
			return;
		}
		Vector3 normalized = vector.normalized;
		Vector3 target = TargetOrePiece.position - normalized * 0.2f;
		IKTarget.transform.position = Vector3.MoveTowards(IKTarget.transform.position, target, moveSpeed * Time.deltaTime);
		if (normalized != Vector3.zero)
		{
			Quaternion b = Quaternion.LookRotation(normalized, Vector3.forward);
			IKTarget.transform.rotation = Quaternion.Slerp(IKTarget.transform.rotation, b, rotateSpeed * Time.deltaTime);
		}
		if (!_isGrabbing && Vector3.Distance(IKTarget.position, TargetOrePiece.position) < grabDistance)
		{
			_isGrabbing = true;
			_grabbedRigidbody = TargetOrePiece.GetComponent<Rigidbody>();
			if (_grabbedRigidbody != null)
			{
				_grabbedRigidbody.isKinematic = true;
			}
			TargetOrePiece.SetParent(null);
			_grabPosition = TargetOrePiece.position;
			_grabProgress = 0f;
		}
		if (_isGrabbing)
		{
			_grabProgress += Time.deltaTime * moveSpeed / Vector3.Distance(_grabPosition, TargetPosition.position);
			Vector3 arcPosition = GetArcPosition(_grabPosition, TargetPosition.position, arcHeight, _grabProgress);
			TargetOrePiece.position = arcPosition;
			if (Vector3.Distance(arcPosition, TargetPosition.position) < releaseDistance)
			{
				TargetOrePiece.position = TargetPosition.position;
				DropObject();
				SelectNewTarget();
			}
		}
	}

	private void OnDisable()
	{
		DropObject();
	}

	public void DropObject()
	{
		if (_grabbedRigidbody != null)
		{
			_grabbedRigidbody.isKinematic = false;
			_grabbedRigidbody = null;
		}
		if (!(TargetOrePiece == null))
		{
			TargetOrePiece.gameObject.tag = "Grabbable";
			TargetOrePiece = null;
			_isGrabbing = false;
		}
	}

	private Vector3 GetArcPosition(Vector3 start, Vector3 end, float height, float t)
	{
		t = Mathf.Clamp01(t);
		Vector3 vector = Vector3.Lerp(start, end, t);
		float num = 4f * height * t * (1f - t);
		return vector + Vector3.up * num;
	}

	private void SelectNewTarget()
	{
		_orePiecesInRange.RemoveWhere((OrePiece ore) => ore == null);
		IEnumerable<OrePiece> source = _orePiecesInRange.Where((OrePiece ore) => ore.CurrentMagnetTool == null && !ore.CompareTag("MarkedForDestruction"));
		if (source.Count() > 0)
		{
			OrePiece orePiece = source.First();
			TargetOrePiece = orePiece.transform;
			TargetOrePiece.tag = "MarkedForDestruction";
			_orePiecesInRange.Remove(orePiece);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			_orePiecesInRange.Add(component);
			if (TargetOrePiece == null)
			{
				SelectNewTarget();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			_orePiecesInRange.Remove(component);
		}
	}
}
