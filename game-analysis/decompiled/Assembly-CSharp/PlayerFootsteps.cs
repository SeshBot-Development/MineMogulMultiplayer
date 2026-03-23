using System;
using UnityEngine;

// Token: 0x02000086 RID: 134
public class PlayerFootsteps : MonoBehaviour
{
	// Token: 0x060003B5 RID: 949 RVA: 0x00013A71 File Offset: 0x00011C71
	private void Awake()
	{
		this._currentFootstepDefinition = this.DefaultFootstepDefinition;
		this._playerController = base.GetComponent<PlayerController>();
	}

	// Token: 0x060003B6 RID: 950 RVA: 0x00013A8C File Offset: 0x00011C8C
	private void Update()
	{
		if (this._playerController.IsUsingNoclip)
		{
			return;
		}
		float magnitude = (this._playerController.MoveInput * this._playerController.SelectedWalkSpeed).magnitude;
		if (magnitude > this.minMoveSpeed && this._playerController.CharacterController.isGrounded)
		{
			this.footstepTimer -= Time.deltaTime;
			float num = Mathf.Clamp01(magnitude / this._playerController.SprintSpeed);
			float num2 = Mathf.Lerp(this.baseFootstepInterval * 1.5f, this.baseFootstepInterval * 0.5f, num);
			if (this.footstepTimer <= 0f)
			{
				this.lastFootstepWasLeft = !this.lastFootstepWasLeft;
				this.PlayFootstep(this.lastFootstepWasLeft);
				this.footstepTimer = num2;
				return;
			}
		}
		else
		{
			this.footstepTimer -= Time.deltaTime * 2f;
		}
	}

	// Token: 0x060003B7 RID: 951 RVA: 0x00013B78 File Offset: 0x00011D78
	private void PlayFootstep(bool isLeft)
	{
		bool flag = this._playerController.IsInWater;
		RaycastHit raycastHit;
		if (!flag && Physics.Raycast(base.transform.position, Vector3.down, out raycastHit, 1.2f, this.GroundCheckLayerMask, QueryTriggerInteraction.Collide) && raycastHit.collider.gameObject.layer == 4)
		{
			flag = true;
		}
		this._currentFootstepDefinition = (flag ? this.WaterFootstepDefinition : this.DefaultFootstepDefinition);
		float num = ((this._playerController.SelectedWalkSpeed <= this._playerController.DuckSpeed) ? 0.5f : 1f);
		if (isLeft)
		{
			this.SoundPlayerLeft.PlaySound(this._currentFootstepDefinition.LeftFootstepDefinition, num, 1f, false);
			return;
		}
		this.SoundPlayerRight.PlaySound(this._currentFootstepDefinition.RightFootstepDefinition, num, 1f, false);
	}

	// Token: 0x040003EE RID: 1006
	public LayerMask GroundCheckLayerMask;

	// Token: 0x040003EF RID: 1007
	public FootstepSoundDefinition DefaultFootstepDefinition;

	// Token: 0x040003F0 RID: 1008
	public FootstepSoundDefinition WaterFootstepDefinition;

	// Token: 0x040003F1 RID: 1009
	public float baseFootstepInterval = 0.6f;

	// Token: 0x040003F2 RID: 1010
	public float minMoveSpeed = 0.1f;

	// Token: 0x040003F3 RID: 1011
	public SoundPlayer SoundPlayerLeft;

	// Token: 0x040003F4 RID: 1012
	public SoundPlayer SoundPlayerRight;

	// Token: 0x040003F5 RID: 1013
	private float footstepTimer;

	// Token: 0x040003F6 RID: 1014
	private bool lastFootstepWasLeft;

	// Token: 0x040003F7 RID: 1015
	private FootstepSoundDefinition _currentFootstepDefinition;

	// Token: 0x040003F8 RID: 1016
	private PlayerController _playerController;
}
