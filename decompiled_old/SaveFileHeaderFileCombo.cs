public class SaveFileHeaderFileCombo
{
	public string FullFilePath { get; private set; }

	public SaveFileHeader SaveFileHeader { get; private set; }

	public SaveFileHeaderFileCombo(string fullFilePath, SaveFileHeader saveFileHeader)
	{
		FullFilePath = fullFilePath;
		SaveFileHeader = saveFileHeader;
	}
}
