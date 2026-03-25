using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-10)]
public class KeybindManager : Singleton<KeybindManager>
{
	private const string KeyBindsFileName = "keybinds.json";

	[SerializeField]
	private List<KeybindEntry> _allBindings = new List<KeybindEntry>();

	private Dictionary<KeybindAction, KeybindEntry> _lookup;

	public bool HasUnsavedKeybindChanges;

	private static readonly Regex TokenRegex = new Regex("\\[([^\\[\\]]+)\\]", RegexOptions.Compiled);

	public PlayerInputActions Input { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		if (!(Singleton<KeybindManager>.Instance != this))
		{
			Input = new PlayerInputActions();
			LoadKeybinds();
			Input.Player.Enable();
			BuildLookup();
		}
	}

	private void BuildLookup()
	{
		_lookup = new Dictionary<KeybindAction, KeybindEntry>(_allBindings.Count);
		foreach (KeybindEntry allBinding in _allBindings)
		{
			if (allBinding != null && allBinding.key != KeybindAction.INAVLID)
			{
				_lookup[allBinding.key] = allBinding;
			}
		}
	}

	public InputAction GetRuntimeAction(InputActionReference actionRef)
	{
		if (actionRef?.action == null || Input == null)
		{
			return null;
		}
		return Input.asset.FindAction(actionRef.action.name);
	}

	public bool TryGetBinding(KeybindAction key, out InputAction action, out int bindingIndex)
	{
		action = null;
		bindingIndex = -1;
		if (!TryGetEntry(key, out var entry))
		{
			return false;
		}
		action = GetRuntimeAction(entry.actionRef);
		bindingIndex = entry.bindingIndex;
		if (action != null && bindingIndex >= 0)
		{
			return bindingIndex < action.bindings.Count;
		}
		return false;
	}

	public void LoadKeybinds()
	{
		if (Input == null)
		{
			Debug.LogError("Cannot load keybinds: Input is null");
			return;
		}
		string keyBindDataFilePath = GetKeyBindDataFilePath();
		if (!File.Exists(keyBindDataFilePath))
		{
			Debug.Log("No keybinds file found, using default keybinds.");
			return;
		}
		string json = File.ReadAllText(keyBindDataFilePath);
		Input.LoadBindingOverridesFromJson(json);
	}

	public void SaveKeybindsIfChanged()
	{
		if (HasUnsavedKeybindChanges)
		{
			SaveKeybinds();
		}
	}

	public void SaveKeybinds()
	{
		if (Input == null)
		{
			Debug.LogError("Cannot save keybinds: Input is null");
			return;
		}
		string contents = Input.SaveBindingOverridesAsJson();
		File.WriteAllText(GetKeyBindDataFilePath(), contents);
		HasUnsavedKeybindChanges = false;
		KeybindTokenText[] array = UnityEngine.Object.FindObjectsOfType<KeybindTokenText>(includeInactive: true);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Refresh();
		}
	}

	private bool TryGetEntry(KeybindAction key, out KeybindEntry entry)
	{
		if (_lookup == null)
		{
			BuildLookup();
		}
		if (_lookup.TryGetValue(key, out entry) && entry != null)
		{
			return entry.actionRef != null;
		}
		return false;
	}

	public string GetBindingText(KeybindAction key)
	{
		if (!TryGetEntry(key, out var entry))
		{
			return "Unbound";
		}
		return GetBindingText(entry.actionRef, entry.bindingIndex);
	}

	public void ResetBindingToDefault(KeybindAction key)
	{
		if (TryGetEntry(key, out var entry))
		{
			ResetBindingToDefault(entry.actionRef, entry.bindingIndex);
		}
	}

	public bool IsUsingDefaultBind(KeybindAction key)
	{
		if (!TryGetEntry(key, out var entry))
		{
			return true;
		}
		return IsUsingDefaultBind(entry.actionRef, entry.bindingIndex);
	}

	public void ResetBindingToDefault(InputActionReference actionRef, int bindingIndex)
	{
		GetRuntimeAction(actionRef)?.RemoveBindingOverride(bindingIndex);
		HasUnsavedKeybindChanges = true;
	}

	public string GetBindingText(InputActionReference actionRef, int bindingIndex)
	{
		InputAction runtimeAction = GetRuntimeAction(actionRef);
		if (runtimeAction != null)
		{
			return runtimeAction.GetBindingDisplayString(bindingIndex);
		}
		return "Unbound";
	}

	public bool IsUsingDefaultBind(InputActionReference actionRef, int bindingIndex)
	{
		InputAction runtimeAction = GetRuntimeAction(actionRef);
		if (runtimeAction == null)
		{
			return true;
		}
		if (bindingIndex < 0 || bindingIndex >= runtimeAction.bindings.Count)
		{
			return true;
		}
		return string.IsNullOrEmpty(runtimeAction.bindings[bindingIndex].overridePath);
	}

	public string GetKeyBindDataFilePath()
	{
		return Path.Combine(Application.persistentDataPath, "keybinds.json");
	}

	public string ReplaceKeybindTokens(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		KeybindAction result;
		return TokenRegex.Replace(input, (Match match) => Enum.TryParse<KeybindAction>(match.Groups[1].Value, out result) ? GetBindingText(result) : match.Value);
	}
}
