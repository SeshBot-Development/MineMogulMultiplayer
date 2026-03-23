using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x020000F8 RID: 248
[RequireComponent(typeof(Button))]
public class UIButtonSounds : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler
{
	// Token: 0x06000692 RID: 1682 RVA: 0x0002263B File Offset: 0x0002083B
	private void Awake()
	{
		this._button = base.GetComponent<Button>();
	}

	// Token: 0x06000693 RID: 1683 RVA: 0x00022649 File Offset: 0x00020849
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!this._button.IsInteractable())
		{
			return;
		}
		Singleton<SoundManager>.Instance.PlayUISound(Singleton<SoundManager>.Instance.Sound_UI_Button_Hover, 1f);
	}

	// Token: 0x040007CF RID: 1999
	private Button _button;
}
