using System;
using UnityEngine;

// Token: 0x02000052 RID: 82
[DefaultExecutionOrder(-100)]
public class GameManager : Singleton<GameManager>
{
	// Token: 0x14000006 RID: 6
	// (add) Token: 0x06000224 RID: 548 RVA: 0x0000AE7C File Offset: 0x0000907C
	// (remove) Token: 0x06000225 RID: 549 RVA: 0x0000AEB4 File Offset: 0x000090B4
	public event Action GamePaused;

	// Token: 0x14000007 RID: 7
	// (add) Token: 0x06000226 RID: 550 RVA: 0x0000AEEC File Offset: 0x000090EC
	// (remove) Token: 0x06000227 RID: 551 RVA: 0x0000AF24 File Offset: 0x00009124
	public event Action GameUnpaused;

	// Token: 0x06000228 RID: 552 RVA: 0x0000AF59 File Offset: 0x00009159
	public void OnGamePauseToggled(bool isPaused)
	{
		if (isPaused)
		{
			Action gamePaused = this.GamePaused;
			if (gamePaused == null)
			{
				return;
			}
			gamePaused();
			return;
		}
		else
		{
			Action gameUnpaused = this.GameUnpaused;
			if (gameUnpaused == null)
			{
				return;
			}
			gameUnpaused();
			return;
		}
	}
}
