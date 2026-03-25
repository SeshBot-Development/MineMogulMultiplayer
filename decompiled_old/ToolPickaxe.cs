using System.Collections;
using UnityEngine;

public class ToolPickaxe : BaseHeldTool
{
	public float UseRange = 2f;

	public float Damage = 10f;

	public float AttackCooldown = 1f;

	public LayerMask HitLayers;

	public bool CanBreakOreIntoCrushedPieces;

	private float _lastAttackTime = -1f;

	[SerializeField]
	private SoundDefinition _sound_hit_node;

	[SerializeField]
	private SoundDefinition _sound_hit_world;

	[SerializeField]
	private SoundDefinition _sound_swing;

	[SerializeField]
	private SoundPlayer _swingSoundPlayer;

	public override string GetControlsText()
	{
		return "Drop - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool) + "\nMine - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.PrimaryAttack);
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
		if (!base.gameObject.activeInHierarchy || Owner == null)
		{
			yield break;
		}
		Camera componentInChildren = Owner.GetComponentInChildren<Camera>();
		if (componentInChildren == null || !Physics.Raycast(componentInChildren.transform.position, componentInChildren.transform.forward, out var hit, UseRange, HitLayers))
		{
			yield break;
		}
		if (CanBreakOreIntoCrushedPieces)
		{
			OrePiece component = hit.collider.GetComponent<OrePiece>();
			if (component != null && component.CrushedPrefab != null && component.CrushedPrefab.GetComponent<OrePiece>() != null && component.PieceType == PieceType.Ore && component.TryConvertToCrushed())
			{
				Singleton<ParticleManager>.Instance.CreateParticle(Singleton<ParticleManager>.Instance.OreNodeHitParticlePrefab, hit.point, Quaternion.LookRotation(hit.normal));
				yield return new WaitForFixedUpdate();
				PhysicsUtils.SimpleExplosion(hit.point, 0.5f, 2f, 0.5f);
				yield break;
			}
		}
		IDamageable component2 = hit.collider.GetComponent<IDamageable>();
		if (component2 != null)
		{
			component2.TakeDamage(Damage, hit.point);
			Singleton<ParticleManager>.Instance.CreateParticle(Singleton<ParticleManager>.Instance.OreNodeHitParticlePrefab, hit.point, Quaternion.LookRotation(hit.normal));
		}
		else
		{
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(_sound_hit_world, hit.point);
			Singleton<ParticleManager>.Instance.CreateParticle(Singleton<ParticleManager>.Instance.GenericHitImpactParticle, hit.point, Quaternion.LookRotation(hit.normal));
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
				component4.PlayImpactSound();
			}
		}
	}

	public override void PrimaryFire()
	{
	}

	public override void PrimaryFireHeld()
	{
		if (Time.time - _lastAttackTime >= AttackCooldown)
		{
			SwingPickaxe();
		}
	}

	public override bool TryAddToInventory(int index = -1)
	{
		Singleton<QuestManager>.Instance?.ActivateQuestTrigger(TriggeredQuestRequirementType.CollectPickaxe);
		return base.TryAddToInventory(index);
	}
}
