using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToolMagnet : BaseHeldTool
{
	private struct DroppedBodyInfo
	{
		public Rigidbody Rb;

		public float Timer;
	}

	public float PullRadius = 2f;

	public float PullForce = 50f;

	public float PushForce = 3f;

	public float DropForce = 1f;

	public Transform PullOrigin;

	public LayerMask GrabbableLayer;

	public MagnetToolSelectionMode SelectionMode;

	[SerializeField]
	private TMP_Text _selectionModeText;

	private List<Rigidbody> _heldBodies = new List<Rigidbody>();

	private List<SpringJoint> _joints = new List<SpringJoint>();

	private List<GameObject> _anchors = new List<GameObject>();

	private bool _wantsToMagnet;

	private Vector3 _pullTargetVelocity;

	private Vector3 _lastPlayerPosition;

	private Vector3 _playerVelocity;

	private readonly List<DroppedBodyInfo> _droppedBodies = new List<DroppedBodyInfo>();

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateScreenUI();
	}

	public override string GetControlsText()
	{
		return "Drop Magnet - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool) + "\nPull Objects - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.SecondaryAttack) + "\nLaunch Objects - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.PrimaryAttack) + "\nDrop Objects - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.RotateObject) + "\n\nChange Mode - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.MirrorObject) + "\nCurrent Grab Mode:\n" + GetCurrentSelectionModeText();
	}

	public string GetCurrentSelectionModeText()
	{
		return SelectionMode switch
		{
			MagnetToolSelectionMode.Everything => "Everything", 
			MagnetToolSelectionMode.ResourcesNotInFilter => "Resources not in baskets", 
			MagnetToolSelectionMode.ResourcesNotOnConveyors => "Resources not on conveyors", 
			_ => "Unknown", 
		};
	}

	private void CycleSelectionMode()
	{
		Singleton<SoundManager>.Instance.PlayUISound(Singleton<SoundManager>.Instance.Sound_UI_Button_Hover);
		MagnetToolSelectionMode[] array = (MagnetToolSelectionMode[])Enum.GetValues(typeof(MagnetToolSelectionMode));
		int num = (Array.IndexOf(array, SelectionMode) + 1) % array.Length;
		SelectionMode = array[num];
		Singleton<UIManager>.Instance.UpdateOnScreenControls(this);
		UpdateScreenUI();
	}

	public void UpdateScreenUI()
	{
		_selectionModeText.text = GetCurrentSelectionModeText();
	}

	private void FixedUpdate()
	{
		if (Owner == null)
		{
			return;
		}
		Vector3 position = Owner.transform.position;
		_playerVelocity = (position - _lastPlayerPosition) / Time.fixedDeltaTime;
		_lastPlayerPosition = position;
		Vector3 target = Owner.MagnetToolPosition.position + _playerVelocity * Time.fixedDeltaTime * 10f;
		PullOrigin.position = Vector3.SmoothDamp(PullOrigin.position, target, ref _pullTargetVelocity, 0.03f, 10f, Time.fixedDeltaTime);
		for (int num = _joints.Count - 1; num >= 0; num--)
		{
			if (_joints[num] == null || _joints[num].connectedBody == null)
			{
				if (num < _anchors.Count && _anchors[num] != null)
				{
					UnityEngine.Object.Destroy(_anchors[num]);
				}
				if (num < _heldBodies.Count)
				{
					Rigidbody rigidbody = _heldBodies[num];
					if (rigidbody != null)
					{
						rigidbody.linearDamping = 0.2f;
						rigidbody.angularDamping = 0.05f;
					}
					_heldBodies.RemoveAt(num);
				}
				_joints.RemoveAt(num);
				_anchors.RemoveAt(num);
			}
		}
		for (int num2 = _droppedBodies.Count - 1; num2 >= 0; num2--)
		{
			DroppedBodyInfo value = _droppedBodies[num2];
			if (value.Rb == null)
			{
				_droppedBodies.RemoveAt(num2);
			}
			else
			{
				value.Timer -= Time.fixedDeltaTime;
				if (value.Timer <= 0f)
				{
					value.Rb.interpolation = RigidbodyInterpolation.None;
					_droppedBodies.RemoveAt(num2);
				}
				else
				{
					_droppedBodies[num2] = value;
				}
			}
		}
		if (!_wantsToMagnet)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(PullOrigin.position, PullRadius, GrabbableLayer);
		foreach (Collider collider in array)
		{
			if (collider.CompareTag("MarkedForDestruction"))
			{
				continue;
			}
			Rigidbody rb = collider.attachedRigidbody;
			if (rb == null || _heldBodies.Contains(rb))
			{
				continue;
			}
			OrePiece componentInParent = rb.GetComponentInParent<OrePiece>();
			if ((SelectionMode != MagnetToolSelectionMode.ResourcesNotInFilter && SelectionMode != MagnetToolSelectionMode.ResourcesNotOnConveyors) || (!(componentInParent == null) && componentInParent.BasketsThisIsInside.Count <= 0 && (SelectionMode != MagnetToolSelectionMode.ResourcesNotOnConveyors || !componentInParent.IsOnAnyConveyor())))
			{
				GameObject gameObject = new GameObject("MagnetAnchor");
				gameObject.transform.position = PullOrigin.position;
				gameObject.transform.parent = PullOrigin;
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
				PhysicsUtils.IgnoreAllCollisions(rb.gameObject, Owner.gameObject, ignore: true);
				_droppedBodies.RemoveAll((DroppedBodyInfo d) => d.Rb == rb);
				_heldBodies.Add(rb);
				_joints.Add(springJoint);
				_anchors.Add(gameObject);
				if (componentInParent != null)
				{
					componentInParent.CurrentMagnetTool = this;
				}
			}
		}
		_wantsToMagnet = false;
	}

	private void DropObjects(float force)
	{
		for (int i = 0; i < _joints.Count; i++)
		{
			if (_joints[i] != null)
			{
				UnityEngine.Object.Destroy(_joints[i].gameObject);
			}
		}
		_joints.Clear();
		_anchors.Clear();
		foreach (Rigidbody heldBody in _heldBodies)
		{
			if (!(heldBody == null))
			{
				heldBody.AddForce(Owner.PlayerCamera.transform.forward * force, ForceMode.Impulse);
				heldBody.linearDamping = 0.2f;
				heldBody.angularDamping = 0.05f;
				PhysicsUtils.IgnoreAllCollisions(heldBody.gameObject, Owner.gameObject, ignore: false);
				_droppedBodies.Add(new DroppedBodyInfo
				{
					Rb = heldBody,
					Timer = 3f
				});
				OrePiece componentInParent = heldBody.GetComponentInParent<OrePiece>();
				if (componentInParent != null && componentInParent.CurrentMagnetTool == this)
				{
					componentInParent.CurrentMagnetTool = null;
				}
			}
		}
		_heldBodies.Clear();
	}

	public void DetachBody(Rigidbody rb)
	{
		if (rb == null)
		{
			return;
		}
		int num = _heldBodies.IndexOf(rb);
		if (num >= 0)
		{
			if (num < _joints.Count && _joints[num] != null)
			{
				UnityEngine.Object.Destroy(_joints[num].gameObject);
			}
			if (num < _anchors.Count && _anchors[num] != null)
			{
				UnityEngine.Object.Destroy(_anchors[num]);
			}
			_joints.RemoveAt(num);
			_anchors.RemoveAt(num);
			_heldBodies.RemoveAt(num);
			rb.linearDamping = 0f;
			rb.angularDamping = 0.05f;
			PhysicsUtils.IgnoreAllCollisions(rb.gameObject, Owner.gameObject, ignore: false);
			_droppedBodies.Add(new DroppedBodyInfo
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

	public override bool TryAddToInventory(int index = -1)
	{
		Singleton<QuestManager>.Instance?.ActivateQuestTrigger(TriggeredQuestRequirementType.CollectMagnetTool);
		return base.TryAddToInventory(index);
	}

	public override void QButtonPressed()
	{
		CycleSelectionMode();
	}

	public override void PrimaryFire()
	{
		DropObjects(PushForce);
	}

	public override void Reload()
	{
		DropObjects(DropForce);
	}

	public override void DropItem()
	{
		DropObjects(DropForce);
		base.DropItem();
	}

	public override void UnEquip()
	{
		DropObjects(DropForce);
		base.UnEquip();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		DropObjects(DropForce);
	}

	public override void SecondaryFireHeld()
	{
		_wantsToMagnet = true;
	}

	public override void LoadFromSave(string json)
	{
		base.LoadFromSave(json);
		ToolMagnetSaveData toolMagnetSaveData = JsonUtility.FromJson<ToolMagnetSaveData>(json);
		if (toolMagnetSaveData == null)
		{
			toolMagnetSaveData = new ToolMagnetSaveData();
		}
		SelectionMode = toolMagnetSaveData.SelectionMode;
	}

	public override string GetCustomSaveData()
	{
		ToolMagnetSaveData toolMagnetSaveData = new ToolMagnetSaveData
		{
			IsInPlayerInventory = (Owner != null)
		};
		if (toolMagnetSaveData.IsInPlayerInventory)
		{
			toolMagnetSaveData.InventorySlotIndex = UnityEngine.Object.FindObjectOfType<PlayerInventory>().GetInventoryIndexForTool(this);
		}
		toolMagnetSaveData.SelectionMode = SelectionMode;
		return JsonUtility.ToJson(toolMagnetSaveData);
	}
}
