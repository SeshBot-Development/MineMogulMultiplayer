using UnityEngine;

[CreateAssetMenu(fileName = "New Interaction", menuName = "Interactions/Interaction")]
public class Interaction : ScriptableObject
{
	public string Name;

	public string Description;

	public Sprite Icon;
}
