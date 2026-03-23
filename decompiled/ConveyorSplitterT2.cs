using System.Collections;
using UnityEngine;

public class ConveyorSplitterT2 : MonoBehaviour
{
	public float minY = -35f;

	public float maxY = 35f;

	public float duration = 1.5f;

	public float pauseTime = 2f;

	public float IdleTime = 1f;

	public Transform RotatingThing;

	private Coroutine swingRoutine;

	private TimeSince _timeSinceLastObject;

	private void OnEnable()
	{
		swingRoutine = StartCoroutine(SwingLoop());
		_timeSinceLastObject = 0f;
	}

	private void OnDisable()
	{
		if (swingRoutine != null)
		{
			StopCoroutine(swingRoutine);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		_timeSinceLastObject = 0f;
	}

	private IEnumerator SwingLoop()
	{
		bool goingToMax = true;
		while (true)
		{
			if (_timeSinceLastObject > IdleTime)
			{
				yield return new WaitForSeconds(0.25f);
				continue;
			}
			float startAngle = (goingToMax ? minY : maxY);
			float endAngle = (goingToMax ? maxY : minY);
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				float t = Mathf.Clamp01(elapsed / duration);
				float t2 = Mathf.SmoothStep(0f, 1f, t);
				float y = Mathf.Lerp(startAngle, endAngle, t2);
				Vector3 localEulerAngles = RotatingThing.localEulerAngles;
				RotatingThing.localEulerAngles = new Vector3(localEulerAngles.x, y, localEulerAngles.z);
				yield return null;
			}
			yield return new WaitForSeconds(pauseTime);
			goingToMax = !goingToMax;
		}
	}
}
