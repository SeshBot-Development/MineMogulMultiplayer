using System.Collections;
using UnityEngine;

public class ToolCastingMold : BaseHeldTool
{
	public CastingMoldType CastingMoldType;

	public float UseRange = 3f;

	public float AttackCooldown = 1f;

	public LayerMask MoldAreaLayer;

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
		return "Drop - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool) + "\nAttach to Casting Furnace - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.PrimaryAttack);
	}

	private void SwingPickaxe()
	{
		if (!(Owner == null) && !(Owner.PlayerCamera == null))
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
		Camera playerCamera = Owner.PlayerCamera;
		if (playerCamera == null)
		{
			yield break;
		}
		if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out var hitInfo, UseRange, MoldAreaLayer, QueryTriggerInteraction.Collide))
		{
			CastingFurnaceMoldArea component = hitInfo.collider.GetComponent<CastingFurnaceMoldArea>();
			if (component != null)
			{
				component.InsertMold(CastingMoldType);
				Object.Destroy(base.gameObject);
				yield break;
			}
		}
		if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hitInfo, UseRange, HitLayers))
		{
			Rigidbody component2 = hitInfo.collider.GetComponent<Rigidbody>();
			if (component2 != null)
			{
				float num = 5f;
				Vector3 forward = playerCamera.transform.forward;
				component2.AddForceAtPosition(forward * num, hitInfo.point, ForceMode.Impulse);
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(_sound_hit_world, hitInfo.point);
				Singleton<ParticleManager>.Instance.CreateParticle(Singleton<ParticleManager>.Instance.GenericHitImpactParticle, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
			}
		}
	}

	public override void PrimaryFire()
	{
		if (Time.time - _lastAttackTime >= AttackCooldown)
		{
			SwingPickaxe();
		}
	}
}
