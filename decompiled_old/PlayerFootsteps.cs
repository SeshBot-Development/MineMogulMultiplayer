using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
	public LayerMask GroundCheckLayerMask;

	public FootstepSoundDefinition DefaultFootstepDefinition;

	public FootstepSoundDefinition WaterFootstepDefinition;

	public float baseFootstepInterval = 0.6f;

	public float minMoveSpeed = 0.1f;

	public SoundPlayer SoundPlayerLeft;

	public SoundPlayer SoundPlayerRight;

	private float footstepTimer;

	private bool lastFootstepWasLeft;

	private FootstepSoundDefinition _currentFootstepDefinition;

	private PlayerController _playerController;

	private void Awake()
	{
		_currentFootstepDefinition = DefaultFootstepDefinition;
		_playerController = GetComponent<PlayerController>();
	}

	private void Update()
	{
		if (_playerController.IsUsingNoclip)
		{
			return;
		}
		float magnitude = (_playerController.MoveInput * _playerController.SelectedWalkSpeed).magnitude;
		if (magnitude > minMoveSpeed && _playerController.CharacterController.isGrounded)
		{
			footstepTimer -= Time.deltaTime;
			float t = Mathf.Clamp01(magnitude / _playerController.SprintSpeed);
			float num = Mathf.Lerp(baseFootstepInterval * 1.5f, baseFootstepInterval * 0.5f, t);
			if (footstepTimer <= 0f)
			{
				lastFootstepWasLeft = !lastFootstepWasLeft;
				PlayFootstep(lastFootstepWasLeft);
				footstepTimer = num;
			}
		}
		else
		{
			footstepTimer -= Time.deltaTime * 2f;
		}
	}

	private void PlayFootstep(bool isLeft)
	{
		bool flag = _playerController.IsInWater;
		if (!flag && Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo, 1.2f, GroundCheckLayerMask, QueryTriggerInteraction.Collide) && hitInfo.collider.gameObject.layer == 4)
		{
			flag = true;
		}
		_currentFootstepDefinition = (flag ? WaterFootstepDefinition : DefaultFootstepDefinition);
		float volumeMultiplier = ((_playerController.SelectedWalkSpeed <= _playerController.DuckSpeed) ? 0.5f : 1f);
		if (isLeft)
		{
			SoundPlayerLeft.PlaySound(_currentFootstepDefinition.LeftFootstepDefinition, volumeMultiplier);
		}
		else
		{
			SoundPlayerRight.PlaySound(_currentFootstepDefinition.RightFootstepDefinition, volumeMultiplier);
		}
	}
}
