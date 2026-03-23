using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000E3 RID: 227
[DefaultExecutionOrder(-10)]
public class SoundManager : Singleton<SoundManager>
{
	// Token: 0x0600060D RID: 1549 RVA: 0x0001F9FC File Offset: 0x0001DBFC
	protected override void Awake()
	{
		base.Awake();
		if (Singleton<SoundManager>.Instance != this)
		{
			return;
		}
		this.InitializePool();
		this.InitializeConveyorAudioSources();
	}

	// Token: 0x0600060E RID: 1550 RVA: 0x0001FA1E File Offset: 0x0001DC1E
	private void OnEnable()
	{
		this.PlayerTransform = Object.FindObjectOfType<AudioListener>().transform;
	}

	// Token: 0x0600060F RID: 1551 RVA: 0x0001FA30 File Offset: 0x0001DC30
	private void InitializePool()
	{
		for (int i = 0; i < this._poolSize; i++)
		{
			SoundPlayer soundPlayer = Object.Instantiate<SoundPlayer>(this._soundPlayerPrefab);
			soundPlayer.gameObject.SetActive(false);
			this.soundPlayersPool.Enqueue(soundPlayer);
		}
	}

	// Token: 0x06000610 RID: 1552 RVA: 0x0001FA74 File Offset: 0x0001DC74
	public void PlayUISound(SoundDefinition definition, float volumeMultiplier = 1f)
	{
		Vector3 position = Object.FindObjectOfType<AudioListener>().transform.position;
		this.PlaySoundAtLocation(definition, position, volumeMultiplier, 1f, false, true);
	}

	// Token: 0x06000611 RID: 1553 RVA: 0x0001FAA4 File Offset: 0x0001DCA4
	public void PlaySoundAtLocation(AudioClip clip, Vector3 position, float volume = 1f, float maxRange = 20f, bool dontPlayIfTooFarFromPlayer = true)
	{
		if (dontPlayIfTooFarFromPlayer)
		{
			float sqrMagnitude = (this.PlayerTransform.position - position).sqrMagnitude;
			float num = maxRange * 1.25f;
			if (sqrMagnitude > num * num)
			{
				return;
			}
		}
		if (this.soundPlayersPool.Count > 0)
		{
			SoundPlayer soundPlayer = this.soundPlayersPool.Dequeue();
			soundPlayer.transform.position = position;
			soundPlayer.gameObject.SetActive(true);
			soundPlayer.PlaySound(clip, volume, maxRange);
		}
	}

	// Token: 0x06000612 RID: 1554 RVA: 0x0001FB18 File Offset: 0x0001DD18
	public void PlaySoundAtLocation(SoundDefinition definition, Vector3 position, float volumeMultiplier = 1f, float pitchMultiplier = 1f, bool dontPlayIfTooFarFromPlayer = true, bool isUISound = false)
	{
		if (dontPlayIfTooFarFromPlayer)
		{
			float sqrMagnitude = (this.PlayerTransform.position - position).sqrMagnitude;
			float num = definition.maxRange * 1.25f;
			if (sqrMagnitude > num * num)
			{
				return;
			}
		}
		if (this.soundPlayersPool.Count > 0)
		{
			SoundPlayer soundPlayer = this.soundPlayersPool.Dequeue();
			soundPlayer.transform.position = position;
			soundPlayer.gameObject.SetActive(true);
			soundPlayer.PlaySound(definition, volumeMultiplier, pitchMultiplier, isUISound);
		}
	}

	// Token: 0x06000613 RID: 1555 RVA: 0x0001FB91 File Offset: 0x0001DD91
	public void ReturnToPool(SoundPlayer player)
	{
		player.gameObject.SetActive(false);
		this.soundPlayersPool.Enqueue(player);
	}

	// Token: 0x06000614 RID: 1556 RVA: 0x0001FBAB File Offset: 0x0001DDAB
	public static AudioClip GetRandomSound(AudioClip[] soundArray)
	{
		return soundArray[Random.Range(0, soundArray.Length)];
	}

	// Token: 0x06000615 RID: 1557 RVA: 0x0001FBB8 File Offset: 0x0001DDB8
	private void InitializeConveyorAudioSources()
	{
		for (int i = 0; i < this._conveyorPoolSize; i++)
		{
			AudioSource audioSource = Object.Instantiate<AudioSource>(this._conveyorAudioSourcePrefab, base.transform);
			audioSource.clip = this._conveyorClip;
			audioSource.Play();
			audioSource.gameObject.SetActive(false);
			this.conveyorAudioSources.Add(audioSource);
		}
	}

	// Token: 0x06000616 RID: 1558 RVA: 0x0001FC12 File Offset: 0x0001DE12
	private void FixedUpdate()
	{
		if (this.PlayerTransform != null)
		{
			this.UpdateConveyorSounds();
		}
	}

	// Token: 0x06000617 RID: 1559 RVA: 0x0001FC28 File Offset: 0x0001DE28
	private void UpdateConveyorSounds()
	{
		Vector3 position = this.PlayerTransform.position;
		int num = Physics.OverlapSphereNonAlloc(position, this.ConveyorCheckRadius, this._conveyorHits, this._conveyorLayerMask, QueryTriggerInteraction.Collide);
		this._conveyorSet.Clear();
		this._bestConveyors.Clear();
		int count = this.conveyorAudioSources.Count;
		for (int i = 0; i < num; i++)
		{
			Collider collider = this._conveyorHits[i];
			ConveyorSoundSource conveyorSoundSource;
			if (collider && collider.TryGetComponent<ConveyorSoundSource>(out conveyorSoundSource) && this._conveyorSet.Add(conveyorSoundSource))
			{
				float sqrMagnitude = (conveyorSoundSource.transform.position - position).sqrMagnitude;
				int num2 = this._bestConveyors.Count;
				while (num2 > 0 && sqrMagnitude < this._bestConveyors[num2 - 1].sqrDist)
				{
					num2--;
				}
				if (num2 < count)
				{
					this._bestConveyors.Insert(num2, new SoundManager.ConveyorCandidate
					{
						conveyor = conveyorSoundSource,
						sqrDist = sqrMagnitude
					});
					if (this._bestConveyors.Count > count)
					{
						this._bestConveyors.RemoveAt(this._bestConveyors.Count - 1);
					}
				}
			}
		}
		int count2 = this._bestConveyors.Count;
		for (int j = 0; j < count2; j++)
		{
			AudioSource audioSource = this.conveyorAudioSources[j];
			audioSource.gameObject.SetActive(true);
			audioSource.transform.position = this._bestConveyors[j].conveyor.transform.position;
		}
		for (int k = count2; k < this.conveyorAudioSources.Count; k++)
		{
			this.conveyorAudioSources[k].gameObject.SetActive(false);
		}
	}

	// Token: 0x0400074B RID: 1867
	public SoundDefinition Sound_Ore_Crush;

	// Token: 0x0400074C RID: 1868
	public SoundDefinition Sound_Node_Break;

	// Token: 0x0400074D RID: 1869
	public SoundDefinition Sound_UI_Button_Hover;

	// Token: 0x0400074E RID: 1870
	public SoundDefinition Sound_UI_Inventory_Icon_Hover;

	// Token: 0x0400074F RID: 1871
	[SerializeField]
	private SoundPlayer _soundPlayerPrefab;

	// Token: 0x04000750 RID: 1872
	[SerializeField]
	private int _poolSize = 30;

	// Token: 0x04000751 RID: 1873
	[SerializeField]
	private int _conveyorPoolSize = 8;

	// Token: 0x04000752 RID: 1874
	private Queue<SoundPlayer> soundPlayersPool = new Queue<SoundPlayer>();

	// Token: 0x04000753 RID: 1875
	[SerializeField]
	private LayerMask _conveyorLayerMask;

	// Token: 0x04000754 RID: 1876
	[SerializeField]
	private AudioSource _conveyorAudioSourcePrefab;

	// Token: 0x04000755 RID: 1877
	[SerializeField]
	private AudioClip _conveyorClip;

	// Token: 0x04000756 RID: 1878
	private List<AudioSource> conveyorAudioSources = new List<AudioSource>();

	// Token: 0x04000757 RID: 1879
	private const int MaxConveyorHits = 128;

	// Token: 0x04000758 RID: 1880
	private readonly Collider[] _conveyorHits = new Collider[128];

	// Token: 0x04000759 RID: 1881
	private readonly HashSet<ConveyorSoundSource> _conveyorSet = new HashSet<ConveyorSoundSource>();

	// Token: 0x0400075A RID: 1882
	private readonly List<SoundManager.ConveyorCandidate> _bestConveyors = new List<SoundManager.ConveyorCandidate>(16);

	// Token: 0x0400075B RID: 1883
	public Transform PlayerTransform;

	// Token: 0x0400075C RID: 1884
	public float ConveyorCheckRadius = 6f;

	// Token: 0x02000182 RID: 386
	private struct ConveyorCandidate
	{
		// Token: 0x0400098B RID: 2443
		public ConveyorSoundSource conveyor;

		// Token: 0x0400098C RID: 2444
		public float sqrDist;
	}
}
