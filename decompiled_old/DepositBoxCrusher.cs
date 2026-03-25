using System.Collections;
using UnityEngine;

public class DepositBoxCrusher : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		OrePiece component = other.GetComponent<OrePiece>();
		if (component != null)
		{
			StartCoroutine(CrushOre(component));
		}
	}

	private IEnumerator CrushOre(OrePiece ore)
	{
		yield return new WaitForSeconds(0.3f);
		if (ore != null && ore.TryConvertToCrushed())
		{
			yield return new WaitForFixedUpdate();
			yield return new WaitForFixedUpdate();
			PhysicsUtils.SimpleExplosion(base.transform.position, 0.5f, 6f, 0.5f);
		}
	}
}
