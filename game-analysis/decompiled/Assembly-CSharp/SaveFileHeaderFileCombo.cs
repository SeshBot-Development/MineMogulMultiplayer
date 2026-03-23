using System;

// Token: 0x020000CC RID: 204
public class SaveFileHeaderFileCombo
{
	// Token: 0x1700001F RID: 31
	// (get) Token: 0x0600055C RID: 1372 RVA: 0x0001C7A4 File Offset: 0x0001A9A4
	// (set) Token: 0x0600055D RID: 1373 RVA: 0x0001C7AC File Offset: 0x0001A9AC
	public string FullFilePath { get; private set; }

	// Token: 0x17000020 RID: 32
	// (get) Token: 0x0600055E RID: 1374 RVA: 0x0001C7B5 File Offset: 0x0001A9B5
	// (set) Token: 0x0600055F RID: 1375 RVA: 0x0001C7BD File Offset: 0x0001A9BD
	public SaveFileHeader SaveFileHeader { get; private set; }

	// Token: 0x06000560 RID: 1376 RVA: 0x0001C7C6 File Offset: 0x0001A9C6
	public SaveFileHeaderFileCombo(string fullFilePath, SaveFileHeader saveFileHeader)
	{
		this.FullFilePath = fullFilePath;
		this.SaveFileHeader = saveFileHeader;
	}
}
