using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x02000085 RID: 133
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	// Token: 0x17000017 RID: 23
	// (get) Token: 0x06000396 RID: 918 RVA: 0x00011C34 File Offset: 0x0000FE34
	// (set) Token: 0x06000397 RID: 919 RVA: 0x00011C3C File Offset: 0x0000FE3C
	public float SelectedWalkSpeed { get; private set; }

	// Token: 0x17000018 RID: 24
	// (get) Token: 0x06000398 RID: 920 RVA: 0x00011C45 File Offset: 0x0000FE45
	// (set) Token: 0x06000399 RID: 921 RVA: 0x00011C4D File Offset: 0x0000FE4D
	public Vector2 MoveInput { get; private set; }

	// Token: 0x17000019 RID: 25
	// (get) Token: 0x0600039A RID: 922 RVA: 0x00011C56 File Offset: 0x0000FE56
	// (set) Token: 0x0600039B RID: 923 RVA: 0x00011C5E File Offset: 0x0000FE5E
	public PlayerInventory Inventory { get; private set; }

	// Token: 0x0600039C RID: 924 RVA: 0x00011C68 File Offset: 0x0000FE68
	private void Awake()
	{
		this._input = Singleton<KeybindManager>.Instance.Input;
		this._input.Player.Enable();
		this.Inventory = base.GetComponent<PlayerInventory>();
		if (this._fresnel == null)
		{
			this._fresnel = Object.FindObjectOfType<FresnelHighlighter>();
		}
	}

	// Token: 0x0600039D RID: 925 RVA: 0x00011CBD File Offset: 0x0000FEBD
	private void OnDisable()
	{
		if (this._fresnel)
		{
			this._fresnel.ClearAll();
		}
	}

	// Token: 0x0600039E RID: 926 RVA: 0x00011CD8 File Offset: 0x0000FED8
	private void Start()
	{
		this.CharacterController = base.GetComponent<CharacterController>();
		this.SelectedWalkSpeed = this.WalkSpeed;
		this._currentFOV = this.GetDesiredFOV();
		this.PlayerCamera.fieldOfView = this._currentFOV;
		this._cameraBaseLocalPos = this.PlayerCamera.transform.localPosition;
		this.InteractionWheelUI = Singleton<UIManager>.Instance.InteractionWheelUI;
	}

	// Token: 0x0600039F RID: 927 RVA: 0x00011D40 File Offset: 0x0000FF40
	public float GetDesiredFOV()
	{
		return Singleton<SettingsManager>.Instance.DesiredFOV;
	}

	// Token: 0x060003A0 RID: 928 RVA: 0x00011D4C File Offset: 0x0000FF4C
	private void Update()
	{
		if (Singleton<UIManager>.Instance.IsInEditTextPopup())
		{
			this._input.Player.Disable();
		}
		else
		{
			this._input.Player.Enable();
			if (Singleton<DebugManager>.Instance.DevModeEnabled && Input.GetKeyDown(KeyCode.V))
			{
				this.ToggleNoclip();
			}
		}
		this.HandleDucking();
		this._isGrounded = !this.IsUsingNoclip && this.CharacterController.isGrounded;
		if (!Cursor.visible)
		{
			this._lookDelta = this._input.Player.Look.ReadValue<Vector2>();
			float num = this._lookDelta.x * Singleton<SettingsManager>.Instance.MouseSensitivity * (float)(Singleton<SettingsManager>.Instance.InvertMouseX ? (-1) : 1);
			float num2 = this._lookDelta.y * Singleton<SettingsManager>.Instance.MouseSensitivity * (float)(Singleton<SettingsManager>.Instance.InvertMouseY ? (-1) : 1);
			this._xRotation -= num2;
			this._xRotation = Mathf.Clamp(this._xRotation, -88f, 88f);
			base.transform.Rotate(Vector3.up * num);
		}
		this.MoveInput = this._input.Player.Move.ReadValue<Vector2>();
		Vector3 vector = base.transform.right * this.MoveInput.x + base.transform.forward * this.MoveInput.y;
		bool flag = this._input.Player.Sprint.IsPressed() && !this._isDucking && this._isGrounded;
		this.SelectedWalkSpeed = (flag ? this.SprintSpeed : (this._isDucking ? this.DuckSpeed : this.WalkSpeed));
		if (this._input.Player.ToggleFlashlight.WasPressedThisFrame())
		{
			this.ToggleMiningLightFromKeybind(!this._miningLightEnabled);
		}
		float num3 = (flag ? (this.GetDesiredFOV() * 1.05f) : this.GetDesiredFOV());
		if (Time.timeScale == 0f)
		{
			this._currentFOV = num3;
		}
		else
		{
			this._currentFOV = Mathf.SmoothDamp(this._currentFOV, num3, ref this._fovVelocity, 0.1f);
		}
		this.PlayerCamera.fieldOfView = this._currentFOV;
		if (this.IsUsingNoclip)
		{
			this.HandleNoclipMovement();
		}
		else
		{
			this.CharacterController.Move(vector * this.SelectedWalkSpeed * Time.deltaTime);
		}
		if (this._isGrounded)
		{
			float num4 = 0f;
			Vector3 vector2 = Vector3.zero;
			int num5 = 0;
			int num6 = 6;
			Vector3 position = base.transform.position;
			float num7 = this.CharacterController.radius * 0.98f;
			float num8 = this.CharacterController.height / 2f + 0.2f;
			for (int i = 0; i < num6; i++)
			{
				float num9 = (float)i * 3.1415927f * 2f / (float)num6;
				Vector3 vector3 = new Vector3(Mathf.Cos(num9), 0f, Mathf.Sin(num9)) * num7;
				Vector3 vector4 = position + vector3;
				Debug.DrawRay(vector4, Vector3.down * num8, Color.red);
				RaycastHit raycastHit;
				if (Physics.Raycast(vector4, Vector3.down, out raycastHit, num8, this.GroundLayer))
				{
					float num10 = Vector3.Angle(raycastHit.normal, Vector3.up);
					if (num10 > this.StandingSlopeLimit)
					{
						Vector3 vector5 = new Vector3(raycastHit.normal.x, -raycastHit.normal.y, raycastHit.normal.z);
						vector2 += vector5;
						num4 = Mathf.Max(num4, num10);
						num5++;
					}
				}
				else
				{
					num5++;
				}
			}
			if (num5 == num6)
			{
				this._velocity += vector2.normalized * this.SlideSpeed * Time.deltaTime;
			}
			else if (this._velocity.y < 0f)
			{
				this._velocity.y = -2f;
				this._velocity.x = 0f;
				this._velocity.z = 0f;
			}
		}
		if (this._input.Player.Jump.triggered && this._isGrounded)
		{
			this._velocity.y = Mathf.Sqrt(this.JumpHeight * -2f * this.Gravity);
			this._velocity.x = 0f;
			this._velocity.z = 0f;
		}
		this._velocity.y = this._velocity.y + this.Gravity * Time.deltaTime;
		if (this.IsUsingNoclip)
		{
			this._velocity.y = 0f;
		}
		if ((((!this.IsUsingNoclip && this.CharacterController.Move(this._velocity * Time.deltaTime) != CollisionFlags.None) ? 1 : 0) & 2) != 0 && this._velocity.y > 0f)
		{
			this._velocity.y = 0f;
		}
		this.ShowLookedAtObjectInfo();
		this.OutlineLookedAtThing();
		if (this._input.Player.Interact.triggered)
		{
			this.TryInteract();
		}
		if (this._input.Player.Grab.triggered)
		{
			this.TryGrabObject();
		}
		if (this.HeldObject != null && !this.HeldObject.activeInHierarchy)
		{
			this.ReleaseObject();
		}
		if (this._grabJoint != null && this.HeldObject != null)
		{
			this.RopeRenderer.SetPosition(0, this.RigidbodyDragger.transform.position);
			Vector3 vector6 = this._grabJoint.connectedBody.transform.TransformPoint(this._grabJoint.connectedAnchor);
			this.RopeRenderer.SetPosition(1, vector6);
			this.RopeRenderer.enabled = true;
		}
		else
		{
			this.RopeRenderer.enabled = false;
		}
		this.HandleCameraBobbing();
		this.HandleViewModelBobbing();
		if (base.transform.position.y <= -200f)
		{
			this.RespawnPlayer();
		}
	}

	// Token: 0x060003A1 RID: 929 RVA: 0x000123AA File Offset: 0x000105AA
	private void ToggleNoclip()
	{
		this.IsUsingNoclip = !this.IsUsingNoclip;
		this.CharacterController.enabled = !this.IsUsingNoclip;
	}

	// Token: 0x060003A2 RID: 930 RVA: 0x000123D0 File Offset: 0x000105D0
	private void HandleNoclipMovement()
	{
		Vector3 forward = this.PlayerCamera.transform.forward;
		Vector3 right = this.PlayerCamera.transform.right;
		Vector3 vector = forward * this.MoveInput.y + right * this.MoveInput.x;
		if (vector.sqrMagnitude > 1f)
		{
			vector.Normalize();
		}
		this.SelectedWalkSpeed = ((this._input.Player.Sprint.IsPressed() && !this._isDucking) ? (this.SprintSpeed * 4f) : (this.WalkSpeed * 2f));
		float num = 0f;
		if (this._input.Player.Jump.IsPressed())
		{
			num += 1f;
		}
		if (this._input.Player.Duck.IsPressed())
		{
			num -= 1f;
		}
		Vector3 vector2 = Vector3.up * num * this.SelectedWalkSpeed;
		Vector3 vector3 = ((vector.sqrMagnitude > 1f) ? vector.normalized : vector);
		this.CharacterController.transform.position += (vector3 * this.SelectedWalkSpeed + vector2) * Time.deltaTime;
	}

	// Token: 0x060003A3 RID: 931 RVA: 0x0001253C File Offset: 0x0001073C
	public void ToggleMiningLightFromKeybind(bool enable)
	{
		ToolMiningHat toolMiningHat = this.Inventory.ActiveTool as ToolMiningHat;
		bool flag = toolMiningHat != null && toolMiningHat.gameObject.activeSelf;
		bool flag2 = false;
		foreach (BaseHeldTool baseHeldTool in this.Inventory.Items)
		{
			ToolMiningHat toolMiningHat2 = baseHeldTool as ToolMiningHat;
			if (toolMiningHat2 != null)
			{
				toolMiningHat2.ToggleLight(enable, true, false);
				flag2 = true;
				break;
			}
		}
		if (!flag2)
		{
			enable = false;
		}
		this._miningLightEnabled = enable;
		if (!flag)
		{
			this._nightVisionLight.gameObject.SetActive(!this._miningLightEnabled);
			this._miningHatLight.gameObject.SetActive(this._miningLightEnabled);
			return;
		}
		this._nightVisionLight.gameObject.SetActive(!this._miningLightEnabled);
		this._miningHatLight.gameObject.SetActive(false);
	}

	// Token: 0x060003A4 RID: 932 RVA: 0x00012634 File Offset: 0x00010834
	public void ToggleMiningLightFromTool(bool enable)
	{
		bool flag = enable;
		if (enable)
		{
			ToolMiningHat toolMiningHat = this.Inventory.ActiveTool as ToolMiningHat;
			if (toolMiningHat != null && toolMiningHat.gameObject.activeSelf)
			{
				flag = false;
			}
			else
			{
				bool flag2 = false;
				using (List<BaseHeldTool>.Enumerator enumerator = this.Inventory.Items.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current is ToolMiningHat)
						{
							flag2 = true;
							break;
						}
					}
				}
				if (!flag2)
				{
					enable = false;
					flag = false;
				}
			}
		}
		this._miningLightEnabled = enable;
		this._nightVisionLight.gameObject.SetActive(!this._miningLightEnabled);
		this._miningHatLight.gameObject.SetActive(flag);
	}

	// Token: 0x060003A5 RID: 933 RVA: 0x000126F4 File Offset: 0x000108F4
	private void HandleDucking()
	{
		bool flag;
		if (Singleton<SettingsManager>.Instance.ToggleDucking)
		{
			if (this._input.Player.Duck.WasPressedThisFrame())
			{
				this._toggleDuckIsToggled = !this._toggleDuckIsToggled;
			}
			flag = this._toggleDuckIsToggled;
		}
		else
		{
			flag = this._input.Player.Duck.IsPressed();
		}
		bool flag2 = true;
		if (!flag && this._isDucking)
		{
			float num = this.StandingHeight - this.DuckHeight;
			Vector3 vector = base.transform.position + this.CharacterController.center + Vector3.up * (num / 2f);
			Vector3 vector2 = new Vector3(this.CharacterController.radius * 0.95f, num / 2f, this.CharacterController.radius * 0.95f);
			flag2 = !Physics.CheckBox(vector, vector2, Quaternion.identity, this.GroundLayer, QueryTriggerInteraction.Ignore);
		}
		if (flag || (this._isDucking && !flag2))
		{
			this._isDucking = true;
		}
		else if (flag2)
		{
			this._isDucking = false;
		}
		float num2 = (this._isDucking ? this.DuckHeight : this.StandingHeight);
		float num3 = Mathf.Lerp(this.CharacterController.height, num2, Time.deltaTime * this.DuckingSpeed);
		this.CharacterController.height = num3;
		float num4 = num3 / 2f - 0.5f;
		float num5 = Mathf.SmoothDamp(this.PlayerCamera.transform.localPosition.y, num4, ref this._cameraHeightVelocity, 0.1f);
		if (float.IsNaN(num5))
		{
			num5 = 0f;
		}
		this.PlayerCamera.transform.localPosition = new Vector3(this.PlayerCamera.transform.localPosition.x, num5, this.PlayerCamera.transform.localPosition.z);
		float num6 = num3 / this.StandingHeight;
		this.CharacterModel.localScale = new Vector3(1f, num6, 1f);
	}

	// Token: 0x060003A6 RID: 934 RVA: 0x0001290C File Offset: 0x00010B0C
	private void HandleViewModelBobbing()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		bool flag = this._input.Player.Move.ReadValue<Vector2>().sqrMagnitude > 0.01f;
		Vector3 viewModelBasePos = this.ViewModelBasePos;
		bool flag2 = this._wasGroundedLastFrame && !this._isGrounded;
		bool flag3 = !this._wasGroundedLastFrame && this._isGrounded;
		if (flag2)
		{
			this._jumpTargetOffset = this.JumpBounceAmount;
		}
		else if (flag3)
		{
			this._jumpTargetOffset = this.LandBounceAmount;
		}
		this._jumpOffset = Mathf.SmoothDamp(this._jumpOffset, this._jumpTargetOffset, ref this._jumpVelocity, this.JumpSmoothTime);
		this._jumpTargetOffset = Mathf.MoveTowards(this._jumpTargetOffset, 0f, Time.deltaTime * Mathf.Abs(this._jumpTargetOffset / this.JumpSmoothTime));
		this._wasGroundedLastFrame = this._isGrounded;
		if (!this._isGrounded)
		{
			this._viewBobPitch = Mathf.SmoothDamp(this._viewBobPitch, 0f, ref this._viewBobPitchVel, 0.2f);
			this._viewBobYaw = Mathf.SmoothDamp(this._viewBobYaw, 0f, ref this._viewBobYawVel, 0.2f);
			this._viewBobVertical = Mathf.SmoothDamp(this._viewBobVertical, 0f, ref this._viewBobVerticalVel, 0.2f);
		}
		else if (!flag)
		{
			this._viewBobPitch = Mathf.SmoothDamp(this._viewBobPitch, 0f, ref this._viewBobPitchVel, 0.1f);
			this._viewBobYaw = Mathf.SmoothDamp(this._viewBobYaw, 0f, ref this._viewBobYawVel, 0.1f);
			this._viewBobVertical = Mathf.SmoothDamp(this._viewBobVertical, 0f, ref this._viewBobVerticalVel, 0.1f);
		}
		else
		{
			float num = this.SelectedWalkSpeed / Mathf.Max(this.WalkSpeed, 0.01f);
			float num2 = this.ViewModelBobSpeed * num;
			this._viewBobCounter += Time.deltaTime * num2;
			if (this._viewBobCounter > 6.2831855f)
			{
				this._viewBobCounter -= 6.2831855f;
				this._viewBobYawDirection *= -1;
			}
			float num3 = Mathf.Sin(this._viewBobCounter);
			float num4 = this.ViewModelBobAmount * num3 * num;
			this._viewBobVertical = Mathf.SmoothDamp(this._viewBobVertical, num4, ref this._viewBobVerticalVel, 0.05f);
			this._viewBobPitch = Mathf.SmoothDamp(this._viewBobPitch, num3 * this.ViewModelBobPitchAmount * num, ref this._viewBobPitchVel, 0.05f);
			this._viewBobYaw = Mathf.SmoothDamp(this._viewBobYaw, num3 * this.ViewModelBobYawAmount * num * (float)this._viewBobYawDirection, ref this._viewBobYawVel, 0.05f);
		}
		float viewmodelBobScale = Singleton<SettingsManager>.Instance.ViewmodelBobScale;
		if (float.IsNaN(this._viewBobVertical))
		{
			this._viewBobVertical = 0f;
		}
		if (float.IsNaN(this._jumpOffset))
		{
			this._jumpOffset = 0f;
		}
		this.ViewModelContainer.localPosition = viewModelBasePos + new Vector3(0f, (this._viewBobVertical + this._jumpOffset) * viewmodelBobScale, 0f);
		float num5 = Mathf.Clamp(this._lookDelta.x * this.ViewModelLookSwayAmount, -this.ViewModelLookSwayMax, this.ViewModelLookSwayMax);
		float num6 = Mathf.Clamp(-this._lookDelta.y * this.ViewModelLookSwayAmount, -this.ViewModelLookSwayMax, this.ViewModelLookSwayMax);
		if (float.IsNaN(this._yawSwayVelocity))
		{
			this._yawSwayVelocity = 0f;
		}
		this._smoothedYawSway = Mathf.SmoothDamp(this._smoothedYawSway, num5, ref this._yawSwayVelocity, 0.06f);
		if (float.IsNaN(this._pitchSwayVelocity))
		{
			this._pitchSwayVelocity = 0f;
		}
		this._smoothedPitchSway = Mathf.SmoothDamp(this._smoothedPitchSway, num6, ref this._pitchSwayVelocity, 0.06f);
		Quaternion quaternion = Quaternion.Euler((this._viewBobPitch + this._smoothedPitchSway) * viewmodelBobScale, (this._viewBobYaw + this._smoothedYawSway) * viewmodelBobScale, 0f);
		this.ViewModelContainer.localRotation = Quaternion.Euler(this.ViewModelBaseRotEuler) * quaternion;
	}

	// Token: 0x060003A7 RID: 935 RVA: 0x00012D38 File Offset: 0x00010F38
	private void HandleCameraBobbing()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		bool flag = this._input.Player.Move.ReadValue<Vector2>().sqrMagnitude > 0.01f;
		this._cameraBaseLocalPos = new Vector3(this.PlayerCamera.transform.localPosition.x, this.CharacterController.height / 2f - 0.5f, this.PlayerCamera.transform.localPosition.z);
		if (!this._isGrounded)
		{
			this._bobbingPitch = Mathf.SmoothDamp(this._bobbingPitch, 0f, ref this._bobbingPitchVelocity, 0.2f);
			this._bobbingYaw = Mathf.SmoothDamp(this._bobbingYaw, 0f, ref this._bobbingYawVelocity, 0.2f);
			this._bobbingVerticalOffset = Mathf.SmoothDamp(this._bobbingVerticalOffset, 0f, ref this._bobbingVerticalVelocity, 0.2f);
		}
		else if (!flag)
		{
			this._bobbingPitch = Mathf.SmoothDamp(this._bobbingPitch, 0f, ref this._bobbingPitchVelocity, 0.1f);
			this._bobbingYaw = Mathf.SmoothDamp(this._bobbingYaw, 0f, ref this._bobbingYawVelocity, 0.1f);
			this._bobbingVerticalOffset = Mathf.SmoothDamp(this._bobbingVerticalOffset, 0f, ref this._bobbingVerticalVelocity, 0.1f);
		}
		else
		{
			float num = this.SelectedWalkSpeed / Mathf.Max(this.WalkSpeed, 0.01f);
			float num2 = this.BaseBobbingSpeed * num;
			this._bobbingCounter += Time.deltaTime * num2;
			if (this._bobbingCounter > 6.2831855f)
			{
				this._bobbingCounter -= 6.2831855f;
				this._yawDirectionMultiplier *= -1f;
			}
			float num3 = Mathf.Sin(this._bobbingCounter);
			float num4 = this.BaseBobbingAmount * num3 * num;
			this._bobbingVerticalOffset = Mathf.SmoothDamp(this._bobbingVerticalOffset, num4, ref this._bobbingVerticalVelocity, 0.05f);
			this._bobbingPitch = Mathf.SmoothDamp(this._bobbingPitch, num3 * this.BaseBobbingPitchAmount * num, ref this._bobbingPitchVelocity, 0.05f);
			this._bobbingYaw = Mathf.SmoothDamp(this._bobbingYaw, num3 * this.BaseBobbingYawAmount * num * this._yawDirectionMultiplier, ref this._bobbingYawVelocity, 0.05f);
		}
		float cameraBobScale = Singleton<SettingsManager>.Instance.CameraBobScale;
		this.PlayerCamera.transform.localPosition = this._cameraBaseLocalPos + new Vector3(0f, this._bobbingVerticalOffset * cameraBobScale, 0f);
		Quaternion quaternion = Quaternion.Euler(this._xRotation, 0f, 0f);
		Quaternion quaternion2 = Quaternion.Euler(this._bobbingPitch * cameraBobScale, this._bobbingYaw * cameraBobScale, 0f);
		this.PlayerCamera.transform.localRotation = quaternion * quaternion2;
	}

	// Token: 0x060003A8 RID: 936 RVA: 0x00013024 File Offset: 0x00011224
	private void ShowLookedAtObjectInfo()
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(this.PlayerCamera.transform.position, this.PlayerCamera.transform.forward, out raycastHit, this._interactRange, this.InteractLayerMask))
		{
			AutoMiner componentInParent = raycastHit.collider.GetComponentInParent<AutoMiner>();
			if (!(componentInParent != null))
			{
				this._previouslyLookedAtAutominer = null;
				Singleton<UIManager>.Instance.HideBuildingInfo();
				return;
			}
			if (this._previouslyLookedAtAutominer != componentInParent)
			{
				this._previouslyLookedAtAutominer = componentInParent;
				if (this._previouslyLookedAtAutominer.ResourceDefinition != null)
				{
					Singleton<UIManager>.Instance.ShowBuildingInfo(componentInParent.ResourceDefinition.GetFormattedAvailableResourcesText(this._previouslyLookedAtAutominer.CanProduceGems));
					return;
				}
			}
		}
		else
		{
			this._previouslyLookedAtAutominer = null;
			Singleton<UIManager>.Instance.HideBuildingInfo();
		}
	}

	// Token: 0x060003A9 RID: 937 RVA: 0x000130ED File Offset: 0x000112ED
	public PlayerInputActions GetInputActions()
	{
		return this._input;
	}

	// Token: 0x060003AA RID: 938 RVA: 0x000130F8 File Offset: 0x000112F8
	private void OutlineLookedAtThing()
	{
		if (this._fresnel == null)
		{
			return;
		}
		this._fresnel.ClearAll();
		RaycastHit raycastHit;
		if (Physics.Raycast(this.PlayerCamera.transform.position, this.PlayerCamera.transform.forward, out raycastHit, this._interactRange, this.InteractLayerMask))
		{
			if (raycastHit.collider.GetComponentInParent<BaseHeldTool>() != null)
			{
				this._fresnel.HighlightObject(raycastHit.collider.gameObject, this._fresnel.ToolPreset);
				return;
			}
			if (raycastHit.collider.GetComponentInParent<BuildingCrate>() != null)
			{
				this._fresnel.HighlightObject(raycastHit.collider.gameObject, this._fresnel.ToolPreset);
				return;
			}
			if (raycastHit.collider.GetComponentInParent<ComputerTerminal>() != null)
			{
				this._fresnel.HighlightObject(raycastHit.collider.gameObject, this._fresnel.ToolPreset);
				return;
			}
			if (raycastHit.collider.GetComponentInParent<ContractsTerminal>() != null)
			{
				this._fresnel.HighlightObject(raycastHit.collider.gameObject, this._fresnel.ToolPreset);
				return;
			}
			DetonatorTrigger componentInParent = raycastHit.collider.GetComponentInParent<DetonatorTrigger>();
			if (componentInParent != null && !componentInParent.HasTriggered)
			{
				this._fresnel.HighlightObject(raycastHit.collider.gameObject, this._fresnel.ToolPreset);
				return;
			}
			if (raycastHit.collider.GetComponentInParent<DetonatorBuySign>() != null)
			{
				this._fresnel.HighlightObject(raycastHit.collider.gameObject, this._fresnel.ToolPreset);
				return;
			}
			if (raycastHit.collider.CompareTag("Grabbable"))
			{
				if (this.Inventory.ActiveTool is ToolMagnet)
				{
					OrePiece componentInParent2 = raycastHit.collider.GetComponentInParent<OrePiece>();
					if (componentInParent2 != null && componentInParent2.CurrentMagnetTool != null)
					{
						return;
					}
				}
				this._fresnel.HighlightObject(raycastHit.collider.gameObject, this._fresnel.GenericGrabbablePreset);
				return;
			}
		}
		float num = 3f;
		RaycastHit raycastHit2;
		if (Physics.Raycast(this.PlayerCamera.transform.position, this.PlayerCamera.transform.forward, out raycastHit2, num, this.BuildingFresnelHighlightMask))
		{
			if (this.Inventory.ActiveTool is ToolHammer)
			{
				BuildingObject componentInParent3 = raycastHit2.collider.GetComponentInParent<BuildingObject>();
				if (componentInParent3 != null)
				{
					this._fresnel.HighlightObject(componentInParent3.gameObject, this._fresnel.BuildingPreset);
					return;
				}
			}
			if (this.Inventory.ActiveTool is ToolSupportsWrench)
			{
				BuildingObject componentInParent4 = raycastHit2.collider.GetComponentInParent<BuildingObject>();
				if (componentInParent4 != null)
				{
					if (!componentInParent4.CanHaveBuildingSupports())
					{
						return;
					}
					if (componentInParent4.BuildingSupportsEnabled)
					{
						this._fresnel.HighlightObject(componentInParent4.gameObject, this._fresnel.WrenchDisableSupports);
						return;
					}
					this._fresnel.HighlightObject(componentInParent4.gameObject, this._fresnel.WrenchEnableSupports);
					return;
				}
			}
		}
	}

	// Token: 0x060003AB RID: 939 RVA: 0x00013420 File Offset: 0x00011620
	private void TryInteract()
	{
		if (Singleton<UIManager>.Instance.IsInAnyMenu() || this.HeldObject != null || this._grabJoint != null)
		{
			return;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(this.PlayerCamera.transform.position, this.PlayerCamera.transform.forward, out raycastHit, this._interactRange, this.InteractLayerMask))
		{
			this.InteractionWheelUI.ClearInteractionWheel();
			List<IInteractable> list = new List<IInteractable>();
			list.AddRange(raycastHit.collider.GetComponentsInParent<IInteractable>());
			if (list.Count == 1 && !list[0].ShouldUseInteractionWheel())
			{
				list[0].Interact(list[0].GetInteractions().FirstOrDefault<Interaction>());
				return;
			}
			if (list.Count > 0)
			{
				this.InteractionWheelUI.gameObject.SetActive(true);
				foreach (IInteractable interactable in list)
				{
					this.InteractionWheelUI.PopulateInteractionWheel(interactable);
				}
			}
		}
	}

	// Token: 0x060003AC RID: 940 RVA: 0x0001354C File Offset: 0x0001174C
	private void TryGrabObject()
	{
		if (Singleton<UIManager>.Instance.IsInAnyMenu())
		{
			return;
		}
		if (this.HeldObject != null || this._grabJoint != null)
		{
			this.ReleaseObject();
			return;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(this.PlayerCamera.transform.position, this.PlayerCamera.transform.forward, out raycastHit, this._interactRange, this.InteractLayerMask) && raycastHit.collider.gameObject.CompareTag("Grabbable"))
		{
			this.GrabObject(raycastHit);
		}
	}

	// Token: 0x060003AD RID: 941 RVA: 0x000135E4 File Offset: 0x000117E4
	private void GrabObject(RaycastHit hit)
	{
		this.HeldObject = hit.collider.gameObject;
		Rigidbody component = this.HeldObject.GetComponent<Rigidbody>();
		if (this._grabJoint == null)
		{
			this.RigidbodyDragger.SetActive(true);
			this.RigidbodyDragger.transform.parent = this.HoldPosition;
			this._grabJoint = this.RigidbodyDragger.AddComponent<SpringJoint>();
			this.RigidbodyDragger.GetComponent<Rigidbody>().isKinematic = true;
		}
		component.isKinematic = false;
		PhysicsUtils.IgnoreAllCollisions(this.HeldObject.gameObject, base.gameObject, true);
		component.interpolation = RigidbodyInterpolation.Interpolate;
		this._grabJoint.breakForce = 120f;
		this._grabJoint.breakTorque = 20f;
		this._grabJoint.transform.position = hit.point;
		this._grabJoint.anchor = Vector3.zero;
		this._grabJoint.spring = 100f;
		this._grabJoint.damper = 25f;
		this._grabJoint.maxDistance = 0f;
		this._grabJoint.connectedBody = component;
		this._grabJoint.gameObject.transform.position = this.HoldPosition.position;
		this.RopeRenderer.positionCount = 2;
		this.RopeRenderer.enabled = true;
		this._grabOriginalDrag = component.linearDamping;
		this._grabOriginalAngularDrag = component.angularDamping;
		component.linearDamping = 2.5f;
		component.angularDamping = 0.3f;
	}

	// Token: 0x060003AE RID: 942 RVA: 0x00013770 File Offset: 0x00011970
	public void ReleaseObject()
	{
		if (Singleton<UIManager>.Instance.IsInAnyMenu())
		{
			return;
		}
		if (this._grabJoint != null)
		{
			this._grabJoint.connectedBody = null;
			Object.Destroy(this._grabJoint);
			this._grabJoint = null;
			this.RigidbodyDragger.SetActive(false);
			if (this.HeldObject != null)
			{
				Rigidbody component = this.HeldObject.GetComponent<Rigidbody>();
				component.linearDamping = this._grabOriginalDrag;
				component.angularDamping = this._grabOriginalAngularDrag;
				PhysicsUtils.IgnoreAllCollisions(this.HeldObject.gameObject, base.gameObject, false);
				base.StartCoroutine(this.WaitThenDisableDroppedObjectInterpolation(component));
			}
		}
		this.HeldObject = null;
		this.RopeRenderer.enabled = false;
	}

	// Token: 0x060003AF RID: 943 RVA: 0x0001382F File Offset: 0x00011A2F
	private IEnumerator WaitThenDisableDroppedObjectInterpolation(Rigidbody body)
	{
		yield return new WaitForSeconds(3f);
		if (body != null)
		{
			if (this.HeldObject != null && this.HeldObject.GetComponent<Rigidbody>() == body)
			{
				yield break;
			}
			OrePiece component = body.GetComponent<OrePiece>();
			if (component != null && component.CurrentMagnetTool != null)
			{
				yield break;
			}
			body.interpolation = RigidbodyInterpolation.None;
		}
		yield break;
	}

	// Token: 0x060003B0 RID: 944 RVA: 0x00013848 File Offset: 0x00011A48
	public void RespawnPlayer()
	{
		Debug.Log("Respawned player");
		this.TeleportPlayer(PlayerSpawnPoint.GetRandomPlayerSpawnPoint().transform.position);
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.PlayerRespawnSound, base.transform.position, 1f, 1f, true, false);
	}

	// Token: 0x060003B1 RID: 945 RVA: 0x0001389C File Offset: 0x00011A9C
	public void TeleportPlayer(Vector3 position)
	{
		this.IsInWater = false;
		if (this.CharacterController != null)
		{
			bool enabled = this.CharacterController.enabled;
			this.CharacterController.enabled = false;
			base.transform.position = position;
			this.CharacterController.enabled = enabled;
			return;
		}
		base.transform.position = position;
	}

	// Token: 0x060003B2 RID: 946 RVA: 0x000138FB File Offset: 0x00011AFB
	public void TeleportPlayer(Vector3 position, Vector3 rotation)
	{
		this.TeleportPlayer(position);
		base.transform.rotation = Quaternion.Euler(rotation);
	}

	// Token: 0x060003B3 RID: 947 RVA: 0x00013918 File Offset: 0x00011B18
	private void OnDestroy()
	{
		this._input.Player.Disable();
		this._input.Dispose();
	}

	// Token: 0x04000395 RID: 917
	public float WalkSpeed = 4f;

	// Token: 0x04000396 RID: 918
	public float SprintSpeed = 6f;

	// Token: 0x04000397 RID: 919
	public float JumpHeight = 2f;

	// Token: 0x04000398 RID: 920
	public float Gravity = -9.81f;

	// Token: 0x04000399 RID: 921
	public float SlideSpeed = 8f;

	// Token: 0x0400039A RID: 922
	public float StandingSlopeLimit = 60f;

	// Token: 0x0400039B RID: 923
	public LayerMask GroundLayer;

	// Token: 0x0400039C RID: 924
	public LayerMask InteractLayerMask;

	// Token: 0x0400039D RID: 925
	public LayerMask BuildingFresnelHighlightMask;

	// Token: 0x0400039E RID: 926
	public Transform GroundCheck;

	// Token: 0x0400039F RID: 927
	public Camera PlayerCamera;

	// Token: 0x040003A0 RID: 928
	public Transform ViewModelContainer;

	// Token: 0x040003A1 RID: 929
	public Transform HoldPosition;

	// Token: 0x040003A2 RID: 930
	public Transform MagnetToolPosition;

	// Token: 0x040003A3 RID: 931
	public GameObject HeldObject;

	// Token: 0x040003A4 RID: 932
	public GameObject RigidbodyDragger;

	// Token: 0x040003A5 RID: 933
	public LineRenderer RopeRenderer;

	// Token: 0x040003A6 RID: 934
	public Transform CharacterModel;

	// Token: 0x040003A7 RID: 935
	public CharacterController CharacterController;

	// Token: 0x040003A8 RID: 936
	public bool IsInWater;

	// Token: 0x040003A9 RID: 937
	public bool IsUsingNoclip;

	// Token: 0x040003AA RID: 938
	[SerializeField]
	private GameObject _miningHatLight;

	// Token: 0x040003AB RID: 939
	[SerializeField]
	private GameObject _nightVisionLight;

	// Token: 0x040003AC RID: 940
	private float _interactRange = 2f;

	// Token: 0x040003AD RID: 941
	private Vector3 _velocity;

	// Token: 0x040003AE RID: 942
	private float _xRotation;

	// Token: 0x040003AF RID: 943
	private bool _isGrounded;

	// Token: 0x040003B0 RID: 944
	private SpringJoint _grabJoint;

	// Token: 0x040003B1 RID: 945
	private float _grabOriginalDrag;

	// Token: 0x040003B2 RID: 946
	private float _grabOriginalAngularDrag;

	// Token: 0x040003B5 RID: 949
	public float DuckHeight = 1f;

	// Token: 0x040003B6 RID: 950
	public float StandingHeight = 2f;

	// Token: 0x040003B7 RID: 951
	public float DuckingSpeed = 10f;

	// Token: 0x040003B8 RID: 952
	public float DuckSpeed = 2f;

	// Token: 0x040003B9 RID: 953
	private bool _isDucking;

	// Token: 0x040003BA RID: 954
	private bool _toggleDuckIsToggled;

	// Token: 0x040003BB RID: 955
	private float _cameraHeightVelocity;

	// Token: 0x040003BC RID: 956
	public float BaseBobbingSpeed = 14f;

	// Token: 0x040003BD RID: 957
	public float BaseBobbingAmount = 0.05f;

	// Token: 0x040003BE RID: 958
	public float BaseBobbingPitchAmount = 1f;

	// Token: 0x040003BF RID: 959
	public float BaseBobbingYawAmount = 1f;

	// Token: 0x040003C0 RID: 960
	private float _bobbingCounter;

	// Token: 0x040003C1 RID: 961
	private float _bobbingPitch;

	// Token: 0x040003C2 RID: 962
	private float _bobbingYaw;

	// Token: 0x040003C3 RID: 963
	private float _yawDirectionMultiplier = 1f;

	// Token: 0x040003C4 RID: 964
	private float _bobbingVerticalOffset;

	// Token: 0x040003C5 RID: 965
	private float _bobbingVerticalVelocity;

	// Token: 0x040003C6 RID: 966
	private float _bobbingYawVelocity;

	// Token: 0x040003C7 RID: 967
	private float _bobbingPitchVelocity;

	// Token: 0x040003C8 RID: 968
	private Vector3 _cameraBaseLocalPos;

	// Token: 0x040003C9 RID: 969
	private float _viewBobPitch;

	// Token: 0x040003CA RID: 970
	private float _viewBobYaw;

	// Token: 0x040003CB RID: 971
	private float _viewBobVertical;

	// Token: 0x040003CC RID: 972
	private float _viewBobPitchVel;

	// Token: 0x040003CD RID: 973
	private float _viewBobYawVel;

	// Token: 0x040003CE RID: 974
	private float _viewBobVerticalVel;

	// Token: 0x040003CF RID: 975
	private float _viewBobCounter;

	// Token: 0x040003D0 RID: 976
	private int _viewBobYawDirection = 1;

	// Token: 0x040003D1 RID: 977
	private float _smoothedYawSway;

	// Token: 0x040003D2 RID: 978
	private float _smoothedPitchSway;

	// Token: 0x040003D3 RID: 979
	private float _yawSwayVelocity;

	// Token: 0x040003D4 RID: 980
	private float _pitchSwayVelocity;

	// Token: 0x040003D5 RID: 981
	public float ViewModelBobSpeed = 8f;

	// Token: 0x040003D6 RID: 982
	public float ViewModelBobAmount = 0.05f;

	// Token: 0x040003D7 RID: 983
	public float ViewModelBobPitchAmount = 1.5f;

	// Token: 0x040003D8 RID: 984
	public float ViewModelBobYawAmount = 1.5f;

	// Token: 0x040003D9 RID: 985
	public float ViewModelLookSwayAmount = 0.05f;

	// Token: 0x040003DA RID: 986
	public float ViewModelLookSwayMax = 2f;

	// Token: 0x040003DB RID: 987
	public Vector3 ViewModelBasePos;

	// Token: 0x040003DC RID: 988
	public Vector3 ViewModelBaseRotEuler;

	// Token: 0x040003DD RID: 989
	public float JumpBounceAmount = -0.12f;

	// Token: 0x040003DE RID: 990
	public float LandBounceAmount = 0.08f;

	// Token: 0x040003DF RID: 991
	public float JumpSmoothTime = 0.2f;

	// Token: 0x040003E0 RID: 992
	private Vector2 _lookDelta;

	// Token: 0x040003E1 RID: 993
	private float _jumpOffset;

	// Token: 0x040003E2 RID: 994
	private float _jumpVelocity;

	// Token: 0x040003E3 RID: 995
	private bool _wasGroundedLastFrame;

	// Token: 0x040003E4 RID: 996
	private float _jumpTargetOffset;

	// Token: 0x040003E5 RID: 997
	private float _currentFOV;

	// Token: 0x040003E6 RID: 998
	private float _fovVelocity;

	// Token: 0x040003E7 RID: 999
	private bool _miningLightEnabled;

	// Token: 0x040003E8 RID: 1000
	public InteractionWheelUI InteractionWheelUI;

	// Token: 0x040003E9 RID: 1001
	public SoundDefinition PlayerRespawnSound;

	// Token: 0x040003EA RID: 1002
	private PlayerInputActions _input;

	// Token: 0x040003EC RID: 1004
	private AutoMiner _previouslyLookedAtAutominer;

	// Token: 0x040003ED RID: 1005
	private FresnelHighlighter _fresnel;
}
