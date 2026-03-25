using System;
using System.Collections.Generic;

[Serializable]
public class CastingFurnaceMoldRecipieSet
{
	public CastingMoldType CastingMoldType;

	public int AmountOfMaterialRequired = 6;

	public List<CastingFurnaceRecipie> Recipies;
}
