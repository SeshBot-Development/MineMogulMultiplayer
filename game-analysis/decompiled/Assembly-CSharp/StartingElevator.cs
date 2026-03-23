using System;
using System.Collections;
using UnityEngine;

// Token: 0x020000E5 RID: 229
[DefaultExecutionOrder(1000)]
public class StartingElevator : MonoBehaviour
{
	// Token: 0x06000620 RID: 1568 RVA: 0x00020040 File Offset: 0x0001E240
	private void OnEnable()
	{
		this.LandingParticle.SetActive(false);
		if (!Singleton<DebugManager>.Instance.DevModeEnabled && Singleton<SavingLoadingManager>.Instance.SceneWasLoadedFromNewGame)
		{
			this.TeleportPlayerAndLowerElevator();
		}
		else
		{
			this.RoofCollider.SetActive(false);
		}
		Singleton<GameManager>.Instance.GamePaused += this.OnGamePaused;
		Singleton<GameManager>.Instance.GameUnpaused += this.OnGameUnpaused;
	}

	// Token: 0x06000621 RID: 1569 RVA: 0x000200B1 File Offset: 0x0001E2B1
	private void OnDisable()
	{
		Singleton<GameManager>.Instance.GamePaused += this.OnGamePaused;
		Singleton<GameManager>.Instance.GameUnpaused += this.OnGameUnpaused;
	}

	// Token: 0x06000622 RID: 1570 RVA: 0x000200E0 File Offset: 0x0001E2E0
	private void Update()
	{
		if (this._isLowering)
		{
			Vector3 localPosition = base.transform.localPosition;
			float num = Mathf.Max(0f, localPosition.y - this.EndHeight);
			float num2 = Mathf.InverseLerp(0.15f, 0f, num);
			float num3 = Mathf.Lerp(1.25f, 0.1f, Mathf.Clamp01(num2));
			float num4 = Mathf.Lerp(0.02f, 0f, Mathf.Clamp01(num2));
			localPosition.y -= num3 * Time.deltaTime;
			localPosition.x = Mathf.PerlinNoise(Time.time * 20f, 0f) * num4 - num4 / 2f;
			localPosition.z = Mathf.PerlinNoise(0f, Time.time * 20f) * num4 - num4 / 2f;
			if (!this._hasPlayedLandingParticle && localPosition.y <= this.EndHeight + 1f)
			{
				this._hasPlayedLandingParticle = true;
				this.LandingParticle.SetActive(true);
			}
			if (localPosition.y <= this.EndHeight + 0.001f)
			{
				localPosition.y = this.EndHeight;
				localPosition.x = 0f;
				localPosition.z = 0f;
				this.RoofCollider.SetActive(false);
				this._isLowering = false;
			}
			base.transform.localPosition = localPosition;
		}
	}

	// Token: 0x06000623 RID: 1571 RVA: 0x00020243 File Offset: 0x0001E443
	private void OnGamePaused()
	{
		if (this._isLowering)
		{
			this.SoundPlayer.Pause();
		}
	}

	// Token: 0x06000624 RID: 1572 RVA: 0x00020258 File Offset: 0x0001E458
	private void OnGameUnpaused()
	{
		if (this._isLowering)
		{
			this.SoundPlayer.UnPause();
		}
	}

	// Token: 0x06000625 RID: 1573 RVA: 0x0002026D File Offset: 0x0001E46D
	public void TeleportPlayerAndLowerElevator()
	{
		this.LowerTheElevator();
		Object.FindObjectOfType<PlayerController>().TeleportPlayer(this.PlayerTeleportPosition.position);
	}

	// Token: 0x06000626 RID: 1574 RVA: 0x0002028C File Offset: 0x0001E48C
	public void LowerTheElevator()
	{
		this.LandingParticle.SetActive(false);
		this.RoofCollider.SetActive(true);
		base.transform.localPosition = new Vector3(0f, this.StartingHeight, 0f);
		this._isLowering = true;
		base.StartCoroutine(this.WaitThenPlayElevatorSound());
	}

	// Token: 0x06000627 RID: 1575 RVA: 0x000202E5 File Offset: 0x0001E4E5
	public IEnumerator WaitThenPlayElevatorSound()
	{
		yield return new WaitForEndOfFrame();
		this.SoundPlayer.PlaySound(this.LoweringSoundDefinition);
		Object.FindObjectOfType<PlayerController>().TeleportPlayer(this.PlayerTeleportPosition.position);
		yield break;
	}

	// Token: 0x0400075F RID: 1887
	public float StartingHeight = 15f;

	// Token: 0x04000760 RID: 1888
	public float EndHeight;

	// Token: 0x04000761 RID: 1889
	public Transform PlayerTeleportPosition;

	// Token: 0x04000762 RID: 1890
	public GameObject RoofCollider;

	// Token: 0x04000763 RID: 1891
	public SoundPlayer SoundPlayer;

	// Token: 0x04000764 RID: 1892
	public SoundDefinition LoweringSoundDefinition;

	// Token: 0x04000765 RID: 1893
	public GameObject LandingParticle;

	// Token: 0x04000766 RID: 1894
	private bool _isLowering;

	// Token: 0x04000767 RID: 1895
	private bool _hasPlayedLandingParticle;
}
