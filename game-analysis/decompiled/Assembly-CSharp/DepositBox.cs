using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000040 RID: 64
[DefaultExecutionOrder(100)]
public class DepositBox : MonoBehaviour
{
	// Token: 0x060001B3 RID: 435 RVA: 0x000093FC File Offset: 0x000075FC
	private void OnEnable()
	{
		this.Tier1Box.SetActive(!this.HasUpgradedToTier2);
		this.Tier2Box.SetActive(this.HasUpgradedToTier2);
		this.SetupBuckets();
		this._timeSinceLastSell = 9999f;
		this._gearAnimation["GearAnimation"].speed = 0f;
		this._objectToDisableWhenStopped.SetActive(false);
		Singleton<EconomyManager>.Instance.ItemSold += this.UpdateLastSellTime;
	}

	// Token: 0x060001B4 RID: 436 RVA: 0x00009480 File Offset: 0x00007680
	private void OnDisable()
	{
		Singleton<EconomyManager>.Instance.ItemSold -= this.UpdateLastSellTime;
	}

	// Token: 0x060001B5 RID: 437 RVA: 0x00009498 File Offset: 0x00007698
	private void UpdateLastSellTime()
	{
		this._timeSinceLastSell = 0f;
	}

	// Token: 0x060001B6 RID: 438 RVA: 0x000094AA File Offset: 0x000076AA
	public void UpgradeToTier2()
	{
		if (this.HasUpgradedToTier2)
		{
			return;
		}
		this.HasUpgradedToTier2 = true;
		this.Tier1Box.SetActive(false);
		this.Tier2Box.SetActive(true);
	}

	// Token: 0x060001B7 RID: 439 RVA: 0x000094D4 File Offset: 0x000076D4
	private void Update()
	{
		float num = this._speed;
		if ((in this._timeSinceLastSell) > this._speed * (float)this._upBuckets.Count)
		{
			num = 0f;
		}
		else
		{
			num = this._speed;
		}
		float num2 = ((num > this._currentSpeed) ? this._bucketAcceleration : this._bucketDeceleration);
		this._currentSpeed = Mathf.MoveTowards(this._currentSpeed, num, num2 * Time.deltaTime);
		bool flag = this._currentSpeed > 0f;
		this.UpdateMotorSound();
		if (flag != this._wasSpinning)
		{
			this._objectToDisableWhenStopped.SetActive(flag);
			Material[] sharedMaterials = this._beltRenderer.sharedMaterials;
			sharedMaterials[1] = (flag ? this._beltMovingMaterial : this._beltStoppedMaterial);
			this._beltRenderer.sharedMaterials = sharedMaterials;
		}
		if (flag)
		{
			this._gearAnimation["GearAnimation"].speed = this._currentSpeed / this._speed;
			float num3 = this._currentSpeed * Time.deltaTime;
			foreach (Transform transform in this._downBuckets)
			{
				if (!(transform == null))
				{
					Vector3 position = transform.position;
					position.y -= num3;
					if (position.y <= this._downBottomY)
					{
						position.y += this._downPathHeight;
					}
					transform.position = position;
				}
			}
			float num4 = this._upBottomY + this._upPathHeight;
			foreach (Transform transform2 in this._upBuckets)
			{
				if (!(transform2 == null))
				{
					Vector3 position2 = transform2.position;
					position2.y += num3;
					if (position2.y >= num4)
					{
						position2.y -= this._upPathHeight;
					}
					transform2.position = position2;
				}
			}
		}
		this._wasSpinning = flag;
	}

	// Token: 0x060001B8 RID: 440 RVA: 0x000096F8 File Offset: 0x000078F8
	private void SetupBuckets()
	{
		this._downPathHeight = this._bucketSpacing * (float)Mathf.Max(1, this._downBuckets.Count);
		this._upPathHeight = this._bucketSpacing * (float)Mathf.Max(1, this._upBuckets.Count);
		if (this._downBuckets.Count > 0)
		{
			float num = this._downBottomY + (float)(this._downBuckets.Count - 1) * this._bucketSpacing;
			for (int i = 0; i < this._downBuckets.Count; i++)
			{
				Transform transform = this._downBuckets[i];
				if (!(transform == null))
				{
					Vector3 position = transform.position;
					position.y = num - (float)i * this._bucketSpacing;
					transform.position = position;
				}
			}
		}
		if (this._upBuckets.Count > 0)
		{
			float upBottomY = this._upBottomY;
			for (int j = 0; j < this._upBuckets.Count; j++)
			{
				Transform transform2 = this._upBuckets[j];
				if (!(transform2 == null))
				{
					Vector3 position2 = transform2.position;
					position2.y = upBottomY + (float)j * this._bucketSpacing;
					transform2.position = position2;
				}
			}
		}
	}

	// Token: 0x060001B9 RID: 441 RVA: 0x0000982C File Offset: 0x00007A2C
	private void UpdateMotorSound()
	{
		if (this._loopAudioSource == null)
		{
			return;
		}
		float num = 0f;
		if (this._speed > 0f)
		{
			num = Mathf.Clamp01(this._currentSpeed / this._speed);
		}
		this._loopAudioSource.pitch = Mathf.Lerp(this._minPitch, this._maxPitch, num);
		this._loopAudioSource.volume = Mathf.Lerp(this._minVolume, this._maxVolume, num);
		if (num > this._soundStopThreshold)
		{
			if (!this._loopAudioSource.isPlaying)
			{
				this._loopAudioSource.Play();
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._startSoundDefinition, this._loopAudioSource.transform.position, 1f, 1f, true, false);
				return;
			}
		}
		else if (this._loopAudioSource.isPlaying)
		{
			this._loopAudioSource.Stop();
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._stopSoundDefinition, this._loopAudioSource.transform.position, 1f, 1f, true, false);
		}
	}

	// Token: 0x0400019B RID: 411
	[SerializeField]
	private GameObject Tier1Box;

	// Token: 0x0400019C RID: 412
	[SerializeField]
	private GameObject Tier2Box;

	// Token: 0x0400019D RID: 413
	[SerializeField]
	private Animation _gearAnimation;

	// Token: 0x0400019E RID: 414
	[SerializeField]
	private GameObject _objectToDisableWhenStopped;

	// Token: 0x0400019F RID: 415
	[SerializeField]
	private Material _beltMovingMaterial;

	// Token: 0x040001A0 RID: 416
	[SerializeField]
	private Material _beltStoppedMaterial;

	// Token: 0x040001A1 RID: 417
	[SerializeField]
	private Renderer _beltRenderer;

	// Token: 0x040001A2 RID: 418
	[Header("Buckets")]
	[SerializeField]
	private float _speed = 2f;

	// Token: 0x040001A3 RID: 419
	[SerializeField]
	private float _downBottomY;

	// Token: 0x040001A4 RID: 420
	[SerializeField]
	private float _upBottomY;

	// Token: 0x040001A5 RID: 421
	[SerializeField]
	private float _bucketSpacing = 1.86f;

	// Token: 0x040001A6 RID: 422
	[SerializeField]
	private float _bucketAcceleration = 0.2f;

	// Token: 0x040001A7 RID: 423
	[SerializeField]
	private float _bucketDeceleration = 0.5f;

	// Token: 0x040001A8 RID: 424
	[SerializeField]
	private List<Transform> _downBuckets = new List<Transform>();

	// Token: 0x040001A9 RID: 425
	[SerializeField]
	private List<Transform> _upBuckets = new List<Transform>();

	// Token: 0x040001AA RID: 426
	[Header("Sound")]
	[SerializeField]
	private float _minPitch = 0.6f;

	// Token: 0x040001AB RID: 427
	[SerializeField]
	private float _maxPitch = 1.1f;

	// Token: 0x040001AC RID: 428
	[SerializeField]
	private float _minVolume;

	// Token: 0x040001AD RID: 429
	[SerializeField]
	private float _maxVolume = 3f;

	// Token: 0x040001AE RID: 430
	[SerializeField]
	private AudioSource _loopAudioSource;

	// Token: 0x040001AF RID: 431
	[SerializeField]
	private float _soundStopThreshold = 0.03f;

	// Token: 0x040001B0 RID: 432
	[SerializeField]
	private SoundDefinition _startSoundDefinition;

	// Token: 0x040001B1 RID: 433
	[SerializeField]
	private SoundDefinition _stopSoundDefinition;

	// Token: 0x040001B2 RID: 434
	private float _downPathHeight;

	// Token: 0x040001B3 RID: 435
	private float _upPathHeight;

	// Token: 0x040001B4 RID: 436
	private float _currentSpeed;

	// Token: 0x040001B5 RID: 437
	private bool _wasSpinning;

	// Token: 0x040001B6 RID: 438
	private TimeSince _timeSinceLastSell;

	// Token: 0x040001B7 RID: 439
	[Header("Misc")]
	public bool HasUpgradedToTier2;
}
