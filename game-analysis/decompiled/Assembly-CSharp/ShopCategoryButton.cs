using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000D9 RID: 217
public class ShopCategoryButton : MonoBehaviour
{
	// Token: 0x1400000F RID: 15
	// (add) Token: 0x060005D5 RID: 1493 RVA: 0x0001EB34 File Offset: 0x0001CD34
	// (remove) Token: 0x060005D6 RID: 1494 RVA: 0x0001EB6C File Offset: 0x0001CD6C
	public event Action<ShopCategory> OnPressed;

	// Token: 0x060005D7 RID: 1495 RVA: 0x0001EBA1 File Offset: 0x0001CDA1
	public void Initialize(ShopCategory shopCategory)
	{
		this.ShopCategory = shopCategory;
		this.NameText.text = this.ShopCategory.CategoryName;
	}

	// Token: 0x060005D8 RID: 1496 RVA: 0x0001EBC0 File Offset: 0x0001CDC0
	public void OnEnable()
	{
		this.RefreshUI();
		Singleton<QuestManager>.Instance.QuestCompleted += this.OnQuestCompleted;
	}

	// Token: 0x060005D9 RID: 1497 RVA: 0x0001EBDE File Offset: 0x0001CDDE
	public void OnDisable()
	{
		Tween colorTween = this._colorTween;
		if (colorTween != null)
		{
			colorTween.Kill(false);
		}
		Singleton<QuestManager>.Instance.QuestCompleted -= this.OnQuestCompleted;
	}

	// Token: 0x060005DA RID: 1498 RVA: 0x0001EC08 File Offset: 0x0001CE08
	private void OnQuestCompleted(Quest quest)
	{
		this.RefreshUI();
	}

	// Token: 0x060005DB RID: 1499 RVA: 0x0001EC10 File Offset: 0x0001CE10
	public void RefreshUI()
	{
		if (this.ShopCategory.ContainsNewItems())
		{
			if (this._colorTween == null)
			{
				this._colorTween = this.NameText.DOColor(this.TextNewColor, 3f).SetLoops(-1, LoopType.Yoyo);
			}
			this.NewIcon.SetActive(true);
			return;
		}
		Tween colorTween = this._colorTween;
		if (colorTween != null)
		{
			colorTween.Kill(false);
		}
		this.NameText.DOColor(this.TextRegularColor, 1f);
		this.NewIcon.SetActive(false);
	}

	// Token: 0x060005DC RID: 1500 RVA: 0x0001EC97 File Offset: 0x0001CE97
	public void OnButtonPressed()
	{
		Action<ShopCategory> onPressed = this.OnPressed;
		if (onPressed == null)
		{
			return;
		}
		onPressed(this.ShopCategory);
	}

	// Token: 0x060005DD RID: 1501 RVA: 0x0001ECAF File Offset: 0x0001CEAF
	public void SetSelected(bool selected)
	{
		if (selected)
		{
			this.BackgroundImage.color = this.SelectedColor;
			return;
		}
		this.BackgroundImage.color = this.NotSelectedColor;
	}

	// Token: 0x04000708 RID: 1800
	public Image BackgroundImage;

	// Token: 0x04000709 RID: 1801
	public Color SelectedColor;

	// Token: 0x0400070A RID: 1802
	public Color NotSelectedColor;

	// Token: 0x0400070B RID: 1803
	public Color TextRegularColor;

	// Token: 0x0400070C RID: 1804
	public Color TextNewColor;

	// Token: 0x0400070D RID: 1805
	public TMP_Text NameText;

	// Token: 0x0400070E RID: 1806
	public GameObject NewIcon;

	// Token: 0x0400070F RID: 1807
	public ShopCategory ShopCategory;

	// Token: 0x04000711 RID: 1809
	private Tween _colorTween;
}
