using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DefaultExecutionOrder(-1)]
public class LoopingSoundPlayer : MonoBehaviour
{
	[HideInInspector]
	public AudioSource AudioSource;

	public bool ShouldPlay = true;

	private static string _triggerLayerName = "LocalPlayerAudioTrigger";

	private bool _isInRange;

	private void Awake()
	{
		int num = LayerMask.NameToLayer(_triggerLayerName);
		if (num != -1)
		{
			base.gameObject.layer = num;
		}
		AudioSource = GetComponent<AudioSource>();
		AudioSource.loop = true;
		AudioSource.playOnAwake = false;
		SphereCollider sphereCollider = base.gameObject.AddComponent<SphereCollider>();
		sphereCollider.isTrigger = true;
		sphereCollider.radius = AudioSource.maxDistance;
	}

	private void OnEnable()
	{
		if (ShouldPlay)
		{
			StartCoroutine(WaitThenPlay());
		}
	}

	private IEnumerator WaitThenPlay()
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForSeconds(Random.Range(0, 1));
		if (ShouldPlay && _isInRange)
		{
			AudioSource.Play();
		}
	}

	public void Toggle(bool shouldPlay)
	{
		if (shouldPlay)
		{
			Play();
		}
		else
		{
			Pause();
		}
	}

	public void Play()
	{
		ShouldPlay = true;
		if (_isInRange)
		{
			AudioSource.Play();
		}
	}

	public void Pause()
	{
		ShouldPlay = false;
		if (AudioSource.isPlaying)
		{
			AudioSource.Pause();
		}
	}

	public void Stop()
	{
		ShouldPlay = false;
		if (AudioSource.isPlaying)
		{
			AudioSource.Stop();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		_isInRange = true;
		if (ShouldPlay && !AudioSource.isPlaying)
		{
			AudioSource.Play();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		_isInRange = false;
		if (AudioSource.isPlaying)
		{
			AudioSource.Stop();
		}
	}
}
