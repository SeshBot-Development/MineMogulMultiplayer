using DG.Tweening;
using UnityEngine;

public class RandomizeTweenDelay : MonoBehaviour
{
	public DOTweenAnimation DOTweenAnimation;

	public float minDelay;

	public float maxDelay = 1f;

	private void Awake()
	{
		DOTweenAnimation = GetComponent<DOTweenAnimation>();
		DOTweenAnimation.delay = Random.Range(minDelay, maxDelay);
		DOTweenAnimation.DORestart();
	}
}
