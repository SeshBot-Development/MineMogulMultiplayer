using System;
using System.Collections;
using UnityEngine;

// Token: 0x0200009F RID: 159
public class RapidAutoMiner : AutoMiner, IInteractable, ICustomSaveDataProvider
{
	// Token: 0x06000447 RID: 1095 RVA: 0x00017878 File Offset: 0x00015A78
	public override void ConfigureFromDefinition()
	{
		if (this.ResourceDefinition != null)
		{
			this.SpawnProbability = this.ResourceDefinition.SpawnProbability;
			this.SpawnRate = this.ResourceDefinition.SpawnRate * 0.25f;
			return;
		}
		Debug.Log("AutoMiner doesn't have a resource definition!");
	}

	// Token: 0x06000448 RID: 1096 RVA: 0x000178C6 File Offset: 0x00015AC6
	protected override void OnEnable()
	{
		this.Enabled = this.AttachedDrillBit != null;
		this.BuildingObject.OnBuildingRemoved += this.EjectCurrentDrillBit;
		base.OnEnable();
	}

	// Token: 0x06000449 RID: 1097 RVA: 0x000178F7 File Offset: 0x00015AF7
	private void OnDisable()
	{
		this.BuildingObject.OnBuildingRemoved -= this.EjectCurrentDrillBit;
	}

	// Token: 0x0600044A RID: 1098 RVA: 0x00017910 File Offset: 0x00015B10
	protected override void Update()
	{
		if (this.Enabled && this.AttachedDrillBit != null)
		{
			this.AttachedDrillBit.UseDrillBit(Time.deltaTime);
			if (!this.AttachedDrillBit.LastsForever && this.AttachedDrillBit.IsBroken())
			{
				this.BreakCurrentDrillBit();
			}
		}
		if (this.Enabled && this.AttachedDrillBit != null)
		{
			if (this.ResourceDefinition != null)
			{
				float currentRate = this.AttachedDrillBit.GetCurrentRate();
				float num = this.AttachedDrillBit.DrillSpeedByDurabilityCurve.Evaluate(currentRate);
				this.SpawnRate = this.ResourceDefinition.SpawnRate * num;
			}
			base.Update();
		}
	}

	// Token: 0x0600044B RID: 1099 RVA: 0x000179C0 File Offset: 0x00015BC0
	public void BreakCurrentDrillBit()
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.DrillBitBreakSound, this.DrillBitAttachmentParent.position, 1f, 1f, true, false);
		Object.Destroy(this.AttachedDrillBit.gameObject);
		base.StartCoroutine(this.SpawnBrokenDrillBitPieces());
		this.Animator.speed = 0f;
		this._audioSource_Loop.Pause();
		base.ChangeLightMaterial(Singleton<BuildingManager>.Instance.OrangeLightMaterial);
	}

	// Token: 0x0600044C RID: 1100 RVA: 0x00017A3C File Offset: 0x00015C3C
	private IEnumerator SpawnBrokenDrillBitPieces()
	{
		yield return new WaitForSeconds(0.4f);
		if (base.gameObject == null || !base.gameObject.activeInHierarchy)
		{
			yield break;
		}
		OrePiece orePiece = Object.Instantiate<OrePiece>(this.BrokenDrillBitPrefab, this.OreSpawnPoint.position, this.OreSpawnPoint.rotation);
		orePiece.MeshID = 1;
		orePiece.UseRandomMesh = false;
		yield return new WaitForSeconds(0.5f);
		if (base.gameObject == null || !base.gameObject.activeInHierarchy)
		{
			yield break;
		}
		OrePiece orePiece2 = Object.Instantiate<OrePiece>(this.BrokenDrillBitPrefab, this.OreSpawnPoint.position, this.OreSpawnPoint.rotation);
		orePiece2.MeshID = 0;
		orePiece2.UseRandomMesh = false;
		yield break;
	}

	// Token: 0x0600044D RID: 1101 RVA: 0x00017A4C File Offset: 0x00015C4C
	public void AttachDrillBit(RapidAutoMinerDrillBit drillBit, bool shouldTurnOnAutominer)
	{
		if (drillBit.IsBroken())
		{
			Debug.Log("Cannot attach a broken drill bit.");
			return;
		}
		this.EjectCurrentDrillBit();
		this.AttachedDrillBit = drillBit;
		drillBit.AttachedAutoMiner = this;
		drillBit.transform.SetParent(this.DrillBitAttachmentParent);
		drillBit.transform.localPosition = Vector3.zero;
		drillBit.transform.localRotation = Quaternion.identity;
		if (shouldTurnOnAutominer)
		{
			this.TurnOn();
		}
	}

	// Token: 0x0600044E RID: 1102 RVA: 0x00017ABC File Offset: 0x00015CBC
	public void EjectCurrentDrillBit()
	{
		if (this.AttachedDrillBit == null)
		{
			return;
		}
		this.AttachedDrillBit.AttachedAutoMiner = null;
		this.AttachedDrillBit.transform.SetParent(null);
		this.AttachedDrillBit.transform.position = this.DrillBitEjectPoint.position;
		this.AttachedDrillBit.transform.rotation = this.DrillBitEjectPoint.rotation;
		Rigidbody componentInChildren = this.AttachedDrillBit.GetComponentInChildren<Rigidbody>();
		if (componentInChildren != null)
		{
			componentInChildren.isKinematic = false;
		}
		this.AttachedDrillBit = null;
		this.Animator.speed = 0f;
		this._audioSource_Loop.Pause();
		base.ChangeLightMaterial(Singleton<BuildingManager>.Instance.OrangeLightMaterial);
	}

	// Token: 0x0600044F RID: 1103 RVA: 0x00017B79 File Offset: 0x00015D79
	protected override void TurnOn()
	{
		if (this.AttachedDrillBit == null)
		{
			Debug.Log("Cannot turn on RapidAutoMiner without a drill bit attached.");
			return;
		}
		this.Animator.speed = 1f;
		base.TurnOn();
	}

	// Token: 0x06000450 RID: 1104 RVA: 0x00017BAA File Offset: 0x00015DAA
	protected override void TurnOff()
	{
		this.Animator.speed = 0f;
		base.TurnOff();
	}

	// Token: 0x06000451 RID: 1105 RVA: 0x00017BC4 File Offset: 0x00015DC4
	public override void Interact(Interaction selectedInteraction)
	{
		string name = selectedInteraction.Name;
		if (name == "Turn On")
		{
			this.TurnOn();
			return;
		}
		if (name == "Turn Off")
		{
			this.TurnOff();
			return;
		}
		if (!(name == "Eject Drill Bit"))
		{
			return;
		}
		this.EjectCurrentDrillBit();
	}

	// Token: 0x06000452 RID: 1106 RVA: 0x00017C14 File Offset: 0x00015E14
	public override void LoadFromSave(string json)
	{
		AutoMinerSaveData autoMinerSaveData = JsonUtility.FromJson<AutoMinerSaveData>(json);
		if (autoMinerSaveData == null)
		{
			autoMinerSaveData = new AutoMinerSaveData();
		}
		if (autoMinerSaveData.IsOn)
		{
			base.StartCoroutine(this.WaitThenTryTurnOn());
		}
	}

	// Token: 0x06000453 RID: 1107 RVA: 0x00017C46 File Offset: 0x00015E46
	private IEnumerator WaitThenTryTurnOn()
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		if (this.AttachedDrillBit != null)
		{
			this.TurnOn();
		}
		yield break;
	}

	// Token: 0x040004ED RID: 1261
	[Header("-- RapidAutoMiner --")]
	public RapidAutoMinerDrillBit AttachedDrillBit;

	// Token: 0x040004EE RID: 1262
	public Transform DrillBitEjectPoint;

	// Token: 0x040004EF RID: 1263
	public Transform DrillBitAttachmentParent;

	// Token: 0x040004F0 RID: 1264
	public Animator Animator;

	// Token: 0x040004F1 RID: 1265
	public BuildingObject BuildingObject;

	// Token: 0x040004F2 RID: 1266
	public OrePiece BrokenDrillBitPrefab;

	// Token: 0x040004F3 RID: 1267
	public SoundDefinition DrillBitBreakSound;
}
