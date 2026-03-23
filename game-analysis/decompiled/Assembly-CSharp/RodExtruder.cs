using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000AB RID: 171
public class RodExtruder : MonoBehaviour
{
	// Token: 0x060004BA RID: 1210 RVA: 0x00019800 File Offset: 0x00017A00
	private void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = this._lightRenderer.sharedMaterials;
		sharedMaterials[3] = material;
		this._lightRenderer.sharedMaterials = sharedMaterials;
	}

	// Token: 0x060004BB RID: 1211 RVA: 0x00019829 File Offset: 0x00017A29
	private void Awake()
	{
		this._rodMeshFilter1 = this.RodRenderer1.GetComponent<MeshFilter>();
		this._rodMeshFilter2 = this.RodRenderer2.GetComponent<MeshFilter>();
	}

	// Token: 0x060004BC RID: 1212 RVA: 0x00019850 File Offset: 0x00017A50
	private void OnEnable()
	{
		this.RodRenderer1.enabled = false;
		this.RodRenderer2.enabled = false;
		this.RodRenderer1.transform.localPosition = this.RodStartTransform.localPosition;
		this.RodRenderer2.transform.localPosition = this.RodStartTransform.localPosition;
	}

	// Token: 0x060004BD RID: 1213 RVA: 0x000198AB File Offset: 0x00017AAB
	private bool IsAvailable()
	{
		return !this._isProcessing1 || !this._isProcessing2;
	}

	// Token: 0x060004BE RID: 1214 RVA: 0x000198C0 File Offset: 0x00017AC0
	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null && component.RodPrefab != null)
		{
			if (this.IsAvailable())
			{
				base.StartCoroutine(this.ExtrudeRod(component));
				return;
			}
			this._waitingList.Add(component);
		}
	}

	// Token: 0x060004BF RID: 1215 RVA: 0x00019910 File Offset: 0x00017B10
	private void OnTriggerExit(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			this._waitingList.Remove(component);
		}
	}

	// Token: 0x060004C0 RID: 1216 RVA: 0x0001993A File Offset: 0x00017B3A
	private IEnumerator ExtrudeRod(OrePiece orePiece)
	{
		OrePiece rod = orePiece.ConvertToRod();
		bool usingRenderer = !this._isProcessing1;
		if (rod != null)
		{
			rod.gameObject.SetActive(false);
			if (usingRenderer)
			{
				this._isProcessing1 = true;
			}
			else
			{
				this._isProcessing2 = true;
			}
			float minTimeBetweenRods = this.ProcessingTime * 0.75f;
			while ((in this._timeSinceProcessed) < minTimeBetweenRods)
			{
				yield return null;
			}
			this._timeSinceProcessed = 0f;
			Renderer selectedRenderer = (usingRenderer ? this.RodRenderer1 : this.RodRenderer2);
			MeshFilter meshFilter = (usingRenderer ? this._rodMeshFilter1 : this._rodMeshFilter2);
			selectedRenderer.sharedMaterial = rod.GetComponent<Renderer>().sharedMaterial;
			meshFilter.sharedMesh = rod.GetComponent<MeshFilter>().sharedMesh;
			selectedRenderer.transform.position = this.RodStartTransform.position;
			selectedRenderer.enabled = true;
			float elapsed = 0f;
			Vector3 startPos = this.RodStartTransform.position;
			Vector3 endPos = this.OutputTransform.position;
			this.ProcessRodSoundPlayer.PlaySound(this.ExtrudeRodSound);
			while (elapsed < this.ProcessingTime)
			{
				elapsed += Time.deltaTime;
				float num = elapsed / this.ProcessingTime;
				selectedRenderer.transform.position = Vector3.Lerp(startPos, endPos, num);
				yield return null;
			}
			selectedRenderer.enabled = false;
			if (rod != null)
			{
				rod.transform.position = this.OutputTransform.position;
				rod.transform.rotation = this.OutputTransform.rotation;
				rod.gameObject.SetActive(true);
			}
			selectedRenderer = null;
			startPos = default(Vector3);
			endPos = default(Vector3);
		}
		if (usingRenderer)
		{
			this._isProcessing1 = false;
		}
		else
		{
			this._isProcessing2 = false;
		}
		if (this._waitingList.Count > 0)
		{
			OrePiece orePiece2 = this._waitingList[0];
			this._waitingList.RemoveAt(0);
			base.StartCoroutine(this.ExtrudeRod(orePiece2));
		}
		yield break;
	}

	// Token: 0x0400054D RID: 1357
	public Transform OutputTransform;

	// Token: 0x0400054E RID: 1358
	public float ProcessingTime = 1.5f;

	// Token: 0x0400054F RID: 1359
	public SoundDefinition ExtrudeRodSound;

	// Token: 0x04000550 RID: 1360
	public SoundPlayer ProcessRodSoundPlayer;

	// Token: 0x04000551 RID: 1361
	public Transform RodStartTransform;

	// Token: 0x04000552 RID: 1362
	public Renderer RodRenderer1;

	// Token: 0x04000553 RID: 1363
	public Renderer RodRenderer2;

	// Token: 0x04000554 RID: 1364
	private List<OrePiece> _waitingList = new List<OrePiece>();

	// Token: 0x04000555 RID: 1365
	private bool _isProcessing1;

	// Token: 0x04000556 RID: 1366
	private bool _isProcessing2;

	// Token: 0x04000557 RID: 1367
	[SerializeField]
	private Renderer _lightRenderer;

	// Token: 0x04000558 RID: 1368
	private MeshFilter _rodMeshFilter1;

	// Token: 0x04000559 RID: 1369
	private MeshFilter _rodMeshFilter2;

	// Token: 0x0400055A RID: 1370
	private TimeSince _timeSinceProcessed;
}
