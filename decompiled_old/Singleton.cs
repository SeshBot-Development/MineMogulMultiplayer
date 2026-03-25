using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T Instance { get; private set; }

	protected virtual void Awake()
	{
		if (Instance == null)
		{
			Instance = this as T;
		}
		else if (Instance != this)
		{
			Debug.Log($"{typeof(T)} singleton already exists, destroying duplicate: {base.gameObject.name}");
			Object.Destroy(base.gameObject);
		}
	}
}
