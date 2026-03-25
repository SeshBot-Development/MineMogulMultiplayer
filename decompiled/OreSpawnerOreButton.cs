using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OreSpawnerOreButton : MonoBehaviour
{
	public OrePiece OrePrefab;

	[SerializeField]
	private TMP_Text _oreNameText;

	[SerializeField]
	private Image _icon;

	private OreSpawnerSelectOreUI _selectUI;

	public void Initialize(OrePiece orePiece, OreSpawnerSelectOreUI selectUI)
	{
		if (orePiece == null)
		{
			Debug.Log("OreSpawnerOreButton: Null OrePiece prefab, destroying button");
			Object.Destroy(base.gameObject);
			return;
		}
		_selectUI = selectUI;
		OrePrefab = orePiece;
		_oreNameText.text = Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(OrePrefab.ResourceType, OrePrefab.PieceType, OrePrefab.IsPolished);
		_icon.sprite = OrePrefab.GetIcon();
	}

	public void OnPressed()
	{
		_selectUI.OnOreSelected(OrePrefab);
	}
}
