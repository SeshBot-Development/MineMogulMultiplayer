using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class StartingElevator : MonoBehaviour
{
	public float StartingHeight = 15f;

	public float EndHeight;

	public Transform PlayerTeleportPosition;

	public GameObject RoofCollider;

	public SoundPlayer SoundPlayer;

	public SoundDefinition LoweringSoundDefinition;

	public GameObject LandingParticle;

	private bool _isLowering;

	private bool _hasPlayedLandingParticle;

	private void OnEnable()
	{
		LandingParticle.SetActive(value: false);
		if (!Singleton<DebugManager>.Instance.DevModeEnabled && Singleton<SavingLoadingManager>.Instance.SceneWasLoadedFromNewGame)
		{
			TeleportPlayerAndLowerElevator();
		}
		else
		{
			RoofCollider.SetActive(value: false);
		}
		Singleton<GameManager>.Instance.GamePaused += OnGamePaused;
		Singleton<GameManager>.Instance.GameUnpaused += OnGameUnpaused;
	}

	private void OnDisable()
	{
		Singleton<GameManager>.Instance.GamePaused += OnGamePaused;
		Singleton<GameManager>.Instance.GameUnpaused += OnGameUnpaused;
	}

	private void Update()
	{
		if (_isLowering)
		{
			Vector3 localPosition = base.transform.localPosition;
			float value = Mathf.Max(0f, localPosition.y - EndHeight);
			float value2 = Mathf.InverseLerp(0.15f, 0f, value);
			float num = Mathf.Lerp(1.25f, 0.1f, Mathf.Clamp01(value2));
			float num2 = Mathf.Lerp(0.02f, 0f, Mathf.Clamp01(value2));
			localPosition.y -= num * Time.deltaTime;
			localPosition.x = Mathf.PerlinNoise(Time.time * 20f, 0f) * num2 - num2 / 2f;
			localPosition.z = Mathf.PerlinNoise(0f, Time.time * 20f) * num2 - num2 / 2f;
			if (!_hasPlayedLandingParticle && localPosition.y <= EndHeight + 1f)
			{
				_hasPlayedLandingParticle = true;
				LandingParticle.SetActive(value: true);
			}
			if (localPosition.y <= EndHeight + 0.001f)
			{
				localPosition.y = EndHeight;
				localPosition.x = 0f;
				localPosition.z = 0f;
				RoofCollider.SetActive(value: false);
				_isLowering = false;
			}
			base.transform.localPosition = localPosition;
		}
	}

	private void OnGamePaused()
	{
		if (_isLowering)
		{
			SoundPlayer.Pause();
		}
	}

	private void OnGameUnpaused()
	{
		if (_isLowering)
		{
			SoundPlayer.UnPause();
		}
	}

	public void TeleportPlayerAndLowerElevator()
	{
		LowerTheElevator();
		Object.FindObjectOfType<PlayerController>().TeleportPlayer(PlayerTeleportPosition.position);
	}

	public void LowerTheElevator()
	{
		LandingParticle.SetActive(value: false);
		RoofCollider.SetActive(value: true);
		base.transform.localPosition = new Vector3(0f, StartingHeight, 0f);
		_isLowering = true;
		StartCoroutine(WaitThenPlayElevatorSound());
	}

	public IEnumerator WaitThenPlayElevatorSound()
	{
		yield return new WaitForEndOfFrame();
		SoundPlayer.PlaySound(LoweringSoundDefinition);
		Object.FindObjectOfType<PlayerController>().TeleportPlayer(PlayerTeleportPosition.position);
	}
}
