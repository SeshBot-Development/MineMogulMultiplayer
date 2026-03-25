using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCategoryButton : MonoBehaviour
{
	public Image BackgroundImage;

	public Color SelectedColor;

	public Color NotSelectedColor;

	public Color TextRegularColor;

	public Color TextNewColor;

	public TMP_Text NameText;

	public GameObject NewIcon;

	public ShopCategory ShopCategory;

	private Tween _colorTween;

	public event Action<ShopCategory> OnPressed;

	public void Initialize(ShopCategory shopCategory)
	{
		ShopCategory = shopCategory;
		NameText.text = ShopCategory.CategoryName;
	}

	public void OnEnable()
	{
		RefreshUI();
		Singleton<QuestManager>.Instance.QuestCompleted += OnQuestCompleted;
	}

	public void OnDisable()
	{
		_colorTween?.Kill();
		Singleton<QuestManager>.Instance.QuestCompleted -= OnQuestCompleted;
	}

	private void OnQuestCompleted(Quest quest)
	{
		RefreshUI();
	}

	public void RefreshUI()
	{
		if (ShopCategory.ContainsNewItems())
		{
			if (_colorTween == null)
			{
				_colorTween = NameText.DOColor(TextNewColor, 3f).SetLoops(-1, LoopType.Yoyo);
			}
			NewIcon.SetActive(value: true);
		}
		else
		{
			_colorTween?.Kill();
			NameText.DOColor(TextRegularColor, 1f);
			NewIcon.SetActive(value: false);
		}
	}

	public void OnButtonPressed()
	{
		this.OnPressed?.Invoke(ShopCategory);
	}

	public void SetSelected(bool selected)
	{
		if (selected)
		{
			BackgroundImage.color = SelectedColor;
		}
		else
		{
			BackgroundImage.color = NotSelectedColor;
		}
	}
}
