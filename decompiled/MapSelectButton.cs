using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapSelectButton : MonoBehaviour
{
	[HideInInspector]
	public LevelInfo LevelInfo;

	public bool IsSelected;

	[SerializeField]
	private TMP_Text _mapNameText;

	[SerializeField]
	private TMP_Text _mapDescriptionText;

	[SerializeField]
	private RawImage _icon;

	[SerializeField]
	private Color _selectedColor;

	[SerializeField]
	private Color _notSelectedColor;

	[SerializeField]
	private Color _lockedColor;

	[SerializeField]
	private List<Graphic> _graphicsToChangeColor;

	[SerializeField]
	private GameObject _selectedGraphic;

	[SerializeField]
	private GameObject _lockedInDemoText;

	[SerializeField]
	private Material _lockedInDemoImageMaterial;

	private NewGameMenu _owner;

	public void Initialize(LevelInfo levelInfo, NewGameMenu owner)
	{
		_owner = owner;
		LevelInfo = levelInfo;
		_mapNameText.text = LevelInfo.DisplayName;
		_mapDescriptionText.text = LevelInfo.Description;
		_icon.texture = LevelInfo.Thumbnail;
		base.name = "Map Select Button - " + LevelInfo.DisplayName;
	}

	public void OnMapSelected()
	{
		_owner.OnMapSelected(this);
	}

	public void UpdateSelected(bool isSelected)
	{
		if (LevelInfo.IsLockedInDemo && Singleton<DemoManager>.Instance.IsDemoVersion)
		{
			GetComponent<Button>().interactable = false;
			IsSelected = false;
			_selectedGraphic.SetActive(value: false);
			_lockedInDemoText.SetActive(value: true);
			SetColors(_lockedColor);
			_icon.material = _lockedInDemoImageMaterial;
			_mapNameText.color = _lockedColor;
			_mapDescriptionText.color = _lockedColor;
		}
		else
		{
			_lockedInDemoText.SetActive(value: false);
			IsSelected = isSelected;
			if (IsSelected)
			{
				_selectedGraphic.SetActive(value: true);
				SetColors(_selectedColor);
			}
			else
			{
				_selectedGraphic.SetActive(value: false);
				SetColors(_notSelectedColor);
			}
		}
	}

	private void SetColors(Color color)
	{
		foreach (Graphic item in _graphicsToChangeColor)
		{
			item.color = color;
		}
	}
}
