using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

// Token: 0x02000049 RID: 73
[Serializable]
public class ShopCategory
{
	// Token: 0x060001FE RID: 510 RVA: 0x0000A523 File Offset: 0x00008723
	public bool IsAnyHolidayCategory()
	{
		return this.HolidayType > HolidayType.None;
	}

	// Token: 0x060001FF RID: 511 RVA: 0x0000A52E File Offset: 0x0000872E
	public bool ContainsNewItems()
	{
		return this.ShopItems.Any((ShopItem item) => item.IsNewlyUnlocked());
	}

	// Token: 0x040001DF RID: 479
	public string CategoryName;

	// Token: 0x040001E0 RID: 480
	[FormerlySerializedAs("_shopItemDefinitions")]
	public List<ShopItemDefinition> ShopItemDefinitions;

	// Token: 0x040001E1 RID: 481
	public List<ShopItem> ShopItems;

	// Token: 0x040001E2 RID: 482
	public bool DontShowIfAllItemsAreLocked;

	// Token: 0x040001E3 RID: 483
	public HolidayType HolidayType;
}
