using System;
using System.Collections.Generic;

// Token: 0x0200001F RID: 31
[Serializable]
public class CastingFurnaceMoldRecipieSet
{
	// Token: 0x04000100 RID: 256
	public CastingMoldType CastingMoldType;

	// Token: 0x04000101 RID: 257
	public int AmountOfMaterialRequired = 6;

	// Token: 0x04000102 RID: 258
	public List<CastingFurnaceRecipie> Recipies;
}
