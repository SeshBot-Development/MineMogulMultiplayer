using System;
using UnityEngine;
using UnityEngine.EventSystems;

// Token: 0x0200005D RID: 93
public class InventoryItemPreviewHoverDetector : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	// Token: 0x0600024A RID: 586 RVA: 0x0000BB4B File Offset: 0x00009D4B
	public void OnPointerEnter(PointerEventData eventData)
	{
		Singleton<InventoryItemPreview>.Instance.PreviewCameraOrbit.IsHovering = true;
	}

	// Token: 0x0600024B RID: 587 RVA: 0x0000BB5D File Offset: 0x00009D5D
	public void OnPointerExit(PointerEventData eventData)
	{
		Singleton<InventoryItemPreview>.Instance.PreviewCameraOrbit.IsHovering = false;
	}
}
