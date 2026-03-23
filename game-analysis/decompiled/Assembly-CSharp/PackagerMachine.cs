using System;
using TMPro;
using UnityEngine;

// Token: 0x0200007D RID: 125
public class PackagerMachine : MonoBehaviour, ICustomSaveDataProvider
{
	// Token: 0x06000351 RID: 849 RVA: 0x00010D9A File Offset: 0x0000EF9A
	private void OnEnable()
	{
		this.UpdateManifestText();
	}

	// Token: 0x06000352 RID: 850 RVA: 0x00010DA4 File Offset: 0x0000EFA4
	private void AddOreToBox(OrePiece orePiece)
	{
		this.CurrentBoxContents.AddOrePiece(orePiece);
		orePiece.Delete();
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.AddObjectSoundEffect, this.AddObjectAudioPosition.position, 1f, 1f, true, false);
		if (this.CurrentBoxContents.GetCurrentVolume() >= this.CurrentBoxContents.MaxVolume)
		{
			this.SpawnNewBox();
			return;
		}
		this.UpdateManifestText();
	}

	// Token: 0x06000353 RID: 851 RVA: 0x00010E10 File Offset: 0x0000F010
	public void SpawnNewBox()
	{
		if (this.CurrentBoxContents.Contents.Count == 0)
		{
			Debug.Log("Cannot spawn box: No contents!");
			return;
		}
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.CompleteBoxSoundEffect, this.CompleteBoxAudioPosition.position, 1f, 1f, true, false);
		Object.Instantiate<BoxObject>(this.BoxPrefab, this.OutputTransform.position, this.OutputTransform.rotation).Initialize(this.CurrentBoxContents);
		this.CurrentBoxContents = new BoxContents();
		this.UpdateManifestText();
	}

	// Token: 0x06000354 RID: 852 RVA: 0x00010EA0 File Offset: 0x0000F0A0
	private void OnTriggerEnter(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			this.AddOreToBox(componentInParent);
		}
	}

	// Token: 0x06000355 RID: 853 RVA: 0x00010EC4 File Offset: 0x0000F0C4
	public void UpdateManifestText()
	{
		this._manifestText.text = this.CurrentBoxContents.GetManifestText();
	}

	// Token: 0x06000356 RID: 854 RVA: 0x00010EDC File Offset: 0x0000F0DC
	public virtual void LoadFromSave(string json)
	{
		BoxContents boxContents = JsonUtility.FromJson<BoxContents>(json);
		if (boxContents == null)
		{
			Debug.Log("Failed to load PackagerMachine's CurrentBoxContents data from save!");
			return;
		}
		this.CurrentBoxContents = boxContents;
		this.UpdateManifestText();
	}

	// Token: 0x06000357 RID: 855 RVA: 0x00010F0B File Offset: 0x0000F10B
	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(this.CurrentBoxContents);
	}

	// Token: 0x04000352 RID: 850
	[Header("Machine")]
	public Transform OutputTransform;

	// Token: 0x04000353 RID: 851
	public BoxObject BoxPrefab;

	// Token: 0x04000354 RID: 852
	public BoxContents CurrentBoxContents = new BoxContents();

	// Token: 0x04000355 RID: 853
	[Header("Manifest")]
	[SerializeField]
	private TMP_Text _manifestText;

	// Token: 0x04000356 RID: 854
	[Header("Audio")]
	[SerializeField]
	private SoundDefinition AddObjectSoundEffect;

	// Token: 0x04000357 RID: 855
	[SerializeField]
	private SoundDefinition CompleteBoxSoundEffect;

	// Token: 0x04000358 RID: 856
	[SerializeField]
	private Transform AddObjectAudioPosition;

	// Token: 0x04000359 RID: 857
	[SerializeField]
	private Transform CompleteBoxAudioPosition;
}
