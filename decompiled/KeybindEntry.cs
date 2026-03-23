using System;
using UnityEngine.InputSystem;

[Serializable]
public class KeybindEntry
{
	public KeybindAction key;

	public InputActionReference actionRef;

	public int bindingIndex;
}
