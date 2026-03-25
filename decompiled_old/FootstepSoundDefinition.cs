using UnityEngine;

[CreateAssetMenu(fileName = "FootstepSoundDefinition", menuName = "Audio/FootstepSoundDefinition")]
public class FootstepSoundDefinition : ScriptableObject
{
	public SoundDefinition LeftFootstepDefinition;

	public SoundDefinition RightFootstepDefinition;
}
