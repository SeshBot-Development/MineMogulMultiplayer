using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x020000AA RID: 170
public class RobotGrabberArm : MonoBehaviour
{
	// Token: 0x060004B2 RID: 1202 RVA: 0x000193D0 File Offset: 0x000175D0
	private void Update()
	{
		if (this.TargetOrePiece == null)
		{
			return;
		}
		Vector3 vector = this.TargetOrePiece.position - this.Origin.position;
		if (!this._isGrabbing && vector.magnitude > 3f)
		{
			this.DropObject();
			return;
		}
		Vector3 normalized = vector.normalized;
		Vector3 vector2 = this.TargetOrePiece.position - normalized * 0.2f;
		this.IKTarget.transform.position = Vector3.MoveTowards(this.IKTarget.transform.position, vector2, this.moveSpeed * Time.deltaTime);
		if (normalized != Vector3.zero)
		{
			Quaternion quaternion = Quaternion.LookRotation(normalized, Vector3.forward);
			this.IKTarget.transform.rotation = Quaternion.Slerp(this.IKTarget.transform.rotation, quaternion, this.rotateSpeed * Time.deltaTime);
		}
		if (!this._isGrabbing && Vector3.Distance(this.IKTarget.position, this.TargetOrePiece.position) < this.grabDistance)
		{
			this._isGrabbing = true;
			this._grabbedRigidbody = this.TargetOrePiece.GetComponent<Rigidbody>();
			if (this._grabbedRigidbody != null)
			{
				this._grabbedRigidbody.isKinematic = true;
			}
			this.TargetOrePiece.SetParent(null);
			this._grabPosition = this.TargetOrePiece.position;
			this._grabProgress = 0f;
		}
		if (this._isGrabbing)
		{
			this._grabProgress += Time.deltaTime * this.moveSpeed / Vector3.Distance(this._grabPosition, this.TargetPosition.position);
			Vector3 arcPosition = this.GetArcPosition(this._grabPosition, this.TargetPosition.position, this.arcHeight, this._grabProgress);
			this.TargetOrePiece.position = arcPosition;
			if (Vector3.Distance(arcPosition, this.TargetPosition.position) < this.releaseDistance)
			{
				this.TargetOrePiece.position = this.TargetPosition.position;
				this.DropObject();
				this.SelectNewTarget();
			}
		}
	}

	// Token: 0x060004B3 RID: 1203 RVA: 0x000195F4 File Offset: 0x000177F4
	private void OnDisable()
	{
		this.DropObject();
	}

	// Token: 0x060004B4 RID: 1204 RVA: 0x000195FC File Offset: 0x000177FC
	public void DropObject()
	{
		if (this._grabbedRigidbody != null)
		{
			this._grabbedRigidbody.isKinematic = false;
			this._grabbedRigidbody = null;
		}
		if (this.TargetOrePiece == null)
		{
			return;
		}
		this.TargetOrePiece.gameObject.tag = "Grabbable";
		this.TargetOrePiece = null;
		this._isGrabbing = false;
	}

	// Token: 0x060004B5 RID: 1205 RVA: 0x0001965C File Offset: 0x0001785C
	private Vector3 GetArcPosition(Vector3 start, Vector3 end, float height, float t)
	{
		t = Mathf.Clamp01(t);
		Vector3 vector = Vector3.Lerp(start, end, t);
		float num = 4f * height * t * (1f - t);
		return vector + Vector3.up * num;
	}

	// Token: 0x060004B6 RID: 1206 RVA: 0x000196A0 File Offset: 0x000178A0
	private void SelectNewTarget()
	{
		this._orePiecesInRange.RemoveWhere((OrePiece ore) => ore == null);
		IEnumerable<OrePiece> enumerable = this._orePiecesInRange.Where((OrePiece ore) => ore.CurrentMagnetTool == null && !ore.CompareTag("MarkedForDestruction"));
		if (enumerable.Count<OrePiece>() > 0)
		{
			OrePiece orePiece = enumerable.First<OrePiece>();
			this.TargetOrePiece = orePiece.transform;
			this.TargetOrePiece.tag = "MarkedForDestruction";
			this._orePiecesInRange.Remove(orePiece);
		}
	}

	// Token: 0x060004B7 RID: 1207 RVA: 0x0001973C File Offset: 0x0001793C
	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			this._orePiecesInRange.Add(component);
			if (this.TargetOrePiece == null)
			{
				this.SelectNewTarget();
			}
		}
	}

	// Token: 0x060004B8 RID: 1208 RVA: 0x0001977C File Offset: 0x0001797C
	private void OnTriggerExit(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			this._orePiecesInRange.Remove(component);
		}
	}

	// Token: 0x0400053D RID: 1341
	private HashSet<OrePiece> _orePiecesInRange = new HashSet<OrePiece>();

	// Token: 0x0400053E RID: 1342
	public ResourceType FilterResourceType;

	// Token: 0x0400053F RID: 1343
	public PieceType FilterPieceType;

	// Token: 0x04000540 RID: 1344
	public Transform Origin;

	// Token: 0x04000541 RID: 1345
	public Transform TargetOrePiece;

	// Token: 0x04000542 RID: 1346
	public Transform IKTarget;

	// Token: 0x04000543 RID: 1347
	public Transform TargetPosition;

	// Token: 0x04000544 RID: 1348
	public float moveSpeed = 5f;

	// Token: 0x04000545 RID: 1349
	public float rotateSpeed = 10f;

	// Token: 0x04000546 RID: 1350
	public float grabDistance = 0.2f;

	// Token: 0x04000547 RID: 1351
	public float releaseDistance = 0.1f;

	// Token: 0x04000548 RID: 1352
	public float arcHeight = 0.5f;

	// Token: 0x04000549 RID: 1353
	private bool _isGrabbing;

	// Token: 0x0400054A RID: 1354
	private Rigidbody _grabbedRigidbody;

	// Token: 0x0400054B RID: 1355
	private Vector3 _grabPosition;

	// Token: 0x0400054C RID: 1356
	private float _grabProgress;
}
