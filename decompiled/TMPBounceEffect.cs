using DG.Tweening;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TMPBounceEffect : MonoBehaviour
{
	[Header("Bounce Settings")]
	public float bounceHeight = 2.5f;

	public float duration = 0.35f;

	public float stagger = 0.04f;

	public Ease ease = Ease.InOutSine;

	[Header("Color Settings")]
	public Color startColor = Color.white;

	public Color targetColor = Color.red;

	public float colorDuration = 0.8f;

	[Header("Randomization")]
	[Tooltip("Randomized start delay for the whole effect.")]
	public float randomStartDelayMin;

	public float randomStartDelayMax = 0.2f;

	private TMP_Text _text;

	private DOTweenTMPAnimator _animator;

	private Sequence _seq;

	private void Awake()
	{
		_text = GetComponent<TMP_Text>();
	}

	private void OnEnable()
	{
		StartBounce();
	}

	private void OnDisable()
	{
		StopBounce();
	}

	public void StartBounce()
	{
		if (_seq != null && _seq.IsActive())
		{
			return;
		}
		_text.ForceMeshUpdate();
		_animator = new DOTweenTMPAnimator(_text);
		_seq = DOTween.Sequence().SetTarget(_text);
		int characterCount = _animator.textInfo.characterCount;
		for (int i = 0; i < characterCount; i++)
		{
			if (_animator.textInfo.characterInfo[i].isVisible)
			{
				float delay = (float)i * stagger;
				_seq.Join(_animator.DOOffsetChar(i, new Vector3(0f, bounceHeight, 0f), duration).SetLoops(int.MaxValue, LoopType.Yoyo).SetEase(ease)
					.SetDelay(delay));
				_seq.Join(_animator.DOColorChar(i, targetColor, colorDuration).SetLoops(int.MaxValue, LoopType.Yoyo).SetDelay(delay));
			}
		}
		float delay2 = Random.Range(randomStartDelayMin, randomStartDelayMax);
		_seq.SetDelay(delay2);
	}

	public void StopBounce()
	{
		if (_seq != null && _seq.IsActive())
		{
			_seq.Kill();
		}
		_seq = null;
		_animator = null;
		_text.color = startColor;
	}

	public void Rebuild()
	{
		StopBounce();
		StartBounce();
	}
}
