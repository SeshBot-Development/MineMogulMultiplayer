using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSounds : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler
{
	private Button _button;

	private void Awake()
	{
		_button = GetComponent<Button>();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (_button.IsInteractable())
		{
			Singleton<SoundManager>.Instance.PlayUISound(Singleton<SoundManager>.Instance.Sound_UI_Button_Hover);
		}
	}
}
