using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200006B RID: 107
public class MapSelectButton : MonoBehaviour
{
	// Token: 0x060002D1 RID: 721 RVA: 0x0000DB34 File Offset: 0x0000BD34
	public void Initialize(LevelInfo levelInfo, NewGameMenu owner)
	{
		this._owner = owner;
		this.LevelInfo = levelInfo;
		this._mapNameText.text = this.LevelInfo.DisplayName;
		this._mapDescriptionText.text = this.LevelInfo.Description;
		this._icon.texture = this.LevelInfo.Thumbnail;
		base.name = "Map Select Button - " + this.LevelInfo.DisplayName;
	}

	// Token: 0x060002D2 RID: 722 RVA: 0x0000DBAC File Offset: 0x0000BDAC
	public void OnMapSelected()
	{
		this._owner.OnMapSelected(this);
	}

	// Token: 0x060002D3 RID: 723 RVA: 0x0000DBBC File Offset: 0x0000BDBC
	public void UpdateSelected(bool isSelected)
	{
		this.IsSelected = isSelected;
		if (this.IsSelected)
		{
			this._selectedGraphic.SetActive(true);
			this.SetColors(this._selectedColor);
			return;
		}
		this._selectedGraphic.SetActive(false);
		this.SetColors(this._notSelectedColor);
	}

	// Token: 0x060002D4 RID: 724 RVA: 0x0000DC0C File Offset: 0x0000BE0C
	private void SetColors(Color color)
	{
		foreach (Graphic graphic in this._graphicsToChangeColor)
		{
			graphic.color = color;
		}
	}

	// Token: 0x040002AA RID: 682
	[HideInInspector]
	public LevelInfo LevelInfo;

	// Token: 0x040002AB RID: 683
	public bool IsSelected;

	// Token: 0x040002AC RID: 684
	[SerializeField]
	private TMP_Text _mapNameText;

	// Token: 0x040002AD RID: 685
	[SerializeField]
	private TMP_Text _mapDescriptionText;

	// Token: 0x040002AE RID: 686
	[SerializeField]
	private RawImage _icon;

	// Token: 0x040002AF RID: 687
	[SerializeField]
	private Color _selectedColor;

	// Token: 0x040002B0 RID: 688
	[SerializeField]
	private Color _notSelectedColor;

	// Token: 0x040002B1 RID: 689
	[SerializeField]
	private List<Graphic> _graphicsToChangeColor;

	// Token: 0x040002B2 RID: 690
	[SerializeField]
	private GameObject _selectedGraphic;

	// Token: 0x040002B3 RID: 691
	private NewGameMenu _owner;
}
