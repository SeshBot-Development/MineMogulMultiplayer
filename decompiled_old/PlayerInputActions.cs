using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class PlayerInputActions : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct PlayerActions
	{
		private PlayerInputActions m_Wrapper;

		public InputAction Move => m_Wrapper.m_Player_Move;

		public InputAction Look => m_Wrapper.m_Player_Look;

		public InputAction Jump => m_Wrapper.m_Player_Jump;

		public InputAction Sprint => m_Wrapper.m_Player_Sprint;

		public InputAction PrimaryAttack => m_Wrapper.m_Player_PrimaryAttack;

		public InputAction SecondaryAttack => m_Wrapper.m_Player_SecondaryAttack;

		public InputAction Interact => m_Wrapper.m_Player_Interact;

		public InputAction Grab => m_Wrapper.m_Player_Grab;

		public InputAction Duck => m_Wrapper.m_Player_Duck;

		public InputAction Inventory => m_Wrapper.m_Player_Inventory;

		public InputAction ToggleHud => m_Wrapper.m_Player_ToggleHud;

		public InputAction QuestMenu => m_Wrapper.m_Player_QuestMenu;

		public InputAction RotateObject => m_Wrapper.m_Player_RotateObject;

		public InputAction MirrorObject => m_Wrapper.m_Player_MirrorObject;

		public InputAction DropTool => m_Wrapper.m_Player_DropTool;

		public InputAction ToggleFlashlight => m_Wrapper.m_Player_ToggleFlashlight;

		public InputAction HotbarSlot1 => m_Wrapper.m_Player_HotbarSlot1;

		public InputAction HotbarSlot2 => m_Wrapper.m_Player_HotbarSlot2;

		public InputAction HotbarSlot3 => m_Wrapper.m_Player_HotbarSlot3;

		public InputAction HotbarSlot4 => m_Wrapper.m_Player_HotbarSlot4;

		public InputAction HotbarSlot5 => m_Wrapper.m_Player_HotbarSlot5;

		public InputAction HotbarSlot6 => m_Wrapper.m_Player_HotbarSlot6;

		public InputAction HotbarSlot7 => m_Wrapper.m_Player_HotbarSlot7;

		public InputAction HotbarSlot8 => m_Wrapper.m_Player_HotbarSlot8;

		public InputAction HotbarSlot9 => m_Wrapper.m_Player_HotbarSlot9;

		public InputAction HotbarSlot10 => m_Wrapper.m_Player_HotbarSlot10;

		public bool enabled => Get().enabled;

		public PlayerActions(PlayerInputActions wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Player;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(PlayerActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IPlayerActions instance)
		{
			if (instance != null && !m_Wrapper.m_PlayerActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_PlayerActionsCallbackInterfaces.Add(instance);
				Move.started += instance.OnMove;
				Move.performed += instance.OnMove;
				Move.canceled += instance.OnMove;
				Look.started += instance.OnLook;
				Look.performed += instance.OnLook;
				Look.canceled += instance.OnLook;
				Jump.started += instance.OnJump;
				Jump.performed += instance.OnJump;
				Jump.canceled += instance.OnJump;
				Sprint.started += instance.OnSprint;
				Sprint.performed += instance.OnSprint;
				Sprint.canceled += instance.OnSprint;
				PrimaryAttack.started += instance.OnPrimaryAttack;
				PrimaryAttack.performed += instance.OnPrimaryAttack;
				PrimaryAttack.canceled += instance.OnPrimaryAttack;
				SecondaryAttack.started += instance.OnSecondaryAttack;
				SecondaryAttack.performed += instance.OnSecondaryAttack;
				SecondaryAttack.canceled += instance.OnSecondaryAttack;
				Interact.started += instance.OnInteract;
				Interact.performed += instance.OnInteract;
				Interact.canceled += instance.OnInteract;
				Grab.started += instance.OnGrab;
				Grab.performed += instance.OnGrab;
				Grab.canceled += instance.OnGrab;
				Duck.started += instance.OnDuck;
				Duck.performed += instance.OnDuck;
				Duck.canceled += instance.OnDuck;
				Inventory.started += instance.OnInventory;
				Inventory.performed += instance.OnInventory;
				Inventory.canceled += instance.OnInventory;
				ToggleHud.started += instance.OnToggleHud;
				ToggleHud.performed += instance.OnToggleHud;
				ToggleHud.canceled += instance.OnToggleHud;
				QuestMenu.started += instance.OnQuestMenu;
				QuestMenu.performed += instance.OnQuestMenu;
				QuestMenu.canceled += instance.OnQuestMenu;
				RotateObject.started += instance.OnRotateObject;
				RotateObject.performed += instance.OnRotateObject;
				RotateObject.canceled += instance.OnRotateObject;
				MirrorObject.started += instance.OnMirrorObject;
				MirrorObject.performed += instance.OnMirrorObject;
				MirrorObject.canceled += instance.OnMirrorObject;
				DropTool.started += instance.OnDropTool;
				DropTool.performed += instance.OnDropTool;
				DropTool.canceled += instance.OnDropTool;
				ToggleFlashlight.started += instance.OnToggleFlashlight;
				ToggleFlashlight.performed += instance.OnToggleFlashlight;
				ToggleFlashlight.canceled += instance.OnToggleFlashlight;
				HotbarSlot1.started += instance.OnHotbarSlot1;
				HotbarSlot1.performed += instance.OnHotbarSlot1;
				HotbarSlot1.canceled += instance.OnHotbarSlot1;
				HotbarSlot2.started += instance.OnHotbarSlot2;
				HotbarSlot2.performed += instance.OnHotbarSlot2;
				HotbarSlot2.canceled += instance.OnHotbarSlot2;
				HotbarSlot3.started += instance.OnHotbarSlot3;
				HotbarSlot3.performed += instance.OnHotbarSlot3;
				HotbarSlot3.canceled += instance.OnHotbarSlot3;
				HotbarSlot4.started += instance.OnHotbarSlot4;
				HotbarSlot4.performed += instance.OnHotbarSlot4;
				HotbarSlot4.canceled += instance.OnHotbarSlot4;
				HotbarSlot5.started += instance.OnHotbarSlot5;
				HotbarSlot5.performed += instance.OnHotbarSlot5;
				HotbarSlot5.canceled += instance.OnHotbarSlot5;
				HotbarSlot6.started += instance.OnHotbarSlot6;
				HotbarSlot6.performed += instance.OnHotbarSlot6;
				HotbarSlot6.canceled += instance.OnHotbarSlot6;
				HotbarSlot7.started += instance.OnHotbarSlot7;
				HotbarSlot7.performed += instance.OnHotbarSlot7;
				HotbarSlot7.canceled += instance.OnHotbarSlot7;
				HotbarSlot8.started += instance.OnHotbarSlot8;
				HotbarSlot8.performed += instance.OnHotbarSlot8;
				HotbarSlot8.canceled += instance.OnHotbarSlot8;
				HotbarSlot9.started += instance.OnHotbarSlot9;
				HotbarSlot9.performed += instance.OnHotbarSlot9;
				HotbarSlot9.canceled += instance.OnHotbarSlot9;
				HotbarSlot10.started += instance.OnHotbarSlot10;
				HotbarSlot10.performed += instance.OnHotbarSlot10;
				HotbarSlot10.canceled += instance.OnHotbarSlot10;
			}
		}

		private void UnregisterCallbacks(IPlayerActions instance)
		{
			Move.started -= instance.OnMove;
			Move.performed -= instance.OnMove;
			Move.canceled -= instance.OnMove;
			Look.started -= instance.OnLook;
			Look.performed -= instance.OnLook;
			Look.canceled -= instance.OnLook;
			Jump.started -= instance.OnJump;
			Jump.performed -= instance.OnJump;
			Jump.canceled -= instance.OnJump;
			Sprint.started -= instance.OnSprint;
			Sprint.performed -= instance.OnSprint;
			Sprint.canceled -= instance.OnSprint;
			PrimaryAttack.started -= instance.OnPrimaryAttack;
			PrimaryAttack.performed -= instance.OnPrimaryAttack;
			PrimaryAttack.canceled -= instance.OnPrimaryAttack;
			SecondaryAttack.started -= instance.OnSecondaryAttack;
			SecondaryAttack.performed -= instance.OnSecondaryAttack;
			SecondaryAttack.canceled -= instance.OnSecondaryAttack;
			Interact.started -= instance.OnInteract;
			Interact.performed -= instance.OnInteract;
			Interact.canceled -= instance.OnInteract;
			Grab.started -= instance.OnGrab;
			Grab.performed -= instance.OnGrab;
			Grab.canceled -= instance.OnGrab;
			Duck.started -= instance.OnDuck;
			Duck.performed -= instance.OnDuck;
			Duck.canceled -= instance.OnDuck;
			Inventory.started -= instance.OnInventory;
			Inventory.performed -= instance.OnInventory;
			Inventory.canceled -= instance.OnInventory;
			ToggleHud.started -= instance.OnToggleHud;
			ToggleHud.performed -= instance.OnToggleHud;
			ToggleHud.canceled -= instance.OnToggleHud;
			QuestMenu.started -= instance.OnQuestMenu;
			QuestMenu.performed -= instance.OnQuestMenu;
			QuestMenu.canceled -= instance.OnQuestMenu;
			RotateObject.started -= instance.OnRotateObject;
			RotateObject.performed -= instance.OnRotateObject;
			RotateObject.canceled -= instance.OnRotateObject;
			MirrorObject.started -= instance.OnMirrorObject;
			MirrorObject.performed -= instance.OnMirrorObject;
			MirrorObject.canceled -= instance.OnMirrorObject;
			DropTool.started -= instance.OnDropTool;
			DropTool.performed -= instance.OnDropTool;
			DropTool.canceled -= instance.OnDropTool;
			ToggleFlashlight.started -= instance.OnToggleFlashlight;
			ToggleFlashlight.performed -= instance.OnToggleFlashlight;
			ToggleFlashlight.canceled -= instance.OnToggleFlashlight;
			HotbarSlot1.started -= instance.OnHotbarSlot1;
			HotbarSlot1.performed -= instance.OnHotbarSlot1;
			HotbarSlot1.canceled -= instance.OnHotbarSlot1;
			HotbarSlot2.started -= instance.OnHotbarSlot2;
			HotbarSlot2.performed -= instance.OnHotbarSlot2;
			HotbarSlot2.canceled -= instance.OnHotbarSlot2;
			HotbarSlot3.started -= instance.OnHotbarSlot3;
			HotbarSlot3.performed -= instance.OnHotbarSlot3;
			HotbarSlot3.canceled -= instance.OnHotbarSlot3;
			HotbarSlot4.started -= instance.OnHotbarSlot4;
			HotbarSlot4.performed -= instance.OnHotbarSlot4;
			HotbarSlot4.canceled -= instance.OnHotbarSlot4;
			HotbarSlot5.started -= instance.OnHotbarSlot5;
			HotbarSlot5.performed -= instance.OnHotbarSlot5;
			HotbarSlot5.canceled -= instance.OnHotbarSlot5;
			HotbarSlot6.started -= instance.OnHotbarSlot6;
			HotbarSlot6.performed -= instance.OnHotbarSlot6;
			HotbarSlot6.canceled -= instance.OnHotbarSlot6;
			HotbarSlot7.started -= instance.OnHotbarSlot7;
			HotbarSlot7.performed -= instance.OnHotbarSlot7;
			HotbarSlot7.canceled -= instance.OnHotbarSlot7;
			HotbarSlot8.started -= instance.OnHotbarSlot8;
			HotbarSlot8.performed -= instance.OnHotbarSlot8;
			HotbarSlot8.canceled -= instance.OnHotbarSlot8;
			HotbarSlot9.started -= instance.OnHotbarSlot9;
			HotbarSlot9.performed -= instance.OnHotbarSlot9;
			HotbarSlot9.canceled -= instance.OnHotbarSlot9;
			HotbarSlot10.started -= instance.OnHotbarSlot10;
			HotbarSlot10.performed -= instance.OnHotbarSlot10;
			HotbarSlot10.canceled -= instance.OnHotbarSlot10;
		}

		public void RemoveCallbacks(IPlayerActions instance)
		{
			if (m_Wrapper.m_PlayerActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IPlayerActions instance)
		{
			foreach (IPlayerActions playerActionsCallbackInterface in m_Wrapper.m_PlayerActionsCallbackInterfaces)
			{
				UnregisterCallbacks(playerActionsCallbackInterface);
			}
			m_Wrapper.m_PlayerActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public interface IPlayerActions
	{
		void OnMove(InputAction.CallbackContext context);

		void OnLook(InputAction.CallbackContext context);

		void OnJump(InputAction.CallbackContext context);

		void OnSprint(InputAction.CallbackContext context);

		void OnPrimaryAttack(InputAction.CallbackContext context);

		void OnSecondaryAttack(InputAction.CallbackContext context);

		void OnInteract(InputAction.CallbackContext context);

		void OnGrab(InputAction.CallbackContext context);

		void OnDuck(InputAction.CallbackContext context);

		void OnInventory(InputAction.CallbackContext context);

		void OnToggleHud(InputAction.CallbackContext context);

		void OnQuestMenu(InputAction.CallbackContext context);

		void OnRotateObject(InputAction.CallbackContext context);

		void OnMirrorObject(InputAction.CallbackContext context);

		void OnDropTool(InputAction.CallbackContext context);

		void OnToggleFlashlight(InputAction.CallbackContext context);

		void OnHotbarSlot1(InputAction.CallbackContext context);

		void OnHotbarSlot2(InputAction.CallbackContext context);

		void OnHotbarSlot3(InputAction.CallbackContext context);

		void OnHotbarSlot4(InputAction.CallbackContext context);

		void OnHotbarSlot5(InputAction.CallbackContext context);

		void OnHotbarSlot6(InputAction.CallbackContext context);

		void OnHotbarSlot7(InputAction.CallbackContext context);

		void OnHotbarSlot8(InputAction.CallbackContext context);

		void OnHotbarSlot9(InputAction.CallbackContext context);

		void OnHotbarSlot10(InputAction.CallbackContext context);
	}

	private readonly InputActionMap m_Player;

	private List<IPlayerActions> m_PlayerActionsCallbackInterfaces = new List<IPlayerActions>();

	private readonly InputAction m_Player_Move;

	private readonly InputAction m_Player_Look;

	private readonly InputAction m_Player_Jump;

	private readonly InputAction m_Player_Sprint;

	private readonly InputAction m_Player_PrimaryAttack;

	private readonly InputAction m_Player_SecondaryAttack;

	private readonly InputAction m_Player_Interact;

	private readonly InputAction m_Player_Grab;

	private readonly InputAction m_Player_Duck;

	private readonly InputAction m_Player_Inventory;

	private readonly InputAction m_Player_ToggleHud;

	private readonly InputAction m_Player_QuestMenu;

	private readonly InputAction m_Player_RotateObject;

	private readonly InputAction m_Player_MirrorObject;

	private readonly InputAction m_Player_DropTool;

	private readonly InputAction m_Player_ToggleFlashlight;

	private readonly InputAction m_Player_HotbarSlot1;

	private readonly InputAction m_Player_HotbarSlot2;

	private readonly InputAction m_Player_HotbarSlot3;

	private readonly InputAction m_Player_HotbarSlot4;

	private readonly InputAction m_Player_HotbarSlot5;

	private readonly InputAction m_Player_HotbarSlot6;

	private readonly InputAction m_Player_HotbarSlot7;

	private readonly InputAction m_Player_HotbarSlot8;

	private readonly InputAction m_Player_HotbarSlot9;

	private readonly InputAction m_Player_HotbarSlot10;

	public InputActionAsset asset { get; }

	public InputBinding? bindingMask
	{
		get
		{
			return asset.bindingMask;
		}
		set
		{
			asset.bindingMask = value;
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return asset.devices;
		}
		set
		{
			asset.devices = value;
		}
	}

	public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

	public IEnumerable<InputBinding> bindings => asset.bindings;

	public PlayerActions Player => new PlayerActions(this);

	public PlayerInputActions()
	{
		asset = InputActionAsset.FromJson("{\n    \"version\": 1,\n    \"name\": \"PlayerInputActions\",\n    \"maps\": [\n        {\n            \"name\": \"Player\",\n            \"id\": \"6bebdbe2-2dbb-44ce-aeb7-076e9ae86a30\",\n            \"actions\": [\n                {\n                    \"name\": \"Move\",\n                    \"type\": \"Value\",\n                    \"id\": \"154e8500-6379-4785-a8bb-6a3bf0ffcbda\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Look\",\n                    \"type\": \"Value\",\n                    \"id\": \"c4ce3180-44cb-41dc-b040-57971d5c8e58\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Jump\",\n                    \"type\": \"Button\",\n                    \"id\": \"5100ab7c-67eb-462d-900e-db1b8079b467\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Sprint\",\n                    \"type\": \"Button\",\n                    \"id\": \"04a8de94-7f45-4c5b-a2a7-10f2e71f966c\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"PrimaryAttack\",\n                    \"type\": \"Button\",\n                    \"id\": \"a8449915-31bb-40bf-aa47-4b4431efb52b\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"SecondaryAttack\",\n                    \"type\": \"Button\",\n                    \"id\": \"107e6391-49d7-48df-ac69-6c48c1fd87c6\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Interact\",\n                    \"type\": \"Button\",\n                    \"id\": \"5d4efe1d-c20e-4c5a-9544-9eb4ac544cc9\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Grab\",\n                    \"type\": \"Button\",\n                    \"id\": \"c824e762-b43e-4c05-b8f5-2dc52ed18080\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Duck\",\n                    \"type\": \"Button\",\n                    \"id\": \"9f76392a-fc6a-4133-9b31-ca33c3b30447\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Inventory\",\n                    \"type\": \"Button\",\n                    \"id\": \"423f6309-aa88-4348-a2b7-c7baa3d74f09\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ToggleHud\",\n                    \"type\": \"Button\",\n                    \"id\": \"28531216-26f7-4136-9f88-6bcd33f23cd8\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"QuestMenu\",\n                    \"type\": \"Button\",\n                    \"id\": \"1d8775e5-0e9c-465e-b61d-d32fac2ae06a\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"RotateObject\",\n                    \"type\": \"Button\",\n                    \"id\": \"4fd6d24d-a54d-40fd-abc2-1ae841a70297\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"MirrorObject\",\n                    \"type\": \"Button\",\n                    \"id\": \"c2ad04cf-8f56-43d6-aede-60c65c89f523\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"DropTool\",\n                    \"type\": \"Button\",\n                    \"id\": \"8e2bae9d-c059-4ac7-980d-bdb5e680fade\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ToggleFlashlight\",\n                    \"type\": \"Button\",\n                    \"id\": \"c49e1201-2ae8-4663-ab80-40998567e7c7\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot1\",\n                    \"type\": \"Button\",\n                    \"id\": \"7c66bd0e-b739-4a19-b4c7-906a77fef18c\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot2\",\n                    \"type\": \"Button\",\n                    \"id\": \"65160997-9fea-415b-aeab-02251ebc766a\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot3\",\n                    \"type\": \"Button\",\n                    \"id\": \"019281cb-64a1-441b-840d-e26ccec184ae\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot4\",\n                    \"type\": \"Button\",\n                    \"id\": \"5760f03c-bed6-46d9-a0fc-ccf1579e6726\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot5\",\n                    \"type\": \"Button\",\n                    \"id\": \"f3e898c3-b9c4-408a-8bf8-702fa734f954\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot6\",\n                    \"type\": \"Button\",\n                    \"id\": \"6228979a-7a40-460b-9d15-a3db7d9cb2bf\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot7\",\n                    \"type\": \"Button\",\n                    \"id\": \"c7c500c5-3161-45fc-9b76-18439425d61f\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot8\",\n                    \"type\": \"Button\",\n                    \"id\": \"c86bc33a-1bbf-4be8-b370-4a8b4afdd8bb\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot9\",\n                    \"type\": \"Button\",\n                    \"id\": \"101a7009-ae1b-448c-a68d-33cfd4c9e90a\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot10\",\n                    \"type\": \"Button\",\n                    \"id\": \"3f5b066e-456d-4b34-a0cc-acbfafdb4adb\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"2D Vector\",\n                    \"id\": \"6b1caed1-b6c6-4ca1-a1e9-5611bfb1d16e\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"f1febe6b-d46b-4c2c-bc0c-246c3ec8d656\",\n                    \"path\": \"<Keyboard>/w\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"b61f6515-de03-4f83-a621-faf15526a5e3\",\n                    \"path\": \"<Keyboard>/s\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"f336c7ed-bfc9-4220-ac65-5aacdef8f18c\",\n                    \"path\": \"<Keyboard>/a\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"27c65668-9f04-4f68-9784-f80363d60d72\",\n                    \"path\": \"<Keyboard>/d\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"577abe1e-233f-4f4e-8038-1388bcb970b1\",\n                    \"path\": \"<Gamepad>/leftStick\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"2bf0f5e2-b742-49cc-9a05-3e84b6871e4f\",\n                    \"path\": \"<Gamepad>/rightStick\",\n                    \"interactions\": \"\",\n                    \"processors\": \"ScaleVector2(x=1.5)\",\n                    \"groups\": \"\",\n                    \"action\": \"Look\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"02f9cdaa-d951-430d-81d8-c79366cf6f64\",\n                    \"path\": \"<Mouse>/delta\",\n                    \"interactions\": \"\",\n                    \"processors\": \"ScaleVector2(x=0.1,y=0.1)\",\n                    \"groups\": \"\",\n                    \"action\": \"Look\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"02b8f376-d05b-4ae5-885e-eca4ec31906c\",\n                    \"path\": \"<Keyboard>/space\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Jump\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e03020a7-fa59-460e-8b90-701f2885dcfa\",\n                    \"path\": \"<Gamepad>/buttonSouth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Jump\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ddb30d65-fd4f-442b-9d6e-2e3158c23888\",\n                    \"path\": \"<Keyboard>/1\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot1\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e0ab9692-4cd5-4280-8b89-fe571b307af0\",\n                    \"path\": \"<Keyboard>/2\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot2\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"12c28458-9186-4c08-8105-5e4c337edd5d\",\n                    \"path\": \"<Keyboard>/3\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot3\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e390bdf7-0661-48e9-af9a-7cd76567f0d8\",\n                    \"path\": \"<Keyboard>/4\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot4\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"6d2ddb63-db90-4062-b09f-b306bc984357\",\n                    \"path\": \"<Keyboard>/5\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot5\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"aec13e50-74ff-4af7-ba89-6c3d7d8895cd\",\n                    \"path\": \"<Keyboard>/6\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot6\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"990fdbf5-bfad-4acb-ad0f-928fed98e6ed\",\n                    \"path\": \"<Keyboard>/7\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot7\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ea5987e1-fb58-4c57-8814-bccf0e0334a7\",\n                    \"path\": \"<Keyboard>/8\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot8\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ca5bcd5d-3635-4680-b4c2-d8f315547066\",\n                    \"path\": \"<Keyboard>/9\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot9\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"c9f1ffbd-abfb-4e47-92f7-4afd216231f9\",\n                    \"path\": \"<Keyboard>/0\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot10\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ba7038d1-b3df-4a66-8608-721c57a884d7\",\n                    \"path\": \"<Keyboard>/shift\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Sprint\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"478f43da-3552-44e0-81ba-2ed40ae0207e\",\n                    \"path\": \"<Gamepad>/leftStick/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Sprint\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"f11730f6-b327-43d8-93ef-877af0818436\",\n                    \"path\": \"<Mouse>/leftButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"PrimaryAttack\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"9193705c-664d-4d4f-b7a4-eb42cbabf294\",\n                    \"path\": \"<Gamepad>/rightTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"PrimaryAttack\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"1a3cf87d-4b71-4444-bef3-4cdb0e574347\",\n                    \"path\": \"<Mouse>/rightButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"SecondaryAttack\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ee999c27-08e7-48e6-8562-afa49fa83e73\",\n                    \"path\": \"<Gamepad>/leftTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"SecondaryAttack\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"07d3e109-87c2-4d62-90f1-ec5707c55d46\",\n                    \"path\": \"<Keyboard>/f\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Interact\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"9caa850b-0e2c-4884-9fd7-74200f7c0f6c\",\n                    \"path\": \"<Gamepad>/buttonWest\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Interact\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"589cdfc3-af76-49b6-96fc-e3945a97f7d8\",\n                    \"path\": \"<Keyboard>/e\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Grab\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"5836864f-288a-4a88-88ae-b13cd33d5bb4\",\n                    \"path\": \"<Gamepad>/rightTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Grab\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"285fbdab-a421-468a-80e7-f099f3c17614\",\n                    \"path\": \"<Keyboard>/ctrl\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Duck\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"008b83fb-ba6a-4d2e-ad6b-0ea7f3977ae2\",\n                    \"path\": \"<Gamepad>/rightStick/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Duck\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"b5e53f79-4140-4aa2-9c60-856541d9118b\",\n                    \"path\": \"<Keyboard>/tab\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Inventory\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e1880e32-e5f2-4199-bbdf-1eb05fedeac0\",\n                    \"path\": \"<Gamepad>/select\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Inventory\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"152dd463-af50-448d-9531-c5ab5481b3b7\",\n                    \"path\": \"<Keyboard>/p\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"ToggleHud\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"28270b55-83fe-415a-a6b5-e049740e3dac\",\n                    \"path\": \"<Keyboard>/t\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"QuestMenu\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"178ae9cc-4afe-41ba-b219-5e6a4fa259aa\",\n                    \"path\": \"<Gamepad>/start\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"QuestMenu\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"8ab84d48-ee46-486d-a974-d7795d00b099\",\n                    \"path\": \"<Keyboard>/r\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"RotateObject\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"7ea5f9fb-3338-406c-bbd5-32523d91e824\",\n                    \"path\": \"<Gamepad>/rightShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"RotateObject\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"bba27162-33cc-4a6b-b094-8fab7d1a7ae1\",\n                    \"path\": \"<Keyboard>/q\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"MirrorObject\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"130b748a-838f-4fb2-b7c6-f3c81695cb80\",\n                    \"path\": \"<Gamepad>/leftShoulder\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"MirrorObject\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"fe77e850-25db-4862-b91d-cdb6e5ac4afc\",\n                    \"path\": \"<Keyboard>/g\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"DropTool\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"134d14bd-1a5f-408f-96b4-4d825de53e31\",\n                    \"path\": \"<Gamepad>/buttonEast\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"DropTool\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"db664de5-0afc-42ab-93be-e5d343953ccb\",\n                    \"path\": \"<Keyboard>/c\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"ToggleFlashlight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"d077463e-c9d4-451d-b18d-19440b340f35\",\n                    \"path\": \"<Gamepad>/buttonWest\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"ToggleFlashlight\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        }\n    ],\n    \"controlSchemes\": []\n}");
		m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
		m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
		m_Player_Look = m_Player.FindAction("Look", throwIfNotFound: true);
		m_Player_Jump = m_Player.FindAction("Jump", throwIfNotFound: true);
		m_Player_Sprint = m_Player.FindAction("Sprint", throwIfNotFound: true);
		m_Player_PrimaryAttack = m_Player.FindAction("PrimaryAttack", throwIfNotFound: true);
		m_Player_SecondaryAttack = m_Player.FindAction("SecondaryAttack", throwIfNotFound: true);
		m_Player_Interact = m_Player.FindAction("Interact", throwIfNotFound: true);
		m_Player_Grab = m_Player.FindAction("Grab", throwIfNotFound: true);
		m_Player_Duck = m_Player.FindAction("Duck", throwIfNotFound: true);
		m_Player_Inventory = m_Player.FindAction("Inventory", throwIfNotFound: true);
		m_Player_ToggleHud = m_Player.FindAction("ToggleHud", throwIfNotFound: true);
		m_Player_QuestMenu = m_Player.FindAction("QuestMenu", throwIfNotFound: true);
		m_Player_RotateObject = m_Player.FindAction("RotateObject", throwIfNotFound: true);
		m_Player_MirrorObject = m_Player.FindAction("MirrorObject", throwIfNotFound: true);
		m_Player_DropTool = m_Player.FindAction("DropTool", throwIfNotFound: true);
		m_Player_ToggleFlashlight = m_Player.FindAction("ToggleFlashlight", throwIfNotFound: true);
		m_Player_HotbarSlot1 = m_Player.FindAction("HotbarSlot1", throwIfNotFound: true);
		m_Player_HotbarSlot2 = m_Player.FindAction("HotbarSlot2", throwIfNotFound: true);
		m_Player_HotbarSlot3 = m_Player.FindAction("HotbarSlot3", throwIfNotFound: true);
		m_Player_HotbarSlot4 = m_Player.FindAction("HotbarSlot4", throwIfNotFound: true);
		m_Player_HotbarSlot5 = m_Player.FindAction("HotbarSlot5", throwIfNotFound: true);
		m_Player_HotbarSlot6 = m_Player.FindAction("HotbarSlot6", throwIfNotFound: true);
		m_Player_HotbarSlot7 = m_Player.FindAction("HotbarSlot7", throwIfNotFound: true);
		m_Player_HotbarSlot8 = m_Player.FindAction("HotbarSlot8", throwIfNotFound: true);
		m_Player_HotbarSlot9 = m_Player.FindAction("HotbarSlot9", throwIfNotFound: true);
		m_Player_HotbarSlot10 = m_Player.FindAction("HotbarSlot10", throwIfNotFound: true);
	}

	~PlayerInputActions()
	{
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(asset);
	}

	public bool Contains(InputAction action)
	{
		return asset.Contains(action);
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return asset.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Enable()
	{
		asset.Enable();
	}

	public void Disable()
	{
		asset.Disable();
	}

	public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
	{
		return asset.FindAction(actionNameOrId, throwIfNotFound);
	}

	public int FindBinding(InputBinding bindingMask, out InputAction action)
	{
		return asset.FindBinding(bindingMask, out action);
	}
}
