using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetonatorTrigger : MonoBehaviour, IInteractable
{
	[SerializeField]
	private SoundDefinition _interactSound;

	[SerializeField]
	private GameObject _physicsVersion;

	[SerializeField]
	private GameObject _parent;

	private DetonatorExplosion _owner;

	[SerializeField]
	private List<Interaction> _interactions;

	public bool HasTriggered { get; private set; }

	public void Initialize(DetonatorExplosion owner)
	{
		_owner = owner;
		ToggleHandle(_owner.HasPurchased());
		if (_owner.HasExploded())
		{
			RemoveDetonatorTrigger();
		}
	}

	public void ToggleHandle(bool enabled)
	{
		base.gameObject.SetActive(enabled);
	}

	private void TriggerExplosion()
	{
		if (!HasTriggered)
		{
			HasTriggered = true;
			if (!_owner.HasExploded())
			{
				StartCoroutine(WaitThenExplode());
			}
		}
	}

	private IEnumerator WaitThenExplode()
	{
		PlayHandleAnimation();
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(_interactSound, base.transform.position);
		yield return new WaitForSeconds(2f);
		_owner.Explode(fromLoadingSaveFile: false, this);
		yield return new WaitForSeconds(5f);
		if (_physicsVersion != null)
		{
			_physicsVersion.transform.SetParent(null);
			_physicsVersion.gameObject.SetActive(value: true);
		}
		RemoveDetonatorTrigger();
	}

	private void PlayHandleAnimation()
	{
		base.transform.position += new Vector3(0f, -0.4f, 0f);
	}

	public void RemoveDetonatorTrigger()
	{
		_parent.gameObject.SetActive(value: false);
	}

	public bool ShouldUseInteractionWheel()
	{
		return false;
	}

	public string GetObjectName()
	{
		return "Detonator Trigger";
	}

	public List<Interaction> GetInteractions()
	{
		return _interactions;
	}

	public void Interact(Interaction selectedInteraction)
	{
		TriggerExplosion();
	}
}
