using System.Collections.Generic;

public interface IInteractable
{
	bool ShouldUseInteractionWheel();

	List<Interaction> GetInteractions();

	string GetObjectName();

	void Interact(Interaction selectedInteraction);
}
