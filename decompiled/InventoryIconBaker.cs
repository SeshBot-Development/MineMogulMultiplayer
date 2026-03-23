using UnityEngine;

[ExecuteAlways]
public class InventoryIconBaker : MonoBehaviour
{
	[Header("Scene References")]
	[SerializeField]
	private InventoryItemPreview _preview;

	[Header("Output")]
	[SerializeField]
	private int _iconSize = 256;

	[SerializeField]
	private string _outputFolder = "Assets/UI/InventoryIconsNew";

	[SerializeField]
	private float _pixelsPerUnit = 100f;
}
