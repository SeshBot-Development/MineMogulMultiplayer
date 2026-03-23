using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItemPreviewHoverDetector : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public void OnPointerEnter(PointerEventData eventData)
	{
		Singleton<InventoryItemPreview>.Instance.PreviewCameraOrbit.IsHovering = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Singleton<InventoryItemPreview>.Instance.PreviewCameraOrbit.IsHovering = false;
	}
}
