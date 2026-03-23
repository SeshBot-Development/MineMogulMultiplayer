using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x020000AD RID: 173
public class RollingMill : MonoBehaviour
{
	// Token: 0x060004C5 RID: 1221 RVA: 0x00019A6E File Offset: 0x00017C6E
	private void OnEnable()
	{
		this._plateRenderer.enabled = false;
		this._platePolishedRenderer.enabled = false;
		this.RefreshJammedEffects();
	}

	// Token: 0x060004C6 RID: 1222 RVA: 0x00019A90 File Offset: 0x00017C90
	private void Update()
	{
		bool isJammed = this._isJammed;
		this._isJammed = this._jammedOre.Count > 0;
		if (this._isJammed != isJammed)
		{
			this.RefreshJammedEffects();
			this._lastLaunchTime = Time.time;
		}
		if (this._isJammed && Time.time - this._lastLaunchTime > this.LaunchJammedObjectCooldown)
		{
			this._lastLaunchTime = Time.time;
			if (Random.value < 0.5f)
			{
				base.StartCoroutine(this.WaitThenLaunchRandomJammedOre());
			}
		}
	}

	// Token: 0x060004C7 RID: 1223 RVA: 0x00019B12 File Offset: 0x00017D12
	private IEnumerator WaitThenLaunchRandomJammedOre()
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.LaunchJammedObjectSound, base.transform.position, 1f, 1f, true, false);
		yield return new WaitForSeconds(0.3f);
		this.LaunchRandomJammedOre();
		yield break;
	}

	// Token: 0x060004C8 RID: 1224 RVA: 0x00019B24 File Offset: 0x00017D24
	private void LaunchRandomJammedOre()
	{
		if (this == null)
		{
			return;
		}
		if (this._jammedOre.Count == 0)
		{
			return;
		}
		int num = Random.Range(0, this._jammedOre.Count);
		OrePiece orePiece = this._jammedOre.ElementAt(num);
		if (orePiece == null)
		{
			return;
		}
		Rigidbody component = orePiece.GetComponent<Rigidbody>();
		if (component != null)
		{
			Vector3 vector = Vector3.up + -base.transform.forward + Random.insideUnitSphere * 0.2f;
			float num2 = 2f;
			float num3 = 6f;
			component.AddForce(vector * num2, ForceMode.Impulse);
			component.AddTorque(vector * num3, ForceMode.Impulse);
		}
	}

	// Token: 0x060004C9 RID: 1225 RVA: 0x00019BE0 File Offset: 0x00017DE0
	private void RefreshJammedEffects()
	{
		foreach (ParticleSystem particleSystem in this.JammedParticles)
		{
			particleSystem.emission.enabled = this._isJammed;
			this.ChangeLightMaterial(this._isJammed ? Singleton<BuildingManager>.Instance.OrangeLightMaterial : Singleton<BuildingManager>.Instance.GreenLightMaterial);
			this._jammedLoopingAudioSource.Toggle(this._isJammed);
		}
	}

	// Token: 0x060004CA RID: 1226 RVA: 0x00019C74 File Offset: 0x00017E74
	private void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = this._lightMeshRenderer.sharedMaterials;
		sharedMaterials[3] = material;
		this._lightMeshRenderer.sharedMaterials = sharedMaterials;
	}

	// Token: 0x060004CB RID: 1227 RVA: 0x00019CA0 File Offset: 0x00017EA0
	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			if (component.PlatePrefab != null)
			{
				if (!this._isProcessing)
				{
					this._isProcessing = true;
					base.StartCoroutine(this.PressIngot(component));
					return;
				}
				this._waitingList.Add(component);
				return;
			}
			else
			{
				this._jammedOre.Add(component);
			}
		}
	}

	// Token: 0x060004CC RID: 1228 RVA: 0x00019D04 File Offset: 0x00017F04
	private void OnTriggerExit(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			this._waitingList.Remove(component);
			this._jammedOre.Remove(component);
		}
	}

	// Token: 0x060004CD RID: 1229 RVA: 0x00019D3B File Offset: 0x00017F3B
	public void OnAllOreCleared()
	{
		this._jammedOre.Clear();
		this._waitingList.Clear();
	}

	// Token: 0x060004CE RID: 1230 RVA: 0x00019D53 File Offset: 0x00017F53
	private IEnumerator PressIngot(OrePiece orePiece)
	{
		if (orePiece.PlatePrefab != null)
		{
			if (orePiece.PlatePrefab.GetComponent<OrePiece>() != null)
			{
				GameObject platePrefab = orePiece.PlatePrefab;
				orePiece.Delete();
				if (orePiece.IsPolished)
				{
					Renderer platePolishedRenderer = this._platePolishedRenderer;
					Renderer component = orePiece.PlatePrefab.GetComponent<Renderer>();
					platePolishedRenderer.sharedMaterial = ((component != null) ? component.sharedMaterial : null);
				}
				else
				{
					Renderer plateRenderer = this._plateRenderer;
					Renderer component2 = orePiece.PlatePrefab.GetComponent<Renderer>();
					plateRenderer.sharedMaterial = ((component2 != null) ? component2.sharedMaterial : null);
				}
				this._plateRenderer.enabled = !orePiece.IsPolished;
				this._platePolishedRenderer.enabled = orePiece.IsPolished;
				this.ProcessPlateSoundPlayer.PlaySound(this.ProcessPlateSound);
				this._isProcessing = true;
				if (this.RollingAnimator != null)
				{
					this.RollingAnimator.speed = 1f / this.ProcessingTime;
					this.RollingAnimator.Play("RollingMill_Process", -1, 0f);
				}
				yield return new WaitForSeconds(this.ProcessingTime * 0.9f);
				this._plateRenderer.enabled = false;
				this._platePolishedRenderer.enabled = false;
				OrePiece orePiece2 = Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(platePrefab, this.OutputTransform.position, Quaternion.identity, null);
				if (orePiece2 != null)
				{
					orePiece2.transform.rotation = this.OutputTransform.rotation;
				}
				platePrefab = null;
			}
			else
			{
				orePiece.ConvertToPlate();
			}
		}
		this._isProcessing = false;
		if (this._waitingList.Count > 0)
		{
			OrePiece orePiece3 = this._waitingList[0];
			this._waitingList.RemoveAt(0);
			base.StartCoroutine(this.PressIngot(orePiece3));
		}
		yield break;
	}

	// Token: 0x04000560 RID: 1376
	public Animator RollingAnimator;

	// Token: 0x04000561 RID: 1377
	public float ProcessingTime = 3f;

	// Token: 0x04000562 RID: 1378
	public Transform PlateTransform;

	// Token: 0x04000563 RID: 1379
	public Transform OutputTransform;

	// Token: 0x04000564 RID: 1380
	public List<ParticleSystem> JammedParticles;

	// Token: 0x04000565 RID: 1381
	public float LaunchJammedObjectCooldown = 1f;

	// Token: 0x04000566 RID: 1382
	public SoundDefinition LaunchJammedObjectSound;

	// Token: 0x04000567 RID: 1383
	public SoundDefinition ProcessPlateSound;

	// Token: 0x04000568 RID: 1384
	public SoundPlayer ProcessPlateSoundPlayer;

	// Token: 0x04000569 RID: 1385
	private float _lastLaunchTime;

	// Token: 0x0400056A RID: 1386
	private bool _isJammed;

	// Token: 0x0400056B RID: 1387
	private List<OrePiece> _waitingList = new List<OrePiece>();

	// Token: 0x0400056C RID: 1388
	private HashSet<OrePiece> _jammedOre = new HashSet<OrePiece>();

	// Token: 0x0400056D RID: 1389
	private bool _isProcessing;

	// Token: 0x0400056E RID: 1390
	[SerializeField]
	private MeshRenderer _lightMeshRenderer;

	// Token: 0x0400056F RID: 1391
	[SerializeField]
	private LoopingSoundPlayer _jammedLoopingAudioSource;

	// Token: 0x04000570 RID: 1392
	[SerializeField]
	private Renderer _plateRenderer;

	// Token: 0x04000571 RID: 1393
	[SerializeField]
	private Renderer _platePolishedRenderer;
}
