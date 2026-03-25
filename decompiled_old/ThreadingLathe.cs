using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadingLathe : MonoBehaviour
{
	public Transform OutputTransform;

	public SoundDefinition ThreadRodSound;

	public SoundPlayer ProcessRodSoundPlayer;

	public Renderer RodRenderer;

	public Renderer DummyWholePartRenderer;

	private List<OrePiece> _waitingList = new List<OrePiece>();

	private bool _isProcessing;

	[SerializeField]
	private Renderer _lightRenderer;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Transform _rodRegularParent;

	[SerializeField]
	private Transform _rodSpinningParent;

	private MeshFilter _rodMeshFilter;

	private MeshFilter _dummyWholePartMeshFilter;

	private void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = _lightRenderer.sharedMaterials;
		sharedMaterials[3] = material;
		_lightRenderer.sharedMaterials = sharedMaterials;
	}

	private void Awake()
	{
		_rodMeshFilter = RodRenderer.GetComponent<MeshFilter>();
		_dummyWholePartMeshFilter = DummyWholePartRenderer.GetComponent<MeshFilter>();
		RodRenderer.enabled = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null && component.RodPrefab != null)
		{
			if (!_isProcessing)
			{
				_isProcessing = true;
				StartCoroutine(ExtrudeRod(component));
			}
			else
			{
				_waitingList.Add(component);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			_waitingList.Remove(component);
		}
	}

	private IEnumerator ExtrudeRod(OrePiece orePiece)
	{
		Material sharedMaterial = orePiece.GetComponent<Renderer>().sharedMaterial;
		Mesh sharedMesh = orePiece.GetComponent<MeshFilter>().sharedMesh;
		OrePiece rod = orePiece.ConvertToThreaded();
		if (rod != null)
		{
			rod.gameObject.SetActive(value: false);
			ProcessRodSoundPlayer.PlaySound(ThreadRodSound);
			_isProcessing = true;
			RodRenderer.sharedMaterial = sharedMaterial;
			_rodMeshFilter.sharedMesh = sharedMesh;
			RodRenderer.enabled = true;
			DummyWholePartRenderer.sharedMaterial = sharedMaterial;
			_dummyWholePartMeshFilter.sharedMesh = sharedMesh;
			_animator.Play("ThreadingLathe_Process");
			yield return new WaitForSeconds(1.5f);
			RodRenderer.transform.SetParent(_rodSpinningParent);
			RodRenderer.transform.localPosition = Vector3.zero;
			yield return new WaitForSeconds(0.75f);
			RodRenderer.sharedMaterial = rod.GetComponent<Renderer>().sharedMaterial;
			_rodMeshFilter.sharedMesh = rod.GetComponent<MeshFilter>().sharedMesh;
			yield return new WaitForSeconds(3.25f);
			RodRenderer.transform.SetParent(_rodRegularParent);
			RodRenderer.transform.localPosition = Vector3.zero;
			RodRenderer.transform.localRotation = Quaternion.identity;
			yield return new WaitForSeconds(2.5f);
			RodRenderer.enabled = false;
			if (rod != null)
			{
				rod.transform.position = OutputTransform.position;
				rod.transform.rotation = OutputTransform.rotation;
				rod.gameObject.SetActive(value: true);
			}
			yield return new WaitForSeconds(1.1f);
		}
		_isProcessing = false;
		if (_waitingList.Count > 0)
		{
			OrePiece orePiece2 = _waitingList[0];
			_waitingList.RemoveAt(0);
			StartCoroutine(ExtrudeRod(orePiece2));
		}
	}
}
