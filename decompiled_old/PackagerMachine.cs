using TMPro;
using UnityEngine;

public class PackagerMachine : MonoBehaviour, ICustomSaveDataProvider
{
	[Header("Machine")]
	public Transform OutputTransform;

	public BoxObject BoxPrefab;

	public BoxContents CurrentBoxContents = new BoxContents();

	[Header("Manifest")]
	[SerializeField]
	private TMP_Text _manifestText;

	[Header("Audio")]
	[SerializeField]
	private SoundDefinition AddObjectSoundEffect;

	[SerializeField]
	private SoundDefinition CompleteBoxSoundEffect;

	[SerializeField]
	private Transform AddObjectAudioPosition;

	[SerializeField]
	private Transform CompleteBoxAudioPosition;

	private void OnEnable()
	{
		UpdateManifestText();
	}

	private void AddOreToBox(OrePiece orePiece)
	{
		CurrentBoxContents.AddOrePiece(orePiece);
		orePiece.Delete();
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(AddObjectSoundEffect, AddObjectAudioPosition.position);
		if (CurrentBoxContents.GetCurrentVolume() >= CurrentBoxContents.MaxVolume)
		{
			SpawnNewBox();
		}
		else
		{
			UpdateManifestText();
		}
	}

	public void SpawnNewBox()
	{
		if (CurrentBoxContents.Contents.Count == 0)
		{
			Debug.Log("Cannot spawn box: No contents!");
			return;
		}
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(CompleteBoxSoundEffect, CompleteBoxAudioPosition.position);
		Object.Instantiate(BoxPrefab, OutputTransform.position, OutputTransform.rotation).Initialize(CurrentBoxContents);
		CurrentBoxContents = new BoxContents();
		UpdateManifestText();
	}

	private void OnTriggerEnter(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			AddOreToBox(componentInParent);
		}
	}

	public void UpdateManifestText()
	{
		_manifestText.text = CurrentBoxContents.GetManifestText();
	}

	public virtual void LoadFromSave(string json)
	{
		BoxContents boxContents = JsonUtility.FromJson<BoxContents>(json);
		if (boxContents == null)
		{
			Debug.Log("Failed to load PackagerMachine's CurrentBoxContents data from save!");
			return;
		}
		CurrentBoxContents = boxContents;
		UpdateManifestText();
	}

	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(CurrentBoxContents);
	}
}
