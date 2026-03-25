using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class SoundManager : Singleton<SoundManager>
{
	private struct ConveyorCandidate
	{
		public ConveyorSoundSource conveyor;

		public float sqrDist;
	}

	public SoundDefinition Sound_Ore_Crush;

	public SoundDefinition Sound_Node_Break;

	public SoundDefinition Sound_UI_Button_Hover;

	public SoundDefinition Sound_UI_Inventory_Icon_Hover;

	[SerializeField]
	private SoundPlayer _soundPlayerPrefab;

	[SerializeField]
	private int _poolSize = 30;

	[SerializeField]
	private int _conveyorPoolSize = 8;

	private Queue<SoundPlayer> soundPlayersPool = new Queue<SoundPlayer>();

	[SerializeField]
	private LayerMask _conveyorLayerMask;

	[SerializeField]
	private AudioSource _conveyorAudioSourcePrefab;

	[SerializeField]
	private AudioClip _conveyorClip;

	private List<AudioSource> conveyorAudioSources = new List<AudioSource>();

	private const int MaxConveyorHits = 128;

	private readonly Collider[] _conveyorHits = new Collider[128];

	private readonly HashSet<ConveyorSoundSource> _conveyorSet = new HashSet<ConveyorSoundSource>();

	private readonly List<ConveyorCandidate> _bestConveyors = new List<ConveyorCandidate>(16);

	public Transform PlayerTransform;

	public float ConveyorCheckRadius = 6f;

	protected override void Awake()
	{
		base.Awake();
		if (!(Singleton<SoundManager>.Instance != this))
		{
			InitializePool();
			InitializeConveyorAudioSources();
		}
	}

	private void OnEnable()
	{
		PlayerTransform = Object.FindObjectOfType<AudioListener>().transform;
	}

	private void InitializePool()
	{
		for (int i = 0; i < _poolSize; i++)
		{
			SoundPlayer soundPlayer = Object.Instantiate(_soundPlayerPrefab);
			soundPlayer.gameObject.SetActive(value: false);
			soundPlayersPool.Enqueue(soundPlayer);
		}
	}

	public void PlayUISound(SoundDefinition definition, float volumeMultiplier = 1f)
	{
		Vector3 position = Object.FindObjectOfType<AudioListener>().transform.position;
		PlaySoundAtLocation(definition, position, volumeMultiplier, 1f, dontPlayIfTooFarFromPlayer: false, isUISound: true);
	}

	public void PlaySoundAtLocation(AudioClip clip, Vector3 position, float volume = 1f, float maxRange = 20f, bool dontPlayIfTooFarFromPlayer = true)
	{
		if (dontPlayIfTooFarFromPlayer)
		{
			float sqrMagnitude = (PlayerTransform.position - position).sqrMagnitude;
			float num = maxRange * 1.25f;
			if (sqrMagnitude > num * num)
			{
				return;
			}
		}
		if (soundPlayersPool.Count > 0)
		{
			SoundPlayer soundPlayer = soundPlayersPool.Dequeue();
			soundPlayer.transform.position = position;
			soundPlayer.gameObject.SetActive(value: true);
			soundPlayer.PlaySound(clip, volume, maxRange);
		}
	}

	public void PlaySoundAtLocation(SoundDefinition definition, Vector3 position, float volumeMultiplier = 1f, float pitchMultiplier = 1f, bool dontPlayIfTooFarFromPlayer = true, bool isUISound = false)
	{
		if (dontPlayIfTooFarFromPlayer)
		{
			float sqrMagnitude = (PlayerTransform.position - position).sqrMagnitude;
			float num = definition.maxRange * 1.25f;
			if (sqrMagnitude > num * num)
			{
				return;
			}
		}
		if (soundPlayersPool.Count > 0)
		{
			SoundPlayer soundPlayer = soundPlayersPool.Dequeue();
			soundPlayer.transform.position = position;
			soundPlayer.gameObject.SetActive(value: true);
			soundPlayer.PlaySound(definition, volumeMultiplier, pitchMultiplier, isUISound);
		}
	}

	public void ReturnToPool(SoundPlayer player)
	{
		player.gameObject.SetActive(value: false);
		soundPlayersPool.Enqueue(player);
	}

	public static AudioClip GetRandomSound(AudioClip[] soundArray)
	{
		return soundArray[Random.Range(0, soundArray.Length)];
	}

	private void InitializeConveyorAudioSources()
	{
		for (int i = 0; i < _conveyorPoolSize; i++)
		{
			AudioSource audioSource = Object.Instantiate(_conveyorAudioSourcePrefab, base.transform);
			audioSource.clip = _conveyorClip;
			audioSource.Play();
			audioSource.gameObject.SetActive(value: false);
			conveyorAudioSources.Add(audioSource);
		}
	}

	private void FixedUpdate()
	{
		if (PlayerTransform != null)
		{
			UpdateConveyorSounds();
		}
	}

	private void UpdateConveyorSounds()
	{
		Vector3 position = PlayerTransform.position;
		int num = Physics.OverlapSphereNonAlloc(position, ConveyorCheckRadius, _conveyorHits, _conveyorLayerMask, QueryTriggerInteraction.Collide);
		_conveyorSet.Clear();
		_bestConveyors.Clear();
		int count = conveyorAudioSources.Count;
		for (int i = 0; i < num; i++)
		{
			Collider collider = _conveyorHits[i];
			if (!collider || !collider.TryGetComponent<ConveyorSoundSource>(out var component) || !_conveyorSet.Add(component))
			{
				continue;
			}
			float sqrMagnitude = (component.transform.position - position).sqrMagnitude;
			int num2 = _bestConveyors.Count;
			while (num2 > 0 && sqrMagnitude < _bestConveyors[num2 - 1].sqrDist)
			{
				num2--;
			}
			if (num2 < count)
			{
				_bestConveyors.Insert(num2, new ConveyorCandidate
				{
					conveyor = component,
					sqrDist = sqrMagnitude
				});
				if (_bestConveyors.Count > count)
				{
					_bestConveyors.RemoveAt(_bestConveyors.Count - 1);
				}
			}
		}
		int count2 = _bestConveyors.Count;
		for (int j = 0; j < count2; j++)
		{
			AudioSource audioSource = conveyorAudioSources[j];
			audioSource.gameObject.SetActive(value: true);
			audioSource.transform.position = _bestConveyors[j].conveyor.transform.position;
		}
		for (int k = count2; k < conveyorAudioSources.Count; k++)
		{
			conveyorAudioSources[k].gameObject.SetActive(value: false);
		}
	}
}
