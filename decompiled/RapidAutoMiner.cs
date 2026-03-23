using System.Collections;
using UnityEngine;

public class RapidAutoMiner : AutoMiner, IInteractable, ICustomSaveDataProvider
{
	[Header("-- RapidAutoMiner --")]
	public RapidAutoMinerDrillBit AttachedDrillBit;

	public Transform DrillBitEjectPoint;

	public Transform DrillBitAttachmentParent;

	public Animator Animator;

	public BuildingObject BuildingObject;

	public OrePiece BrokenDrillBitPrefab;

	public SoundDefinition DrillBitBreakSound;

	public override void ConfigureFromDefinition()
	{
		if (ResourceDefinition != null)
		{
			SpawnProbability = ResourceDefinition.SpawnProbability;
			SpawnRate = ResourceDefinition.SpawnRate * 0.25f;
		}
		else
		{
			Debug.Log("AutoMiner doesn't have a resource definition!");
		}
	}

	protected override void OnEnable()
	{
		Enabled = AttachedDrillBit != null;
		BuildingObject.OnBuildingRemoved += EjectCurrentDrillBit;
		base.OnEnable();
	}

	private void OnDisable()
	{
		BuildingObject.OnBuildingRemoved -= EjectCurrentDrillBit;
	}

	protected override void Update()
	{
		if (Enabled && AttachedDrillBit != null)
		{
			AttachedDrillBit.UseDrillBit(Time.deltaTime);
			if (!AttachedDrillBit.LastsForever && AttachedDrillBit.IsBroken())
			{
				BreakCurrentDrillBit();
			}
		}
		if (Enabled && AttachedDrillBit != null)
		{
			if (ResourceDefinition != null)
			{
				float currentRate = AttachedDrillBit.GetCurrentRate();
				float num = AttachedDrillBit.DrillSpeedByDurabilityCurve.Evaluate(currentRate);
				SpawnRate = ResourceDefinition.SpawnRate * num;
			}
			base.Update();
		}
	}

	public void BreakCurrentDrillBit()
	{
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(DrillBitBreakSound, DrillBitAttachmentParent.position);
		Object.Destroy(AttachedDrillBit.gameObject);
		StartCoroutine(SpawnBrokenDrillBitPieces());
		Animator.speed = 0f;
		_audioSource_Loop.Pause();
		ChangeLightMaterial(Singleton<BuildingManager>.Instance.OrangeLightMaterial);
	}

	private IEnumerator SpawnBrokenDrillBitPieces()
	{
		yield return new WaitForSeconds(0.4f);
		if (!(base.gameObject == null) && base.gameObject.activeInHierarchy)
		{
			OrePiece orePiece = Object.Instantiate(BrokenDrillBitPrefab, OreSpawnPoint.position, OreSpawnPoint.rotation);
			orePiece.MeshID = 1;
			orePiece.UseRandomMesh = false;
			yield return new WaitForSeconds(0.5f);
			if (!(base.gameObject == null) && base.gameObject.activeInHierarchy)
			{
				OrePiece orePiece2 = Object.Instantiate(BrokenDrillBitPrefab, OreSpawnPoint.position, OreSpawnPoint.rotation);
				orePiece2.MeshID = 0;
				orePiece2.UseRandomMesh = false;
			}
		}
	}

	public void AttachDrillBit(RapidAutoMinerDrillBit drillBit, bool shouldTurnOnAutominer)
	{
		if (drillBit.IsBroken())
		{
			Debug.Log("Cannot attach a broken drill bit.");
			return;
		}
		EjectCurrentDrillBit();
		AttachedDrillBit = drillBit;
		drillBit.AttachedAutoMiner = this;
		drillBit.transform.SetParent(DrillBitAttachmentParent);
		drillBit.transform.localPosition = Vector3.zero;
		drillBit.transform.localRotation = Quaternion.identity;
		if (shouldTurnOnAutominer)
		{
			TurnOn();
		}
	}

	public void EjectCurrentDrillBit()
	{
		if (!(AttachedDrillBit == null))
		{
			AttachedDrillBit.AttachedAutoMiner = null;
			AttachedDrillBit.transform.SetParent(null);
			AttachedDrillBit.transform.position = DrillBitEjectPoint.position;
			AttachedDrillBit.transform.rotation = DrillBitEjectPoint.rotation;
			Rigidbody componentInChildren = AttachedDrillBit.GetComponentInChildren<Rigidbody>();
			if (componentInChildren != null)
			{
				componentInChildren.isKinematic = false;
			}
			AttachedDrillBit = null;
			Animator.speed = 0f;
			_audioSource_Loop.Pause();
			ChangeLightMaterial(Singleton<BuildingManager>.Instance.OrangeLightMaterial);
		}
	}

	protected override void TurnOn()
	{
		if (AttachedDrillBit == null)
		{
			Debug.Log("Cannot turn on RapidAutoMiner without a drill bit attached.");
			return;
		}
		Animator.speed = 1f;
		base.TurnOn();
	}

	protected override void TurnOff()
	{
		Animator.speed = 0f;
		base.TurnOff();
	}

	public override void Interact(Interaction selectedInteraction)
	{
		switch (selectedInteraction.Name)
		{
		case "Turn On":
			TurnOn();
			break;
		case "Turn Off":
			TurnOff();
			break;
		case "Eject Drill Bit":
			EjectCurrentDrillBit();
			break;
		}
	}

	public override void LoadFromSave(string json)
	{
		AutoMinerSaveData autoMinerSaveData = JsonUtility.FromJson<AutoMinerSaveData>(json);
		if (autoMinerSaveData == null)
		{
			autoMinerSaveData = new AutoMinerSaveData();
		}
		if (autoMinerSaveData.IsOn)
		{
			StartCoroutine(WaitThenTryTurnOn());
		}
	}

	private IEnumerator WaitThenTryTurnOn()
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		if (AttachedDrillBit != null)
		{
			TurnOn();
		}
	}
}
