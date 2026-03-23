using System;
using UnityEngine;

// Token: 0x020000DE RID: 222
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	// Token: 0x17000024 RID: 36
	// (get) Token: 0x060005F5 RID: 1525 RVA: 0x0001F284 File Offset: 0x0001D484
	// (set) Token: 0x060005F6 RID: 1526 RVA: 0x0001F28B File Offset: 0x0001D48B
	public static T Instance { get; private set; }

	// Token: 0x060005F7 RID: 1527 RVA: 0x0001F294 File Offset: 0x0001D494
	protected virtual void Awake()
	{
		if (Singleton<T>.Instance == null)
		{
			Singleton<T>.Instance = this as T;
			return;
		}
		if (Singleton<T>.Instance != this)
		{
			Debug.Log(string.Format("{0} singleton already exists, destroying duplicate: {1}", typeof(T), base.gameObject.name));
			Object.Destroy(base.gameObject);
		}
	}
}
