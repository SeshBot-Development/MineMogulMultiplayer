using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000E7 RID: 231
public class ThreadingLathe : MonoBehaviour
{
	// Token: 0x0600062E RID: 1582 RVA: 0x000204BC File Offset: 0x0001E6BC
	private void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = this._lightRenderer.sharedMaterials;
		sharedMaterials[3] = material;
		this._lightRenderer.sharedMaterials = sharedMaterials;
	}

	// Token: 0x0600062F RID: 1583 RVA: 0x000204E5 File Offset: 0x0001E6E5
	private void Awake()
	{
		this._rodMeshFilter = this.RodRenderer.GetComponent<MeshFilter>();
		this._dummyWholePartMeshFilter = this.DummyWholePartRenderer.GetComponent<MeshFilter>();
		this.RodRenderer.enabled = false;
	}

	// Token: 0x06000630 RID: 1584 RVA: 0x00020518 File Offset: 0x0001E718
	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null && component.RodPrefab != null)
		{
			if (!this._isProcessing)
			{
				this._isProcessing = true;
				base.StartCoroutine(this.ExtrudeRod(component));
				return;
			}
			this._waitingList.Add(component);
		}
	}

	// Token: 0x06000631 RID: 1585 RVA: 0x00020570 File Offset: 0x0001E770
	private void OnTriggerExit(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			this._waitingList.Remove(component);
		}
	}

	// Token: 0x06000632 RID: 1586 RVA: 0x0002059A File Offset: 0x0001E79A
	private IEnumerator ExtrudeRod(OrePiece orePiece)
	{
		Material sharedMaterial = orePiece.GetComponent<Renderer>().sharedMaterial;
		Mesh sharedMesh = orePiece.GetComponent<MeshFilter>().sharedMesh;
		OrePiece rod = orePiece.ConvertToThreaded();
		if (rod != null)
		{
			rod.gameObject.SetActive(false);
			this.ProcessRodSoundPlayer.PlaySound(this.ThreadRodSound);
			this._isProcessing = true;
			this.RodRenderer.sharedMaterial = sharedMaterial;
			this._rodMeshFilter.sharedMesh = sharedMesh;
			this.RodRenderer.enabled = true;
			this.DummyWholePartRenderer.sharedMaterial = sharedMaterial;
			this._dummyWholePartMeshFilter.sharedMesh = sharedMesh;
			this._animator.Play("ThreadingLathe_Process");
			yield return new WaitForSeconds(1.5f);
			this.RodRenderer.transform.SetParent(this._rodSpinningParent);
			this.RodRenderer.transform.localPosition = Vector3.zero;
			yield return new WaitForSeconds(0.75f);
			this.RodRenderer.sharedMaterial = rod.GetComponent<Renderer>().sharedMaterial;
			this._rodMeshFilter.sharedMesh = rod.GetComponent<MeshFilter>().sharedMesh;
			yield return new WaitForSeconds(3.25f);
			this.RodRenderer.transform.SetParent(this._rodRegularParent);
			this.RodRenderer.transform.localPosition = Vector3.zero;
			this.RodRenderer.transform.localRotation = Quaternion.identity;
			yield return new WaitForSeconds(2.5f);
			this.RodRenderer.enabled = false;
			if (rod != null)
			{
				rod.transform.position = this.OutputTransform.position;
				rod.transform.rotation = this.OutputTransform.rotation;
				rod.gameObject.SetActive(true);
			}
			yield return new WaitForSeconds(1.1f);
		}
		this._isProcessing = false;
		if (this._waitingList.Count > 0)
		{
			OrePiece orePiece2 = this._waitingList[0];
			this._waitingList.RemoveAt(0);
			base.StartCoroutine(this.ExtrudeRod(orePiece2));
		}
		yield break;
	}

	// Token: 0x0400076B RID: 1899
	public Transform OutputTransform;

	// Token: 0x0400076C RID: 1900
	public SoundDefinition ThreadRodSound;

	// Token: 0x0400076D RID: 1901
	public SoundPlayer ProcessRodSoundPlayer;

	// Token: 0x0400076E RID: 1902
	public Renderer RodRenderer;

	// Token: 0x0400076F RID: 1903
	public Renderer DummyWholePartRenderer;

	// Token: 0x04000770 RID: 1904
	private List<OrePiece> _waitingList = new List<OrePiece>();

	// Token: 0x04000771 RID: 1905
	private bool _isProcessing;

	// Token: 0x04000772 RID: 1906
	[SerializeField]
	private Renderer _lightRenderer;

	// Token: 0x04000773 RID: 1907
	[SerializeField]
	private Animator _animator;

	// Token: 0x04000774 RID: 1908
	[SerializeField]
	private Transform _rodRegularParent;

	// Token: 0x04000775 RID: 1909
	[SerializeField]
	private Transform _rodSpinningParent;

	// Token: 0x04000776 RID: 1910
	private MeshFilter _rodMeshFilter;

	// Token: 0x04000777 RID: 1911
	private MeshFilter _dummyWholePartMeshFilter;
}
