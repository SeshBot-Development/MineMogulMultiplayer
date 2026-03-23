using System;
using UnityEngine;

// Token: 0x02000105 RID: 261
public class GunShoot : MonoBehaviour
{
	// Token: 0x060006F0 RID: 1776 RVA: 0x00023351 File Offset: 0x00021551
	private void Start()
	{
		this.anim = base.GetComponent<Animator>();
		this.gunAim = base.GetComponentInParent<GunAim>();
	}

	// Token: 0x060006F1 RID: 1777 RVA: 0x0002336C File Offset: 0x0002156C
	private void Update()
	{
		if (Input.GetButtonDown("Fire1") && Time.time > this.nextFire && !this.gunAim.GetIsOutOfBounds())
		{
			this.nextFire = Time.time + this.fireRate;
			this.muzzleFlash.Play();
			this.cartridgeEjection.Play();
			this.anim.SetTrigger("Fire");
			RaycastHit raycastHit;
			if (Physics.Raycast(this.gunEnd.position, this.gunEnd.forward, out raycastHit, this.weaponRange))
			{
				this.HandleHit(raycastHit);
			}
		}
	}

	// Token: 0x060006F2 RID: 1778 RVA: 0x00023404 File Offset: 0x00021604
	private void HandleHit(RaycastHit hit)
	{
		if (hit.collider.sharedMaterial != null)
		{
			string name = hit.collider.sharedMaterial.name;
			uint num = <PrivateImplementationDetails>.ComputeStringHash(name);
			if (num <= 1044434307U)
			{
				if (num <= 329707512U)
				{
					if (num != 81868168U)
					{
						if (num != 329707512U)
						{
							return;
						}
						if (!(name == "WaterFilledExtinguish"))
						{
							return;
						}
						this.SpawnDecal(hit, this.waterLeakExtinguishEffect);
						this.SpawnDecal(hit, this.metalHitEffect);
					}
					else
					{
						if (!(name == "Wood"))
						{
							return;
						}
						this.SpawnDecal(hit, this.woodHitEffect);
						return;
					}
				}
				else if (num != 970575400U)
				{
					if (num != 1044434307U)
					{
						return;
					}
					if (!(name == "Sand"))
					{
						return;
					}
					this.SpawnDecal(hit, this.sandHitEffect);
					return;
				}
				else
				{
					if (!(name == "WaterFilled"))
					{
						return;
					}
					this.SpawnDecal(hit, this.waterLeakEffect);
					this.SpawnDecal(hit, this.metalHitEffect);
					return;
				}
			}
			else if (num <= 2840670588U)
			{
				if (num != 1842662042U)
				{
					if (num != 2840670588U)
					{
						return;
					}
					if (!(name == "Metal"))
					{
						return;
					}
					this.SpawnDecal(hit, this.metalHitEffect);
					return;
				}
				else
				{
					if (!(name == "Stone"))
					{
						return;
					}
					this.SpawnDecal(hit, this.stoneHitEffect);
					return;
				}
			}
			else if (num != 3966976176U)
			{
				if (num != 4022181330U)
				{
					return;
				}
				if (!(name == "Meat"))
				{
					return;
				}
				this.SpawnDecal(hit, this.fleshHitEffects[Random.Range(0, this.fleshHitEffects.Length)]);
				return;
			}
			else
			{
				if (!(name == "Character"))
				{
					return;
				}
				this.SpawnDecal(hit, this.fleshHitEffects[Random.Range(0, this.fleshHitEffects.Length)]);
				return;
			}
		}
	}

	// Token: 0x060006F3 RID: 1779 RVA: 0x000235BD File Offset: 0x000217BD
	private void SpawnDecal(RaycastHit hit, GameObject prefab)
	{
		Object.Instantiate<GameObject>(prefab, hit.point, Quaternion.LookRotation(hit.normal)).transform.SetParent(hit.collider.transform);
	}

	// Token: 0x040007F4 RID: 2036
	public float fireRate = 0.25f;

	// Token: 0x040007F5 RID: 2037
	public float weaponRange = 20f;

	// Token: 0x040007F6 RID: 2038
	public Transform gunEnd;

	// Token: 0x040007F7 RID: 2039
	public ParticleSystem muzzleFlash;

	// Token: 0x040007F8 RID: 2040
	public ParticleSystem cartridgeEjection;

	// Token: 0x040007F9 RID: 2041
	public GameObject metalHitEffect;

	// Token: 0x040007FA RID: 2042
	public GameObject sandHitEffect;

	// Token: 0x040007FB RID: 2043
	public GameObject stoneHitEffect;

	// Token: 0x040007FC RID: 2044
	public GameObject waterLeakEffect;

	// Token: 0x040007FD RID: 2045
	public GameObject waterLeakExtinguishEffect;

	// Token: 0x040007FE RID: 2046
	public GameObject[] fleshHitEffects;

	// Token: 0x040007FF RID: 2047
	public GameObject woodHitEffect;

	// Token: 0x04000800 RID: 2048
	private float nextFire;

	// Token: 0x04000801 RID: 2049
	private Animator anim;

	// Token: 0x04000802 RID: 2050
	private GunAim gunAim;
}
