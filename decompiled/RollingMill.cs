using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RollingMill : MonoBehaviour
{
	public Animator RollingAnimator;

	public float ProcessingTime = 3f;

	public Transform PlateTransform;

	public Transform OutputTransform;

	public List<ParticleSystem> JammedParticles;

	public float LaunchJammedObjectCooldown = 1f;

	public SoundDefinition LaunchJammedObjectSound;

	public SoundDefinition ProcessPlateSound;

	public SoundPlayer ProcessPlateSoundPlayer;

	private float _lastLaunchTime;

	private bool _isJammed;

	private List<OrePiece> _waitingList = new List<OrePiece>();

	private HashSet<OrePiece> _jammedOre = new HashSet<OrePiece>();

	private bool _isProcessing;

	[SerializeField]
	private MeshRenderer _lightMeshRenderer;

	[SerializeField]
	private LoopingSoundPlayer _jammedLoopingAudioSource;

	[SerializeField]
	private Renderer _plateRenderer;

	[SerializeField]
	private Renderer _platePolishedRenderer;

	private void OnEnable()
	{
		_plateRenderer.enabled = false;
		_platePolishedRenderer.enabled = false;
		RefreshJammedEffects();
	}

	private void Update()
	{
		bool isJammed = _isJammed;
		_isJammed = _jammedOre.Count > 0;
		if (_isJammed != isJammed)
		{
			RefreshJammedEffects();
			_lastLaunchTime = Time.time;
		}
		if (_isJammed && Time.time - _lastLaunchTime > LaunchJammedObjectCooldown)
		{
			_lastLaunchTime = Time.time;
			if (Random.value < 0.5f)
			{
				StartCoroutine(WaitThenLaunchRandomJammedOre());
			}
		}
	}

	private IEnumerator WaitThenLaunchRandomJammedOre()
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(LaunchJammedObjectSound, base.transform.position);
		yield return new WaitForSeconds(0.3f);
		LaunchRandomJammedOre();
	}

	private void LaunchRandomJammedOre()
	{
		if (this == null || _jammedOre.Count == 0)
		{
			return;
		}
		int index = Random.Range(0, _jammedOre.Count);
		OrePiece orePiece = _jammedOre.ElementAt(index);
		if (!(orePiece == null))
		{
			Rigidbody component = orePiece.GetComponent<Rigidbody>();
			if (component != null)
			{
				Vector3 vector = Vector3.up + -base.transform.forward + Random.insideUnitSphere * 0.2f;
				float num = 2f;
				float num2 = 6f;
				component.AddForce(vector * num, ForceMode.Impulse);
				component.AddTorque(vector * num2, ForceMode.Impulse);
			}
		}
	}

	private void RefreshJammedEffects()
	{
		foreach (ParticleSystem jammedParticle in JammedParticles)
		{
			ParticleSystem.EmissionModule emission = jammedParticle.emission;
			emission.enabled = _isJammed;
			ChangeLightMaterial(_isJammed ? Singleton<BuildingManager>.Instance.OrangeLightMaterial : Singleton<BuildingManager>.Instance.GreenLightMaterial);
			_jammedLoopingAudioSource.Toggle(_isJammed);
		}
	}

	private void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = _lightMeshRenderer.sharedMaterials;
		sharedMaterials[3] = material;
		_lightMeshRenderer.sharedMaterials = sharedMaterials;
	}

	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (!(component != null))
		{
			return;
		}
		if (component.PlatePrefab != null)
		{
			if (!_isProcessing)
			{
				_isProcessing = true;
				StartCoroutine(PressIngot(component));
			}
			else
			{
				_waitingList.Add(component);
			}
		}
		else
		{
			_jammedOre.Add(component);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			_waitingList.Remove(component);
			_jammedOre.Remove(component);
		}
	}

	public void OnAllOreCleared()
	{
		_jammedOre.Clear();
		_waitingList.Clear();
	}

	private IEnumerator PressIngot(OrePiece orePiece)
	{
		if (orePiece.PlatePrefab != null)
		{
			if (orePiece.PlatePrefab.GetComponent<OrePiece>() != null)
			{
				GameObject platePrefab = orePiece.PlatePrefab;
				orePiece.Delete();
				if (orePiece.IsPolished)
				{
					_platePolishedRenderer.sharedMaterial = orePiece.PlatePrefab.GetComponent<Renderer>()?.sharedMaterial;
				}
				else
				{
					_plateRenderer.sharedMaterial = orePiece.PlatePrefab.GetComponent<Renderer>()?.sharedMaterial;
				}
				_plateRenderer.enabled = !orePiece.IsPolished;
				_platePolishedRenderer.enabled = orePiece.IsPolished;
				ProcessPlateSoundPlayer.PlaySound(ProcessPlateSound);
				_isProcessing = true;
				if (RollingAnimator != null)
				{
					RollingAnimator.speed = 1f / ProcessingTime;
					RollingAnimator.Play("RollingMill_Process", -1, 0f);
				}
				yield return new WaitForSeconds(ProcessingTime * 0.9f);
				_plateRenderer.enabled = false;
				_platePolishedRenderer.enabled = false;
				OrePiece orePiece2 = Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(platePrefab, OutputTransform.position, Quaternion.identity);
				if (orePiece2 != null)
				{
					orePiece2.transform.rotation = OutputTransform.rotation;
				}
			}
			else
			{
				orePiece.ConvertToPlate();
			}
		}
		_isProcessing = false;
		if (_waitingList.Count > 0)
		{
			OrePiece orePiece3 = _waitingList[0];
			_waitingList.RemoveAt(0);
			StartCoroutine(PressIngot(orePiece3));
		}
	}
}
