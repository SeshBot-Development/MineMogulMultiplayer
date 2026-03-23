using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000046 RID: 70
public class DetonatorTrigger : MonoBehaviour, IInteractable
{
	// Token: 0x17000011 RID: 17
	// (get) Token: 0x060001D4 RID: 468 RVA: 0x00009E63 File Offset: 0x00008063
	// (set) Token: 0x060001D5 RID: 469 RVA: 0x00009E6B File Offset: 0x0000806B
	public bool HasTriggered { get; private set; }

	// Token: 0x060001D6 RID: 470 RVA: 0x00009E74 File Offset: 0x00008074
	public void Initialize(DetonatorExplosion owner)
	{
		this._owner = owner;
		this.ToggleHandle(this._owner.HasPurchased());
		if (this._owner.HasExploded())
		{
			this.RemoveDetonatorTrigger();
		}
	}

	// Token: 0x060001D7 RID: 471 RVA: 0x00009EA1 File Offset: 0x000080A1
	public void ToggleHandle(bool enabled)
	{
		base.gameObject.SetActive(enabled);
	}

	// Token: 0x060001D8 RID: 472 RVA: 0x00009EAF File Offset: 0x000080AF
	private void TriggerExplosion()
	{
		if (this.HasTriggered)
		{
			return;
		}
		this.HasTriggered = true;
		if (this._owner.HasExploded())
		{
			return;
		}
		base.StartCoroutine(this.WaitThenExplode());
	}

	// Token: 0x060001D9 RID: 473 RVA: 0x00009EDC File Offset: 0x000080DC
	private IEnumerator WaitThenExplode()
	{
		this.PlayHandleAnimation();
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this._interactSound, base.transform.position, 1f, 1f, true, false);
		yield return new WaitForSeconds(2f);
		this._owner.Explode(false, this);
		yield return new WaitForSeconds(5f);
		if (this._physicsVersion != null)
		{
			this._physicsVersion.transform.SetParent(null);
			this._physicsVersion.gameObject.SetActive(true);
		}
		this.RemoveDetonatorTrigger();
		yield break;
	}

	// Token: 0x060001DA RID: 474 RVA: 0x00009EEB File Offset: 0x000080EB
	private void PlayHandleAnimation()
	{
		base.transform.position += new Vector3(0f, -0.4f, 0f);
	}

	// Token: 0x060001DB RID: 475 RVA: 0x00009F17 File Offset: 0x00008117
	public void RemoveDetonatorTrigger()
	{
		this._parent.gameObject.SetActive(false);
	}

	// Token: 0x060001DC RID: 476 RVA: 0x00009F2A File Offset: 0x0000812A
	public bool ShouldUseInteractionWheel()
	{
		return false;
	}

	// Token: 0x060001DD RID: 477 RVA: 0x00009F2D File Offset: 0x0000812D
	public string GetObjectName()
	{
		return "Detonator Trigger";
	}

	// Token: 0x060001DE RID: 478 RVA: 0x00009F34 File Offset: 0x00008134
	public List<Interaction> GetInteractions()
	{
		return this._interactions;
	}

	// Token: 0x060001DF RID: 479 RVA: 0x00009F3C File Offset: 0x0000813C
	public void Interact(Interaction selectedInteraction)
	{
		this.TriggerExplosion();
	}

	// Token: 0x040001CF RID: 463
	[SerializeField]
	private SoundDefinition _interactSound;

	// Token: 0x040001D0 RID: 464
	[SerializeField]
	private GameObject _physicsVersion;

	// Token: 0x040001D1 RID: 465
	[SerializeField]
	private GameObject _parent;

	// Token: 0x040001D2 RID: 466
	private DetonatorExplosion _owner;

	// Token: 0x040001D4 RID: 468
	[SerializeField]
	private List<Interaction> _interactions;
}
