using System;
using System.Collections.Generic;

// Token: 0x02000057 RID: 87
public interface IInteractable
{
	// Token: 0x06000231 RID: 561
	bool ShouldUseInteractionWheel();

	// Token: 0x06000232 RID: 562
	List<Interaction> GetInteractions();

	// Token: 0x06000233 RID: 563
	string GetObjectName();

	// Token: 0x06000234 RID: 564
	void Interact(Interaction selectedInteraction);
}
