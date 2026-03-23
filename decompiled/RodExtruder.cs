using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RodExtruder : MonoBehaviour
{
	public Transform OutputTransform;

	public float ProcessingTime = 1.5f;

	public SoundDefinition ExtrudeRodSound;

	public SoundPlayer ProcessRodSoundPlayer;

	public Transform RodStartTransform;

	public Renderer RodRenderer1;

	public Renderer RodRenderer2;

	private List<OrePiece> _waitingList = new List<OrePiece>();

	private bool _isProcessing1;

	private bool _isProcessing2;

	[SerializeField]
	private Renderer _lightRenderer;

	private MeshFilter _rodMeshFilter1;

	private MeshFilter _rodMeshFilter2;

	private TimeSince _timeSinceProcessed;

	private void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = _lightRenderer.sharedMaterials;
		sharedMaterials[3] = material;
		_lightRenderer.sharedMaterials = sharedMaterials;
	}

	private void Awake()
	{
		_rodMeshFilter1 = RodRenderer1.GetComponent<MeshFilter>();
		_rodMeshFilter2 = RodRenderer2.GetComponent<MeshFilter>();
	}

	private void OnEnable()
	{
		RodRenderer1.enabled = false;
		RodRenderer2.enabled = false;
		RodRenderer1.transform.localPosition = RodStartTransform.localPosition;
		RodRenderer2.transform.localPosition = RodStartTransform.localPosition;
	}

	private bool IsAvailable()
	{
		if (_isProcessing1)
		{
			return !_isProcessing2;
		}
		return true;
	}

	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null && component.RodPrefab != null)
		{
			if (IsAvailable())
			{
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
		OrePiece rod = orePiece.ConvertToRod();
		bool usingRenderer1 = !_isProcessing1;
		if (rod != null)
		{
			rod.gameObject.SetActive(value: false);
			if (usingRenderer1)
			{
				_isProcessing1 = true;
			}
			else
			{
				_isProcessing2 = true;
			}
			float minTimeBetweenRods = ProcessingTime * 0.75f;
			while (_timeSinceProcessed < minTimeBetweenRods)
			{
				yield return null;
			}
			_timeSinceProcessed = 0f;
			Renderer selectedRenderer = (usingRenderer1 ? RodRenderer1 : RodRenderer2);
			MeshFilter obj = (usingRenderer1 ? _rodMeshFilter1 : _rodMeshFilter2);
			selectedRenderer.sharedMaterial = rod.GetComponent<Renderer>().sharedMaterial;
			obj.sharedMesh = rod.GetComponent<MeshFilter>().sharedMesh;
			selectedRenderer.transform.position = RodStartTransform.position;
			selectedRenderer.enabled = true;
			float elapsed = 0f;
			Vector3 startPos = RodStartTransform.position;
			Vector3 endPos = OutputTransform.position;
			ProcessRodSoundPlayer.PlaySound(ExtrudeRodSound);
			while (elapsed < ProcessingTime)
			{
				elapsed += Time.deltaTime;
				float t = elapsed / ProcessingTime;
				selectedRenderer.transform.position = Vector3.Lerp(startPos, endPos, t);
				yield return null;
			}
			selectedRenderer.enabled = false;
			if (rod != null)
			{
				rod.transform.position = OutputTransform.position;
				rod.transform.rotation = OutputTransform.rotation;
				rod.gameObject.SetActive(value: true);
			}
		}
		if (usingRenderer1)
		{
			_isProcessing1 = false;
		}
		else
		{
			_isProcessing2 = false;
		}
		if (_waitingList.Count > 0)
		{
			OrePiece orePiece2 = _waitingList[0];
			_waitingList.RemoveAt(0);
			StartCoroutine(ExtrudeRod(orePiece2));
		}
	}
}
