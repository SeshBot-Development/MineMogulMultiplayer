using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Token: 0x020000F1 RID: 241
public class ToolMagnet : BaseHeldTool
{
	// Token: 0x06000666 RID: 1638 RVA: 0x0002166A File Offset: 0x0001F86A
	protected override void OnEnable()
	{
		base.OnEnable();
		this.UpdateScreenUI();
	}

	// Token: 0x06000667 RID: 1639 RVA: 0x00021678 File Offset: 0x0001F878
	public override string GetControlsText()
	{
		return string.Concat(new string[]
		{
			"Drop Magnet - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool),
			"\nPull Objects - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.SecondaryAttack),
			"\nLaunch Objects - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.PrimaryAttack),
			"\nDrop Objects - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.RotateObject),
			"\n\nChange Mode - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.MirrorObject),
			"\nCurrent Grab Mode:\n",
			this.GetCurrentSelectionModeText()
		});
	}

	// Token: 0x06000668 RID: 1640 RVA: 0x00021718 File Offset: 0x0001F918
	public string GetCurrentSelectionModeText()
	{
		switch (this.SelectionMode)
		{
		case MagnetToolSelectionMode.Everything:
			return "Everything";
		case MagnetToolSelectionMode.ResourcesNotInFilter:
			return "Resources not in baskets";
		case MagnetToolSelectionMode.ResourcesNotOnConveyors:
			return "Resources not on conveyors";
		default:
			return "Unknown";
		}
	}

	// Token: 0x06000669 RID: 1641 RVA: 0x00021758 File Offset: 0x0001F958
	private void CycleSelectionMode()
	{
		Singleton<SoundManager>.Instance.PlayUISound(Singleton<SoundManager>.Instance.Sound_UI_Button_Hover, 1f);
		MagnetToolSelectionMode[] array = (MagnetToolSelectionMode[])Enum.GetValues(typeof(MagnetToolSelectionMode));
		int num = (Array.IndexOf<MagnetToolSelectionMode>(array, this.SelectionMode) + 1) % array.Length;
		this.SelectionMode = array[num];
		Singleton<UIManager>.Instance.UpdateOnScreenControls(this);
		this.UpdateScreenUI();
	}

	// Token: 0x0600066A RID: 1642 RVA: 0x000217C0 File Offset: 0x0001F9C0
	public void UpdateScreenUI()
	{
		this._selectionModeText.text = this.GetCurrentSelectionModeText();
	}

	// Token: 0x0600066B RID: 1643 RVA: 0x000217D4 File Offset: 0x0001F9D4
	private void FixedUpdate()
	{
		if (this.Owner == null)
		{
			return;
		}
		Vector3 position = this.Owner.transform.position;
		this._playerVelocity = (position - this._lastPlayerPosition) / Time.fixedDeltaTime;
		this._lastPlayerPosition = position;
		Vector3 vector = this.Owner.MagnetToolPosition.position + this._playerVelocity * Time.fixedDeltaTime * 10f;
		this.PullOrigin.position = Vector3.SmoothDamp(this.PullOrigin.position, vector, ref this._pullTargetVelocity, 0.03f, 10f, Time.fixedDeltaTime);
		for (int i = this._joints.Count - 1; i >= 0; i--)
		{
			if (this._joints[i] == null || this._joints[i].connectedBody == null)
			{
				if (i < this._anchors.Count && this._anchors[i] != null)
				{
					Object.Destroy(this._anchors[i]);
				}
				if (i < this._heldBodies.Count)
				{
					Rigidbody rigidbody = this._heldBodies[i];
					if (rigidbody != null)
					{
						rigidbody.linearDamping = 0.2f;
						rigidbody.angularDamping = 0.05f;
					}
					this._heldBodies.RemoveAt(i);
				}
				this._joints.RemoveAt(i);
				this._anchors.RemoveAt(i);
			}
		}
		for (int j = this._droppedBodies.Count - 1; j >= 0; j--)
		{
			ToolMagnet.DroppedBodyInfo droppedBodyInfo = this._droppedBodies[j];
			if (droppedBodyInfo.Rb == null)
			{
				this._droppedBodies.RemoveAt(j);
			}
			else
			{
				droppedBodyInfo.Timer -= Time.fixedDeltaTime;
				if (droppedBodyInfo.Timer <= 0f)
				{
					droppedBodyInfo.Rb.interpolation = RigidbodyInterpolation.None;
					this._droppedBodies.RemoveAt(j);
				}
				else
				{
					this._droppedBodies[j] = droppedBodyInfo;
				}
			}
		}
		if (!this._wantsToMagnet)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(this.PullOrigin.position, this.PullRadius, this.GrabbableLayer);
		for (int k = 0; k < array.Length; k++)
		{
			Collider collider = array[k];
			if (!collider.CompareTag("MarkedForDestruction"))
			{
				Rigidbody rb = collider.attachedRigidbody;
				if (!(rb == null) && !this._heldBodies.Contains(rb))
				{
					OrePiece componentInParent = rb.GetComponentInParent<OrePiece>();
					if ((this.SelectionMode != MagnetToolSelectionMode.ResourcesNotInFilter && this.SelectionMode != MagnetToolSelectionMode.ResourcesNotOnConveyors) || (!(componentInParent == null) && componentInParent.BasketsThisIsInside.Count <= 0 && (this.SelectionMode != MagnetToolSelectionMode.ResourcesNotOnConveyors || !componentInParent.IsOnAnyConveyor())))
					{
						GameObject gameObject = new GameObject("MagnetAnchor");
						gameObject.transform.position = this.PullOrigin.position;
						gameObject.transform.parent = this.PullOrigin;
						gameObject.AddComponent<Rigidbody>().isKinematic = true;
						SpringJoint springJoint = gameObject.AddComponent<SpringJoint>();
						springJoint.connectedBody = rb;
						springJoint.autoConfigureConnectedAnchor = false;
						springJoint.connectedAnchor = rb.transform.InverseTransformPoint(collider.transform.position);
						springJoint.spring = 100f;
						springJoint.damper = 25f;
						springJoint.maxDistance = 0.01f;
						springJoint.breakForce = 120f;
						springJoint.breakTorque = 20f;
						rb.linearDamping = 3f;
						rb.angularDamping = 1.5f;
						rb.interpolation = RigidbodyInterpolation.Interpolate;
						PhysicsUtils.IgnoreAllCollisions(rb.gameObject, this.Owner.gameObject, true);
						this._droppedBodies.RemoveAll((ToolMagnet.DroppedBodyInfo d) => d.Rb == rb);
						this._heldBodies.Add(rb);
						this._joints.Add(springJoint);
						this._anchors.Add(gameObject);
						if (componentInParent != null)
						{
							componentInParent.CurrentMagnetTool = this;
						}
					}
				}
			}
		}
		this._wantsToMagnet = false;
	}

	// Token: 0x0600066C RID: 1644 RVA: 0x00021C58 File Offset: 0x0001FE58
	private void DropObjects(float force)
	{
		for (int i = 0; i < this._joints.Count; i++)
		{
			if (this._joints[i] != null)
			{
				Object.Destroy(this._joints[i].gameObject);
			}
		}
		this._joints.Clear();
		this._anchors.Clear();
		foreach (Rigidbody rigidbody in this._heldBodies)
		{
			if (!(rigidbody == null))
			{
				rigidbody.AddForce(this.Owner.PlayerCamera.transform.forward * force, ForceMode.Impulse);
				rigidbody.linearDamping = 0.2f;
				rigidbody.angularDamping = 0.05f;
				PhysicsUtils.IgnoreAllCollisions(rigidbody.gameObject, this.Owner.gameObject, false);
				this._droppedBodies.Add(new ToolMagnet.DroppedBodyInfo
				{
					Rb = rigidbody,
					Timer = 3f
				});
				OrePiece componentInParent = rigidbody.GetComponentInParent<OrePiece>();
				if (componentInParent != null && componentInParent.CurrentMagnetTool == this)
				{
					componentInParent.CurrentMagnetTool = null;
				}
			}
		}
		this._heldBodies.Clear();
	}

	// Token: 0x0600066D RID: 1645 RVA: 0x00021DB4 File Offset: 0x0001FFB4
	public void DetachBody(Rigidbody rb)
	{
		if (rb == null)
		{
			return;
		}
		int num = this._heldBodies.IndexOf(rb);
		if (num >= 0)
		{
			if (num < this._joints.Count && this._joints[num] != null)
			{
				Object.Destroy(this._joints[num].gameObject);
			}
			if (num < this._anchors.Count && this._anchors[num] != null)
			{
				Object.Destroy(this._anchors[num]);
			}
			this._joints.RemoveAt(num);
			this._anchors.RemoveAt(num);
			this._heldBodies.RemoveAt(num);
			rb.linearDamping = 0f;
			rb.angularDamping = 0.05f;
			PhysicsUtils.IgnoreAllCollisions(rb.gameObject, this.Owner.gameObject, false);
			this._droppedBodies.Add(new ToolMagnet.DroppedBodyInfo
			{
				Rb = rb,
				Timer = 3f
			});
			OrePiece componentInParent = rb.GetComponentInParent<OrePiece>();
			if (componentInParent != null && componentInParent.CurrentMagnetTool == this)
			{
				componentInParent.CurrentMagnetTool = null;
			}
		}
	}

	// Token: 0x0600066E RID: 1646 RVA: 0x00021EE8 File Offset: 0x000200E8
	public override bool TryAddToInventory(int index = -1)
	{
		QuestManager instance = Singleton<QuestManager>.Instance;
		if (instance != null)
		{
			instance.ActivateQuestTrigger(TriggeredQuestRequirementType.CollectMagnetTool, 1);
		}
		return base.TryAddToInventory(index);
	}

	// Token: 0x0600066F RID: 1647 RVA: 0x00021F03 File Offset: 0x00020103
	public override void QButtonPressed()
	{
		this.CycleSelectionMode();
	}

	// Token: 0x06000670 RID: 1648 RVA: 0x00021F0B File Offset: 0x0002010B
	public override void PrimaryFire()
	{
		this.DropObjects(this.PushForce);
	}

	// Token: 0x06000671 RID: 1649 RVA: 0x00021F19 File Offset: 0x00020119
	public override void Reload()
	{
		this.DropObjects(this.DropForce);
	}

	// Token: 0x06000672 RID: 1650 RVA: 0x00021F27 File Offset: 0x00020127
	public override void DropItem()
	{
		this.DropObjects(this.DropForce);
		base.DropItem();
	}

	// Token: 0x06000673 RID: 1651 RVA: 0x00021F3B File Offset: 0x0002013B
	public override void UnEquip()
	{
		this.DropObjects(this.DropForce);
		base.UnEquip();
	}

	// Token: 0x06000674 RID: 1652 RVA: 0x00021F4F File Offset: 0x0002014F
	protected override void OnDisable()
	{
		base.OnDisable();
		this.DropObjects(this.DropForce);
	}

	// Token: 0x06000675 RID: 1653 RVA: 0x00021F63 File Offset: 0x00020163
	public override void SecondaryFireHeld()
	{
		this._wantsToMagnet = true;
	}

	// Token: 0x06000676 RID: 1654 RVA: 0x00021F6C File Offset: 0x0002016C
	public override void LoadFromSave(string json)
	{
		base.LoadFromSave(json);
		ToolMagnetSaveData toolMagnetSaveData = JsonUtility.FromJson<ToolMagnetSaveData>(json);
		if (toolMagnetSaveData == null)
		{
			toolMagnetSaveData = new ToolMagnetSaveData();
		}
		this.SelectionMode = toolMagnetSaveData.SelectionMode;
	}

	// Token: 0x06000677 RID: 1655 RVA: 0x00021F9C File Offset: 0x0002019C
	public override string GetCustomSaveData()
	{
		ToolMagnetSaveData toolMagnetSaveData = new ToolMagnetSaveData
		{
			IsInPlayerInventory = (this.Owner != null)
		};
		if (toolMagnetSaveData.IsInPlayerInventory)
		{
			toolMagnetSaveData.InventorySlotIndex = Object.FindObjectOfType<PlayerInventory>().GetInventoryIndexForTool(this);
		}
		toolMagnetSaveData.SelectionMode = this.SelectionMode;
		return JsonUtility.ToJson(toolMagnetSaveData);
	}

	// Token: 0x040007A7 RID: 1959
	public float PullRadius = 2f;

	// Token: 0x040007A8 RID: 1960
	public float PullForce = 50f;

	// Token: 0x040007A9 RID: 1961
	public float PushForce = 3f;

	// Token: 0x040007AA RID: 1962
	public float DropForce = 1f;

	// Token: 0x040007AB RID: 1963
	public Transform PullOrigin;

	// Token: 0x040007AC RID: 1964
	public LayerMask GrabbableLayer;

	// Token: 0x040007AD RID: 1965
	public MagnetToolSelectionMode SelectionMode;

	// Token: 0x040007AE RID: 1966
	[SerializeField]
	private TMP_Text _selectionModeText;

	// Token: 0x040007AF RID: 1967
	private List<Rigidbody> _heldBodies = new List<Rigidbody>();

	// Token: 0x040007B0 RID: 1968
	private List<SpringJoint> _joints = new List<SpringJoint>();

	// Token: 0x040007B1 RID: 1969
	private List<GameObject> _anchors = new List<GameObject>();

	// Token: 0x040007B2 RID: 1970
	private bool _wantsToMagnet;

	// Token: 0x040007B3 RID: 1971
	private Vector3 _pullTargetVelocity;

	// Token: 0x040007B4 RID: 1972
	private Vector3 _lastPlayerPosition;

	// Token: 0x040007B5 RID: 1973
	private Vector3 _playerVelocity;

	// Token: 0x040007B6 RID: 1974
	private readonly List<ToolMagnet.DroppedBodyInfo> _droppedBodies = new List<ToolMagnet.DroppedBodyInfo>();

	// Token: 0x02000187 RID: 391
	private struct DroppedBodyInfo
	{
		// Token: 0x0400099D RID: 2461
		public Rigidbody Rb;

		// Token: 0x0400099E RID: 2462
		public float Timer;
	}
}
