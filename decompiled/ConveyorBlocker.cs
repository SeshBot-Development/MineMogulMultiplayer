using UnityEngine;

public class ConveyorBlocker : MonoBehaviour
{
	public HingeJoint Hinge;

	public ConveyorBelt Conveyor;

	private float closedAngle = -80f;

	private void Update()
	{
		if ((bool)Hinge && (bool)Conveyor)
		{
			bool disabled = Hinge.angle < closedAngle;
			Conveyor.Disabled = disabled;
		}
	}
}
