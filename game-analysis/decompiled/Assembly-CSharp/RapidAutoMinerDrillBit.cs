using System;
using System.Collections;
using UnityEngine;

// Token: 0x020000A0 RID: 160
public class RapidAutoMinerDrillBit : BaseHeldTool
{
	// Token: 0x06000455 RID: 1109 RVA: 0x00017C5D File Offset: 0x00015E5D
	public override string GetControlsText()
	{
		return "Drop - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool) + "\nAttach to Rapid Auto-Miner - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.PrimaryAttack);
	}

	// Token: 0x06000456 RID: 1110 RVA: 0x00017C88 File Offset: 0x00015E88
	private void SwingPickaxe()
	{
		if (this.Owner == null)
		{
			return;
		}
		if (this.Owner.GetComponentInChildren<Camera>() == null)
		{
			return;
		}
		if (this.ViewModelAnimator != null)
		{
			this.ViewModelAnimator.Play("Attack1", -1, 0f);
		}
		this._swingSoundPlayer.PlaySound(this._sound_swing);
		base.StartCoroutine(this.PerformAttack(0.2f));
		this._lastAttackTime = Time.time;
	}

	// Token: 0x06000457 RID: 1111 RVA: 0x00017D0A File Offset: 0x00015F0A
	private IEnumerator PerformAttack(float delaySeconds)
	{
		yield return new WaitForSeconds(delaySeconds);
		Camera componentInChildren = this.Owner.GetComponentInChildren<Camera>();
		if (componentInChildren == null)
		{
			yield break;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(componentInChildren.transform.position, componentInChildren.transform.forward, out raycastHit, this.UseRange, this.HitLayers))
		{
			RapidAutoMiner componentInParent = raycastHit.collider.GetComponentInParent<RapidAutoMiner>();
			if (componentInParent != null && !this.IsBroken())
			{
				this.AttachToAutominer(componentInParent, true);
			}
			Rigidbody component = raycastHit.collider.GetComponent<Rigidbody>();
			if (component != null)
			{
				float num = 5f;
				Vector3 forward = componentInChildren.transform.forward;
				component.AddForceAtPosition(forward * num, raycastHit.point, ForceMode.Impulse);
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._sound_hit_world, raycastHit.point, 1f, 1f, true, false);
				Singleton<ParticleManager>.Instance.CreateParticle(Singleton<ParticleManager>.Instance.GenericHitImpactParticle, raycastHit.point, Quaternion.LookRotation(raycastHit.normal), default(Vector3));
			}
		}
		yield break;
	}

	// Token: 0x06000458 RID: 1112 RVA: 0x00017D20 File Offset: 0x00015F20
	private void AttachToAutominer(RapidAutoMiner autoMiner, bool shouldTurnOnAutominer)
	{
		if (this.Owner != null)
		{
			this.Owner.Inventory.RemoveFromInventory(this, 1);
		}
		this.HideWorldModel(false);
		this.HideViewModel(true);
		Rigidbody componentInChildren = base.GetComponentInChildren<Rigidbody>();
		if (componentInChildren != null)
		{
			componentInChildren.isKinematic = true;
			componentInChildren.linearVelocity = Vector3.zero;
		}
		this.WorldModel.transform.localPosition = Vector3.zero;
		this.WorldModel.transform.localRotation = Quaternion.identity;
		this.Owner = null;
		autoMiner.AttachDrillBit(this, shouldTurnOnAutominer);
	}

	// Token: 0x06000459 RID: 1113 RVA: 0x00017DB6 File Offset: 0x00015FB6
	public override void PrimaryFire()
	{
		if (Time.time - this._lastAttackTime >= this.AttackCooldown)
		{
			this.SwingPickaxe();
		}
	}

	// Token: 0x0600045A RID: 1114 RVA: 0x00017DD2 File Offset: 0x00015FD2
	public override float GetSellValue()
	{
		return this.GetDurabilityPercentage() * this.NewSellPrice + (1f - this.GetDurabilityPercentage()) * this.BrokenSellPrice;
	}

	// Token: 0x0600045B RID: 1115 RVA: 0x00017DF5 File Offset: 0x00015FF5
	public float GetDurabilityPercentage()
	{
		return Mathf.Clamp01(1f - this._currentDrillTimeSeconds / this.MaxDrillTimeSeconds);
	}

	// Token: 0x0600045C RID: 1116 RVA: 0x00017E0F File Offset: 0x0001600F
	public float GetCurrentRate()
	{
		return this._currentDrillTimeSeconds / this.MaxDrillTimeSeconds;
	}

	// Token: 0x0600045D RID: 1117 RVA: 0x00017E1E File Offset: 0x0001601E
	public void UseDrillBit(float deltaTime)
	{
		this._currentDrillTimeSeconds += deltaTime;
		this._currentDrillTimeSeconds = Mathf.Min(this._currentDrillTimeSeconds, this.MaxDrillTimeSeconds);
		this.UpdateDrillBitVisuals();
	}

	// Token: 0x0600045E RID: 1118 RVA: 0x00017E4B File Offset: 0x0001604B
	public bool IsBroken()
	{
		return !this.LastsForever && this.GetDurabilityPercentage() <= 0f;
	}

	// Token: 0x0600045F RID: 1119 RVA: 0x00017E67 File Offset: 0x00016067
	protected override void OnEnable()
	{
		this.UpdateDrillBitVisuals();
		this.UpdateDescription();
		base.OnEnable();
	}

	// Token: 0x06000460 RID: 1120 RVA: 0x00017E7B File Offset: 0x0001607B
	public override bool TryAddToInventory(int slotIndex = -1)
	{
		if (this.AttachedAutoMiner != null)
		{
			this.AttachedAutoMiner.EjectCurrentDrillBit();
		}
		bool flag = base.TryAddToInventory(slotIndex);
		if (flag)
		{
			this.UpdateDescription();
		}
		return flag;
	}

	// Token: 0x06000461 RID: 1121 RVA: 0x00017EA8 File Offset: 0x000160A8
	public void UpdateDescription()
	{
		if (this.LastsForever)
		{
			return;
		}
		string text = ((!this.IsBroken()) ? string.Format("Required for Rapid Auto-Miner\nDurability: {0}%", Mathf.Round(this.GetDurabilityPercentage() * 100f)) : "It's garbage.");
		this.Description = text;
	}

	// Token: 0x06000462 RID: 1122 RVA: 0x00017EF8 File Offset: 0x000160F8
	public void UpdateDrillBitVisuals()
	{
		float durabilityPercentage = this.GetDurabilityPercentage();
		Mesh mesh = this.NewMesh;
		if (durabilityPercentage > 0.75f)
		{
			mesh = this.NewMesh;
		}
		else if (durabilityPercentage > 0.15f)
		{
			mesh = this.UsedMesh;
		}
		else
		{
			mesh = this.BrokenMesh;
		}
		if (this.WorldModelDrillBit.sharedMesh != mesh)
		{
			this.WorldModelDrillBit.sharedMesh = mesh;
			this.ViewModelDrillBit.sharedMesh = mesh;
		}
	}

	// Token: 0x06000463 RID: 1123 RVA: 0x00017F68 File Offset: 0x00016168
	public override void LoadFromSave(string json)
	{
		RapidAutoMinerDrillBitToolSaveData rapidAutoMinerDrillBitToolSaveData = JsonUtility.FromJson<RapidAutoMinerDrillBitToolSaveData>(json);
		if (rapidAutoMinerDrillBitToolSaveData == null)
		{
			rapidAutoMinerDrillBitToolSaveData = new RapidAutoMinerDrillBitToolSaveData();
		}
		if (rapidAutoMinerDrillBitToolSaveData.IsInPlayerInventory)
		{
			base.StartCoroutine(base.WaitThenAddToInventory(rapidAutoMinerDrillBitToolSaveData.InventorySlotIndex));
		}
		this._currentDrillTimeSeconds = (1f - rapidAutoMinerDrillBitToolSaveData.DurabilityPercentage) * this.MaxDrillTimeSeconds;
		if (rapidAutoMinerDrillBitToolSaveData.IsAttachedToAutoMiner)
		{
			base.StartCoroutine(this.WaitThenReattachToNearestAutoMiner());
		}
	}

	// Token: 0x06000464 RID: 1124 RVA: 0x00017FD0 File Offset: 0x000161D0
	public override string GetCustomSaveData()
	{
		RapidAutoMinerDrillBitToolSaveData rapidAutoMinerDrillBitToolSaveData = new RapidAutoMinerDrillBitToolSaveData
		{
			IsInPlayerInventory = (this.Owner != null)
		};
		if (rapidAutoMinerDrillBitToolSaveData.IsInPlayerInventory)
		{
			rapidAutoMinerDrillBitToolSaveData.InventorySlotIndex = Object.FindObjectOfType<PlayerInventory>().GetInventoryIndexForTool(this);
		}
		rapidAutoMinerDrillBitToolSaveData.DurabilityPercentage = this.GetDurabilityPercentage();
		rapidAutoMinerDrillBitToolSaveData.IsAttachedToAutoMiner = this.AttachedAutoMiner != null;
		return JsonUtility.ToJson(rapidAutoMinerDrillBitToolSaveData);
	}

	// Token: 0x06000465 RID: 1125 RVA: 0x00018032 File Offset: 0x00016232
	protected IEnumerator WaitThenReattachToNearestAutoMiner()
	{
		yield return new WaitForFixedUpdate();
		if (base.gameObject == null)
		{
			yield break;
		}
		RapidAutoMiner[] array = Object.FindObjectsOfType<RapidAutoMiner>();
		RapidAutoMiner rapidAutoMiner = null;
		float num = float.PositiveInfinity;
		foreach (RapidAutoMiner rapidAutoMiner2 in array)
		{
			float num2 = Vector3.Distance(base.transform.position, rapidAutoMiner2.transform.position);
			if (num2 < num)
			{
				num = num2;
				rapidAutoMiner = rapidAutoMiner2;
			}
		}
		if (rapidAutoMiner != null)
		{
			this.AttachToAutominer(rapidAutoMiner, false);
		}
		yield break;
	}

	// Token: 0x040004F4 RID: 1268
	[Header("-- RapidAutoMiner DrillBit --")]
	public float MaxDrillTimeSeconds = 60f;

	// Token: 0x040004F5 RID: 1269
	public bool LastsForever;

	// Token: 0x040004F6 RID: 1270
	public AnimationCurve DrillSpeedByDurabilityCurve;

	// Token: 0x040004F7 RID: 1271
	public float NewSellPrice;

	// Token: 0x040004F8 RID: 1272
	public float BrokenSellPrice;

	// Token: 0x040004F9 RID: 1273
	public Mesh NewMesh;

	// Token: 0x040004FA RID: 1274
	public Mesh UsedMesh;

	// Token: 0x040004FB RID: 1275
	public Mesh BrokenMesh;

	// Token: 0x040004FC RID: 1276
	public MeshFilter WorldModelDrillBit;

	// Token: 0x040004FD RID: 1277
	public MeshFilter ViewModelDrillBit;

	// Token: 0x040004FE RID: 1278
	public RapidAutoMiner AttachedAutoMiner;

	// Token: 0x040004FF RID: 1279
	private float _currentDrillTimeSeconds;

	// Token: 0x04000500 RID: 1280
	public float UseRange = 2f;

	// Token: 0x04000501 RID: 1281
	public float AttackCooldown = 1f;

	// Token: 0x04000502 RID: 1282
	public LayerMask HitLayers;

	// Token: 0x04000503 RID: 1283
	private float _lastAttackTime = -1f;

	// Token: 0x04000504 RID: 1284
	[SerializeField]
	private SoundDefinition _sound_hit_world;

	// Token: 0x04000505 RID: 1285
	[SerializeField]
	private SoundDefinition _sound_swing;

	// Token: 0x04000506 RID: 1286
	[SerializeField]
	private SoundPlayer _swingSoundPlayer;
}
