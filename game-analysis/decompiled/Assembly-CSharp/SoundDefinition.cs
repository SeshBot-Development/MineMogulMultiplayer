using System;
using UnityEngine;

// Token: 0x020000E2 RID: 226
[CreateAssetMenu(fileName = "SoundDefinition", menuName = "Audio/SoundDefinition")]
public class SoundDefinition : ScriptableObject
{
	// Token: 0x0600060A RID: 1546 RVA: 0x0001F8D4 File Offset: 0x0001DAD4
	public AudioClipDescription GetSound()
	{
		if (this.sounds.Length == 0)
		{
			Debug.LogError("SoundDefinition contains no sounds.");
			return default(AudioClipDescription);
		}
		AudioClipDescription audioClipDescription = this.sounds[Random.Range(0, this.sounds.Length)];
		audioClipDescription.pitch = Random.Range(this.minPitch, this.maxPitch);
		audioClipDescription.maxRange = this.maxRange;
		audioClipDescription.priority = this.Priority;
		return audioClipDescription;
	}

	// Token: 0x0600060B RID: 1547 RVA: 0x0001F94C File Offset: 0x0001DB4C
	private void OnValidate()
	{
		for (int i = 0; i < this.sounds.Length; i++)
		{
			if (this.sounds[i].volume == 0f)
			{
				this.sounds[i].volume = 1f;
			}
			if (this.sounds[i].pitch == 0f)
			{
				this.sounds[i].pitch = 1f;
			}
		}
	}

	// Token: 0x04000746 RID: 1862
	public AudioClipDescription[] sounds;

	// Token: 0x04000747 RID: 1863
	[Range(0.5f, 2f)]
	public float minPitch = 0.9f;

	// Token: 0x04000748 RID: 1864
	[Range(0.5f, 2f)]
	public float maxPitch = 1.1f;

	// Token: 0x04000749 RID: 1865
	[Range(0f, 100f)]
	public float maxRange = 20f;

	// Token: 0x0400074A RID: 1866
	[Range(0f, 256f)]
	public int Priority = 180;
}
