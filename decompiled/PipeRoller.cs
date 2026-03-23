using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PipeRoller : MonoBehaviour
{
	public Animator Animator;

	public Transform Plate1Transform;

	public Transform Plate2Transform;

	public Transform PipeTransform;

	public Transform OutputTransform;

	public List<ParticleSystem> JammedParticles;

	public float LaunchJammedObjectCooldown = 1f;

	public SoundDefinition LaunchJammedObjectSound;

	public SoundDefinition Process1Sound;

	public SoundDefinition CreatePipeSound;

	public SoundPlayer ProcessSoundPlayer;

	public GameObject SlagPipePrefab;

	private float _lastLaunchTime;

	private bool _isJammed;

	private List<OrePiece> _waitingList = new List<OrePiece>();

	private HashSet<OrePiece> _jammedOre = new HashSet<OrePiece>();

	private bool _isProcessing;

	private AnimationClip _firstPlateAnimationClip;

	private AnimationClip _secondPlateAnimationClip;

	[SerializeField]
	private SkinnedMeshRenderer _lightMeshRenderer;

	[SerializeField]
	private LoopingSoundPlayer _jammedLoopingAudioSource;

	[SerializeField]
	private SkinnedMeshRenderer _plate1Renderer;

	[SerializeField]
	private SkinnedMeshRenderer _plate1PolishedRenderer;

	[SerializeField]
	private SkinnedMeshRenderer _plate2Renderer;

	[SerializeField]
	private SkinnedMeshRenderer _plate2PolishedRenderer;

	[SerializeField]
	private ParticleSystem _wieldParticle;

	[SerializeField]
	private ParticleSystem _slagWieldParticle;

	private ResourceType _firstResourceType;

	private bool _firstIsPolished;

	private GameObject _firstPipePrefab;

	private void OnEnable()
	{
		RefreshJammedEffects();
		_firstPlateAnimationClip = Animator.runtimeAnimatorController.animationClips.FirstOrDefault((AnimationClip c) => c.name == "PipeRoller_Process1");
		_secondPlateAnimationClip = Animator.runtimeAnimatorController.animationClips.FirstOrDefault((AnimationClip c) => c.name == "PipeRoller_Process2");
		_plate1Renderer.enabled = false;
		_plate1PolishedRenderer.enabled = false;
		_plate2Renderer.enabled = false;
		_plate2PolishedRenderer.enabled = false;
		Singleton<DebugManager>.Instance.ClearedAllPhysicsOrePieces += OnClearedAllPhysicsOrePieces;
	}

	private void OnDisable()
	{
		Singleton<DebugManager>.Instance.ClearedAllPhysicsOrePieces -= OnClearedAllPhysicsOrePieces;
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
				Vector3 vector = Vector3.up * 2f + -base.transform.forward + Random.insideUnitSphere * 0.2f;
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

	private void OnClearedAllPhysicsOrePieces()
	{
		_jammedOre.Clear();
		_waitingList.Clear();
	}

	private void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = _lightMeshRenderer.sharedMaterials;
		sharedMaterials[1] = material;
		_lightMeshRenderer.sharedMaterials = sharedMaterials;
	}

	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (!(component != null))
		{
			return;
		}
		if (component.PieceType == PieceType.Plate && component.PipePrefab != null)
		{
			if (!_isProcessing)
			{
				_isProcessing = true;
				if (_firstResourceType == ResourceType.INVALID)
				{
					StartCoroutine(ProcessFirstPlate(component));
				}
				else
				{
					StartCoroutine(CreatePipe(component));
				}
			}
			else
			{
				_waitingList.Add(component);
			}
		}
		else if (component.PipePrefab != null)
		{
			component.ConvertToPipe();
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

	private IEnumerator ProcessFirstPlate(OrePiece orePiece1)
	{
		_plate2Renderer.enabled = false;
		_plate2PolishedRenderer.enabled = false;
		if (orePiece1 != null)
		{
			_firstResourceType = orePiece1.ResourceType;
			_firstIsPolished = orePiece1.PolishedPercent >= 1f;
			_firstPipePrefab = orePiece1.PipePrefab;
			_plate1Renderer.enabled = !_firstIsPolished;
			_plate1PolishedRenderer.enabled = _firstIsPolished;
			if (_firstIsPolished)
			{
				_plate1PolishedRenderer.sharedMaterial = orePiece1.GetComponent<Renderer>()?.sharedMaterial;
			}
			else
			{
				_plate1Renderer.sharedMaterial = orePiece1.GetComponent<Renderer>()?.sharedMaterial;
			}
			orePiece1.Delete();
			ProcessSoundPlayer.PlaySound(Process1Sound);
			_isProcessing = true;
			if (Animator != null)
			{
				Animator.Play("PipeRoller_Process1", -1, 0f);
			}
			yield return new WaitForSeconds(_firstPlateAnimationClip.length * 0.95f);
		}
		_isProcessing = false;
		if (_waitingList.Count > 0)
		{
			OrePiece orePiece2 = _waitingList[0];
			_waitingList.RemoveAt(0);
			StartCoroutine(CreatePipe(orePiece2));
		}
	}

	private IEnumerator CreatePipe(OrePiece orePiece2)
	{
		if (orePiece2 != null)
		{
			bool flag = orePiece2.PolishedPercent >= 1f;
			GameObject selectedPrefab = SlagPipePrefab;
			if (_firstResourceType == orePiece2.ResourceType)
			{
				selectedPrefab = ((_firstIsPolished == flag) ? orePiece2.PipePrefab : (_firstIsPolished ? orePiece2.PipePrefab : _firstPipePrefab));
			}
			_plate2Renderer.enabled = !flag;
			_plate2PolishedRenderer.enabled = flag;
			if (flag)
			{
				_plate2PolishedRenderer.sharedMaterial = orePiece2.GetComponent<Renderer>()?.sharedMaterial;
			}
			else
			{
				_plate2Renderer.sharedMaterial = orePiece2.GetComponent<Renderer>()?.sharedMaterial;
			}
			orePiece2.Delete();
			ProcessSoundPlayer.PlaySound(CreatePipeSound);
			_isProcessing = true;
			if (Animator != null)
			{
				Animator.Play("PipeRoller_Process2", -1, 0f);
			}
			yield return new WaitForSeconds(_secondPlateAnimationClip.length * 0.52f);
			_plate1Renderer.enabled = false;
			_plate1PolishedRenderer.enabled = false;
			_plate2Renderer.enabled = false;
			_plate2PolishedRenderer.enabled = false;
			OrePiece pipe = Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(selectedPrefab, PipeTransform.position, Quaternion.identity);
			if (selectedPrefab == SlagPipePrefab)
			{
				_slagWieldParticle.Play();
			}
			else
			{
				_wieldParticle.Play();
			}
			Rigidbody pipeRB = pipe.GetComponent<Rigidbody>();
			Collider pipeCol = pipe.GetComponent<Collider>();
			if (pipe != null)
			{
				if (pipeRB != null)
				{
					pipeRB.isKinematic = true;
				}
				if (pipeCol != null)
				{
					pipeCol.enabled = false;
				}
				pipe.tag = "Untagged";
				pipe.transform.parent = PipeTransform;
				pipe.transform.localPosition = Vector3.zero;
				pipe.transform.localRotation = Quaternion.identity;
			}
			yield return new WaitForSeconds(_secondPlateAnimationClip.length * 0.2f);
			if (pipe != null)
			{
				if (pipeRB != null)
				{
					pipeRB.isKinematic = false;
				}
				if (pipeCol != null)
				{
					pipeCol.enabled = true;
				}
				pipe.transform.parent = null;
				pipe.transform.position = OutputTransform.position;
				pipe.transform.rotation = OutputTransform.rotation;
				pipe.tag = "Grabbable";
			}
			yield return new WaitForSeconds(_secondPlateAnimationClip.length * 0.24f);
		}
		_firstResourceType = ResourceType.INVALID;
		_isProcessing = false;
		if (_waitingList.Count > 0)
		{
			OrePiece orePiece3 = _waitingList[0];
			_waitingList.RemoveAt(0);
			StartCoroutine(ProcessFirstPlate(orePiece3));
		}
	}
}
