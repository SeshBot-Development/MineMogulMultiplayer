using System;
using UnityEngine;

[Serializable]
public struct AudioClipDescription
{
	public AudioClip clip;

	[Range(0f, 2f)]
	public float volume;

	[HideInInspector]
	public float maxRange;

	[Range(0.5f, 2f)]
	public float pitch;

	[HideInInspector]
	public int priority;
}
