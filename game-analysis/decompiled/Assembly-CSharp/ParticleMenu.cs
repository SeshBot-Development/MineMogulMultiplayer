using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000108 RID: 264
public class ParticleMenu : MonoBehaviour
{
	// Token: 0x060006F9 RID: 1785 RVA: 0x0002368C File Offset: 0x0002188C
	private void Start()
	{
		this.Navigate(0);
		this.currentIndex = 0;
	}

	// Token: 0x060006FA RID: 1786 RVA: 0x0002369C File Offset: 0x0002189C
	public void Navigate(int i)
	{
		this.currentIndex = (this.particleSystems.Length + this.currentIndex + i) % this.particleSystems.Length;
		if (this.currentGO != null)
		{
			Object.Destroy(this.currentGO);
		}
		this.currentGO = Object.Instantiate<GameObject>(this.particleSystems[this.currentIndex].particleSystemGO, this.spawnLocation.position + this.particleSystems[this.currentIndex].particlePosition, Quaternion.Euler(this.particleSystems[this.currentIndex].particleRotation));
		this.gunGameObject.SetActive(this.particleSystems[this.currentIndex].isWeaponEffect);
		this.title.text = this.particleSystems[this.currentIndex].title;
		this.description.text = this.particleSystems[this.currentIndex].description;
		this.navigationDetails.text = (this.currentIndex + 1).ToString() + " out of " + this.particleSystems.Length.ToString();
	}

	// Token: 0x0400080B RID: 2059
	public ParticleExamples[] particleSystems;

	// Token: 0x0400080C RID: 2060
	public GameObject gunGameObject;

	// Token: 0x0400080D RID: 2061
	private int currentIndex;

	// Token: 0x0400080E RID: 2062
	private GameObject currentGO;

	// Token: 0x0400080F RID: 2063
	public Transform spawnLocation;

	// Token: 0x04000810 RID: 2064
	public Text title;

	// Token: 0x04000811 RID: 2065
	public Text description;

	// Token: 0x04000812 RID: 2066
	public Text navigationDetails;
}
