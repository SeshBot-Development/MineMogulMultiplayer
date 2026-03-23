using System;
using System.Collections;
using UnityEngine;

// Token: 0x020000EC RID: 236
public class ToolCastingMold : BaseHeldTool
{
	// Token: 0x0600064F RID: 1615 RVA: 0x00020F7E File Offset: 0x0001F17E
	public override string GetControlsText()
	{
		return "Drop - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool) + "\nAttach to Casting Furnace - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.PrimaryAttack);
	}

	// Token: 0x06000650 RID: 1616 RVA: 0x00020FA8 File Offset: 0x0001F1A8
	private void SwingPickaxe()
	{
		if (this.Owner == null)
		{
			return;
		}
		if (this.Owner.PlayerCamera == null)
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

	// Token: 0x06000651 RID: 1617 RVA: 0x0002102A File Offset: 0x0001F22A
	private IEnumerator PerformAttack(float delaySeconds)
	{
		yield return new WaitForSeconds(delaySeconds);
		Camera playerCamera = this.Owner.PlayerCamera;
		if (playerCamera == null)
		{
			yield break;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out raycastHit, this.UseRange, this.MoldAreaLayer, QueryTriggerInteraction.Collide))
		{
			CastingFurnaceMoldArea component = raycastHit.collider.GetComponent<CastingFurnaceMoldArea>();
			if (component != null)
			{
				component.InsertMold(this.CastingMoldType);
				Object.Destroy(base.gameObject);
				yield break;
			}
		}
		if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out raycastHit, this.UseRange, this.HitLayers))
		{
			Rigidbody component2 = raycastHit.collider.GetComponent<Rigidbody>();
			if (component2 != null)
			{
				float num = 5f;
				Vector3 forward = playerCamera.transform.forward;
				component2.AddForceAtPosition(forward * num, raycastHit.point, ForceMode.Impulse);
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._sound_hit_world, raycastHit.point, 1f, 1f, true, false);
				Singleton<ParticleManager>.Instance.CreateParticle(Singleton<ParticleManager>.Instance.GenericHitImpactParticle, raycastHit.point, Quaternion.LookRotation(raycastHit.normal), default(Vector3));
			}
		}
		yield break;
	}

	// Token: 0x06000652 RID: 1618 RVA: 0x00021040 File Offset: 0x0001F240
	public override void PrimaryFire()
	{
		if (Time.time - this._lastAttackTime >= this.AttackCooldown)
		{
			this.SwingPickaxe();
		}
	}

	// Token: 0x0400078C RID: 1932
	public CastingMoldType CastingMoldType;

	// Token: 0x0400078D RID: 1933
	public float UseRange = 3f;

	// Token: 0x0400078E RID: 1934
	public float AttackCooldown = 1f;

	// Token: 0x0400078F RID: 1935
	public LayerMask MoldAreaLayer;

	// Token: 0x04000790 RID: 1936
	public LayerMask HitLayers;

	// Token: 0x04000791 RID: 1937
	private float _lastAttackTime = -1f;

	// Token: 0x04000792 RID: 1938
	[SerializeField]
	private SoundDefinition _sound_hit_world;

	// Token: 0x04000793 RID: 1939
	[SerializeField]
	private SoundDefinition _sound_swing;

	// Token: 0x04000794 RID: 1940
	[SerializeField]
	private SoundPlayer _swingSoundPlayer;
}
