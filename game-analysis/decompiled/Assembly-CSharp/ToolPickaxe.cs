using System;
using System.Collections;
using UnityEngine;

// Token: 0x020000F5 RID: 245
public class ToolPickaxe : BaseHeldTool
{
	// Token: 0x06000681 RID: 1665 RVA: 0x000221A2 File Offset: 0x000203A2
	public override string GetControlsText()
	{
		return "Drop - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool) + "\nMine - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.PrimaryAttack);
	}

	// Token: 0x06000682 RID: 1666 RVA: 0x000221CC File Offset: 0x000203CC
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

	// Token: 0x06000683 RID: 1667 RVA: 0x0002224E File Offset: 0x0002044E
	private IEnumerator PerformAttack(float delaySeconds)
	{
		yield return new WaitForSeconds(delaySeconds);
		if (!base.gameObject.activeInHierarchy)
		{
			yield break;
		}
		if (this.Owner == null)
		{
			yield break;
		}
		Camera componentInChildren = this.Owner.GetComponentInChildren<Camera>();
		if (componentInChildren == null)
		{
			yield break;
		}
		RaycastHit hit;
		if (Physics.Raycast(componentInChildren.transform.position, componentInChildren.transform.forward, out hit, this.UseRange, this.HitLayers))
		{
			if (this.CanBreakOreIntoCrushedPieces)
			{
				OrePiece component = hit.collider.GetComponent<OrePiece>();
				if (component != null && component.CrushedPrefab != null && component.CrushedPrefab.GetComponent<OrePiece>() != null && component.PieceType == PieceType.Ore && component.TryConvertToCrushed())
				{
					Singleton<ParticleManager>.Instance.CreateParticle(Singleton<ParticleManager>.Instance.OreNodeHitParticlePrefab, hit.point, Quaternion.LookRotation(hit.normal), default(Vector3));
					yield return new WaitForFixedUpdate();
					PhysicsUtils.SimpleExplosion(hit.point, 0.5f, 2f, 0.5f);
					yield break;
				}
			}
			IDamageable component2 = hit.collider.GetComponent<IDamageable>();
			if (component2 != null)
			{
				component2.TakeDamage(this.Damage, hit.point);
				Singleton<ParticleManager>.Instance.CreateParticle(Singleton<ParticleManager>.Instance.OreNodeHitParticlePrefab, hit.point, Quaternion.LookRotation(hit.normal), default(Vector3));
			}
			else
			{
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._sound_hit_world, hit.point, 1f, 1f, true, false);
				Singleton<ParticleManager>.Instance.CreateParticle(Singleton<ParticleManager>.Instance.GenericHitImpactParticle, hit.point, Quaternion.LookRotation(hit.normal), default(Vector3));
			}
			Rigidbody component3 = hit.collider.GetComponent<Rigidbody>();
			if (component3 != null)
			{
				float num = 5f;
				Vector3 forward = componentInChildren.transform.forward;
				component3.AddForceAtPosition(forward * num, hit.point, ForceMode.Impulse);
				PhysicsSoundPlayer component4 = component3.GetComponent<PhysicsSoundPlayer>();
				if (component4 != null)
				{
					component4.PlayImpactSound(1f, 1f);
				}
			}
		}
		yield break;
	}

	// Token: 0x06000684 RID: 1668 RVA: 0x00022264 File Offset: 0x00020464
	public override void PrimaryFire()
	{
	}

	// Token: 0x06000685 RID: 1669 RVA: 0x00022266 File Offset: 0x00020466
	public override void PrimaryFireHeld()
	{
		if (Time.time - this._lastAttackTime >= this.AttackCooldown)
		{
			this.SwingPickaxe();
		}
	}

	// Token: 0x06000686 RID: 1670 RVA: 0x00022282 File Offset: 0x00020482
	public override bool TryAddToInventory(int index = -1)
	{
		QuestManager instance = Singleton<QuestManager>.Instance;
		if (instance != null)
		{
			instance.ActivateQuestTrigger(TriggeredQuestRequirementType.CollectPickaxe, 1);
		}
		return base.TryAddToInventory(index);
	}

	// Token: 0x040007C2 RID: 1986
	public float UseRange = 2f;

	// Token: 0x040007C3 RID: 1987
	public float Damage = 10f;

	// Token: 0x040007C4 RID: 1988
	public float AttackCooldown = 1f;

	// Token: 0x040007C5 RID: 1989
	public LayerMask HitLayers;

	// Token: 0x040007C6 RID: 1990
	public bool CanBreakOreIntoCrushedPieces;

	// Token: 0x040007C7 RID: 1991
	private float _lastAttackTime = -1f;

	// Token: 0x040007C8 RID: 1992
	[SerializeField]
	private SoundDefinition _sound_hit_node;

	// Token: 0x040007C9 RID: 1993
	[SerializeField]
	private SoundDefinition _sound_hit_world;

	// Token: 0x040007CA RID: 1994
	[SerializeField]
	private SoundDefinition _sound_swing;

	// Token: 0x040007CB RID: 1995
	[SerializeField]
	private SoundPlayer _swingSoundPlayer;
}
