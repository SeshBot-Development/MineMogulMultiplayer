using UnityEngine;

[CreateAssetMenu(fileName = "SoundDefinition", menuName = "Audio/SoundDefinition")]
public class SoundDefinition : ScriptableObject
{
	public AudioClipDescription[] sounds;

	[Range(0.5f, 2f)]
	public float minPitch = 0.9f;

	[Range(0.5f, 2f)]
	public float maxPitch = 1.1f;

	[Range(0f, 100f)]
	public float maxRange = 20f;

	[Range(0f, 256f)]
	public int Priority = 180;

	public AudioClipDescription GetSound()
	{
		if (sounds.Length == 0)
		{
			Debug.LogError("SoundDefinition contains no sounds.");
			return default(AudioClipDescription);
		}
		AudioClipDescription result = sounds[Random.Range(0, sounds.Length)];
		result.pitch = Random.Range(minPitch, maxPitch);
		result.maxRange = maxRange;
		result.priority = Priority;
		return result;
	}

	private void OnValidate()
	{
		for (int i = 0; i < sounds.Length; i++)
		{
			if (sounds[i].volume == 0f)
			{
				sounds[i].volume = 1f;
			}
			if (sounds[i].pitch == 0f)
			{
				sounds[i].pitch = 1f;
			}
		}
	}
}
