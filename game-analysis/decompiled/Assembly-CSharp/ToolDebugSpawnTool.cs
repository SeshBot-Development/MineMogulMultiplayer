using System;
using TMPro;
using UnityEngine;

// Token: 0x020000EE RID: 238
public class ToolDebugSpawnTool : BaseHeldTool
{
	// Token: 0x06000654 RID: 1620 RVA: 0x00021085 File Offset: 0x0001F285
	protected override void OnEnable()
	{
		base.OnEnable();
		this.UpdateScreenUI();
	}

	// Token: 0x06000655 RID: 1621 RVA: 0x00021094 File Offset: 0x0001F294
	public override string GetControlsText()
	{
		return string.Concat(new string[]
		{
			"Drop Tool - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool),
			"\nSpawn Single - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.PrimaryAttack),
			"\nSpawn Multiple - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.SecondaryAttack),
			"\nClone Object - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.RotateObject),
			"\nOpen Menu - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.MirrorObject)
		});
	}

	// Token: 0x06000656 RID: 1622 RVA: 0x00021120 File Offset: 0x0001F320
	public void SpawnObject()
	{
		Camera componentInChildren = this.Owner.GetComponentInChildren<Camera>();
		if (componentInChildren == null)
		{
			return;
		}
		RaycastHit raycastHit;
		Vector3 vector;
		if (Physics.Raycast(componentInChildren.transform.position, componentInChildren.transform.forward, out raycastHit, this.SpawnRange, this.HitLayers))
		{
			vector = raycastHit.point;
		}
		else
		{
			vector = componentInChildren.transform.position + componentInChildren.transform.forward * this.SpawnRange;
		}
		Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(this.SelectedResourceType, this.SelectedPieceType, this.SelectedIsPolished, vector - componentInChildren.transform.forward * 0.25f, Quaternion.identity, null);
	}

	// Token: 0x06000657 RID: 1623 RVA: 0x000211E4 File Offset: 0x0001F3E4
	public void LaunchObject()
	{
		Camera componentInChildren = this.Owner.GetComponentInChildren<Camera>();
		if (componentInChildren == null)
		{
			return;
		}
		Vector3 vector = componentInChildren.transform.position + componentInChildren.transform.forward * 1f;
		Vector3 vector2 = Quaternion.Euler(Random.Range(-this.AngleSpread, this.AngleSpread), Random.Range(-this.AngleSpread, this.AngleSpread), 0f) * componentInChildren.transform.forward;
		Quaternion quaternion = Quaternion.LookRotation(vector2);
		OrePiece orePiece = Singleton<OrePiecePoolManager>.Instance.SpawnPooledOre(this.SelectedResourceType, this.SelectedPieceType, this.SelectedIsPolished, vector, quaternion, null);
		if (orePiece == null)
		{
			return;
		}
		Rigidbody component = orePiece.GetComponent<Rigidbody>();
		if (component == null)
		{
			return;
		}
		component.AddForce(vector2.normalized * this.LaunchForce, ForceMode.Impulse);
		Vector3 vector3 = new Vector3(Random.Range(-this.SpinForce, this.SpinForce), Random.Range(-this.SpinForce, this.SpinForce), Random.Range(-this.SpinForce, this.SpinForce));
		component.AddTorque(vector3, ForceMode.Impulse);
	}

	// Token: 0x06000658 RID: 1624 RVA: 0x00021314 File Offset: 0x0001F514
	public string GetSelectedObjectText()
	{
		return Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(this.SelectedResourceType, this.SelectedPieceType, this.SelectedIsPolished);
	}

	// Token: 0x06000659 RID: 1625 RVA: 0x00021334 File Offset: 0x0001F534
	private void SelectLookedAtObject()
	{
		Camera componentInChildren = this.Owner.GetComponentInChildren<Camera>();
		if (componentInChildren == null)
		{
			return;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(componentInChildren.transform.position, componentInChildren.transform.forward, out raycastHit, this.SpawnRange, this.HitLayers) && raycastHit.collider != null)
		{
			OrePiece component = raycastHit.collider.GetComponent<OrePiece>();
			if (component != null)
			{
				this.SelectedResourceType = component.ResourceType;
				this.SelectedPieceType = component.PieceType;
				this.SelectedIsPolished = component.IsPolished;
				Singleton<SoundManager>.Instance.PlayUISound(this.CloneSound, 1f);
				this.UpdateScreenUI();
				return;
			}
			OreNode component2 = raycastHit.collider.GetComponent<OreNode>();
			if (component2 != null)
			{
				OrePiece firstOrePrefab = component2.GetFirstOrePrefab();
				this.SelectedResourceType = firstOrePrefab.ResourceType;
				this.SelectedPieceType = firstOrePrefab.PieceType;
				this.SelectedIsPolished = firstOrePrefab.IsPolished;
				Singleton<SoundManager>.Instance.PlayUISound(this.CloneSound, 1f);
				this.UpdateScreenUI();
				return;
			}
		}
	}

	// Token: 0x0600065A RID: 1626 RVA: 0x00021453 File Offset: 0x0001F653
	private void OpenMenu()
	{
		this.UpdateScreenUI();
	}

	// Token: 0x0600065B RID: 1627 RVA: 0x0002145B File Offset: 0x0001F65B
	public void UpdateScreenUI()
	{
		this._selectedObjectText.text = this.GetSelectedObjectText();
	}

	// Token: 0x0600065C RID: 1628 RVA: 0x0002146E File Offset: 0x0001F66E
	public override void QButtonPressed()
	{
		this.OpenMenu();
	}

	// Token: 0x0600065D RID: 1629 RVA: 0x00021476 File Offset: 0x0001F676
	public override void PrimaryFire()
	{
		this.SpawnObject();
	}

	// Token: 0x0600065E RID: 1630 RVA: 0x0002147E File Offset: 0x0001F67E
	public override void Reload()
	{
		this.SelectLookedAtObject();
	}

	// Token: 0x0600065F RID: 1631 RVA: 0x00021486 File Offset: 0x0001F686
	public override void SecondaryFireHeld()
	{
		if ((in this._timeSinceLastSpawn) > this.SpawnRate)
		{
			this._timeSinceLastSpawn = 0f;
			this.LaunchObject();
		}
	}

	// Token: 0x0400079A RID: 1946
	public float LaunchForce = 5f;

	// Token: 0x0400079B RID: 1947
	public float AngleSpread = 5f;

	// Token: 0x0400079C RID: 1948
	public float SpinForce = 2f;

	// Token: 0x0400079D RID: 1949
	public float SpawnRate = 0.2f;

	// Token: 0x0400079E RID: 1950
	public float SpawnRange = 25f;

	// Token: 0x0400079F RID: 1951
	public LayerMask HitLayers;

	// Token: 0x040007A0 RID: 1952
	public SoundDefinition CloneSound;

	// Token: 0x040007A1 RID: 1953
	public ResourceType SelectedResourceType;

	// Token: 0x040007A2 RID: 1954
	public PieceType SelectedPieceType;

	// Token: 0x040007A3 RID: 1955
	public bool SelectedIsPolished;

	// Token: 0x040007A4 RID: 1956
	[SerializeField]
	private TMP_Text _selectedObjectText;

	// Token: 0x040007A5 RID: 1957
	private TimeSince _timeSinceLastSpawn;
}
