using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	public float WalkSpeed = 4f;

	public float SprintSpeed = 6f;

	public float JumpHeight = 2f;

	public float Gravity = -9.81f;

	public float SlideSpeed = 8f;

	public float StandingSlopeLimit = 60f;

	public LayerMask GroundLayer;

	public LayerMask InteractLayerMask;

	public LayerMask BuildingFresnelHighlightMask;

	public Transform GroundCheck;

	public Camera PlayerCamera;

	public Transform ViewModelContainer;

	public Transform HoldPosition;

	public Transform MagnetToolPosition;

	public GameObject HeldObject;

	public GameObject RigidbodyDragger;

	public LineRenderer RopeRenderer;

	public Transform CharacterModel;

	public CharacterController CharacterController;

	public bool IsInWater;

	public bool IsUsingNoclip;

	[SerializeField]
	private GameObject _miningHatLight;

	[SerializeField]
	private GameObject _nightVisionLight;

	private float _interactRange = 2f;

	private Vector3 _velocity;

	private float _xRotation;

	private bool _isGrounded;

	private SpringJoint _grabJoint;

	private float _grabOriginalDrag;

	private float _grabOriginalAngularDrag;

	public float DuckHeight = 1f;

	public float StandingHeight = 2f;

	public float DuckingSpeed = 10f;

	public float DuckSpeed = 2f;

	private bool _isDucking;

	private bool _toggleDuckIsToggled;

	private float _cameraHeightVelocity;

	public float BaseBobbingSpeed = 14f;

	public float BaseBobbingAmount = 0.05f;

	public float BaseBobbingPitchAmount = 1f;

	public float BaseBobbingYawAmount = 1f;

	private float _bobbingCounter;

	private float _bobbingPitch;

	private float _bobbingYaw;

	private float _yawDirectionMultiplier = 1f;

	private float _bobbingVerticalOffset;

	private float _bobbingVerticalVelocity;

	private float _bobbingYawVelocity;

	private float _bobbingPitchVelocity;

	private Vector3 _cameraBaseLocalPos;

	private float _viewBobPitch;

	private float _viewBobYaw;

	private float _viewBobVertical;

	private float _viewBobPitchVel;

	private float _viewBobYawVel;

	private float _viewBobVerticalVel;

	private float _viewBobCounter;

	private int _viewBobYawDirection = 1;

	private float _smoothedYawSway;

	private float _smoothedPitchSway;

	private float _yawSwayVelocity;

	private float _pitchSwayVelocity;

	public float ViewModelBobSpeed = 8f;

	public float ViewModelBobAmount = 0.05f;

	public float ViewModelBobPitchAmount = 1.5f;

	public float ViewModelBobYawAmount = 1.5f;

	public float ViewModelLookSwayAmount = 0.05f;

	public float ViewModelLookSwayMax = 2f;

	public Vector3 ViewModelBasePos;

	public Vector3 ViewModelBaseRotEuler;

	public float JumpBounceAmount = -0.12f;

	public float LandBounceAmount = 0.08f;

	public float JumpSmoothTime = 0.2f;

	private Vector2 _lookDelta;

	private float _jumpOffset;

	private float _jumpVelocity;

	private bool _wasGroundedLastFrame;

	private float _jumpTargetOffset;

	private float _currentFOV;

	private float _fovVelocity;

	private bool _miningLightEnabled;

	public InteractionWheelUI InteractionWheelUI;

	public SoundDefinition PlayerRespawnSound;

	private PlayerInputActions _input;

	private AutoMiner _previouslyLookedAtAutominer;

	private FresnelHighlighter _fresnel;

	public float SelectedWalkSpeed { get; private set; }

	public Vector2 MoveInput { get; private set; }

	public PlayerInventory Inventory { get; private set; }

	private void Awake()
	{
		_input = Singleton<KeybindManager>.Instance.Input;
		_input.Player.Enable();
		Inventory = GetComponent<PlayerInventory>();
		if (_fresnel == null)
		{
			_fresnel = UnityEngine.Object.FindObjectOfType<FresnelHighlighter>();
		}
	}

	private void OnDisable()
	{
		if ((bool)_fresnel)
		{
			_fresnel.ClearAll();
		}
	}

	private void Start()
	{
		CharacterController = GetComponent<CharacterController>();
		SelectedWalkSpeed = WalkSpeed;
		_currentFOV = GetDesiredFOV();
		PlayerCamera.fieldOfView = _currentFOV;
		_cameraBaseLocalPos = PlayerCamera.transform.localPosition;
		InteractionWheelUI = Singleton<UIManager>.Instance.InteractionWheelUI;
	}

	public float GetDesiredFOV()
	{
		return Singleton<SettingsManager>.Instance.DesiredFOV;
	}

	private void Update()
	{
		if (Singleton<UIManager>.Instance.IsInEditTextPopup())
		{
			_input.Player.Disable();
		}
		else
		{
			_input.Player.Enable();
			if (Singleton<GamemodeManager>.Instance.ShouldAllowNoclip() && _input.Player.Noclip.WasPressedThisFrame())
			{
				ToggleNoclip();
			}
		}
		HandleDucking();
		_isGrounded = !IsUsingNoclip && CharacterController.isGrounded;
		if (!Cursor.visible)
		{
			_lookDelta = _input.Player.Look.ReadValue<Vector2>();
			float num = _lookDelta.x * Singleton<SettingsManager>.Instance.MouseSensitivity * (float)((!Singleton<SettingsManager>.Instance.InvertMouseX) ? 1 : (-1));
			float num2 = _lookDelta.y * Singleton<SettingsManager>.Instance.MouseSensitivity * (float)((!Singleton<SettingsManager>.Instance.InvertMouseY) ? 1 : (-1));
			_xRotation -= num2;
			_xRotation = Mathf.Clamp(_xRotation, -88f, 88f);
			base.transform.Rotate(Vector3.up * num);
		}
		MoveInput = _input.Player.Move.ReadValue<Vector2>();
		Vector3 vector = base.transform.right * MoveInput.x + base.transform.forward * MoveInput.y;
		bool flag = _input.Player.Sprint.IsPressed() && !_isDucking && _isGrounded;
		SelectedWalkSpeed = (flag ? SprintSpeed : (_isDucking ? DuckSpeed : WalkSpeed));
		if (_input.Player.ToggleFlashlight.WasPressedThisFrame())
		{
			ToggleMiningLightFromKeybind(!_miningLightEnabled);
		}
		float num3 = (flag ? (GetDesiredFOV() * 1.05f) : GetDesiredFOV());
		if (Time.timeScale == 0f)
		{
			_currentFOV = num3;
		}
		else
		{
			_currentFOV = Mathf.SmoothDamp(_currentFOV, num3, ref _fovVelocity, 0.1f);
		}
		PlayerCamera.fieldOfView = _currentFOV;
		if (IsUsingNoclip)
		{
			HandleNoclipMovement();
		}
		else
		{
			CharacterController.Move(vector * SelectedWalkSpeed * Time.deltaTime);
		}
		if (_isGrounded)
		{
			float a = 0f;
			Vector3 zero = Vector3.zero;
			int num4 = 0;
			int num5 = 6;
			Vector3 position = base.transform.position;
			float num6 = CharacterController.radius * 0.98f;
			float num7 = CharacterController.height / 2f + 0.2f;
			for (int i = 0; i < num5; i++)
			{
				float f = (float)i * MathF.PI * 2f / (float)num5;
				Vector3 vector2 = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f)) * num6;
				Vector3 vector3 = position + vector2;
				Debug.DrawRay(vector3, Vector3.down * num7, Color.red);
				if (Physics.Raycast(vector3, Vector3.down, out var hitInfo, num7, GroundLayer))
				{
					float num8 = Vector3.Angle(hitInfo.normal, Vector3.up);
					if (num8 > StandingSlopeLimit)
					{
						Vector3 vector4 = new Vector3(hitInfo.normal.x, 0f - hitInfo.normal.y, hitInfo.normal.z);
						zero += vector4;
						a = Mathf.Max(a, num8);
						num4++;
					}
				}
				else
				{
					num4++;
				}
			}
			if (num4 == num5)
			{
				_velocity += zero.normalized * SlideSpeed * Time.deltaTime;
			}
			else if (_velocity.y < 0f)
			{
				_velocity.y = -2f;
				_velocity.x = 0f;
				_velocity.z = 0f;
			}
		}
		if (_input.Player.Jump.triggered && _isGrounded)
		{
			_velocity.y = Mathf.Sqrt(JumpHeight * -2f * Gravity);
			_velocity.x = 0f;
			_velocity.z = 0f;
		}
		_velocity.y += Gravity * Time.deltaTime;
		if (IsUsingNoclip)
		{
			_velocity.y = 0f;
		}
		if ((((!IsUsingNoclip) ? CharacterController.Move(_velocity * Time.deltaTime) : CollisionFlags.None) & CollisionFlags.Above) != CollisionFlags.None && _velocity.y > 0f)
		{
			_velocity.y = 0f;
		}
		ShowLookedAtObjectInfo();
		OutlineLookedAtThing();
		if (_input.Player.Interact.triggered)
		{
			TryInteract();
		}
		if (_input.Player.Grab.triggered)
		{
			TryGrabObject();
		}
		if (HeldObject != null && !HeldObject.activeInHierarchy)
		{
			ReleaseObject();
		}
		if (_grabJoint != null && HeldObject != null)
		{
			RopeRenderer.SetPosition(0, RigidbodyDragger.transform.position);
			Vector3 position2 = _grabJoint.connectedBody.transform.TransformPoint(_grabJoint.connectedAnchor);
			RopeRenderer.SetPosition(1, position2);
			RopeRenderer.enabled = true;
		}
		else
		{
			RopeRenderer.enabled = false;
		}
		HandleCameraBobbing();
		HandleViewModelBobbing();
		if (base.transform.position.y <= -200f)
		{
			RespawnPlayer();
		}
	}

	private void ToggleNoclip()
	{
		IsUsingNoclip = !IsUsingNoclip;
		CharacterController.enabled = !IsUsingNoclip;
	}

	private void HandleNoclipMovement()
	{
		Vector3 forward = PlayerCamera.transform.forward;
		Vector3 right = PlayerCamera.transform.right;
		Vector3 vector = forward * MoveInput.y + right * MoveInput.x;
		if (vector.sqrMagnitude > 1f)
		{
			vector.Normalize();
		}
		bool flag = _input.Player.Sprint.IsPressed() && !_isDucking;
		SelectedWalkSpeed = (flag ? (SprintSpeed * 4f) : (WalkSpeed * 2f));
		float num = 0f;
		if (_input.Player.Jump.IsPressed())
		{
			num += 1f;
		}
		if (_input.Player.Duck.IsPressed())
		{
			num -= 1f;
		}
		Vector3 vector2 = Vector3.up * num * SelectedWalkSpeed;
		Vector3 vector3 = ((vector.sqrMagnitude > 1f) ? vector.normalized : vector);
		CharacterController.transform.position += (vector3 * SelectedWalkSpeed + vector2) * Time.deltaTime;
	}

	public void ToggleMiningLightFromKeybind(bool enable)
	{
		bool flag = Inventory.ActiveTool is ToolMiningHat toolMiningHat && toolMiningHat.gameObject.activeSelf;
		bool flag2 = false;
		foreach (BaseHeldTool item in Inventory.Items)
		{
			if (item is ToolMiningHat toolMiningHat2)
			{
				toolMiningHat2.ToggleLight(enable, playSound: true, updateOnOwner: false);
				flag2 = true;
				break;
			}
		}
		if (!flag2)
		{
			enable = false;
		}
		_miningLightEnabled = enable;
		if (!flag)
		{
			_nightVisionLight.gameObject.SetActive(!_miningLightEnabled);
			_miningHatLight.gameObject.SetActive(_miningLightEnabled);
		}
		else
		{
			_nightVisionLight.gameObject.SetActive(!_miningLightEnabled);
			_miningHatLight.gameObject.SetActive(value: false);
		}
	}

	public void ToggleMiningLightFromTool(bool enable)
	{
		bool active = enable;
		if (enable)
		{
			if (Inventory.ActiveTool is ToolMiningHat toolMiningHat && toolMiningHat.gameObject.activeSelf)
			{
				active = false;
			}
			else
			{
				bool flag = false;
				foreach (BaseHeldTool item in Inventory.Items)
				{
					if (item is ToolMiningHat)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					enable = false;
					active = false;
				}
			}
		}
		_miningLightEnabled = enable;
		_nightVisionLight.gameObject.SetActive(!_miningLightEnabled);
		_miningHatLight.gameObject.SetActive(active);
	}

	private void HandleDucking()
	{
		bool flag;
		if (Singleton<SettingsManager>.Instance.ToggleDucking)
		{
			if (_input.Player.Duck.WasPressedThisFrame())
			{
				_toggleDuckIsToggled = !_toggleDuckIsToggled;
			}
			flag = _toggleDuckIsToggled;
		}
		else
		{
			flag = _input.Player.Duck.IsPressed();
		}
		bool flag2 = true;
		if (!flag && _isDucking)
		{
			float num = StandingHeight - DuckHeight;
			Vector3 center = base.transform.position + CharacterController.center + Vector3.up * (num / 2f);
			Vector3 halfExtents = new Vector3(CharacterController.radius * 0.95f, num / 2f, CharacterController.radius * 0.95f);
			flag2 = !Physics.CheckBox(center, halfExtents, Quaternion.identity, GroundLayer, QueryTriggerInteraction.Ignore);
		}
		if (flag || (_isDucking && !flag2))
		{
			_isDucking = true;
		}
		else if (flag2)
		{
			_isDucking = false;
		}
		float b = (_isDucking ? DuckHeight : StandingHeight);
		float num2 = Mathf.Lerp(CharacterController.height, b, Time.deltaTime * DuckingSpeed);
		CharacterController.height = num2;
		float target = num2 / 2f - 0.5f;
		float num3 = Mathf.SmoothDamp(PlayerCamera.transform.localPosition.y, target, ref _cameraHeightVelocity, 0.1f);
		if (float.IsNaN(num3))
		{
			num3 = 0f;
		}
		PlayerCamera.transform.localPosition = new Vector3(PlayerCamera.transform.localPosition.x, num3, PlayerCamera.transform.localPosition.z);
		float y = num2 / StandingHeight;
		CharacterModel.localScale = new Vector3(1f, y, 1f);
	}

	private void HandleViewModelBobbing()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		bool flag = _input.Player.Move.ReadValue<Vector2>().sqrMagnitude > 0.01f;
		Vector3 viewModelBasePos = ViewModelBasePos;
		bool num = _wasGroundedLastFrame && !_isGrounded;
		bool flag2 = !_wasGroundedLastFrame && _isGrounded;
		if (num)
		{
			_jumpTargetOffset = JumpBounceAmount;
		}
		else if (flag2)
		{
			_jumpTargetOffset = LandBounceAmount;
		}
		_jumpOffset = Mathf.SmoothDamp(_jumpOffset, _jumpTargetOffset, ref _jumpVelocity, JumpSmoothTime);
		_jumpTargetOffset = Mathf.MoveTowards(_jumpTargetOffset, 0f, Time.deltaTime * Mathf.Abs(_jumpTargetOffset / JumpSmoothTime));
		_wasGroundedLastFrame = _isGrounded;
		if (!_isGrounded)
		{
			_viewBobPitch = Mathf.SmoothDamp(_viewBobPitch, 0f, ref _viewBobPitchVel, 0.2f);
			_viewBobYaw = Mathf.SmoothDamp(_viewBobYaw, 0f, ref _viewBobYawVel, 0.2f);
			_viewBobVertical = Mathf.SmoothDamp(_viewBobVertical, 0f, ref _viewBobVerticalVel, 0.2f);
		}
		else if (!flag)
		{
			_viewBobPitch = Mathf.SmoothDamp(_viewBobPitch, 0f, ref _viewBobPitchVel, 0.1f);
			_viewBobYaw = Mathf.SmoothDamp(_viewBobYaw, 0f, ref _viewBobYawVel, 0.1f);
			_viewBobVertical = Mathf.SmoothDamp(_viewBobVertical, 0f, ref _viewBobVerticalVel, 0.1f);
		}
		else
		{
			float num2 = SelectedWalkSpeed / Mathf.Max(WalkSpeed, 0.01f);
			float num3 = ViewModelBobSpeed * num2;
			_viewBobCounter += Time.deltaTime * num3;
			if (_viewBobCounter > MathF.PI * 2f)
			{
				_viewBobCounter -= MathF.PI * 2f;
				_viewBobYawDirection *= -1;
			}
			float num4 = Mathf.Sin(_viewBobCounter);
			float target = ViewModelBobAmount * num4 * num2;
			_viewBobVertical = Mathf.SmoothDamp(_viewBobVertical, target, ref _viewBobVerticalVel, 0.05f);
			_viewBobPitch = Mathf.SmoothDamp(_viewBobPitch, num4 * ViewModelBobPitchAmount * num2, ref _viewBobPitchVel, 0.05f);
			_viewBobYaw = Mathf.SmoothDamp(_viewBobYaw, num4 * ViewModelBobYawAmount * num2 * (float)_viewBobYawDirection, ref _viewBobYawVel, 0.05f);
		}
		float viewmodelBobScale = Singleton<SettingsManager>.Instance.ViewmodelBobScale;
		if (float.IsNaN(_viewBobVertical))
		{
			_viewBobVertical = 0f;
		}
		if (float.IsNaN(_jumpOffset))
		{
			_jumpOffset = 0f;
		}
		ViewModelContainer.localPosition = viewModelBasePos + new Vector3(0f, (_viewBobVertical + _jumpOffset) * viewmodelBobScale, 0f);
		float target2 = Mathf.Clamp(_lookDelta.x * ViewModelLookSwayAmount, 0f - ViewModelLookSwayMax, ViewModelLookSwayMax);
		float target3 = Mathf.Clamp((0f - _lookDelta.y) * ViewModelLookSwayAmount, 0f - ViewModelLookSwayMax, ViewModelLookSwayMax);
		if (float.IsNaN(_yawSwayVelocity))
		{
			_yawSwayVelocity = 0f;
		}
		_smoothedYawSway = Mathf.SmoothDamp(_smoothedYawSway, target2, ref _yawSwayVelocity, 0.06f);
		if (float.IsNaN(_pitchSwayVelocity))
		{
			_pitchSwayVelocity = 0f;
		}
		_smoothedPitchSway = Mathf.SmoothDamp(_smoothedPitchSway, target3, ref _pitchSwayVelocity, 0.06f);
		Quaternion quaternion = Quaternion.Euler((_viewBobPitch + _smoothedPitchSway) * viewmodelBobScale, (_viewBobYaw + _smoothedYawSway) * viewmodelBobScale, 0f);
		ViewModelContainer.localRotation = Quaternion.Euler(ViewModelBaseRotEuler) * quaternion;
	}

	private void HandleCameraBobbing()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		bool flag = _input.Player.Move.ReadValue<Vector2>().sqrMagnitude > 0.01f;
		_cameraBaseLocalPos = new Vector3(PlayerCamera.transform.localPosition.x, CharacterController.height / 2f - 0.5f, PlayerCamera.transform.localPosition.z);
		if (!_isGrounded)
		{
			_bobbingPitch = Mathf.SmoothDamp(_bobbingPitch, 0f, ref _bobbingPitchVelocity, 0.2f);
			_bobbingYaw = Mathf.SmoothDamp(_bobbingYaw, 0f, ref _bobbingYawVelocity, 0.2f);
			_bobbingVerticalOffset = Mathf.SmoothDamp(_bobbingVerticalOffset, 0f, ref _bobbingVerticalVelocity, 0.2f);
		}
		else if (!flag)
		{
			_bobbingPitch = Mathf.SmoothDamp(_bobbingPitch, 0f, ref _bobbingPitchVelocity, 0.1f);
			_bobbingYaw = Mathf.SmoothDamp(_bobbingYaw, 0f, ref _bobbingYawVelocity, 0.1f);
			_bobbingVerticalOffset = Mathf.SmoothDamp(_bobbingVerticalOffset, 0f, ref _bobbingVerticalVelocity, 0.1f);
		}
		else
		{
			float num = SelectedWalkSpeed / Mathf.Max(WalkSpeed, 0.01f);
			float num2 = BaseBobbingSpeed * num;
			_bobbingCounter += Time.deltaTime * num2;
			if (_bobbingCounter > MathF.PI * 2f)
			{
				_bobbingCounter -= MathF.PI * 2f;
				_yawDirectionMultiplier *= -1f;
			}
			float num3 = Mathf.Sin(_bobbingCounter);
			float target = BaseBobbingAmount * num3 * num;
			_bobbingVerticalOffset = Mathf.SmoothDamp(_bobbingVerticalOffset, target, ref _bobbingVerticalVelocity, 0.05f);
			_bobbingPitch = Mathf.SmoothDamp(_bobbingPitch, num3 * BaseBobbingPitchAmount * num, ref _bobbingPitchVelocity, 0.05f);
			_bobbingYaw = Mathf.SmoothDamp(_bobbingYaw, num3 * BaseBobbingYawAmount * num * _yawDirectionMultiplier, ref _bobbingYawVelocity, 0.05f);
		}
		float cameraBobScale = Singleton<SettingsManager>.Instance.CameraBobScale;
		PlayerCamera.transform.localPosition = _cameraBaseLocalPos + new Vector3(0f, _bobbingVerticalOffset * cameraBobScale, 0f);
		Quaternion quaternion = Quaternion.Euler(_xRotation, 0f, 0f);
		Quaternion quaternion2 = Quaternion.Euler(_bobbingPitch * cameraBobScale, _bobbingYaw * cameraBobScale, 0f);
		PlayerCamera.transform.localRotation = quaternion * quaternion2;
	}

	private void ShowLookedAtObjectInfo()
	{
		if (Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out var hitInfo, _interactRange, InteractLayerMask))
		{
			AutoMiner componentInParent = hitInfo.collider.GetComponentInParent<AutoMiner>();
			if (componentInParent != null)
			{
				if (_previouslyLookedAtAutominer != componentInParent)
				{
					_previouslyLookedAtAutominer = componentInParent;
					if (_previouslyLookedAtAutominer.ResourceDefinition != null)
					{
						Singleton<UIManager>.Instance.ShowBuildingInfo(componentInParent.ResourceDefinition.GetFormattedAvailableResourcesText(_previouslyLookedAtAutominer.CanProduceGems));
					}
				}
			}
			else
			{
				_previouslyLookedAtAutominer = null;
				Singleton<UIManager>.Instance.HideBuildingInfo();
			}
		}
		else
		{
			_previouslyLookedAtAutominer = null;
			Singleton<UIManager>.Instance.HideBuildingInfo();
		}
	}

	public PlayerInputActions GetInputActions()
	{
		return _input;
	}

	private void OutlineLookedAtThing()
	{
		if (_fresnel == null)
		{
			return;
		}
		_fresnel.ClearAll();
		if (Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out var hitInfo, _interactRange, InteractLayerMask))
		{
			if (hitInfo.collider.GetComponentInParent<BaseHeldTool>() != null)
			{
				_fresnel.HighlightObject(hitInfo.collider.gameObject, _fresnel.ToolPreset);
				return;
			}
			if (hitInfo.collider.GetComponentInParent<BuildingCrate>() != null)
			{
				_fresnel.HighlightObject(hitInfo.collider.gameObject, _fresnel.ToolPreset);
				return;
			}
			if (hitInfo.collider.GetComponentInParent<ComputerTerminal>() != null)
			{
				_fresnel.HighlightObject(hitInfo.collider.gameObject, _fresnel.ToolPreset);
				return;
			}
			if (hitInfo.collider.GetComponentInParent<ContractsTerminal>() != null)
			{
				_fresnel.HighlightObject(hitInfo.collider.gameObject, _fresnel.ToolPreset);
				return;
			}
			DetonatorTrigger componentInParent = hitInfo.collider.GetComponentInParent<DetonatorTrigger>();
			if (componentInParent != null && !componentInParent.HasTriggered)
			{
				_fresnel.HighlightObject(hitInfo.collider.gameObject, _fresnel.ToolPreset);
				return;
			}
			if (hitInfo.collider.GetComponentInParent<DetonatorBuySign>() != null)
			{
				_fresnel.HighlightObject(hitInfo.collider.gameObject, _fresnel.ToolPreset);
				return;
			}
			if (hitInfo.collider.CompareTag("Grabbable"))
			{
				if (Inventory.ActiveTool is ToolMagnet)
				{
					OrePiece componentInParent2 = hitInfo.collider.GetComponentInParent<OrePiece>();
					if (componentInParent2 != null && componentInParent2.CurrentMagnetTool != null)
					{
						return;
					}
				}
				_fresnel.HighlightObject(hitInfo.collider.gameObject, _fresnel.GenericGrabbablePreset);
				return;
			}
		}
		float maxDistance = 3f;
		if (!Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out var hitInfo2, maxDistance, BuildingFresnelHighlightMask))
		{
			return;
		}
		if (Inventory.ActiveTool is ToolHammer)
		{
			BuildingObject componentInParent3 = hitInfo2.collider.GetComponentInParent<BuildingObject>();
			if (componentInParent3 != null)
			{
				_fresnel.HighlightObject(componentInParent3.gameObject, _fresnel.BuildingPreset);
				return;
			}
		}
		if (!(Inventory.ActiveTool is ToolSupportsWrench))
		{
			return;
		}
		BuildingObject componentInParent4 = hitInfo2.collider.GetComponentInParent<BuildingObject>();
		if (componentInParent4 != null && componentInParent4.CanHaveBuildingSupports())
		{
			if (componentInParent4.BuildingSupportsEnabled)
			{
				_fresnel.HighlightObject(componentInParent4.gameObject, _fresnel.WrenchDisableSupports);
			}
			else
			{
				_fresnel.HighlightObject(componentInParent4.gameObject, _fresnel.WrenchEnableSupports);
			}
		}
	}

	private void TryInteract()
	{
		if (Singleton<UIManager>.Instance.IsInAnyMenu() || HeldObject != null || _grabJoint != null || !Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out var hitInfo, _interactRange, InteractLayerMask))
		{
			return;
		}
		InteractionWheelUI.ClearInteractionWheel();
		List<IInteractable> list = new List<IInteractable>();
		list.AddRange(hitInfo.collider.GetComponentsInParent<IInteractable>());
		if (list.Count == 1 && !list[0].ShouldUseInteractionWheel())
		{
			list[0].Interact(list[0].GetInteractions().FirstOrDefault());
		}
		else
		{
			if (list.Count <= 0)
			{
				return;
			}
			InteractionWheelUI.gameObject.SetActive(value: true);
			foreach (IInteractable item in list)
			{
				InteractionWheelUI.PopulateInteractionWheel(item);
			}
		}
	}

	private void TryGrabObject()
	{
		if (!Singleton<UIManager>.Instance.IsInAnyMenu())
		{
			RaycastHit hitInfo;
			if (HeldObject != null || _grabJoint != null)
			{
				ReleaseObject();
			}
			else if (Physics.Raycast(PlayerCamera.transform.position, PlayerCamera.transform.forward, out hitInfo, _interactRange, InteractLayerMask) && hitInfo.collider.gameObject.CompareTag("Grabbable"))
			{
				GrabObject(hitInfo);
			}
		}
	}

	private void GrabObject(RaycastHit hit)
	{
		HeldObject = hit.collider.gameObject;
		Rigidbody component = HeldObject.GetComponent<Rigidbody>();
		if (_grabJoint == null)
		{
			RigidbodyDragger.SetActive(value: true);
			RigidbodyDragger.transform.parent = HoldPosition;
			_grabJoint = RigidbodyDragger.AddComponent<SpringJoint>();
			RigidbodyDragger.GetComponent<Rigidbody>().isKinematic = true;
		}
		component.isKinematic = false;
		PhysicsUtils.IgnoreAllCollisions(HeldObject.gameObject, base.gameObject, ignore: true);
		component.interpolation = RigidbodyInterpolation.Interpolate;
		_grabJoint.breakForce = 120f;
		_grabJoint.breakTorque = 20f;
		_grabJoint.transform.position = hit.point;
		_grabJoint.anchor = Vector3.zero;
		_grabJoint.spring = 100f;
		_grabJoint.damper = 25f;
		_grabJoint.maxDistance = 0f;
		_grabJoint.connectedBody = component;
		_grabJoint.gameObject.transform.position = HoldPosition.position;
		RopeRenderer.positionCount = 2;
		RopeRenderer.enabled = true;
		_grabOriginalDrag = component.linearDamping;
		_grabOriginalAngularDrag = component.angularDamping;
		component.linearDamping = 2.5f;
		component.angularDamping = 0.3f;
	}

	public void ReleaseObject()
	{
		if (Singleton<UIManager>.Instance.IsInAnyMenu())
		{
			return;
		}
		if (_grabJoint != null)
		{
			_grabJoint.connectedBody = null;
			UnityEngine.Object.Destroy(_grabJoint);
			_grabJoint = null;
			RigidbodyDragger.SetActive(value: false);
			if (HeldObject != null)
			{
				Rigidbody component = HeldObject.GetComponent<Rigidbody>();
				component.linearDamping = _grabOriginalDrag;
				component.angularDamping = _grabOriginalAngularDrag;
				PhysicsUtils.IgnoreAllCollisions(HeldObject.gameObject, base.gameObject, ignore: false);
				StartCoroutine(WaitThenDisableDroppedObjectInterpolation(component));
			}
		}
		HeldObject = null;
		RopeRenderer.enabled = false;
	}

	private IEnumerator WaitThenDisableDroppedObjectInterpolation(Rigidbody body)
	{
		yield return new WaitForSeconds(3f);
		if (body != null && (!(HeldObject != null) || !(HeldObject.GetComponent<Rigidbody>() == body)))
		{
			OrePiece component = body.GetComponent<OrePiece>();
			if (!(component != null) || !(component.CurrentMagnetTool != null))
			{
				body.interpolation = RigidbodyInterpolation.None;
			}
		}
	}

	public void RespawnPlayer()
	{
		Debug.Log("Respawned player");
		TeleportPlayer(PlayerSpawnPoint.GetRandomPlayerSpawnPoint().transform.position);
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(PlayerRespawnSound, base.transform.position);
	}

	public void TeleportPlayer(Vector3 position)
	{
		IsInWater = false;
		if (CharacterController != null)
		{
			bool flag = CharacterController.enabled;
			CharacterController.enabled = false;
			base.transform.position = position;
			CharacterController.enabled = flag;
		}
		else
		{
			base.transform.position = position;
		}
	}

	public void TeleportPlayer(Vector3 position, Vector3 rotation)
	{
		TeleportPlayer(position);
		base.transform.rotation = Quaternion.Euler(rotation);
	}

	private void OnDestroy()
	{
		_input.Player.Disable();
		_input.Dispose();
	}
}
