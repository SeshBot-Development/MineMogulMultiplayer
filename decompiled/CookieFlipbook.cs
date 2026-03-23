using UnityEngine;

[RequireComponent(typeof(Light))]
public class CookieFlipbook : MonoBehaviour
{
	public Texture[] frames;

	public float fps = 20f;

	private Light _light;

	private int _i;

	private float _t;

	private void Awake()
	{
		_light = GetComponent<Light>();
	}

	private void Update()
	{
		if (frames != null && frames.Length != 0)
		{
			_t += Time.deltaTime * fps;
			int num = (int)_t % frames.Length;
			if (num != _i)
			{
				_i = num;
				_light.cookie = frames[_i];
			}
		}
	}
}
