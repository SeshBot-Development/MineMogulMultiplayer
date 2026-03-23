using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

// Token: 0x02000002 RID: 2
public class PlayerInputActions : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	// Token: 0x17000001 RID: 1
	// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
	public InputActionAsset asset { get; }

	// Token: 0x06000002 RID: 2 RVA: 0x00002058 File Offset: 0x00000258
	public PlayerInputActions()
	{
		this.asset = InputActionAsset.FromJson("{\n    \"version\": 1,\n    \"name\": \"PlayerInputActions\",\n    \"maps\": [\n        {\n            \"name\": \"Player\",\n            \"id\": \"6bebdbe2-2dbb-44ce-aeb7-076e9ae86a30\",\n            \"actions\": [\n                {\n                    \"name\": \"Move\",\n                    \"type\": \"Value\",\n                    \"id\": \"154e8500-6379-4785-a8bb-6a3bf0ffcbda\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Look\",\n                    \"type\": \"Value\",\n                    \"id\": \"c4ce3180-44cb-41dc-b040-57971d5c8e58\",\n                    \"expectedControlType\": \"Vector2\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": true\n                },\n                {\n                    \"name\": \"Jump\",\n                    \"type\": \"Button\",\n                    \"id\": \"5100ab7c-67eb-462d-900e-db1b8079b467\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Sprint\",\n                    \"type\": \"Button\",\n                    \"id\": \"04a8de94-7f45-4c5b-a2a7-10f2e71f966c\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"PrimaryAttack\",\n                    \"type\": \"Button\",\n                    \"id\": \"a8449915-31bb-40bf-aa47-4b4431efb52b\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"SecondaryAttack\",\n                    \"type\": \"Button\",\n                    \"id\": \"107e6391-49d7-48df-ac69-6c48c1fd87c6\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Interact\",\n                    \"type\": \"Button\",\n                    \"id\": \"5d4efe1d-c20e-4c5a-9544-9eb4ac544cc9\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Grab\",\n                    \"type\": \"Button\",\n                    \"id\": \"c824e762-b43e-4c05-b8f5-2dc52ed18080\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Duck\",\n                    \"type\": \"Button\",\n                    \"id\": \"9f76392a-fc6a-4133-9b31-ca33c3b30447\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"Inventory\",\n                    \"type\": \"Button\",\n                    \"id\": \"423f6309-aa88-4348-a2b7-c7baa3d74f09\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ToggleHud\",\n                    \"type\": \"Button\",\n                    \"id\": \"28531216-26f7-4136-9f88-6bcd33f23cd8\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"QuestMenu\",\n                    \"type\": \"Button\",\n                    \"id\": \"1d8775e5-0e9c-465e-b61d-d32fac2ae06a\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"RotateObject\",\n                    \"type\": \"Button\",\n                    \"id\": \"4fd6d24d-a54d-40fd-abc2-1ae841a70297\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"MirrorObject\",\n                    \"type\": \"Button\",\n                    \"id\": \"c2ad04cf-8f56-43d6-aede-60c65c89f523\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"DropTool\",\n                    \"type\": \"Button\",\n                    \"id\": \"8e2bae9d-c059-4ac7-980d-bdb5e680fade\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"ToggleFlashlight\",\n                    \"type\": \"Button\",\n                    \"id\": \"c49e1201-2ae8-4663-ab80-40998567e7c7\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot1\",\n                    \"type\": \"Button\",\n                    \"id\": \"7c66bd0e-b739-4a19-b4c7-906a77fef18c\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot2\",\n                    \"type\": \"Button\",\n                    \"id\": \"65160997-9fea-415b-aeab-02251ebc766a\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot3\",\n                    \"type\": \"Button\",\n                    \"id\": \"019281cb-64a1-441b-840d-e26ccec184ae\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot4\",\n                    \"type\": \"Button\",\n                    \"id\": \"5760f03c-bed6-46d9-a0fc-ccf1579e6726\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot5\",\n                    \"type\": \"Button\",\n                    \"id\": \"f3e898c3-b9c4-408a-8bf8-702fa734f954\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot6\",\n                    \"type\": \"Button\",\n                    \"id\": \"6228979a-7a40-460b-9d15-a3db7d9cb2bf\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot7\",\n                    \"type\": \"Button\",\n                    \"id\": \"c7c500c5-3161-45fc-9b76-18439425d61f\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot8\",\n                    \"type\": \"Button\",\n                    \"id\": \"c86bc33a-1bbf-4be8-b370-4a8b4afdd8bb\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot9\",\n                    \"type\": \"Button\",\n                    \"id\": \"101a7009-ae1b-448c-a68d-33cfd4c9e90a\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                },\n                {\n                    \"name\": \"HotbarSlot10\",\n                    \"type\": \"Button\",\n                    \"id\": \"3f5b066e-456d-4b34-a0cc-acbfafdb4adb\",\n                    \"expectedControlType\": \"\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\",\n                    \"initialStateCheck\": false\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"2D Vector\",\n                    \"id\": \"6b1caed1-b6c6-4ca1-a1e9-5611bfb1d16e\",\n                    \"path\": \"2DVector\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"up\",\n                    \"id\": \"f1febe6b-d46b-4c2c-bc0c-246c3ec8d656\",\n                    \"path\": \"<Keyboard>/w\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"down\",\n                    \"id\": \"b61f6515-de03-4f83-a621-faf15526a5e3\",\n                    \"path\": \"<Keyboard>/s\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"left\",\n                    \"id\": \"f336c7ed-bfc9-4220-ac65-5aacdef8f18c\",\n                    \"path\": \"<Keyboard>/a\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"right\",\n                    \"id\": \"27c65668-9f04-4f68-9784-f80363d60d72\",\n                    \"path\": \"<Keyboard>/d\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"577abe1e-233f-4f4e-8038-1388bcb970b1\",\n                    \"path\": \"<Gamepad>/leftStick\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Move\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"2bf0f5e2-b742-49cc-9a05-3e84b6871e4f\",\n                    \"path\": \"<Gamepad>/rightStick\",\n                    \"interactions\": \"\",\n                    \"processors\": \"ScaleVector2(x=1.5)\",\n                    \"groups\": \"\",\n                    \"action\": \"Look\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"02f9cdaa-d951-430d-81d8-c79366cf6f64\",\n                    \"path\": \"<Mouse>/delta\",\n                    \"interactions\": \"\",\n                    \"processors\": \"ScaleVector2(x=0.1,y=0.1)\",\n                    \"groups\": \"\",\n                    \"action\": \"Look\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"02b8f376-d05b-4ae5-885e-eca4ec31906c\",\n                    \"path\": \"<Keyboard>/space\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Jump\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e03020a7-fa59-460e-8b90-701f2885dcfa\",\n                    \"path\": \"<Gamepad>/buttonSouth\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Jump\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ddb30d65-fd4f-442b-9d6e-2e3158c23888\",\n                    \"path\": \"<Keyboard>/1\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot1\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e0ab9692-4cd5-4280-8b89-fe571b307af0\",\n                    \"path\": \"<Keyboard>/2\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot2\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"12c28458-9186-4c08-8105-5e4c337edd5d\",\n                    \"path\": \"<Keyboard>/3\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot3\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e390bdf7-0661-48e9-af9a-7cd76567f0d8\",\n                    \"path\": \"<Keyboard>/4\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot4\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"6d2ddb63-db90-4062-b09f-b306bc984357\",\n                    \"path\": \"<Keyboard>/5\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot5\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"aec13e50-74ff-4af7-ba89-6c3d7d8895cd\",\n                    \"path\": \"<Keyboard>/6\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot6\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"990fdbf5-bfad-4acb-ad0f-928fed98e6ed\",\n                    \"path\": \"<Keyboard>/7\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot7\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ea5987e1-fb58-4c57-8814-bccf0e0334a7\",\n                    \"path\": \"<Keyboard>/8\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot8\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ca5bcd5d-3635-4680-b4c2-d8f315547066\",\n                    \"path\": \"<Keyboard>/9\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot9\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"c9f1ffbd-abfb-4e47-92f7-4afd216231f9\",\n                    \"path\": \"<Keyboard>/0\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"HotbarSlot10\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"ba7038d1-b3df-4a66-8608-721c57a884d7\",\n                    \"path\": \"<Keyboard>/shift\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Sprint\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"478f43da-3552-44e0-81ba-2ed40ae0207e\",\n                    \"path\": \"<Gamepad>/leftStick/down\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Sprint\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"f11730f6-b327-43d8-93ef-877af0818436\",\n                    \"path\": \"<Mouse>/leftButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"PrimaryAttack\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"9193705c-664d-4d4f-b7a4-eb42cbabf294\",\n                    \"path\": \"<Gamepad>/rightTrigger\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"PrimaryAttack\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"1a3cf87d-4b71-4444-bef3-4cdb0e574347\",\n                    \"path\": \"<Mouse>/rightButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                 [...string is too long...]");
		this.m_Player = this.asset.FindActionMap("Player", true);
		this.m_Player_Move = this.m_Player.FindAction("Move", true);
		this.m_Player_Look = this.m_Player.FindAction("Look", true);
		this.m_Player_Jump = this.m_Player.FindAction("Jump", true);
		this.m_Player_Sprint = this.m_Player.FindAction("Sprint", true);
		this.m_Player_PrimaryAttack = this.m_Player.FindAction("PrimaryAttack", true);
		this.m_Player_SecondaryAttack = this.m_Player.FindAction("SecondaryAttack", true);
		this.m_Player_Interact = this.m_Player.FindAction("Interact", true);
		this.m_Player_Grab = this.m_Player.FindAction("Grab", true);
		this.m_Player_Duck = this.m_Player.FindAction("Duck", true);
		this.m_Player_Inventory = this.m_Player.FindAction("Inventory", true);
		this.m_Player_ToggleHud = this.m_Player.FindAction("ToggleHud", true);
		this.m_Player_QuestMenu = this.m_Player.FindAction("QuestMenu", true);
		this.m_Player_RotateObject = this.m_Player.FindAction("RotateObject", true);
		this.m_Player_MirrorObject = this.m_Player.FindAction("MirrorObject", true);
		this.m_Player_DropTool = this.m_Player.FindAction("DropTool", true);
		this.m_Player_ToggleFlashlight = this.m_Player.FindAction("ToggleFlashlight", true);
		this.m_Player_HotbarSlot1 = this.m_Player.FindAction("HotbarSlot1", true);
		this.m_Player_HotbarSlot2 = this.m_Player.FindAction("HotbarSlot2", true);
		this.m_Player_HotbarSlot3 = this.m_Player.FindAction("HotbarSlot3", true);
		this.m_Player_HotbarSlot4 = this.m_Player.FindAction("HotbarSlot4", true);
		this.m_Player_HotbarSlot5 = this.m_Player.FindAction("HotbarSlot5", true);
		this.m_Player_HotbarSlot6 = this.m_Player.FindAction("HotbarSlot6", true);
		this.m_Player_HotbarSlot7 = this.m_Player.FindAction("HotbarSlot7", true);
		this.m_Player_HotbarSlot8 = this.m_Player.FindAction("HotbarSlot8", true);
		this.m_Player_HotbarSlot9 = this.m_Player.FindAction("HotbarSlot9", true);
		this.m_Player_HotbarSlot10 = this.m_Player.FindAction("HotbarSlot10", true);
	}

	// Token: 0x06000003 RID: 3 RVA: 0x000022F4 File Offset: 0x000004F4
	~PlayerInputActions()
	{
	}

	// Token: 0x06000004 RID: 4 RVA: 0x0000231C File Offset: 0x0000051C
	public void Dispose()
	{
		Object.Destroy(this.asset);
	}

	// Token: 0x17000002 RID: 2
	// (get) Token: 0x06000005 RID: 5 RVA: 0x00002329 File Offset: 0x00000529
	// (set) Token: 0x06000006 RID: 6 RVA: 0x00002336 File Offset: 0x00000536
	public InputBinding? bindingMask
	{
		get
		{
			return this.asset.bindingMask;
		}
		set
		{
			this.asset.bindingMask = value;
		}
	}

	// Token: 0x17000003 RID: 3
	// (get) Token: 0x06000007 RID: 7 RVA: 0x00002344 File Offset: 0x00000544
	// (set) Token: 0x06000008 RID: 8 RVA: 0x00002351 File Offset: 0x00000551
	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return this.asset.devices;
		}
		set
		{
			this.asset.devices = value;
		}
	}

	// Token: 0x17000004 RID: 4
	// (get) Token: 0x06000009 RID: 9 RVA: 0x0000235F File Offset: 0x0000055F
	public ReadOnlyArray<InputControlScheme> controlSchemes
	{
		get
		{
			return this.asset.controlSchemes;
		}
	}

	// Token: 0x0600000A RID: 10 RVA: 0x0000236C File Offset: 0x0000056C
	public bool Contains(InputAction action)
	{
		return this.asset.Contains(action);
	}

	// Token: 0x0600000B RID: 11 RVA: 0x0000237A File Offset: 0x0000057A
	public IEnumerator<InputAction> GetEnumerator()
	{
		return this.asset.GetEnumerator();
	}

	// Token: 0x0600000C RID: 12 RVA: 0x00002387 File Offset: 0x00000587
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	// Token: 0x0600000D RID: 13 RVA: 0x0000238F File Offset: 0x0000058F
	public void Enable()
	{
		this.asset.Enable();
	}

	// Token: 0x0600000E RID: 14 RVA: 0x0000239C File Offset: 0x0000059C
	public void Disable()
	{
		this.asset.Disable();
	}

	// Token: 0x17000005 RID: 5
	// (get) Token: 0x0600000F RID: 15 RVA: 0x000023A9 File Offset: 0x000005A9
	public IEnumerable<InputBinding> bindings
	{
		get
		{
			return this.asset.bindings;
		}
	}

	// Token: 0x06000010 RID: 16 RVA: 0x000023B6 File Offset: 0x000005B6
	public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
	{
		return this.asset.FindAction(actionNameOrId, throwIfNotFound);
	}

	// Token: 0x06000011 RID: 17 RVA: 0x000023C5 File Offset: 0x000005C5
	public int FindBinding(InputBinding bindingMask, out InputAction action)
	{
		return this.asset.FindBinding(bindingMask, out action);
	}

	// Token: 0x17000006 RID: 6
	// (get) Token: 0x06000012 RID: 18 RVA: 0x000023D4 File Offset: 0x000005D4
	public PlayerInputActions.PlayerActions Player
	{
		get
		{
			return new PlayerInputActions.PlayerActions(this);
		}
	}

	// Token: 0x04000002 RID: 2
	private readonly InputActionMap m_Player;

	// Token: 0x04000003 RID: 3
	private List<PlayerInputActions.IPlayerActions> m_PlayerActionsCallbackInterfaces = new List<PlayerInputActions.IPlayerActions>();

	// Token: 0x04000004 RID: 4
	private readonly InputAction m_Player_Move;

	// Token: 0x04000005 RID: 5
	private readonly InputAction m_Player_Look;

	// Token: 0x04000006 RID: 6
	private readonly InputAction m_Player_Jump;

	// Token: 0x04000007 RID: 7
	private readonly InputAction m_Player_Sprint;

	// Token: 0x04000008 RID: 8
	private readonly InputAction m_Player_PrimaryAttack;

	// Token: 0x04000009 RID: 9
	private readonly InputAction m_Player_SecondaryAttack;

	// Token: 0x0400000A RID: 10
	private readonly InputAction m_Player_Interact;

	// Token: 0x0400000B RID: 11
	private readonly InputAction m_Player_Grab;

	// Token: 0x0400000C RID: 12
	private readonly InputAction m_Player_Duck;

	// Token: 0x0400000D RID: 13
	private readonly InputAction m_Player_Inventory;

	// Token: 0x0400000E RID: 14
	private readonly InputAction m_Player_ToggleHud;

	// Token: 0x0400000F RID: 15
	private readonly InputAction m_Player_QuestMenu;

	// Token: 0x04000010 RID: 16
	private readonly InputAction m_Player_RotateObject;

	// Token: 0x04000011 RID: 17
	private readonly InputAction m_Player_MirrorObject;

	// Token: 0x04000012 RID: 18
	private readonly InputAction m_Player_DropTool;

	// Token: 0x04000013 RID: 19
	private readonly InputAction m_Player_ToggleFlashlight;

	// Token: 0x04000014 RID: 20
	private readonly InputAction m_Player_HotbarSlot1;

	// Token: 0x04000015 RID: 21
	private readonly InputAction m_Player_HotbarSlot2;

	// Token: 0x04000016 RID: 22
	private readonly InputAction m_Player_HotbarSlot3;

	// Token: 0x04000017 RID: 23
	private readonly InputAction m_Player_HotbarSlot4;

	// Token: 0x04000018 RID: 24
	private readonly InputAction m_Player_HotbarSlot5;

	// Token: 0x04000019 RID: 25
	private readonly InputAction m_Player_HotbarSlot6;

	// Token: 0x0400001A RID: 26
	private readonly InputAction m_Player_HotbarSlot7;

	// Token: 0x0400001B RID: 27
	private readonly InputAction m_Player_HotbarSlot8;

	// Token: 0x0400001C RID: 28
	private readonly InputAction m_Player_HotbarSlot9;

	// Token: 0x0400001D RID: 29
	private readonly InputAction m_Player_HotbarSlot10;

	// Token: 0x02000110 RID: 272
	public struct PlayerActions
	{
		// Token: 0x06000717 RID: 1815 RVA: 0x0002409C File Offset: 0x0002229C
		public PlayerActions(PlayerInputActions wrapper)
		{
			this.m_Wrapper = wrapper;
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000718 RID: 1816 RVA: 0x000240A5 File Offset: 0x000222A5
		public InputAction Move
		{
			get
			{
				return this.m_Wrapper.m_Player_Move;
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x06000719 RID: 1817 RVA: 0x000240B2 File Offset: 0x000222B2
		public InputAction Look
		{
			get
			{
				return this.m_Wrapper.m_Player_Look;
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x0600071A RID: 1818 RVA: 0x000240BF File Offset: 0x000222BF
		public InputAction Jump
		{
			get
			{
				return this.m_Wrapper.m_Player_Jump;
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x0600071B RID: 1819 RVA: 0x000240CC File Offset: 0x000222CC
		public InputAction Sprint
		{
			get
			{
				return this.m_Wrapper.m_Player_Sprint;
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x0600071C RID: 1820 RVA: 0x000240D9 File Offset: 0x000222D9
		public InputAction PrimaryAttack
		{
			get
			{
				return this.m_Wrapper.m_Player_PrimaryAttack;
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x0600071D RID: 1821 RVA: 0x000240E6 File Offset: 0x000222E6
		public InputAction SecondaryAttack
		{
			get
			{
				return this.m_Wrapper.m_Player_SecondaryAttack;
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x0600071E RID: 1822 RVA: 0x000240F3 File Offset: 0x000222F3
		public InputAction Interact
		{
			get
			{
				return this.m_Wrapper.m_Player_Interact;
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x0600071F RID: 1823 RVA: 0x00024100 File Offset: 0x00022300
		public InputAction Grab
		{
			get
			{
				return this.m_Wrapper.m_Player_Grab;
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000720 RID: 1824 RVA: 0x0002410D File Offset: 0x0002230D
		public InputAction Duck
		{
			get
			{
				return this.m_Wrapper.m_Player_Duck;
			}
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x06000721 RID: 1825 RVA: 0x0002411A File Offset: 0x0002231A
		public InputAction Inventory
		{
			get
			{
				return this.m_Wrapper.m_Player_Inventory;
			}
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x06000722 RID: 1826 RVA: 0x00024127 File Offset: 0x00022327
		public InputAction ToggleHud
		{
			get
			{
				return this.m_Wrapper.m_Player_ToggleHud;
			}
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x06000723 RID: 1827 RVA: 0x00024134 File Offset: 0x00022334
		public InputAction QuestMenu
		{
			get
			{
				return this.m_Wrapper.m_Player_QuestMenu;
			}
		}

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000724 RID: 1828 RVA: 0x00024141 File Offset: 0x00022341
		public InputAction RotateObject
		{
			get
			{
				return this.m_Wrapper.m_Player_RotateObject;
			}
		}

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x06000725 RID: 1829 RVA: 0x0002414E File Offset: 0x0002234E
		public InputAction MirrorObject
		{
			get
			{
				return this.m_Wrapper.m_Player_MirrorObject;
			}
		}

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x06000726 RID: 1830 RVA: 0x0002415B File Offset: 0x0002235B
		public InputAction DropTool
		{
			get
			{
				return this.m_Wrapper.m_Player_DropTool;
			}
		}

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x06000727 RID: 1831 RVA: 0x00024168 File Offset: 0x00022368
		public InputAction ToggleFlashlight
		{
			get
			{
				return this.m_Wrapper.m_Player_ToggleFlashlight;
			}
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x06000728 RID: 1832 RVA: 0x00024175 File Offset: 0x00022375
		public InputAction HotbarSlot1
		{
			get
			{
				return this.m_Wrapper.m_Player_HotbarSlot1;
			}
		}

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x06000729 RID: 1833 RVA: 0x00024182 File Offset: 0x00022382
		public InputAction HotbarSlot2
		{
			get
			{
				return this.m_Wrapper.m_Player_HotbarSlot2;
			}
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x0600072A RID: 1834 RVA: 0x0002418F File Offset: 0x0002238F
		public InputAction HotbarSlot3
		{
			get
			{
				return this.m_Wrapper.m_Player_HotbarSlot3;
			}
		}

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x0600072B RID: 1835 RVA: 0x0002419C File Offset: 0x0002239C
		public InputAction HotbarSlot4
		{
			get
			{
				return this.m_Wrapper.m_Player_HotbarSlot4;
			}
		}

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x0600072C RID: 1836 RVA: 0x000241A9 File Offset: 0x000223A9
		public InputAction HotbarSlot5
		{
			get
			{
				return this.m_Wrapper.m_Player_HotbarSlot5;
			}
		}

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x0600072D RID: 1837 RVA: 0x000241B6 File Offset: 0x000223B6
		public InputAction HotbarSlot6
		{
			get
			{
				return this.m_Wrapper.m_Player_HotbarSlot6;
			}
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x0600072E RID: 1838 RVA: 0x000241C3 File Offset: 0x000223C3
		public InputAction HotbarSlot7
		{
			get
			{
				return this.m_Wrapper.m_Player_HotbarSlot7;
			}
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x0600072F RID: 1839 RVA: 0x000241D0 File Offset: 0x000223D0
		public InputAction HotbarSlot8
		{
			get
			{
				return this.m_Wrapper.m_Player_HotbarSlot8;
			}
		}

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x06000730 RID: 1840 RVA: 0x000241DD File Offset: 0x000223DD
		public InputAction HotbarSlot9
		{
			get
			{
				return this.m_Wrapper.m_Player_HotbarSlot9;
			}
		}

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x06000731 RID: 1841 RVA: 0x000241EA File Offset: 0x000223EA
		public InputAction HotbarSlot10
		{
			get
			{
				return this.m_Wrapper.m_Player_HotbarSlot10;
			}
		}

		// Token: 0x06000732 RID: 1842 RVA: 0x000241F7 File Offset: 0x000223F7
		public InputActionMap Get()
		{
			return this.m_Wrapper.m_Player;
		}

		// Token: 0x06000733 RID: 1843 RVA: 0x00024204 File Offset: 0x00022404
		public void Enable()
		{
			this.Get().Enable();
		}

		// Token: 0x06000734 RID: 1844 RVA: 0x00024211 File Offset: 0x00022411
		public void Disable()
		{
			this.Get().Disable();
		}

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x06000735 RID: 1845 RVA: 0x0002421E File Offset: 0x0002241E
		public bool enabled
		{
			get
			{
				return this.Get().enabled;
			}
		}

		// Token: 0x06000736 RID: 1846 RVA: 0x0002422B File Offset: 0x0002242B
		public static implicit operator InputActionMap(PlayerInputActions.PlayerActions set)
		{
			return set.Get();
		}

		// Token: 0x06000737 RID: 1847 RVA: 0x00024234 File Offset: 0x00022434
		public void AddCallbacks(PlayerInputActions.IPlayerActions instance)
		{
			if (instance == null || this.m_Wrapper.m_PlayerActionsCallbackInterfaces.Contains(instance))
			{
				return;
			}
			this.m_Wrapper.m_PlayerActionsCallbackInterfaces.Add(instance);
			this.Move.started += instance.OnMove;
			this.Move.performed += instance.OnMove;
			this.Move.canceled += instance.OnMove;
			this.Look.started += instance.OnLook;
			this.Look.performed += instance.OnLook;
			this.Look.canceled += instance.OnLook;
			this.Jump.started += instance.OnJump;
			this.Jump.performed += instance.OnJump;
			this.Jump.canceled += instance.OnJump;
			this.Sprint.started += instance.OnSprint;
			this.Sprint.performed += instance.OnSprint;
			this.Sprint.canceled += instance.OnSprint;
			this.PrimaryAttack.started += instance.OnPrimaryAttack;
			this.PrimaryAttack.performed += instance.OnPrimaryAttack;
			this.PrimaryAttack.canceled += instance.OnPrimaryAttack;
			this.SecondaryAttack.started += instance.OnSecondaryAttack;
			this.SecondaryAttack.performed += instance.OnSecondaryAttack;
			this.SecondaryAttack.canceled += instance.OnSecondaryAttack;
			this.Interact.started += instance.OnInteract;
			this.Interact.performed += instance.OnInteract;
			this.Interact.canceled += instance.OnInteract;
			this.Grab.started += instance.OnGrab;
			this.Grab.performed += instance.OnGrab;
			this.Grab.canceled += instance.OnGrab;
			this.Duck.started += instance.OnDuck;
			this.Duck.performed += instance.OnDuck;
			this.Duck.canceled += instance.OnDuck;
			this.Inventory.started += instance.OnInventory;
			this.Inventory.performed += instance.OnInventory;
			this.Inventory.canceled += instance.OnInventory;
			this.ToggleHud.started += instance.OnToggleHud;
			this.ToggleHud.performed += instance.OnToggleHud;
			this.ToggleHud.canceled += instance.OnToggleHud;
			this.QuestMenu.started += instance.OnQuestMenu;
			this.QuestMenu.performed += instance.OnQuestMenu;
			this.QuestMenu.canceled += instance.OnQuestMenu;
			this.RotateObject.started += instance.OnRotateObject;
			this.RotateObject.performed += instance.OnRotateObject;
			this.RotateObject.canceled += instance.OnRotateObject;
			this.MirrorObject.started += instance.OnMirrorObject;
			this.MirrorObject.performed += instance.OnMirrorObject;
			this.MirrorObject.canceled += instance.OnMirrorObject;
			this.DropTool.started += instance.OnDropTool;
			this.DropTool.performed += instance.OnDropTool;
			this.DropTool.canceled += instance.OnDropTool;
			this.ToggleFlashlight.started += instance.OnToggleFlashlight;
			this.ToggleFlashlight.performed += instance.OnToggleFlashlight;
			this.ToggleFlashlight.canceled += instance.OnToggleFlashlight;
			this.HotbarSlot1.started += instance.OnHotbarSlot1;
			this.HotbarSlot1.performed += instance.OnHotbarSlot1;
			this.HotbarSlot1.canceled += instance.OnHotbarSlot1;
			this.HotbarSlot2.started += instance.OnHotbarSlot2;
			this.HotbarSlot2.performed += instance.OnHotbarSlot2;
			this.HotbarSlot2.canceled += instance.OnHotbarSlot2;
			this.HotbarSlot3.started += instance.OnHotbarSlot3;
			this.HotbarSlot3.performed += instance.OnHotbarSlot3;
			this.HotbarSlot3.canceled += instance.OnHotbarSlot3;
			this.HotbarSlot4.started += instance.OnHotbarSlot4;
			this.HotbarSlot4.performed += instance.OnHotbarSlot4;
			this.HotbarSlot4.canceled += instance.OnHotbarSlot4;
			this.HotbarSlot5.started += instance.OnHotbarSlot5;
			this.HotbarSlot5.performed += instance.OnHotbarSlot5;
			this.HotbarSlot5.canceled += instance.OnHotbarSlot5;
			this.HotbarSlot6.started += instance.OnHotbarSlot6;
			this.HotbarSlot6.performed += instance.OnHotbarSlot6;
			this.HotbarSlot6.canceled += instance.OnHotbarSlot6;
			this.HotbarSlot7.started += instance.OnHotbarSlot7;
			this.HotbarSlot7.performed += instance.OnHotbarSlot7;
			this.HotbarSlot7.canceled += instance.OnHotbarSlot7;
			this.HotbarSlot8.started += instance.OnHotbarSlot8;
			this.HotbarSlot8.performed += instance.OnHotbarSlot8;
			this.HotbarSlot8.canceled += instance.OnHotbarSlot8;
			this.HotbarSlot9.started += instance.OnHotbarSlot9;
			this.HotbarSlot9.performed += instance.OnHotbarSlot9;
			this.HotbarSlot9.canceled += instance.OnHotbarSlot9;
			this.HotbarSlot10.started += instance.OnHotbarSlot10;
			this.HotbarSlot10.performed += instance.OnHotbarSlot10;
			this.HotbarSlot10.canceled += instance.OnHotbarSlot10;
		}

		// Token: 0x06000738 RID: 1848 RVA: 0x000249BC File Offset: 0x00022BBC
		private void UnregisterCallbacks(PlayerInputActions.IPlayerActions instance)
		{
			this.Move.started -= instance.OnMove;
			this.Move.performed -= instance.OnMove;
			this.Move.canceled -= instance.OnMove;
			this.Look.started -= instance.OnLook;
			this.Look.performed -= instance.OnLook;
			this.Look.canceled -= instance.OnLook;
			this.Jump.started -= instance.OnJump;
			this.Jump.performed -= instance.OnJump;
			this.Jump.canceled -= instance.OnJump;
			this.Sprint.started -= instance.OnSprint;
			this.Sprint.performed -= instance.OnSprint;
			this.Sprint.canceled -= instance.OnSprint;
			this.PrimaryAttack.started -= instance.OnPrimaryAttack;
			this.PrimaryAttack.performed -= instance.OnPrimaryAttack;
			this.PrimaryAttack.canceled -= instance.OnPrimaryAttack;
			this.SecondaryAttack.started -= instance.OnSecondaryAttack;
			this.SecondaryAttack.performed -= instance.OnSecondaryAttack;
			this.SecondaryAttack.canceled -= instance.OnSecondaryAttack;
			this.Interact.started -= instance.OnInteract;
			this.Interact.performed -= instance.OnInteract;
			this.Interact.canceled -= instance.OnInteract;
			this.Grab.started -= instance.OnGrab;
			this.Grab.performed -= instance.OnGrab;
			this.Grab.canceled -= instance.OnGrab;
			this.Duck.started -= instance.OnDuck;
			this.Duck.performed -= instance.OnDuck;
			this.Duck.canceled -= instance.OnDuck;
			this.Inventory.started -= instance.OnInventory;
			this.Inventory.performed -= instance.OnInventory;
			this.Inventory.canceled -= instance.OnInventory;
			this.ToggleHud.started -= instance.OnToggleHud;
			this.ToggleHud.performed -= instance.OnToggleHud;
			this.ToggleHud.canceled -= instance.OnToggleHud;
			this.QuestMenu.started -= instance.OnQuestMenu;
			this.QuestMenu.performed -= instance.OnQuestMenu;
			this.QuestMenu.canceled -= instance.OnQuestMenu;
			this.RotateObject.started -= instance.OnRotateObject;
			this.RotateObject.performed -= instance.OnRotateObject;
			this.RotateObject.canceled -= instance.OnRotateObject;
			this.MirrorObject.started -= instance.OnMirrorObject;
			this.MirrorObject.performed -= instance.OnMirrorObject;
			this.MirrorObject.canceled -= instance.OnMirrorObject;
			this.DropTool.started -= instance.OnDropTool;
			this.DropTool.performed -= instance.OnDropTool;
			this.DropTool.canceled -= instance.OnDropTool;
			this.ToggleFlashlight.started -= instance.OnToggleFlashlight;
			this.ToggleFlashlight.performed -= instance.OnToggleFlashlight;
			this.ToggleFlashlight.canceled -= instance.OnToggleFlashlight;
			this.HotbarSlot1.started -= instance.OnHotbarSlot1;
			this.HotbarSlot1.performed -= instance.OnHotbarSlot1;
			this.HotbarSlot1.canceled -= instance.OnHotbarSlot1;
			this.HotbarSlot2.started -= instance.OnHotbarSlot2;
			this.HotbarSlot2.performed -= instance.OnHotbarSlot2;
			this.HotbarSlot2.canceled -= instance.OnHotbarSlot2;
			this.HotbarSlot3.started -= instance.OnHotbarSlot3;
			this.HotbarSlot3.performed -= instance.OnHotbarSlot3;
			this.HotbarSlot3.canceled -= instance.OnHotbarSlot3;
			this.HotbarSlot4.started -= instance.OnHotbarSlot4;
			this.HotbarSlot4.performed -= instance.OnHotbarSlot4;
			this.HotbarSlot4.canceled -= instance.OnHotbarSlot4;
			this.HotbarSlot5.started -= instance.OnHotbarSlot5;
			this.HotbarSlot5.performed -= instance.OnHotbarSlot5;
			this.HotbarSlot5.canceled -= instance.OnHotbarSlot5;
			this.HotbarSlot6.started -= instance.OnHotbarSlot6;
			this.HotbarSlot6.performed -= instance.OnHotbarSlot6;
			this.HotbarSlot6.canceled -= instance.OnHotbarSlot6;
			this.HotbarSlot7.started -= instance.OnHotbarSlot7;
			this.HotbarSlot7.performed -= instance.OnHotbarSlot7;
			this.HotbarSlot7.canceled -= instance.OnHotbarSlot7;
			this.HotbarSlot8.started -= instance.OnHotbarSlot8;
			this.HotbarSlot8.performed -= instance.OnHotbarSlot8;
			this.HotbarSlot8.canceled -= instance.OnHotbarSlot8;
			this.HotbarSlot9.started -= instance.OnHotbarSlot9;
			this.HotbarSlot9.performed -= instance.OnHotbarSlot9;
			this.HotbarSlot9.canceled -= instance.OnHotbarSlot9;
			this.HotbarSlot10.started -= instance.OnHotbarSlot10;
			this.HotbarSlot10.performed -= instance.OnHotbarSlot10;
			this.HotbarSlot10.canceled -= instance.OnHotbarSlot10;
		}

		// Token: 0x06000739 RID: 1849 RVA: 0x00025119 File Offset: 0x00023319
		public void RemoveCallbacks(PlayerInputActions.IPlayerActions instance)
		{
			if (this.m_Wrapper.m_PlayerActionsCallbackInterfaces.Remove(instance))
			{
				this.UnregisterCallbacks(instance);
			}
		}

		// Token: 0x0600073A RID: 1850 RVA: 0x00025138 File Offset: 0x00023338
		public void SetCallbacks(PlayerInputActions.IPlayerActions instance)
		{
			foreach (PlayerInputActions.IPlayerActions playerActions in this.m_Wrapper.m_PlayerActionsCallbackInterfaces)
			{
				this.UnregisterCallbacks(playerActions);
			}
			this.m_Wrapper.m_PlayerActionsCallbackInterfaces.Clear();
			this.AddCallbacks(instance);
		}

		// Token: 0x0400082F RID: 2095
		private PlayerInputActions m_Wrapper;
	}

	// Token: 0x02000111 RID: 273
	public interface IPlayerActions
	{
		// Token: 0x0600073B RID: 1851
		void OnMove(InputAction.CallbackContext context);

		// Token: 0x0600073C RID: 1852
		void OnLook(InputAction.CallbackContext context);

		// Token: 0x0600073D RID: 1853
		void OnJump(InputAction.CallbackContext context);

		// Token: 0x0600073E RID: 1854
		void OnSprint(InputAction.CallbackContext context);

		// Token: 0x0600073F RID: 1855
		void OnPrimaryAttack(InputAction.CallbackContext context);

		// Token: 0x06000740 RID: 1856
		void OnSecondaryAttack(InputAction.CallbackContext context);

		// Token: 0x06000741 RID: 1857
		void OnInteract(InputAction.CallbackContext context);

		// Token: 0x06000742 RID: 1858
		void OnGrab(InputAction.CallbackContext context);

		// Token: 0x06000743 RID: 1859
		void OnDuck(InputAction.CallbackContext context);

		// Token: 0x06000744 RID: 1860
		void OnInventory(InputAction.CallbackContext context);

		// Token: 0x06000745 RID: 1861
		void OnToggleHud(InputAction.CallbackContext context);

		// Token: 0x06000746 RID: 1862
		void OnQuestMenu(InputAction.CallbackContext context);

		// Token: 0x06000747 RID: 1863
		void OnRotateObject(InputAction.CallbackContext context);

		// Token: 0x06000748 RID: 1864
		void OnMirrorObject(InputAction.CallbackContext context);

		// Token: 0x06000749 RID: 1865
		void OnDropTool(InputAction.CallbackContext context);

		// Token: 0x0600074A RID: 1866
		void OnToggleFlashlight(InputAction.CallbackContext context);

		// Token: 0x0600074B RID: 1867
		void OnHotbarSlot1(InputAction.CallbackContext context);

		// Token: 0x0600074C RID: 1868
		void OnHotbarSlot2(InputAction.CallbackContext context);

		// Token: 0x0600074D RID: 1869
		void OnHotbarSlot3(InputAction.CallbackContext context);

		// Token: 0x0600074E RID: 1870
		void OnHotbarSlot4(InputAction.CallbackContext context);

		// Token: 0x0600074F RID: 1871
		void OnHotbarSlot5(InputAction.CallbackContext context);

		// Token: 0x06000750 RID: 1872
		void OnHotbarSlot6(InputAction.CallbackContext context);

		// Token: 0x06000751 RID: 1873
		void OnHotbarSlot7(InputAction.CallbackContext context);

		// Token: 0x06000752 RID: 1874
		void OnHotbarSlot8(InputAction.CallbackContext context);

		// Token: 0x06000753 RID: 1875
		void OnHotbarSlot9(InputAction.CallbackContext context);

		// Token: 0x06000754 RID: 1876
		void OnHotbarSlot10(InputAction.CallbackContext context);
	}
}
