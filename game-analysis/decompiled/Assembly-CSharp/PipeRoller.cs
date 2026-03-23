using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x02000084 RID: 132
public class PipeRoller : MonoBehaviour
{
	// Token: 0x06000389 RID: 905 RVA: 0x000117F4 File Offset: 0x0000F9F4
	private void OnEnable()
	{
		this.RefreshJammedEffects();
		this._firstPlateAnimationClip = this.Animator.runtimeAnimatorController.animationClips.FirstOrDefault((AnimationClip c) => c.name == "PipeRoller_Process1");
		this._secondPlateAnimationClip = this.Animator.runtimeAnimatorController.animationClips.FirstOrDefault((AnimationClip c) => c.name == "PipeRoller_Process2");
		this._plate1Renderer.enabled = false;
		this._plate1PolishedRenderer.enabled = false;
		this._plate2Renderer.enabled = false;
		this._plate2PolishedRenderer.enabled = false;
		Singleton<DebugManager>.Instance.ClearedAllPhysicsOrePieces += this.OnClearedAllPhysicsOrePieces;
	}

	// Token: 0x0600038A RID: 906 RVA: 0x000118C1 File Offset: 0x0000FAC1
	private void OnDisable()
	{
		Singleton<DebugManager>.Instance.ClearedAllPhysicsOrePieces -= this.OnClearedAllPhysicsOrePieces;
	}

	// Token: 0x0600038B RID: 907 RVA: 0x000118DC File Offset: 0x0000FADC
	private void Update()
	{
		bool isJammed = this._isJammed;
		this._isJammed = this._jammedOre.Count > 0;
		if (this._isJammed != isJammed)
		{
			this.RefreshJammedEffects();
			this._lastLaunchTime = Time.time;
		}
		if (this._isJammed && Time.time - this._lastLaunchTime > this.LaunchJammedObjectCooldown)
		{
			this._lastLaunchTime = Time.time;
			if (Random.value < 0.5f)
			{
				base.StartCoroutine(this.WaitThenLaunchRandomJammedOre());
			}
		}
	}

	// Token: 0x0600038C RID: 908 RVA: 0x0001195E File Offset: 0x0000FB5E
	private IEnumerator WaitThenLaunchRandomJammedOre()
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.LaunchJammedObjectSound, base.transform.position, 1f, 1f, true, false);
		yield return new WaitForSeconds(0.3f);
		this.LaunchRandomJammedOre();
		yield break;
	}

	// Token: 0x0600038D RID: 909 RVA: 0x00011970 File Offset: 0x0000FB70
	private void LaunchRandomJammedOre()
	{
		if (this == null)
		{
			return;
		}
		if (this._jammedOre.Count == 0)
		{
			return;
		}
		int num = Random.Range(0, this._jammedOre.Count);
		OrePiece orePiece = this._jammedOre.ElementAt(num);
		if (orePiece == null)
		{
			return;
		}
		Rigidbody component = orePiece.GetComponent<Rigidbody>();
		if (component != null)
		{
			Vector3 vector = Vector3.up * 2f + -base.transform.forward + Random.insideUnitSphere * 0.2f;
			float num2 = 2f;
			float num3 = 6f;
			component.AddForce(vector * num2, ForceMode.Impulse);
			component.AddTorque(vector * num3, ForceMode.Impulse);
		}
	}

	// Token: 0x0600038E RID: 910 RVA: 0x00011A34 File Offset: 0x0000FC34
	private void RefreshJammedEffects()
	{
		foreach (ParticleSystem particleSystem in this.JammedParticles)
		{
			particleSystem.emission.enabled = this._isJammed;
			this.ChangeLightMaterial(this._isJammed ? Singleton<BuildingManager>.Instance.OrangeLightMaterial : Singleton<BuildingManager>.Instance.GreenLightMaterial);
			this._jammedLoopingAudioSource.Toggle(this._isJammed);
		}
	}

	// Token: 0x0600038F RID: 911 RVA: 0x00011AC8 File Offset: 0x0000FCC8
	private void OnClearedAllPhysicsOrePieces()
	{
		this._jammedOre.Clear();
		this._waitingList.Clear();
	}

	// Token: 0x06000390 RID: 912 RVA: 0x00011AE0 File Offset: 0x0000FCE0
	private void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = this._lightMeshRenderer.sharedMaterials;
		sharedMaterials[1] = material;
		this._lightMeshRenderer.sharedMaterials = sharedMaterials;
	}

	// Token: 0x06000391 RID: 913 RVA: 0x00011B0C File Offset: 0x0000FD0C
	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			if (component.PieceType == PieceType.Plate && component.PipePrefab != null)
			{
				if (this._isProcessing)
				{
					this._waitingList.Add(component);
					return;
				}
				this._isProcessing = true;
				if (this._firstResourceType == ResourceType.INVALID)
				{
					base.StartCoroutine(this.ProcessFirstPlate(component));
					return;
				}
				base.StartCoroutine(this.CreatePipe(component));
				return;
			}
			else
			{
				if (component.PipePrefab != null)
				{
					component.ConvertToPipe();
					return;
				}
				this._jammedOre.Add(component);
			}
		}
	}

	// Token: 0x06000392 RID: 914 RVA: 0x00011BA8 File Offset: 0x0000FDA8
	private void OnTriggerExit(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			this._waitingList.Remove(component);
			this._jammedOre.Remove(component);
		}
	}

	// Token: 0x06000393 RID: 915 RVA: 0x00011BDF File Offset: 0x0000FDDF
	private IEnumerator ProcessFirstPlate(OrePiece orePiece1)
	{
		this._plate2Renderer.enabled = false;
		this._plate2PolishedRenderer.enabled = false;
		if (orePiece1 != null)
		{
			this._firstResourceType = orePiece1.ResourceType;
			this._firstIsPolished = orePiece1.PolishedPercent >= 1f;
			this._firstPipePrefab = orePiece1.PipePrefab;
			this._plate1Renderer.enabled = !this._firstIsPolished;
			this._plate1PolishedRenderer.enabled = this._firstIsPolished;
			if (this._firstIsPolished)
			{
				Renderer plate1PolishedRenderer = this._plate1PolishedRenderer;
				Renderer component = orePiece1.GetComponent<Renderer>();
				plate1PolishedRenderer.sharedMaterial = ((component != null) ? component.sharedMaterial : null);
			}
			else
			{
				Renderer plate1Renderer = this._plate1Renderer;
				Renderer component2 = orePiece1.GetComponent<Renderer>();
				plate1Renderer.sharedMaterial = ((component2 != null) ? component2.sharedMaterial : null);
			}
			orePiece1.Delete();
			this.ProcessSoundPlayer.PlaySound(this.Process1Sound);
			this._isProcessing = true;
			if (this.Animator != null)
			{
				this.Animator.Play("PipeRoller_Process1", -1, 0f);
			}
			yield return new WaitForSeconds(this._firstPlateAnimationClip.length * 0.95f);
		}
		this._isProcessing = false;
		if (this._waitingList.Count > 0)
		{
			OrePiece orePiece2 = this._waitingList[0];
			this._waitingList.RemoveAt(0);
			base.StartCoroutine(this.CreatePipe(orePiece2));
		}
		yield break;
	}

	// Token: 0x06000394 RID: 916 RVA: 0x00011BF5 File Offset: 0x0000FDF5
	private IEnumerator CreatePipe(OrePiece orePiece2)
	{
		if (orePiece2 != null)
		{
			bool flag = orePiece2.PolishedPercent >= 1f;
			GameObject selectedPrefab = this.SlagPipePrefab;
			if (this._firstResourceType == orePiece2.ResourceType)
			{
				if (this._firstIsPolished == flag)
				{
					selectedPrefab = orePiece2.PipePrefab;
				}
				else if (!this._firstIsPolished)
				{
					selectedPrefab = this._firstPipePrefab;
				}
				else
				{
					selectedPrefab = orePiece2.PipePrefab;
				}
			}
			this._plate2Renderer.enabled = !flag;
			this._plate2PolishedRenderer.enabled = flag;
			if (flag)
			{
				Renderer plate2PolishedRenderer = this._plate2PolishedRenderer;
				Renderer component = orePiece2.GetComponent<Renderer>();
				plate2PolishedRenderer.sharedMaterial = ((component != null) ? component.sharedMaterial : null);
			}
			else
			{
				Renderer plate2Renderer = this._plate2Renderer;
				Renderer component2 = orePiece2.GetComponent<Renderer>();
				plate2Renderer.sharedMaterial = ((component2 != null) ? component2.sharedMaterial : null);
			}
			orePiece2.Delete();
			this.ProcessSoundPlayer.PlaySound(this.CreatePipeSound);
			this._isProcessing = true;
			if (this.Animator != null)
			{
				this.Animator.Play("PipeRoller_Process2", -1, 0f);
			}
			yield return new WaitForSeconds(this._secondPlateAnimationClip.length * 0.52f);
			this._plate1Renderer.enabled = false;
			this._plate1PolishedRenderer.enabled = false;
			this._plate2Renderer.enabled = false;
			this._plate2PolishedRenderer.enabled = false;
			OrePiece pipe = Singleton<OrePiecePoolManager>.Instance.TrySpawnPooledOre(selectedPrefab, this.PipeTransform.position, Quaternion.identity, null);
			if (selectedPrefab == this.SlagPipePrefab)
			{
				this._slagWieldParticle.Play();
			}
			else
			{
				this._wieldParticle.Play();
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
				pipe.transform.parent = this.PipeTransform;
				pipe.transform.localPosition = Vector3.zero;
				pipe.transform.localRotation = Quaternion.identity;
			}
			yield return new WaitForSeconds(this._secondPlateAnimationClip.length * 0.2f);
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
				pipe.transform.position = this.OutputTransform.position;
				pipe.transform.rotation = this.OutputTransform.rotation;
				pipe.tag = "Grabbable";
			}
			yield return new WaitForSeconds(this._secondPlateAnimationClip.length * 0.24f);
			selectedPrefab = null;
			pipe = null;
			pipeRB = null;
			pipeCol = null;
		}
		this._firstResourceType = ResourceType.INVALID;
		this._isProcessing = false;
		if (this._waitingList.Count > 0)
		{
			OrePiece orePiece3 = this._waitingList[0];
			this._waitingList.RemoveAt(0);
			base.StartCoroutine(this.ProcessFirstPlate(orePiece3));
		}
		yield break;
	}

	// Token: 0x04000377 RID: 887
	public Animator Animator;

	// Token: 0x04000378 RID: 888
	public Transform Plate1Transform;

	// Token: 0x04000379 RID: 889
	public Transform Plate2Transform;

	// Token: 0x0400037A RID: 890
	public Transform PipeTransform;

	// Token: 0x0400037B RID: 891
	public Transform OutputTransform;

	// Token: 0x0400037C RID: 892
	public List<ParticleSystem> JammedParticles;

	// Token: 0x0400037D RID: 893
	public float LaunchJammedObjectCooldown = 1f;

	// Token: 0x0400037E RID: 894
	public SoundDefinition LaunchJammedObjectSound;

	// Token: 0x0400037F RID: 895
	public SoundDefinition Process1Sound;

	// Token: 0x04000380 RID: 896
	public SoundDefinition CreatePipeSound;

	// Token: 0x04000381 RID: 897
	public SoundPlayer ProcessSoundPlayer;

	// Token: 0x04000382 RID: 898
	public GameObject SlagPipePrefab;

	// Token: 0x04000383 RID: 899
	private float _lastLaunchTime;

	// Token: 0x04000384 RID: 900
	private bool _isJammed;

	// Token: 0x04000385 RID: 901
	private List<OrePiece> _waitingList = new List<OrePiece>();

	// Token: 0x04000386 RID: 902
	private HashSet<OrePiece> _jammedOre = new HashSet<OrePiece>();

	// Token: 0x04000387 RID: 903
	private bool _isProcessing;

	// Token: 0x04000388 RID: 904
	private AnimationClip _firstPlateAnimationClip;

	// Token: 0x04000389 RID: 905
	private AnimationClip _secondPlateAnimationClip;

	// Token: 0x0400038A RID: 906
	[SerializeField]
	private SkinnedMeshRenderer _lightMeshRenderer;

	// Token: 0x0400038B RID: 907
	[SerializeField]
	private LoopingSoundPlayer _jammedLoopingAudioSource;

	// Token: 0x0400038C RID: 908
	[SerializeField]
	private SkinnedMeshRenderer _plate1Renderer;

	// Token: 0x0400038D RID: 909
	[SerializeField]
	private SkinnedMeshRenderer _plate1PolishedRenderer;

	// Token: 0x0400038E RID: 910
	[SerializeField]
	private SkinnedMeshRenderer _plate2Renderer;

	// Token: 0x0400038F RID: 911
	[SerializeField]
	private SkinnedMeshRenderer _plate2PolishedRenderer;

	// Token: 0x04000390 RID: 912
	[SerializeField]
	private ParticleSystem _wieldParticle;

	// Token: 0x04000391 RID: 913
	[SerializeField]
	private ParticleSystem _slagWieldParticle;

	// Token: 0x04000392 RID: 914
	private ResourceType _firstResourceType;

	// Token: 0x04000393 RID: 915
	private bool _firstIsPolished;

	// Token: 0x04000394 RID: 916
	private GameObject _firstPipePrefab;
}
