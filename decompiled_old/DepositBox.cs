using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class DepositBox : MonoBehaviour
{
	[SerializeField]
	private GameObject Tier1Box;

	[SerializeField]
	private GameObject Tier2Box;

	[SerializeField]
	private Animation _gearAnimation;

	[SerializeField]
	private GameObject _objectToDisableWhenStopped;

	[SerializeField]
	private Material _beltMovingMaterial;

	[SerializeField]
	private Material _beltStoppedMaterial;

	[SerializeField]
	private Renderer _beltRenderer;

	[Header("Buckets")]
	[SerializeField]
	private float _speed = 2f;

	[SerializeField]
	private float _downBottomY;

	[SerializeField]
	private float _upBottomY;

	[SerializeField]
	private float _bucketSpacing = 1.86f;

	[SerializeField]
	private float _bucketAcceleration = 0.2f;

	[SerializeField]
	private float _bucketDeceleration = 0.5f;

	[SerializeField]
	private List<Transform> _downBuckets = new List<Transform>();

	[SerializeField]
	private List<Transform> _upBuckets = new List<Transform>();

	[Header("Sound")]
	[SerializeField]
	private float _minPitch = 0.6f;

	[SerializeField]
	private float _maxPitch = 1.1f;

	[SerializeField]
	private float _minVolume;

	[SerializeField]
	private float _maxVolume = 3f;

	[SerializeField]
	private AudioSource _loopAudioSource;

	[SerializeField]
	private float _soundStopThreshold = 0.03f;

	[SerializeField]
	private SoundDefinition _startSoundDefinition;

	[SerializeField]
	private SoundDefinition _stopSoundDefinition;

	private float _downPathHeight;

	private float _upPathHeight;

	private float _currentSpeed;

	private bool _wasSpinning;

	private TimeSince _timeSinceLastSell;

	[Header("Misc")]
	public bool HasUpgradedToTier2;

	private void OnEnable()
	{
		Tier1Box.SetActive(!HasUpgradedToTier2);
		Tier2Box.SetActive(HasUpgradedToTier2);
		SetupBuckets();
		_timeSinceLastSell = 9999f;
		_gearAnimation["GearAnimation"].speed = 0f;
		_objectToDisableWhenStopped.SetActive(value: false);
		Singleton<EconomyManager>.Instance.ItemSold += UpdateLastSellTime;
	}

	private void OnDisable()
	{
		Singleton<EconomyManager>.Instance.ItemSold -= UpdateLastSellTime;
	}

	private void UpdateLastSellTime()
	{
		_timeSinceLastSell = 0f;
	}

	public void UpgradeToTier2()
	{
		if (!HasUpgradedToTier2)
		{
			HasUpgradedToTier2 = true;
			Tier1Box.SetActive(value: false);
			Tier2Box.SetActive(value: true);
		}
	}

	private void Update()
	{
		float speed = _speed;
		speed = ((!(_timeSinceLastSell > _speed * (float)_upBuckets.Count)) ? _speed : 0f);
		float num = ((speed > _currentSpeed) ? _bucketAcceleration : _bucketDeceleration);
		_currentSpeed = Mathf.MoveTowards(_currentSpeed, speed, num * Time.deltaTime);
		bool flag = _currentSpeed > 0f;
		UpdateMotorSound();
		if (flag != _wasSpinning)
		{
			_objectToDisableWhenStopped.SetActive(flag);
			Material[] sharedMaterials = _beltRenderer.sharedMaterials;
			sharedMaterials[1] = (flag ? _beltMovingMaterial : _beltStoppedMaterial);
			_beltRenderer.sharedMaterials = sharedMaterials;
		}
		if (flag)
		{
			_gearAnimation["GearAnimation"].speed = _currentSpeed / _speed;
			float num2 = _currentSpeed * Time.deltaTime;
			foreach (Transform downBucket in _downBuckets)
			{
				if (!(downBucket == null))
				{
					Vector3 position = downBucket.position;
					position.y -= num2;
					if (position.y <= _downBottomY)
					{
						position.y += _downPathHeight;
					}
					downBucket.position = position;
				}
			}
			float num3 = _upBottomY + _upPathHeight;
			foreach (Transform upBucket in _upBuckets)
			{
				if (!(upBucket == null))
				{
					Vector3 position2 = upBucket.position;
					position2.y += num2;
					if (position2.y >= num3)
					{
						position2.y -= _upPathHeight;
					}
					upBucket.position = position2;
				}
			}
		}
		_wasSpinning = flag;
	}

	private void SetupBuckets()
	{
		_downPathHeight = _bucketSpacing * (float)Mathf.Max(1, _downBuckets.Count);
		_upPathHeight = _bucketSpacing * (float)Mathf.Max(1, _upBuckets.Count);
		if (_downBuckets.Count > 0)
		{
			float num = _downBottomY + (float)(_downBuckets.Count - 1) * _bucketSpacing;
			for (int i = 0; i < _downBuckets.Count; i++)
			{
				Transform transform = _downBuckets[i];
				if (!(transform == null))
				{
					Vector3 position = transform.position;
					position.y = num - (float)i * _bucketSpacing;
					transform.position = position;
				}
			}
		}
		if (_upBuckets.Count <= 0)
		{
			return;
		}
		float upBottomY = _upBottomY;
		for (int j = 0; j < _upBuckets.Count; j++)
		{
			Transform transform2 = _upBuckets[j];
			if (!(transform2 == null))
			{
				Vector3 position2 = transform2.position;
				position2.y = upBottomY + (float)j * _bucketSpacing;
				transform2.position = position2;
			}
		}
	}

	private void UpdateMotorSound()
	{
		if (_loopAudioSource == null)
		{
			return;
		}
		float num = 0f;
		if (_speed > 0f)
		{
			num = Mathf.Clamp01(_currentSpeed / _speed);
		}
		_loopAudioSource.pitch = Mathf.Lerp(_minPitch, _maxPitch, num);
		_loopAudioSource.volume = Mathf.Lerp(_minVolume, _maxVolume, num);
		if (num > _soundStopThreshold)
		{
			if (!_loopAudioSource.isPlaying)
			{
				_loopAudioSource.Play();
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(_startSoundDefinition, _loopAudioSource.transform.position);
			}
		}
		else if (_loopAudioSource.isPlaying)
		{
			_loopAudioSource.Stop();
			Singleton<SoundManager>.Instance.PlaySoundAtLocation(_stopSoundDefinition, _loopAudioSource.transform.position);
		}
	}
}
