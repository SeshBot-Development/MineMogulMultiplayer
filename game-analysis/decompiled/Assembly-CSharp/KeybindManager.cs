using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;

// Token: 0x02000061 RID: 97
[DefaultExecutionOrder(-10)]
public class KeybindManager : Singleton<KeybindManager>
{
	// Token: 0x17000015 RID: 21
	// (get) Token: 0x0600027E RID: 638 RVA: 0x0000C635 File Offset: 0x0000A835
	// (set) Token: 0x0600027F RID: 639 RVA: 0x0000C63D File Offset: 0x0000A83D
	public PlayerInputActions Input { get; private set; }

	// Token: 0x06000280 RID: 640 RVA: 0x0000C648 File Offset: 0x0000A848
	protected override void Awake()
	{
		base.Awake();
		if (Singleton<KeybindManager>.Instance != this)
		{
			return;
		}
		this.Input = new PlayerInputActions();
		this.LoadKeybinds();
		this.Input.Player.Enable();
		this.BuildLookup();
	}

	// Token: 0x06000281 RID: 641 RVA: 0x0000C694 File Offset: 0x0000A894
	private void BuildLookup()
	{
		this._lookup = new Dictionary<KeybindAction, KeybindEntry>(this._allBindings.Count);
		foreach (KeybindEntry keybindEntry in this._allBindings)
		{
			if (keybindEntry != null && keybindEntry.key != KeybindAction.INAVLID)
			{
				this._lookup[keybindEntry.key] = keybindEntry;
			}
		}
	}

	// Token: 0x06000282 RID: 642 RVA: 0x0000C714 File Offset: 0x0000A914
	public InputAction GetRuntimeAction(InputActionReference actionRef)
	{
		if (((actionRef != null) ? actionRef.action : null) == null || this.Input == null)
		{
			return null;
		}
		return this.Input.asset.FindAction(actionRef.action.name, false);
	}

	// Token: 0x06000283 RID: 643 RVA: 0x0000C74C File Offset: 0x0000A94C
	public bool TryGetBinding(KeybindAction key, out InputAction action, out int bindingIndex)
	{
		action = null;
		bindingIndex = -1;
		KeybindEntry keybindEntry;
		if (!this.TryGetEntry(key, out keybindEntry))
		{
			return false;
		}
		action = this.GetRuntimeAction(keybindEntry.actionRef);
		bindingIndex = keybindEntry.bindingIndex;
		return action != null && bindingIndex >= 0 && bindingIndex < action.bindings.Count;
	}

	// Token: 0x06000284 RID: 644 RVA: 0x0000C7A0 File Offset: 0x0000A9A0
	public void LoadKeybinds()
	{
		if (this.Input == null)
		{
			Debug.LogError("Cannot load keybinds: Input is null");
			return;
		}
		string keyBindDataFilePath = this.GetKeyBindDataFilePath();
		if (!File.Exists(keyBindDataFilePath))
		{
			Debug.Log("No keybinds file found, using default keybinds.");
			return;
		}
		string text = File.ReadAllText(keyBindDataFilePath);
		this.Input.LoadBindingOverridesFromJson(text, true);
	}

	// Token: 0x06000285 RID: 645 RVA: 0x0000C7EE File Offset: 0x0000A9EE
	public void SaveKeybindsIfChanged()
	{
		if (this.HasUnsavedKeybindChanges)
		{
			this.SaveKeybinds();
		}
	}

	// Token: 0x06000286 RID: 646 RVA: 0x0000C800 File Offset: 0x0000AA00
	public void SaveKeybinds()
	{
		if (this.Input == null)
		{
			Debug.LogError("Cannot save keybinds: Input is null");
			return;
		}
		string text = this.Input.SaveBindingOverridesAsJson();
		File.WriteAllText(this.GetKeyBindDataFilePath(), text);
		this.HasUnsavedKeybindChanges = false;
		KeybindTokenText[] array = Object.FindObjectsOfType<KeybindTokenText>(true);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Refresh();
		}
	}

	// Token: 0x06000287 RID: 647 RVA: 0x0000C85C File Offset: 0x0000AA5C
	private bool TryGetEntry(KeybindAction key, out KeybindEntry entry)
	{
		if (this._lookup == null)
		{
			this.BuildLookup();
		}
		return this._lookup.TryGetValue(key, out entry) && entry != null && entry.actionRef != null;
	}

	// Token: 0x06000288 RID: 648 RVA: 0x0000C890 File Offset: 0x0000AA90
	public string GetBindingText(KeybindAction key)
	{
		KeybindEntry keybindEntry;
		if (!this.TryGetEntry(key, out keybindEntry))
		{
			return "Unbound";
		}
		return this.GetBindingText(keybindEntry.actionRef, keybindEntry.bindingIndex);
	}

	// Token: 0x06000289 RID: 649 RVA: 0x0000C8C0 File Offset: 0x0000AAC0
	public void ResetBindingToDefault(KeybindAction key)
	{
		KeybindEntry keybindEntry;
		if (!this.TryGetEntry(key, out keybindEntry))
		{
			return;
		}
		this.ResetBindingToDefault(keybindEntry.actionRef, keybindEntry.bindingIndex);
	}

	// Token: 0x0600028A RID: 650 RVA: 0x0000C8EC File Offset: 0x0000AAEC
	public bool IsUsingDefaultBind(KeybindAction key)
	{
		KeybindEntry keybindEntry;
		return !this.TryGetEntry(key, out keybindEntry) || this.IsUsingDefaultBind(keybindEntry.actionRef, keybindEntry.bindingIndex);
	}

	// Token: 0x0600028B RID: 651 RVA: 0x0000C918 File Offset: 0x0000AB18
	public void ResetBindingToDefault(InputActionReference actionRef, int bindingIndex)
	{
		InputAction runtimeAction = this.GetRuntimeAction(actionRef);
		if (runtimeAction != null)
		{
			runtimeAction.RemoveBindingOverride(bindingIndex);
		}
		this.HasUnsavedKeybindChanges = true;
	}

	// Token: 0x0600028C RID: 652 RVA: 0x0000C934 File Offset: 0x0000AB34
	public string GetBindingText(InputActionReference actionRef, int bindingIndex)
	{
		InputAction runtimeAction = this.GetRuntimeAction(actionRef);
		if (runtimeAction != null)
		{
			return runtimeAction.GetBindingDisplayString(bindingIndex, (InputBinding.DisplayStringOptions)0);
		}
		return "Unbound";
	}

	// Token: 0x0600028D RID: 653 RVA: 0x0000C95C File Offset: 0x0000AB5C
	public bool IsUsingDefaultBind(InputActionReference actionRef, int bindingIndex)
	{
		InputAction runtimeAction = this.GetRuntimeAction(actionRef);
		return runtimeAction == null || (bindingIndex < 0 || bindingIndex >= runtimeAction.bindings.Count) || string.IsNullOrEmpty(runtimeAction.bindings[bindingIndex].overridePath);
	}

	// Token: 0x0600028E RID: 654 RVA: 0x0000C9A9 File Offset: 0x0000ABA9
	public string GetKeyBindDataFilePath()
	{
		return Path.Combine(Application.persistentDataPath, "keybinds.json");
	}

	// Token: 0x0600028F RID: 655 RVA: 0x0000C9BA File Offset: 0x0000ABBA
	public string ReplaceKeybindTokens(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		return KeybindManager.TokenRegex.Replace(input, delegate(Match match)
		{
			KeybindAction keybindAction;
			if (Enum.TryParse<KeybindAction>(match.Groups[1].Value, out keybindAction))
			{
				return this.GetBindingText(keybindAction);
			}
			return match.Value;
		});
	}

	// Token: 0x0400024C RID: 588
	private const string KeyBindsFileName = "keybinds.json";

	// Token: 0x0400024D RID: 589
	[SerializeField]
	private List<KeybindEntry> _allBindings = new List<KeybindEntry>();

	// Token: 0x0400024E RID: 590
	private Dictionary<KeybindAction, KeybindEntry> _lookup;

	// Token: 0x0400024F RID: 591
	public bool HasUnsavedKeybindChanges;

	// Token: 0x04000250 RID: 592
	private static readonly Regex TokenRegex = new Regex("\\[([^\\[\\]]+)\\]", RegexOptions.Compiled);
}
