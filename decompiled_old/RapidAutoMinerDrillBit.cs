using System.Collections;
using UnityEngine;

public class RapidAutoMinerDrillBit : BaseHeldTool
{
	[Header("-- RapidAutoMiner DrillBit --")]
	public float MaxDrillTimeSeconds = 60f;

	public bool LastsForever;

	public AnimationCurve DrillSpeedByDurabilityCurve;

	public float NewSellPrice;

	public float BrokenSellPrice;

	public Mesh NewMesh;

	public Mesh UsedMesh;

	public Mesh BrokenMesh;

	public MeshFilter WorldModelDrillBit;

	public MeshFilter ViewModelDrillBit;

	public RapidAutoMiner AttachedAutoMiner;

	private float _currentDrillTimeSeconds;

	public float UseRange = 2f;

	public float AttackCooldown = 1f;

	public LayerMask HitLayers;

	private float _lastAttackTime = -1f;

	[SerializeField]
	private SoundDefinition _sound_hit_world;

	[SerializeField]
	private SoundDefinition _sound_swing;

	[SerializeField]
	private SoundPlayer _swingSoundPlayer;

	public override string GetControlsText()
	{
		return "Drop - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool) + "\nAttach to Rapid Auto-Miner - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.PrimaryAttack);
	}

	private void SwingPickaxe()
	{
		if (!(Owner == null) && !(Owner.GetComponentInChildren<Camera>() == null))
		{
			if (ViewModelAnimator != null)
			{
				ViewModelAnimator.Play("Attack1", -1, 0f);
			}
			_swingSoundPlayer.PlaySound(_sound_swing);
			StartCoroutine(PerformAttack(0.2f));
			_lastAttackTime = Time.time;
		}
	}

	private IEnumerator PerformAttack(float delaySeconds)
	{
		yield return new WaitForSeconds(delaySeconds);
		Camera componentInChildren = Owner.GetComponentInChildren<Camera>();
		if (!(componentInChildren == null) && Physics.Raycast(componentInChildren.transform.position, componentInChildren.transform.forward, out var hitInfo, UseRange, HitLayers))
		{
			RapidAutoMiner componentInParent = hitInfo.collider.GetComponentInParent<RapidAutoMiner>();
			if (componentInParent != null && !IsBroken())
			{
				AttachToAutominer(componentInParent, shouldTurnOnAutominer: true);
			}
			Rigidbody component = hitInfo.collider.GetComponent<Rigidbody>();
			if (component != null)
			{
				float num = 5f;
				Vector3 forward = componentInChildren.transform.forward;
				component.AddForceAtPosition(forward * num, hitInfo.point, ForceMode.Impulse);
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(_sound_hit_world, hitInfo.point);
				Singleton<ParticleManager>.Instance.CreateParticle(Singleton<ParticleManager>.Instance.GenericHitImpactParticle, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
			}
		}
	}

	private void AttachToAutominer(RapidAutoMiner autoMiner, bool shouldTurnOnAutominer)
	{
		if (Owner != null)
		{
			Owner.Inventory.RemoveFromInventory(this);
		}
		HideWorldModel(hide: false);
		HideViewModel();
		Rigidbody componentInChildren = GetComponentInChildren<Rigidbody>();
		if (componentInChildren != null)
		{
			componentInChildren.isKinematic = true;
			componentInChildren.linearVelocity = Vector3.zero;
		}
		WorldModel.transform.localPosition = Vector3.zero;
		WorldModel.transform.localRotation = Quaternion.identity;
		Owner = null;
		autoMiner.AttachDrillBit(this, shouldTurnOnAutominer);
	}

	public override void PrimaryFire()
	{
		if (Time.time - _lastAttackTime >= AttackCooldown)
		{
			SwingPickaxe();
		}
	}

	public override float GetSellValue()
	{
		return GetDurabilityPercentage() * NewSellPrice + (1f - GetDurabilityPercentage()) * BrokenSellPrice;
	}

	public float GetDurabilityPercentage()
	{
		return Mathf.Clamp01(1f - _currentDrillTimeSeconds / MaxDrillTimeSeconds);
	}

	public float GetCurrentRate()
	{
		return _currentDrillTimeSeconds / MaxDrillTimeSeconds;
	}

	public void UseDrillBit(float deltaTime)
	{
		_currentDrillTimeSeconds += deltaTime;
		_currentDrillTimeSeconds = Mathf.Min(_currentDrillTimeSeconds, MaxDrillTimeSeconds);
		UpdateDrillBitVisuals();
	}

	public bool IsBroken()
	{
		if (!LastsForever)
		{
			return GetDurabilityPercentage() <= 0f;
		}
		return false;
	}

	protected override void OnEnable()
	{
		UpdateDrillBitVisuals();
		UpdateDescription();
		base.OnEnable();
	}

	public override bool TryAddToInventory(int slotIndex = -1)
	{
		if (AttachedAutoMiner != null)
		{
			AttachedAutoMiner.EjectCurrentDrillBit();
		}
		bool num = base.TryAddToInventory(slotIndex);
		if (num)
		{
			UpdateDescription();
		}
		return num;
	}

	public void UpdateDescription()
	{
		if (!LastsForever)
		{
			string description = ((!IsBroken()) ? $"Required for Rapid Auto-Miner\nDurability: {Mathf.Round(GetDurabilityPercentage() * 100f)}%" : "It's garbage.");
			Description = description;
		}
	}

	public void UpdateDrillBitVisuals()
	{
		float durabilityPercentage = GetDurabilityPercentage();
		Mesh newMesh = NewMesh;
		newMesh = ((durabilityPercentage > 0.75f) ? NewMesh : ((!(durabilityPercentage > 0.15f)) ? BrokenMesh : UsedMesh));
		if (WorldModelDrillBit.sharedMesh != newMesh)
		{
			WorldModelDrillBit.sharedMesh = newMesh;
			ViewModelDrillBit.sharedMesh = newMesh;
		}
	}

	public override void LoadFromSave(string json)
	{
		RapidAutoMinerDrillBitToolSaveData rapidAutoMinerDrillBitToolSaveData = JsonUtility.FromJson<RapidAutoMinerDrillBitToolSaveData>(json);
		if (rapidAutoMinerDrillBitToolSaveData == null)
		{
			rapidAutoMinerDrillBitToolSaveData = new RapidAutoMinerDrillBitToolSaveData();
		}
		if (rapidAutoMinerDrillBitToolSaveData.IsInPlayerInventory)
		{
			StartCoroutine(WaitThenAddToInventory(rapidAutoMinerDrillBitToolSaveData.InventorySlotIndex));
		}
		_currentDrillTimeSeconds = (1f - rapidAutoMinerDrillBitToolSaveData.DurabilityPercentage) * MaxDrillTimeSeconds;
		if (rapidAutoMinerDrillBitToolSaveData.IsAttachedToAutoMiner)
		{
			StartCoroutine(WaitThenReattachToNearestAutoMiner());
		}
	}

	public override string GetCustomSaveData()
	{
		RapidAutoMinerDrillBitToolSaveData rapidAutoMinerDrillBitToolSaveData = new RapidAutoMinerDrillBitToolSaveData
		{
			IsInPlayerInventory = (Owner != null)
		};
		if (rapidAutoMinerDrillBitToolSaveData.IsInPlayerInventory)
		{
			rapidAutoMinerDrillBitToolSaveData.InventorySlotIndex = Object.FindObjectOfType<PlayerInventory>().GetInventoryIndexForTool(this);
		}
		rapidAutoMinerDrillBitToolSaveData.DurabilityPercentage = GetDurabilityPercentage();
		rapidAutoMinerDrillBitToolSaveData.IsAttachedToAutoMiner = AttachedAutoMiner != null;
		return JsonUtility.ToJson(rapidAutoMinerDrillBitToolSaveData);
	}

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
		RapidAutoMiner[] array2 = array;
		foreach (RapidAutoMiner rapidAutoMiner2 in array2)
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
			AttachToAutominer(rapidAutoMiner, shouldTurnOnAutominer: false);
		}
	}
}
