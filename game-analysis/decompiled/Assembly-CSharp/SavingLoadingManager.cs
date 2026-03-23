using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020000C0 RID: 192
[DefaultExecutionOrder(-900)]
public class SavingLoadingManager : global::Singleton<SavingLoadingManager>
{
	// Token: 0x1700001E RID: 30
	// (get) Token: 0x06000529 RID: 1321 RVA: 0x0001ACEE File Offset: 0x00018EEE
	// (set) Token: 0x0600052A RID: 1322 RVA: 0x0001ACF6 File Offset: 0x00018EF6
	public bool IsCurrentlyLoadingGame { get; private set; }

	// Token: 0x0600052B RID: 1323 RVA: 0x0001AD00 File Offset: 0x00018F00
	protected override void Awake()
	{
		base.Awake();
		if (global::Singleton<SavingLoadingManager>.Instance != this)
		{
			return;
		}
		Object.DontDestroyOnLoad(base.gameObject);
		this.ResetSessionStartTime();
		this._totalPlayTimeSeconds = 0.0;
		this.TryToMigrateLegacySaveFileToNewLocation();
		this._lookup = new Dictionary<SavableObjectID, GameObject>();
		HashSet<SavableObjectID> hashSet = new HashSet<SavableObjectID>();
		foreach (GameObject gameObject in this.AllSavableObjectPrefabs)
		{
			if (gameObject == null)
			{
				Debug.LogError("SavingLoadingManager: Null prefab in list.");
			}
			else
			{
				ISaveLoadableObject component = gameObject.GetComponent<ISaveLoadableObject>();
				if (component == null)
				{
					Debug.LogError("SavingLoadingManager: Prefab '" + gameObject.name + "' is missing ISaveLoadableObject.");
				}
				else
				{
					SavableObjectID savableObjectID = component.GetSavableObjectID();
					if (savableObjectID == SavableObjectID.INVALID)
					{
						Debug.LogError("SavingLoadingManager: Prefab '" + gameObject.name + "' has INVALID SavableObjectID.");
					}
					else if (!hashSet.Add(savableObjectID))
					{
						Debug.LogError(string.Format("SavingLoadingManager: Duplicate SavableObjectID '{0}' found in prefab list.", savableObjectID));
					}
					else
					{
						this._lookup[savableObjectID] = gameObject;
					}
				}
			}
		}
		this._questLookup = new Dictionary<QuestID, QuestDefinition>();
		HashSet<QuestID> hashSet2 = new HashSet<QuestID>();
		foreach (QuestDefinition questDefinition in this.AllQuestDefinitions)
		{
			if (questDefinition == null)
			{
				Debug.LogError("Quest Validation: Null quest in list.");
			}
			else
			{
				QuestID questID = questDefinition.QuestID;
				if (questID == QuestID.INVALID)
				{
					Debug.LogError("Quest Validation: Quest '" + questDefinition.name + "' has INVALID QuestID.");
				}
				else if (!hashSet2.Add(questID))
				{
					Debug.LogError(string.Format("Quest Validation: Duplicate QuestID '{0}' found in prefab list.", questID));
				}
				else
				{
					this._questLookup[questID] = questDefinition;
				}
			}
		}
		this._orePieceLookup = new Dictionary<OrePieceKey, OrePiece>();
		foreach (OrePiece orePiece in this.AllOrePiecePrefabs)
		{
			if (orePiece == null)
			{
				Debug.LogError("Null OrePiece prefab in list.");
			}
			else
			{
				ResourceType resourceType = orePiece.ResourceType;
				PieceType pieceType = orePiece.PieceType;
				bool flag = orePiece.PolishedPercent > 0.95f;
				OrePieceKey orePieceKey = new OrePieceKey(resourceType, pieceType, flag);
				if (this._orePieceLookup.ContainsKey(orePieceKey))
				{
					Debug.LogError(string.Format("Duplicate OrePieceKey found for {0}. Skipping.", orePieceKey));
				}
				else
				{
					this._orePieceLookup[orePieceKey] = orePiece;
				}
			}
		}
		if (global::Singleton<DebugManager>.Instance != null && this.ValidateMissingSavableObjectIDs && global::Singleton<DebugManager>.Instance.DevModeEnabled)
		{
			foreach (object obj in Enum.GetValues(typeof(SavableObjectID)))
			{
				SavableObjectID savableObjectID2 = (SavableObjectID)obj;
				if (savableObjectID2 != SavableObjectID.INVALID && !this._lookup.ContainsKey(savableObjectID2))
				{
					Debug.Log(string.Format("SavingLoadingManager: No prefab assigned for SavableObjectID '{0}'.", savableObjectID2));
				}
			}
			foreach (object obj2 in Enum.GetValues(typeof(QuestID)))
			{
				QuestID questID2 = (QuestID)obj2;
				if (questID2 != QuestID.INVALID && !this._questLookup.ContainsKey(questID2))
				{
					Debug.Log(string.Format("Quest Validation: No QuestDefinition assigned for QuestID '{0}'.", questID2));
				}
			}
		}
	}

	// Token: 0x0600052C RID: 1324 RVA: 0x0001B0E4 File Offset: 0x000192E4
	public void ResetSessionStartTime()
	{
		this._sessionStartTime = Time.timeAsDouble;
	}

	// Token: 0x0600052D RID: 1325 RVA: 0x0001B0F4 File Offset: 0x000192F4
	public string GetFormattedLastSaveTime()
	{
		if (this.LastSaveTime == 0f)
		{
			return "Never";
		}
		float num = Time.time - this.LastSaveTime;
		int num2 = Mathf.FloorToInt(num / 60f);
		if (num2 < 1)
		{
			return string.Format("{0} seconds ago", Mathf.FloorToInt(num));
		}
		if (num2 == 1)
		{
			return "1 minute ago";
		}
		return num2.ToString() + " minutes ago";
	}

	// Token: 0x0600052E RID: 1326 RVA: 0x0001B164 File Offset: 0x00019364
	public bool IsSaveFileCompatible(int version)
	{
		return version == 15 || version == 4 || version == 5 || version == 6 || version == 7 || version == 8 || version == 9 || version == 10 || version == 11 || version == 12 || version == 13 || version == 14;
	}

	// Token: 0x0600052F RID: 1327 RVA: 0x0001B1C4 File Offset: 0x000193C4
	public static string GetFullSaveFilePath(string saveFileName, bool includeJsonType = true)
	{
		string text = Path.Combine(Application.persistentDataPath, "Saves");
		Directory.CreateDirectory(text);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(saveFileName);
		if (includeJsonType)
		{
			return Path.Combine(text, fileNameWithoutExtension + ".json");
		}
		return Path.Combine(text, fileNameWithoutExtension);
	}

	// Token: 0x06000530 RID: 1328 RVA: 0x0001B20C File Offset: 0x0001940C
	public static bool HasAnySaveFiles()
	{
		string text = Path.Combine(Application.persistentDataPath, "Saves");
		if (!Directory.Exists(text))
		{
			return false;
		}
		try
		{
			using (IEnumerator<string> enumerator = Directory.EnumerateFiles(text, "*", SearchOption.TopDirectoryOnly).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (string.Equals(Path.GetExtension(enumerator.Current), ".json", StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
			}
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
		return false;
	}

	// Token: 0x06000531 RID: 1329 RVA: 0x0001B2AC File Offset: 0x000194AC
	public static List<string> GetAllSaveFilePaths()
	{
		List<string> list = new List<string>();
		string text = Path.Combine(Application.persistentDataPath, "Saves");
		if (!Directory.Exists(text))
		{
			return list;
		}
		try
		{
			foreach (string text2 in Directory.EnumerateFiles(text, "*", SearchOption.TopDirectoryOnly))
			{
				if (string.Equals(Path.GetExtension(text2), ".json", StringComparison.OrdinalIgnoreCase))
				{
					list.Add(text2);
				}
			}
			list.Sort(StringComparer.OrdinalIgnoreCase);
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
		return list;
	}

	// Token: 0x06000532 RID: 1330 RVA: 0x0001B360 File Offset: 0x00019560
	public static List<SaveFileHeaderFileCombo> GetAllSaveFileHeaderFileCombos()
	{
		List<SaveFileHeaderFileCombo> list = new List<SaveFileHeaderFileCombo>();
		foreach (string text in SavingLoadingManager.GetAllSaveFilePaths())
		{
			SaveFileHeader saveFileHeader = SavingLoadingManager.GetSaveFileHeader(text);
			if (saveFileHeader != null)
			{
				list.Add(new SaveFileHeaderFileCombo(text, saveFileHeader));
			}
		}
		return list;
	}

	// Token: 0x06000533 RID: 1331 RVA: 0x0001B3CC File Offset: 0x000195CC
	public void TryToMigrateLegacySaveFileToNewLocation()
	{
		if (!this.HasLegacySaveFile())
		{
			return;
		}
		string text = Path.Combine(Application.persistentDataPath, "save.json");
		string fullSaveFilePath = SavingLoadingManager.GetFullSaveFilePath("Demo Game", true);
		string directoryName = Path.GetDirectoryName(fullSaveFilePath);
		if (!string.IsNullOrEmpty(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		if (File.Exists(fullSaveFilePath))
		{
			Debug.Log("Tried to migrate old save file, but a new one already exists in the desired location");
			return;
		}
		try
		{
			File.Move(text, fullSaveFilePath);
			Debug.Log("Migrated legacy save:" + text + " -> " + fullSaveFilePath);
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Format("Failed to migrate legacy save from '{0}' to '{1}'.\n{2}", text, fullSaveFilePath, ex));
		}
	}

	// Token: 0x06000534 RID: 1332 RVA: 0x0001B46C File Offset: 0x0001966C
	public bool HasLegacySaveFile()
	{
		return File.Exists(Path.Combine(Application.persistentDataPath, "save.json"));
	}

	// Token: 0x06000535 RID: 1333 RVA: 0x0001B484 File Offset: 0x00019684
	public void DeleteSaveFile(string fileName)
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			return;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
		string fullSaveFilePath = SavingLoadingManager.GetFullSaveFilePath(fileNameWithoutExtension, true);
		string text = SavingLoadingManager.GetFullSaveFilePath(fileNameWithoutExtension, false) + ".jpg";
		SavingLoadingManager.<DeleteSaveFile>g__TryDelete|31_0(fullSaveFilePath);
		SavingLoadingManager.<DeleteSaveFile>g__TryDelete|31_0(text);
	}

	// Token: 0x06000536 RID: 1334 RVA: 0x0001B4C4 File Offset: 0x000196C4
	public GameObject GetPrefab(SavableObjectID objectID)
	{
		GameObject gameObject;
		if (!this._lookup.TryGetValue(objectID, out gameObject))
		{
			return null;
		}
		return gameObject;
	}

	// Token: 0x06000537 RID: 1335 RVA: 0x0001B4E4 File Offset: 0x000196E4
	public OrePiece GetOrePiecePrefab(ResourceType resourceType, PieceType pieceType, bool isPolished)
	{
		OrePieceKey orePieceKey = new OrePieceKey(resourceType, pieceType, isPolished);
		OrePiece orePiece;
		if (this._orePieceLookup.TryGetValue(orePieceKey, out orePiece))
		{
			return orePiece;
		}
		new OrePieceKey(resourceType, pieceType, isPolished);
		OrePiece orePiece2;
		if (this._orePieceLookup.TryGetValue(orePieceKey, out orePiece2))
		{
			Debug.Log(string.Format("Loading: Couldn't find Polished prefab for: {0}, {1}, spawning Unpolished prefab instead", resourceType, pieceType));
			return orePiece2;
		}
		Debug.LogError(string.Format("Loading: {0}, {1}, {2} prefab is missing!", resourceType, pieceType, isPolished ? "Polished" : "Unpolished"));
		return null;
	}

	// Token: 0x06000538 RID: 1336 RVA: 0x0001B570 File Offset: 0x00019770
	public BuildingInventoryDefinition GetBuildingInventoryDefinition(SavableObjectID objectID)
	{
		GameObject prefab = this.GetPrefab(objectID);
		if (prefab == null)
		{
			Debug.Log(string.Format("GetBuildingInventoryDefinition: No prefab found for SavableObjectID '{0}'", objectID));
			return null;
		}
		BuildingObject component = prefab.GetComponent<BuildingObject>();
		if (component == null)
		{
			Debug.Log("GetBuildingInventoryDefinition: Prefab '" + prefab.name + "' is missing BuildingObject component");
			return null;
		}
		return component.Definition;
	}

	// Token: 0x06000539 RID: 1337 RVA: 0x0001B5D8 File Offset: 0x000197D8
	public QuestDefinition GetQuestDefinition(QuestID questID)
	{
		QuestDefinition questDefinition;
		if (!this._questLookup.TryGetValue(questID, out questDefinition))
		{
			return null;
		}
		return questDefinition;
	}

	// Token: 0x0600053A RID: 1338 RVA: 0x0001B5F8 File Offset: 0x000197F8
	public void AddDestroyedStaticBreakablePosition(Vector3 position)
	{
		this._destroyedStaticBreakablePositions.Add(position);
	}

	// Token: 0x0600053B RID: 1339 RVA: 0x0001B606 File Offset: 0x00019806
	public IEnumerator SaveJpgScreenshot(string filePathWithoutExt, int quality = 85)
	{
		if (string.IsNullOrWhiteSpace(filePathWithoutExt))
		{
			yield break;
		}
		SaveFileScreenshotCamera screenshotRig = Object.FindObjectOfType<SaveFileScreenshotCamera>();
		Camera captureCamera = ((screenshotRig != null) ? screenshotRig.Camera : null);
		if (captureCamera == null)
		{
			Debug.LogError("Couldn't find a camera to take the save file screenshot with!");
			yield break;
		}
		string directoryName = Path.GetDirectoryName(filePathWithoutExt);
		if (!string.IsNullOrEmpty(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		string jpgPath = Path.ChangeExtension(filePathWithoutExt, ".jpg");
		yield return new WaitForEndOfFrame();
		int pixelWidth = captureCamera.pixelWidth;
		int pixelHeight = captureCamera.pixelHeight;
		RenderTexture renderTexture = null;
		Texture2D texture2D = null;
		RenderTexture active = RenderTexture.active;
		try
		{
			screenshotRig.SSCC.enabled = true;
			RenderTextureDescriptor renderTextureDescriptor = new RenderTextureDescriptor(pixelWidth, pixelHeight, RenderTextureFormat.ARGB32, 24)
			{
				msaaSamples = 1,
				sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear)
			};
			renderTexture = RenderTexture.GetTemporary(renderTextureDescriptor);
			captureCamera.targetTexture = renderTexture;
			captureCamera.Render();
			RenderTexture.active = renderTexture;
			texture2D = new Texture2D(pixelWidth, pixelHeight, TextureFormat.RGB24, false, renderTextureDescriptor.sRGB);
			texture2D.ReadPixels(new Rect(0f, 0f, (float)pixelWidth, (float)pixelHeight), 0, 0);
			texture2D.Apply();
			byte[] array = texture2D.EncodeToJPG(Mathf.Clamp(quality, 1, 100));
			File.WriteAllBytes(jpgPath, array);
			screenshotRig.SSCC.enabled = false;
			yield break;
		}
		finally
		{
			RenderTexture.active = active;
			if (renderTexture != null)
			{
				RenderTexture.ReleaseTemporary(renderTexture);
			}
			if (texture2D != null)
			{
				Object.Destroy(texture2D);
			}
		}
		yield break;
	}

	// Token: 0x0600053C RID: 1340 RVA: 0x0001B61C File Offset: 0x0001981C
	public void LoadSceneAndStartNewSaveFile(string newSaveFileName, string sceneName)
	{
		if (this.IsCurrentlyLoadingGame)
		{
			return;
		}
		this.IsCurrentlyLoadingGame = true;
		this.SceneWasLoadedFromNewGame = true;
		this.ActiveSaveFileName = newSaveFileName;
		this._destroyedStaticBreakablePositions.Clear();
		base.StartCoroutine(this.LoadSceneForNewGame(sceneName));
	}

	// Token: 0x0600053D RID: 1341 RVA: 0x0001B655 File Offset: 0x00019855
	private IEnumerator LoadSceneForNewGame(string sceneName)
	{
		MainMenu mainMenu = Object.FindObjectOfType<MainMenu>();
		if (mainMenu != null)
		{
			yield return base.StartCoroutine(mainMenu.PlayElevatorLowerAnimation());
		}
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
		this.IsCurrentlyLoadingGame = false;
		this.ResetSessionStartTime();
		this._totalPlayTimeSeconds = 0.0;
		yield break;
	}

	// Token: 0x0600053E RID: 1342 RVA: 0x0001B66B File Offset: 0x0001986B
	public void LoadSceneThenLoadSave(string fullFilePath, string sceneName)
	{
		if (this.IsCurrentlyLoadingGame)
		{
			return;
		}
		this.IsCurrentlyLoadingGame = true;
		this.SceneWasLoadedFromNewGame = false;
		base.StartCoroutine(this.LoadSceneThenRunLoadGame(fullFilePath, sceneName));
	}

	// Token: 0x0600053F RID: 1343 RVA: 0x0001B693 File Offset: 0x00019893
	private IEnumerator LoadSceneThenRunLoadGame(string fullFilePath, string sceneName)
	{
		MainMenu mainMenu = Object.FindObjectOfType<MainMenu>();
		if (mainMenu != null)
		{
			yield return base.StartCoroutine(mainMenu.PlayElevatorLowerAnimation());
		}
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
		this.LoadGame(fullFilePath);
		yield break;
	}

	// Token: 0x06000540 RID: 1344 RVA: 0x0001B6B0 File Offset: 0x000198B0
	public void SaveGameWithActiveSaveFileName()
	{
		if (string.IsNullOrEmpty(this.ActiveSaveFileName))
		{
			Debug.LogError("Error: Tried to save the game, but there is no ActiveSaveFileName!");
			return;
		}
		this.SaveGame(this.ActiveSaveFileName, true);
	}

	// Token: 0x06000541 RID: 1345 RVA: 0x0001B6D8 File Offset: 0x000198D8
	private void TryBackupExistingSave(string fullFilePath)
	{
		if (!File.Exists(fullFilePath))
		{
			return;
		}
		if (SavingLoadingManager.GetSaveFileHeader(fullFilePath) == null)
		{
			Debug.LogWarning("Failed to create save backup, Original save files seems to be corrupted: " + fullFilePath);
			return;
		}
		try
		{
			string text = fullFilePath + ".bak";
			File.Copy(fullFilePath, text, true);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to create save backup: " + ex.Message);
		}
	}

	// Token: 0x06000542 RID: 1346 RVA: 0x0001B748 File Offset: 0x00019948
	public double GetTotalPlayTimeSeconds()
	{
		double num = Time.timeAsDouble - this._sessionStartTime;
		if (num < 0.0)
		{
			num = 0.0;
		}
		return this._totalPlayTimeSeconds + num;
	}

	// Token: 0x06000543 RID: 1347 RVA: 0x0001B780 File Offset: 0x00019980
	public static string GetFormattedPlaytime(double totalSeconds)
	{
		if (totalSeconds < 0.0)
		{
			totalSeconds = 0.0;
		}
		TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
		int num = (int)timeSpan.TotalHours;
		if (num < 1)
		{
			return string.Format("{0:D2}m {1:D2}s", timeSpan.Minutes, timeSpan.Seconds);
		}
		return string.Format("{0}h {1:D2}m {2:D2}s", num, timeSpan.Minutes, timeSpan.Seconds);
	}

	// Token: 0x06000544 RID: 1348 RVA: 0x0001B804 File Offset: 0x00019A04
	public void SaveGame(string saveFileName, bool shouldTakeScreenshot = true)
	{
		string fullSaveFilePath = SavingLoadingManager.GetFullSaveFilePath(saveFileName, true);
		this.TryBackupExistingSave(fullSaveFilePath);
		if (shouldTakeScreenshot)
		{
			base.StartCoroutine(this.SaveJpgScreenshot(fullSaveFilePath, 85));
		}
		this._totalPlayTimeSeconds = this.GetTotalPlayTimeSeconds();
		this.ResetSessionStartTime();
		SaveFileHeader saveFileHeader = new SaveFileHeader();
		saveFileHeader.SaveVersion = 15;
		saveFileHeader.GameVersion = global::Singleton<VersionManager>.Instance.VersionNumber;
		saveFileHeader.SaveTimestamp = DateTime.Now.ToString("o");
		saveFileHeader.LevelID = global::Singleton<LevelManager>.Instance.GetCurrentLevelID();
		saveFileHeader.Money = global::Singleton<EconomyManager>.Instance.Money;
		saveFileHeader.ResearchTickets = global::Singleton<ResearchManager>.Instance.ResearchTickets;
		saveFileHeader.TotalPlayTimeSeconds = this._totalPlayTimeSeconds;
		SaveFile saveFile = new SaveFile();
		saveFile.SaveVersion = saveFileHeader.SaveVersion;
		saveFile.GameVersion = saveFileHeader.GameVersion;
		saveFile.SaveTimestamp = saveFileHeader.SaveTimestamp;
		saveFile.LevelID = saveFileHeader.LevelID;
		saveFile.Money = saveFileHeader.Money;
		saveFile.ResearchTickets = saveFileHeader.ResearchTickets;
		saveFile.TotalPlayTimeSeconds = saveFileHeader.TotalPlayTimeSeconds;
		HashSet<ISaveLoadableObject> hashSet = Object.FindObjectsOfType<MonoBehaviour>().OfType<ISaveLoadableObject>().ToHashSet<ISaveLoadableObject>();
		hashSet.AddRange(Object.FindObjectOfType<PlayerInventory>().Items.Where((BaseHeldTool item) => item != null));
		foreach (ISaveLoadableObject saveLoadableObject in hashSet)
		{
			if (saveLoadableObject.ShouldBeSaved())
			{
				ISaveLoadableBuildingObject saveLoadableBuildingObject = saveLoadableObject as ISaveLoadableBuildingObject;
				if (saveLoadableBuildingObject != null)
				{
					BuildingObjectEntry buildingObjectEntry = new BuildingObjectEntry
					{
						SavableObjectID = saveLoadableObject.GetSavableObjectID(),
						Position = saveLoadableObject.GetPosition(),
						Rotation = saveLoadableObject.GetRotation(),
						BuildingSupportsEnable = saveLoadableBuildingObject.GetBuildingSupportsEnabled()
					};
					string customSaveData = saveLoadableObject.GetCustomSaveData();
					if (!string.IsNullOrEmpty(customSaveData))
					{
						buildingObjectEntry.CustomDataJson = customSaveData;
					}
					saveFile.BuildingObjects.Add(buildingObjectEntry);
				}
				else
				{
					SaveEntry saveEntry = new SaveEntry
					{
						SavableObjectID = saveLoadableObject.GetSavableObjectID(),
						Position = saveLoadableObject.GetPosition(),
						Rotation = saveLoadableObject.GetRotation()
					};
					string customSaveData2 = saveLoadableObject.GetCustomSaveData();
					if (!string.IsNullOrEmpty(customSaveData2))
					{
						saveEntry.CustomDataJson = customSaveData2;
					}
					saveFile.Entries.Add(saveEntry);
				}
			}
		}
		foreach (OrePiece orePiece in Object.FindObjectsOfType<OrePiece>())
		{
			OrePieceEntry orePieceEntry = new OrePieceEntry
			{
				Position = orePiece.transform.position,
				Rotation = orePiece.transform.rotation.eulerAngles,
				Scale = orePiece.transform.localScale,
				MeshID = orePiece.MeshID,
				ResourceType = orePiece.ResourceType,
				PieceType = orePiece.PieceType,
				PolishedPercent = orePiece.PolishedPercent
			};
			saveFile.OrePieces.Add(orePieceEntry);
		}
		foreach (ISaveLoadableWorldEvent saveLoadableWorldEvent in Object.FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveLoadableWorldEvent>().ToList<ISaveLoadableWorldEvent>())
		{
			if (saveLoadableWorldEvent.GetHasHappened())
			{
				WorldEventEntry worldEventEntry = new WorldEventEntry
				{
					SavableWorldEventType = saveLoadableWorldEvent.GetWorldEventType(),
					WorldEventID = saveLoadableWorldEvent.GetWorldEventID(),
					CustomDataJson = saveLoadableWorldEvent.GetCustomSaveData()
				};
				saveFile.WorldEventEntries.Add(worldEventEntry);
			}
		}
		saveFile.ShopPurchases = global::Singleton<EconomyManager>.Instance.ShopPurchases;
		saveFile.CompletedResearchItems = global::Singleton<ResearchManager>.Instance.CompletedResearchItems;
		saveFile.DestroyedStaticBreakablePositions = this._destroyedStaticBreakablePositions;
		saveFile.CompletedQuestsIDs = global::Singleton<QuestManager>.Instance.GetCompletedQuestIDs();
		saveFile.ActiveQuests = global::Singleton<QuestManager>.Instance.GetActiveQuestSaveEntries();
		PlayerController playerController = Object.FindObjectOfType<PlayerController>();
		saveFile.PlayerPosition = playerController.transform.position;
		saveFile.PlayerRotation = playerController.transform.rotation.eulerAngles;
		saveFile.HasShownOreLimitPopup = global::Singleton<OreLimitManager>.Instance.HasShownOreLimitPopup;
		string text = JsonUtility.ToJson(saveFile, true);
		this.WriteSaveAtomically(fullSaveFilePath, text);
		this.LastSaveTime = Time.time;
	}

	// Token: 0x06000545 RID: 1349 RVA: 0x0001BC50 File Offset: 0x00019E50
	private void WriteSaveAtomically(string fullFilePath, string json)
	{
		string text = fullFilePath + ".tmp";
		try
		{
			File.WriteAllText(text, json);
			if (File.Exists(fullFilePath))
			{
				File.Delete(fullFilePath);
			}
			File.Move(text, fullFilePath);
		}
		catch
		{
			try
			{
				if (File.Exists(text))
				{
					File.Delete(text);
				}
			}
			catch
			{
			}
			throw;
		}
	}

	// Token: 0x06000546 RID: 1350 RVA: 0x0001BCBC File Offset: 0x00019EBC
	public static SaveFileHeader GetSaveFileHeader(string fullFilePath)
	{
		if (!File.Exists(fullFilePath))
		{
			return null;
		}
		string text = File.ReadAllText(fullFilePath);
		try
		{
			SaveFileHeader saveFileHeader = JsonUtility.FromJson<SaveFileHeader>(text);
			if (saveFileHeader != null)
			{
				return saveFileHeader;
			}
		}
		catch
		{
			Debug.Log("Failed to read save file header from '" + fullFilePath + "'");
			return null;
		}
		return null;
	}

	// Token: 0x06000547 RID: 1351 RVA: 0x0001BD18 File Offset: 0x00019F18
	public SaveFileHeader GetLegacySaveFileHeader()
	{
		return SavingLoadingManager.GetSaveFileHeader(Path.Combine(Application.persistentDataPath, "save.json"));
	}

	// Token: 0x06000548 RID: 1352 RVA: 0x0001BD30 File Offset: 0x00019F30
	public void LoadGame(string fullFilePath)
	{
		this.IsCurrentlyLoadingGame = true;
		if (!File.Exists(fullFilePath))
		{
			this.IsCurrentlyLoadingGame = false;
			return;
		}
		foreach (ISaveLoadableObject saveLoadableObject in Object.FindObjectsOfType<MonoBehaviour>().OfType<ISaveLoadableObject>())
		{
			Object.Destroy(((MonoBehaviour)saveLoadableObject).gameObject);
		}
		SaveFile saveFile = JsonUtility.FromJson<SaveFile>(File.ReadAllText(fullFilePath));
		if (saveFile.SaveVersion != 1)
		{
			OrePiece[] array = Object.FindObjectsOfType<OrePiece>();
			for (int i = 0; i < array.Length; i++)
			{
				Object.Destroy(array[i].gameObject);
			}
		}
		this._destroyedStaticBreakablePositions = saveFile.DestroyedStaticBreakablePositions;
		foreach (ISaveLoadableStaticBreakable saveLoadableStaticBreakable in Object.FindObjectsOfType<MonoBehaviour>().OfType<ISaveLoadableStaticBreakable>())
		{
			if (this._destroyedStaticBreakablePositions.Contains(saveLoadableStaticBreakable.GetPosition()))
			{
				saveLoadableStaticBreakable.DestroyFromLoading();
			}
		}
		if (saveFile.SaveVersion < 11)
		{
			saveFile.CompletedQuestsIDs.RemoveAll((QuestID id) => id == QuestID.Mogul_MineMogul_Final);
			List<SaveEntry> list = saveFile.Entries.Where((SaveEntry entry) => entry.SavableObjectID == SavableObjectID.Trophy_MineMogul).ToList<SaveEntry>();
			if (list.Count != 0)
			{
				Debug.Log("Migrating MineMogul trophies to old version...");
				foreach (SaveEntry saveEntry in list)
				{
					saveEntry.SavableObjectID = SavableObjectID.Trophy_MineMogul_Broken;
				}
			}
		}
		foreach (SaveEntry saveEntry2 in saveFile.Entries)
		{
			GameObject prefab = this.GetPrefab(saveEntry2.SavableObjectID);
			ISaveLoadableObject saveLoadableObject2;
			if (prefab != null && Object.Instantiate<GameObject>(prefab, saveEntry2.Position, Quaternion.Euler(saveEntry2.Rotation)).TryGetComponent<ISaveLoadableObject>(out saveLoadableObject2))
			{
				saveLoadableObject2.LoadFromSave(saveEntry2.CustomDataJson);
			}
		}
		foreach (BuildingObjectEntry buildingObjectEntry in saveFile.BuildingObjects)
		{
			GameObject prefab2 = this.GetPrefab(buildingObjectEntry.SavableObjectID);
			ISaveLoadableBuildingObject saveLoadableBuildingObject;
			if (prefab2 != null && Object.Instantiate<GameObject>(prefab2, buildingObjectEntry.Position, Quaternion.Euler(buildingObjectEntry.Rotation)).TryGetComponent<ISaveLoadableBuildingObject>(out saveLoadableBuildingObject))
			{
				saveLoadableBuildingObject.LoadBuildingSaveData(buildingObjectEntry);
				saveLoadableBuildingObject.LoadFromSave(buildingObjectEntry.CustomDataJson);
			}
		}
		foreach (OrePieceEntry orePieceEntry in saveFile.OrePieces)
		{
			if (!Vector3Utils.IsValid(orePieceEntry.Position))
			{
				Debug.Log(string.Format("Loading - OrePiece ({0} {1}) has invalid position, skipping", orePieceEntry.ResourceType, orePieceEntry.PieceType));
			}
			else
			{
				OrePiece orePiecePrefab = this.GetOrePiecePrefab(orePieceEntry.ResourceType, orePieceEntry.PieceType, orePieceEntry.PolishedPercent > 0.95f);
				if (orePiecePrefab != null)
				{
					OrePiece orePiece = Object.Instantiate<OrePiece>(orePiecePrefab, orePieceEntry.Position, Quaternion.Euler(orePieceEntry.Rotation));
					orePiece.gameObject.name = orePiecePrefab.gameObject.name + " [Loaded]";
					if (saveFile.SaveVersion > 3)
					{
						orePiece.UseRandomScale = false;
						orePiece.transform.localScale = orePieceEntry.Scale;
						orePiece.UseRandomMesh = false;
						orePiece.MeshID = orePieceEntry.MeshID;
					}
					if (orePiece.PolishedPercent != 1f)
					{
						orePiece.PolishedPercent = orePieceEntry.PolishedPercent;
					}
					if (orePiece.PossibleSievedPrefabs.Count > 0)
					{
						orePiece.SievePercent = 0.8f;
					}
				}
			}
		}
		List<ISaveLoadableWorldEvent> list2 = Object.FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveLoadableWorldEvent>().ToList<ISaveLoadableWorldEvent>();
		foreach (WorldEventEntry worldEventEntry in saveFile.WorldEventEntries)
		{
			foreach (ISaveLoadableWorldEvent saveLoadableWorldEvent in list2)
			{
				if (saveLoadableWorldEvent.GetWorldEventID() == worldEventEntry.WorldEventID)
				{
					saveLoadableWorldEvent.LoadFromSave(worldEventEntry.CustomDataJson);
				}
			}
		}
		global::Singleton<EconomyManager>.Instance.ShopPurchases = saveFile.ShopPurchases;
		global::Singleton<EconomyManager>.Instance.SetMoney(saveFile.Money);
		global::Singleton<ResearchManager>.Instance.SetResearchTickets(saveFile.ResearchTickets);
		global::Singleton<ResearchManager>.Instance.LoadFromSaveFile(saveFile.CompletedResearchItems);
		global::Singleton<OreLimitManager>.Instance.HasShownOreLimitPopup = saveFile.HasShownOreLimitPopup;
		global::Singleton<QuestManager>.Instance.LoadFromSaveFile(saveFile);
		if (saveFile.SaveVersion < 15)
		{
			global::Singleton<ResearchManager>.Instance.MigrateNewResearchPrices();
		}
		Object.FindObjectOfType<PlayerInventory>().ClearInventory();
		Object.FindObjectOfType<PlayerController>().TeleportPlayer(saveFile.PlayerPosition, saveFile.PlayerRotation);
		this.ResetSessionStartTime();
		this._totalPlayTimeSeconds = saveFile.TotalPlayTimeSeconds;
		if (global::Singleton<UIManager>.Instance != null)
		{
			global::Singleton<UIManager>.Instance.PauseMenu.OnResumePressed();
		}
		this.LastSaveTime = Time.time;
		this.ActiveSaveFileName = Path.GetFileNameWithoutExtension(fullFilePath);
		this.IsCurrentlyLoadingGame = false;
		if (saveFile.SaveVersion < 15 && saveFile.LevelID == "BigCave")
		{
			foreach (AutoMiner autoMiner in Object.FindObjectsOfType<AutoMiner>())
			{
				if (autoMiner.enabled)
				{
					autoMiner.Toggle(false);
				}
			}
			global::Singleton<UIManager>.Instance.ShowInfoMessagePopup("New Update!", "The Big Cave has recieved an update! \nThere are now more explodable walls around the cave.\n \nAll Auto-Miners have been automatically turned off.\nIt's recommended to follow your conveyors explode any walls that may have been placed in their paths.");
		}
		if (saveFile.SaveVersion < 15 && saveFile.LevelID == "NewCave")
		{
			foreach (AutoMiner autoMiner2 in Object.FindObjectsOfType<AutoMiner>())
			{
				if (autoMiner2.enabled)
				{
					autoMiner2.Toggle(false);
				}
			}
			if (global::Singleton<UpdatedLevelNewObjectAdder>.Instance.AgeOfSteelObjectPrefab != null)
			{
				Object.Instantiate<GameObject>(global::Singleton<UpdatedLevelNewObjectAdder>.Instance.AgeOfSteelObjectPrefab);
			}
			global::Singleton<UIManager>.Instance.ShowInfoMessagePopup("Age of Steel", "Welcome to the Age of Steel Update! \nThis is MineMogul's biggest update yet, featuring many new machines and quests.\n \nThe 'Classic Cave' has been updated. Many areas (including the starting area) have been expanded to provide more space to build. Some Auto-Miner spots have been moved around and upgraded to Heavy Auto-Miner spots. Please take a look around your factory.\n \nFull patch notes are available on Steam. Hope you enjoy!");
		}
	}

	// Token: 0x0600054A RID: 1354 RVA: 0x0001C464 File Offset: 0x0001A664
	[CompilerGenerated]
	internal static void <DeleteSaveFile>g__TryDelete|31_0(string path)
	{
		try
		{
			if (File.Exists(path))
			{
				File.Delete(path);
				Debug.Log("Deleted : " + path);
			}
			else
			{
				Debug.Log("Couldn't find file to delete: " + path);
			}
		}
		catch (IOException ex)
		{
			Debug.LogError("IO error deleting " + path + ": " + ex.Message);
		}
		catch (UnauthorizedAccessException ex2)
		{
			Debug.LogError("No permission to delete " + path + ": " + ex2.Message);
		}
	}

	// Token: 0x04000656 RID: 1622
	public const int CurrentSaveFileVersion = 15;

	// Token: 0x04000658 RID: 1624
	public bool ValidateMissingSavableObjectIDs;

	// Token: 0x04000659 RID: 1625
	public List<GameObject> AllSavableObjectPrefabs;

	// Token: 0x0400065A RID: 1626
	public List<QuestDefinition> AllQuestDefinitions;

	// Token: 0x0400065B RID: 1627
	public List<OrePiece> AllOrePiecePrefabs;

	// Token: 0x0400065C RID: 1628
	public List<ResearchItemDefinition> AllResearchItemDefinitions;

	// Token: 0x0400065D RID: 1629
	public bool SceneWasLoadedFromNewGame = true;

	// Token: 0x0400065E RID: 1630
	public float LastSaveTime;

	// Token: 0x0400065F RID: 1631
	public string ActiveSaveFileName = "Editor Quick Save";

	// Token: 0x04000660 RID: 1632
	private Dictionary<SavableObjectID, GameObject> _lookup;

	// Token: 0x04000661 RID: 1633
	private Dictionary<QuestID, QuestDefinition> _questLookup;

	// Token: 0x04000662 RID: 1634
	private Dictionary<OrePieceKey, OrePiece> _orePieceLookup;

	// Token: 0x04000663 RID: 1635
	private List<Vector3> _destroyedStaticBreakablePositions = new List<Vector3>();

	// Token: 0x04000664 RID: 1636
	private double _totalPlayTimeSeconds;

	// Token: 0x04000665 RID: 1637
	private double _sessionStartTime;

	// Token: 0x04000666 RID: 1638
	private const string LegacySaveFilePath = "save.json";

	// Token: 0x04000667 RID: 1639
	private const string SaveFolderPath = "Saves";
}
