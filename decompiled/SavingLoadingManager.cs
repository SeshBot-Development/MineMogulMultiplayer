using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-900)]
public class SavingLoadingManager : Singleton<SavingLoadingManager>
{
	public const int CurrentSaveFileVersion = 16;

	public bool ValidateMissingSavableObjectIDs;

	public List<GameObject> AllSavableObjectPrefabs;

	public List<QuestDefinition> AllQuestDefinitions;

	public List<OrePiece> AllOrePiecePrefabs;

	public List<ResearchItemDefinition> AllResearchItemDefinitions;

	public bool SceneWasLoadedFromNewGame = true;

	public float LastSaveTime;

	public string ActiveSaveFileName = "Editor Quick Save";

	private Dictionary<SavableObjectID, GameObject> _lookup;

	private Dictionary<QuestID, QuestDefinition> _questLookup;

	private Dictionary<OrePieceKey, OrePiece> _orePieceLookup;

	private List<Vector3> _destroyedStaticBreakablePositions = new List<Vector3>();

	private double _totalPlayTimeSeconds;

	private double _sessionStartTime;

	private const string LegacySaveFilePath = "save.json";

	private const string SaveFolderPath = "Saves";

	public bool IsCurrentlyLoadingGame { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		if (Singleton<SavingLoadingManager>.Instance != this)
		{
			return;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		ResetSessionStartTime();
		_totalPlayTimeSeconds = 0.0;
		TryToMigrateLegacySaveFileToNewLocation();
		_lookup = new Dictionary<SavableObjectID, GameObject>();
		HashSet<SavableObjectID> hashSet = new HashSet<SavableObjectID>();
		foreach (GameObject allSavableObjectPrefab in AllSavableObjectPrefabs)
		{
			if (allSavableObjectPrefab == null)
			{
				Debug.LogError("SavingLoadingManager: Null prefab in list.");
				continue;
			}
			ISaveLoadableObject component = allSavableObjectPrefab.GetComponent<ISaveLoadableObject>();
			if (component == null)
			{
				Debug.LogError("SavingLoadingManager: Prefab '" + allSavableObjectPrefab.name + "' is missing ISaveLoadableObject.");
				continue;
			}
			SavableObjectID savableObjectID = component.GetSavableObjectID();
			if (savableObjectID == SavableObjectID.INVALID)
			{
				Debug.LogError("SavingLoadingManager: Prefab '" + allSavableObjectPrefab.name + "' has INVALID SavableObjectID.");
			}
			else if (!hashSet.Add(savableObjectID))
			{
				Debug.LogError($"SavingLoadingManager: Duplicate SavableObjectID '{savableObjectID}' found in prefab list.");
			}
			else
			{
				_lookup[savableObjectID] = allSavableObjectPrefab;
			}
		}
		_questLookup = new Dictionary<QuestID, QuestDefinition>();
		HashSet<QuestID> hashSet2 = new HashSet<QuestID>();
		foreach (QuestDefinition allQuestDefinition in AllQuestDefinitions)
		{
			if (allQuestDefinition == null)
			{
				Debug.LogError("Quest Validation: Null quest in list.");
				continue;
			}
			QuestID questID = allQuestDefinition.QuestID;
			if (questID == QuestID.INVALID)
			{
				Debug.LogError("Quest Validation: Quest '" + allQuestDefinition.name + "' has INVALID QuestID.");
			}
			else if (!hashSet2.Add(questID))
			{
				Debug.LogError($"Quest Validation: Duplicate QuestID '{questID}' found in prefab list.");
			}
			else
			{
				_questLookup[questID] = allQuestDefinition;
			}
		}
		_orePieceLookup = new Dictionary<OrePieceKey, OrePiece>();
		foreach (OrePiece allOrePiecePrefab in AllOrePiecePrefabs)
		{
			if (allOrePiecePrefab == null)
			{
				Debug.LogError("Null OrePiece prefab in list.");
				continue;
			}
			ResourceType resourceType = allOrePiecePrefab.ResourceType;
			PieceType pieceType = allOrePiecePrefab.PieceType;
			bool isPolished = allOrePiecePrefab.PolishedPercent > 0.95f;
			OrePieceKey orePieceKey = new OrePieceKey(resourceType, pieceType, isPolished);
			if (_orePieceLookup.ContainsKey(orePieceKey))
			{
				Debug.LogError($"Duplicate OrePieceKey found for {orePieceKey}. Skipping.");
			}
			else
			{
				_orePieceLookup[orePieceKey] = allOrePiecePrefab;
			}
		}
		if (!(Singleton<DebugManager>.Instance != null) || !ValidateMissingSavableObjectIDs || !Singleton<DebugManager>.Instance.DevModeEnabled)
		{
			return;
		}
		foreach (SavableObjectID value in Enum.GetValues(typeof(SavableObjectID)))
		{
			if (value != SavableObjectID.INVALID && !_lookup.ContainsKey(value))
			{
				Debug.Log($"SavingLoadingManager: No prefab assigned for SavableObjectID '{value}'.");
			}
		}
		foreach (QuestID value2 in Enum.GetValues(typeof(QuestID)))
		{
			if (value2 != QuestID.INVALID && !_questLookup.ContainsKey(value2))
			{
				Debug.Log($"Quest Validation: No QuestDefinition assigned for QuestID '{value2}'.");
			}
		}
	}

	public void ResetSessionStartTime()
	{
		_sessionStartTime = Time.timeAsDouble;
	}

	public string GetFormattedLastSaveTime()
	{
		if (LastSaveTime == 0f)
		{
			return "Never";
		}
		float num = Time.time - LastSaveTime;
		int num2 = Mathf.FloorToInt(num / 60f);
		if (num2 < 1)
		{
			return $"{Mathf.FloorToInt(num)} seconds ago";
		}
		if (num2 == 1)
		{
			return "1 minute ago";
		}
		return num2 + " minutes ago";
	}

	public bool IsSaveFileCompatible(int version)
	{
		return version switch
		{
			16 => true, 
			4 => true, 
			5 => true, 
			6 => true, 
			7 => true, 
			8 => true, 
			9 => true, 
			10 => true, 
			11 => true, 
			12 => true, 
			13 => true, 
			14 => true, 
			15 => true, 
			_ => false, 
		};
	}

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

	public static bool HasAnySaveFiles()
	{
		string path = Path.Combine(Application.persistentDataPath, "Saves");
		if (!Directory.Exists(path))
		{
			return false;
		}
		try
		{
			foreach (string item in Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly))
			{
				if (string.Equals(Path.GetExtension(item), ".json", StringComparison.OrdinalIgnoreCase))
				{
					return true;
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

	public static List<string> GetAllSaveFilePaths()
	{
		List<string> list = new List<string>();
		string path = Path.Combine(Application.persistentDataPath, "Saves");
		if (!Directory.Exists(path))
		{
			return list;
		}
		try
		{
			foreach (string item in Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly))
			{
				if (string.Equals(Path.GetExtension(item), ".json", StringComparison.OrdinalIgnoreCase))
				{
					list.Add(item);
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

	public static List<SaveFileHeaderFileCombo> GetAllSaveFileHeaderFileCombos()
	{
		List<SaveFileHeaderFileCombo> list = new List<SaveFileHeaderFileCombo>();
		foreach (string allSaveFilePath in GetAllSaveFilePaths())
		{
			SaveFileHeader saveFileHeader = GetSaveFileHeader(allSaveFilePath);
			if (saveFileHeader != null)
			{
				list.Add(new SaveFileHeaderFileCombo(allSaveFilePath, saveFileHeader));
			}
		}
		return list;
	}

	public void TryToMigrateLegacySaveFileToNewLocation()
	{
		if (!HasLegacySaveFile())
		{
			return;
		}
		string text = Path.Combine(Application.persistentDataPath, "save.json");
		string fullSaveFilePath = GetFullSaveFilePath("Demo Game");
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
		catch (Exception arg)
		{
			Debug.LogError($"Failed to migrate legacy save from '{text}' to '{fullSaveFilePath}'.\n{arg}");
		}
	}

	public bool HasLegacySaveFile()
	{
		return File.Exists(Path.Combine(Application.persistentDataPath, "save.json"));
	}

	public void DeleteSaveFile(string fileName)
	{
		if (!string.IsNullOrWhiteSpace(fileName))
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
			string fullSaveFilePath = GetFullSaveFilePath(fileNameWithoutExtension);
			string path = GetFullSaveFilePath(fileNameWithoutExtension, includeJsonType: false) + ".jpg";
			TryDelete(fullSaveFilePath);
			TryDelete(path);
		}
		static void TryDelete(string text)
		{
			try
			{
				if (File.Exists(text))
				{
					File.Delete(text);
					Debug.Log("Deleted : " + text);
				}
				else
				{
					Debug.Log("Couldn't find file to delete: " + text);
				}
			}
			catch (IOException ex)
			{
				Debug.LogError("IO error deleting " + text + ": " + ex.Message);
			}
			catch (UnauthorizedAccessException ex2)
			{
				Debug.LogError("No permission to delete " + text + ": " + ex2.Message);
			}
		}
	}

	public GameObject GetPrefab(SavableObjectID objectID)
	{
		if (!_lookup.TryGetValue(objectID, out var value))
		{
			return null;
		}
		return value;
	}

	public OrePiece GetOrePiecePrefab(ResourceType resourceType, PieceType pieceType, bool isPolished)
	{
		OrePieceKey key = new OrePieceKey(resourceType, pieceType, isPolished);
		if (_orePieceLookup.TryGetValue(key, out var value))
		{
			return value;
		}
		new OrePieceKey(resourceType, pieceType, isPolished);
		if (_orePieceLookup.TryGetValue(key, out var value2))
		{
			Debug.Log($"Loading: Couldn't find Polished prefab for: {resourceType}, {pieceType}, spawning Unpolished prefab instead");
			return value2;
		}
		Debug.LogError(string.Format("Loading: {0}, {1}, {2} prefab is missing!", resourceType, pieceType, isPolished ? "Polished" : "Unpolished"));
		return null;
	}

	public BuildingInventoryDefinition GetBuildingInventoryDefinition(SavableObjectID objectID)
	{
		GameObject prefab = GetPrefab(objectID);
		if (prefab == null)
		{
			Debug.Log($"GetBuildingInventoryDefinition: No prefab found for SavableObjectID '{objectID}'");
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

	public QuestDefinition GetQuestDefinition(QuestID questID)
	{
		if (!_questLookup.TryGetValue(questID, out var value))
		{
			return null;
		}
		return value;
	}

	public void AddDestroyedStaticBreakablePosition(Vector3 position)
	{
		_destroyedStaticBreakablePositions.Add(position);
	}

	public IEnumerator SaveJpgScreenshot(string filePathWithoutExt, int quality = 85)
	{
		if (string.IsNullOrWhiteSpace(filePathWithoutExt))
		{
			yield break;
		}
		SaveFileScreenshotCamera screenshotRig = UnityEngine.Object.FindObjectOfType<SaveFileScreenshotCamera>();
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
			RenderTextureDescriptor renderTextureDescriptor = new RenderTextureDescriptor(pixelWidth, pixelHeight, RenderTextureFormat.ARGB32, 24);
			renderTextureDescriptor.msaaSamples = 1;
			renderTextureDescriptor.sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear;
			RenderTextureDescriptor desc = renderTextureDescriptor;
			renderTexture = (captureCamera.targetTexture = RenderTexture.GetTemporary(desc));
			captureCamera.Render();
			RenderTexture.active = renderTexture;
			texture2D = new Texture2D(pixelWidth, pixelHeight, TextureFormat.RGB24, mipChain: false, desc.sRGB);
			texture2D.ReadPixels(new Rect(0f, 0f, pixelWidth, pixelHeight), 0, 0);
			texture2D.Apply();
			byte[] bytes = texture2D.EncodeToJPG(Mathf.Clamp(quality, 1, 100));
			File.WriteAllBytes(jpgPath, bytes);
			screenshotRig.SSCC.enabled = false;
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
				UnityEngine.Object.Destroy(texture2D);
			}
		}
	}

	public void LoadSceneAndStartNewSaveFile(string newSaveFileName, string sceneName)
	{
		if (!IsCurrentlyLoadingGame)
		{
			IsCurrentlyLoadingGame = true;
			SceneWasLoadedFromNewGame = true;
			ActiveSaveFileName = newSaveFileName;
			_destroyedStaticBreakablePositions.Clear();
			StartCoroutine(LoadSceneForNewGame(sceneName));
		}
	}

	private IEnumerator LoadSceneForNewGame(string sceneName)
	{
		MainMenu mainMenu = UnityEngine.Object.FindObjectOfType<MainMenu>();
		if (mainMenu != null)
		{
			yield return StartCoroutine(mainMenu.PlayElevatorLowerAnimation());
		}
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
		IsCurrentlyLoadingGame = false;
		ResetSessionStartTime();
		_totalPlayTimeSeconds = 0.0;
	}

	public void LoadSceneThenLoadSave(string fullFilePath, string sceneName)
	{
		if (!IsCurrentlyLoadingGame)
		{
			IsCurrentlyLoadingGame = true;
			SceneWasLoadedFromNewGame = false;
			StartCoroutine(LoadSceneThenRunLoadGame(fullFilePath, sceneName));
		}
	}

	private IEnumerator LoadSceneThenRunLoadGame(string fullFilePath, string sceneName)
	{
		MainMenu mainMenu = UnityEngine.Object.FindObjectOfType<MainMenu>();
		if (mainMenu != null)
		{
			yield return StartCoroutine(mainMenu.PlayElevatorLowerAnimation());
		}
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
		LoadGame(fullFilePath);
	}

	public void SaveGameWithActiveSaveFileName()
	{
		if (string.IsNullOrEmpty(ActiveSaveFileName))
		{
			Debug.LogError("Error: Tried to save the game, but there is no ActiveSaveFileName!");
		}
		else
		{
			SaveGame(ActiveSaveFileName);
		}
	}

	private void TryBackupExistingSave(string fullFilePath)
	{
		if (!File.Exists(fullFilePath))
		{
			return;
		}
		if (GetSaveFileHeader(fullFilePath) == null)
		{
			Debug.LogWarning("Failed to create save backup, Original save files seems to be corrupted: " + fullFilePath);
			return;
		}
		try
		{
			string destFileName = fullFilePath + ".bak";
			File.Copy(fullFilePath, destFileName, overwrite: true);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to create save backup: " + ex.Message);
		}
	}

	public double GetTotalPlayTimeSeconds()
	{
		double num = Time.timeAsDouble - _sessionStartTime;
		if (num < 0.0)
		{
			num = 0.0;
		}
		return _totalPlayTimeSeconds + num;
	}

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
			return $"{timeSpan.Minutes:D2}m {timeSpan.Seconds:D2}s";
		}
		return $"{num}h {timeSpan.Minutes:D2}m {timeSpan.Seconds:D2}s";
	}

	public void SaveGame(string saveFileName, bool shouldTakeScreenshot = true)
	{
		string fullSaveFilePath = GetFullSaveFilePath(saveFileName);
		TryBackupExistingSave(fullSaveFilePath);
		if (shouldTakeScreenshot)
		{
			StartCoroutine(SaveJpgScreenshot(fullSaveFilePath));
		}
		_totalPlayTimeSeconds = GetTotalPlayTimeSeconds();
		ResetSessionStartTime();
		SaveFileHeader saveFileHeader = new SaveFileHeader();
		saveFileHeader.SaveVersion = 16;
		saveFileHeader.GameVersion = Singleton<VersionManager>.Instance.VersionNumber;
		saveFileHeader.SaveTimestamp = DateTime.Now.ToString("o");
		saveFileHeader.LevelID = Singleton<LevelManager>.Instance.GetCurrentLevelID();
		saveFileHeader.Money = Singleton<EconomyManager>.Instance.Money;
		saveFileHeader.ResearchTickets = Singleton<ResearchManager>.Instance.ResearchTickets;
		saveFileHeader.TotalPlayTimeSeconds = _totalPlayTimeSeconds;
		saveFileHeader.IsDemoVersion = Singleton<DemoManager>.Instance.IsDemoVersion;
		saveFileHeader.GameMode = Singleton<GamemodeManager>.Instance.GameModeType;
		SaveFile saveFile = new SaveFile();
		saveFile.SaveVersion = saveFileHeader.SaveVersion;
		saveFile.GameVersion = saveFileHeader.GameVersion;
		saveFile.SaveTimestamp = saveFileHeader.SaveTimestamp;
		saveFile.LevelID = saveFileHeader.LevelID;
		saveFile.Money = saveFileHeader.Money;
		saveFile.ResearchTickets = saveFileHeader.ResearchTickets;
		saveFile.TotalPlayTimeSeconds = saveFileHeader.TotalPlayTimeSeconds;
		saveFile.IsDemoVersion = saveFileHeader.IsDemoVersion;
		saveFile.GameMode = saveFileHeader.GameMode;
		HashSet<ISaveLoadableObject> hashSet = Enumerable.ToHashSet(UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().OfType<ISaveLoadableObject>());
		PlayerInventory playerInventory = UnityEngine.Object.FindObjectOfType<PlayerInventory>();
		hashSet.AddRange(playerInventory.Items.Where((BaseHeldTool baseHeldTool) => baseHeldTool != null));
		foreach (ISaveLoadableObject item3 in hashSet)
		{
			if (!item3.ShouldBeSaved())
			{
				continue;
			}
			if (item3 is ISaveLoadableBuildingObject saveLoadableBuildingObject)
			{
				BuildingObjectEntry buildingObjectEntry = new BuildingObjectEntry
				{
					SavableObjectID = item3.GetSavableObjectID(),
					Position = item3.GetPosition(),
					Rotation = item3.GetRotation(),
					BuildingSupportsEnable = saveLoadableBuildingObject.GetBuildingSupportsEnabled()
				};
				string customSaveData = item3.GetCustomSaveData();
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
					SavableObjectID = item3.GetSavableObjectID(),
					Position = item3.GetPosition(),
					Rotation = item3.GetRotation()
				};
				string customSaveData2 = item3.GetCustomSaveData();
				if (!string.IsNullOrEmpty(customSaveData2))
				{
					saveEntry.CustomDataJson = customSaveData2;
				}
				saveFile.Entries.Add(saveEntry);
			}
		}
		OrePiece[] array = UnityEngine.Object.FindObjectsOfType<OrePiece>();
		foreach (OrePiece orePiece in array)
		{
			OrePieceEntry item = new OrePieceEntry
			{
				Position = orePiece.transform.position,
				Rotation = orePiece.transform.rotation.eulerAngles,
				Scale = orePiece.transform.localScale,
				MeshID = orePiece.MeshID,
				ResourceType = orePiece.ResourceType,
				PieceType = orePiece.PieceType,
				PolishedPercent = orePiece.PolishedPercent
			};
			saveFile.OrePieces.Add(item);
		}
		foreach (ISaveLoadableWorldEvent item4 in UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(includeInactive: true).OfType<ISaveLoadableWorldEvent>().ToList())
		{
			if (item4.GetHasHappened())
			{
				WorldEventEntry item2 = new WorldEventEntry
				{
					SavableWorldEventType = item4.GetWorldEventType(),
					WorldEventID = item4.GetWorldEventID(),
					CustomDataJson = item4.GetCustomSaveData()
				};
				saveFile.WorldEventEntries.Add(item2);
			}
		}
		saveFile.ShopPurchases = Singleton<EconomyManager>.Instance.ShopPurchases;
		saveFile.CompletedResearchItems = Singleton<ResearchManager>.Instance.CompletedResearchItems;
		saveFile.DestroyedStaticBreakablePositions = _destroyedStaticBreakablePositions;
		saveFile.CompletedQuestsIDs = Singleton<QuestManager>.Instance.GetCompletedQuestIDs();
		saveFile.ActiveQuests = Singleton<QuestManager>.Instance.GetActiveQuestSaveEntries();
		PlayerController playerController = UnityEngine.Object.FindObjectOfType<PlayerController>();
		saveFile.PlayerPosition = playerController.transform.position;
		saveFile.PlayerRotation = playerController.transform.rotation.eulerAngles;
		saveFile.HasShownOreLimitPopup = Singleton<OreLimitManager>.Instance.HasShownOreLimitPopup;
		saveFile.LastActiveInventorySlot = playerInventory.GetActiveToolSlotIndex();
		string json = JsonUtility.ToJson(saveFile, prettyPrint: true);
		WriteSaveAtomically(fullSaveFilePath, json);
		LastSaveTime = Time.time;
	}

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

	public static SaveFileHeader GetSaveFileHeader(string fullFilePath)
	{
		if (!File.Exists(fullFilePath))
		{
			return null;
		}
		string json = File.ReadAllText(fullFilePath);
		try
		{
			SaveFileHeader saveFileHeader = JsonUtility.FromJson<SaveFileHeader>(json);
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

	public SaveFileHeader GetLegacySaveFileHeader()
	{
		return GetSaveFileHeader(Path.Combine(Application.persistentDataPath, "save.json"));
	}

	public void LoadGame(string fullFilePath)
	{
		IsCurrentlyLoadingGame = true;
		if (!File.Exists(fullFilePath))
		{
			IsCurrentlyLoadingGame = false;
			return;
		}
		foreach (MonoBehaviour item in UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().OfType<ISaveLoadableObject>())
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		SaveFile saveFile = JsonUtility.FromJson<SaveFile>(File.ReadAllText(fullFilePath));
		if (saveFile.SaveVersion != 1)
		{
			OrePiece[] array = UnityEngine.Object.FindObjectsOfType<OrePiece>();
			for (int i = 0; i < array.Length; i++)
			{
				UnityEngine.Object.Destroy(array[i].gameObject);
			}
		}
		_destroyedStaticBreakablePositions = saveFile.DestroyedStaticBreakablePositions;
		foreach (ISaveLoadableStaticBreakable item2 in UnityEngine.Object.FindObjectsOfType<MonoBehaviour>().OfType<ISaveLoadableStaticBreakable>())
		{
			if (_destroyedStaticBreakablePositions.Contains(item2.GetPosition()))
			{
				item2.DestroyFromLoading();
			}
		}
		if (saveFile.SaveVersion < 11)
		{
			saveFile.CompletedQuestsIDs.RemoveAll((QuestID id) => id == QuestID.Mogul_MineMogul_Final);
			List<SaveEntry> list = saveFile.Entries.Where((SaveEntry entry) => entry.SavableObjectID == SavableObjectID.Trophy_MineMogul).ToList();
			if (list.Count != 0)
			{
				Debug.Log("Migrating MineMogul trophies to old version...");
				foreach (SaveEntry item3 in list)
				{
					item3.SavableObjectID = SavableObjectID.Trophy_MineMogul_Broken;
				}
			}
		}
		foreach (SaveEntry entry in saveFile.Entries)
		{
			GameObject prefab = GetPrefab(entry.SavableObjectID);
			if (prefab != null && UnityEngine.Object.Instantiate(prefab, entry.Position, Quaternion.Euler(entry.Rotation)).TryGetComponent<ISaveLoadableObject>(out var component))
			{
				component.LoadFromSave(entry.CustomDataJson);
			}
		}
		foreach (BuildingObjectEntry buildingObject in saveFile.BuildingObjects)
		{
			GameObject prefab2 = GetPrefab(buildingObject.SavableObjectID);
			if (prefab2 != null && UnityEngine.Object.Instantiate(prefab2, buildingObject.Position, Quaternion.Euler(buildingObject.Rotation)).TryGetComponent<ISaveLoadableBuildingObject>(out var component2))
			{
				component2.LoadBuildingSaveData(buildingObject);
				component2.LoadFromSave(buildingObject.CustomDataJson);
			}
		}
		foreach (OrePieceEntry orePiece2 in saveFile.OrePieces)
		{
			if (!Vector3Utils.IsValid(orePiece2.Position))
			{
				Debug.Log($"Loading - OrePiece ({orePiece2.ResourceType} {orePiece2.PieceType}) has invalid position, skipping");
				continue;
			}
			OrePiece orePiecePrefab = GetOrePiecePrefab(orePiece2.ResourceType, orePiece2.PieceType, orePiece2.PolishedPercent > 0.95f);
			if (orePiecePrefab != null)
			{
				OrePiece orePiece = UnityEngine.Object.Instantiate(orePiecePrefab, orePiece2.Position, Quaternion.Euler(orePiece2.Rotation));
				orePiece.gameObject.name = orePiecePrefab.gameObject.name + " [Loaded]";
				if (saveFile.SaveVersion > 3)
				{
					orePiece.UseRandomScale = false;
					orePiece.transform.localScale = orePiece2.Scale;
					orePiece.UseRandomMesh = false;
					orePiece.MeshID = orePiece2.MeshID;
				}
				if (orePiece.PolishedPercent != 1f)
				{
					orePiece.PolishedPercent = orePiece2.PolishedPercent;
				}
				if (orePiece.PossibleSievedPrefabs.Count > 0)
				{
					orePiece.SievePercent = 0.8f;
				}
			}
		}
		List<ISaveLoadableWorldEvent> list2 = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(includeInactive: true).OfType<ISaveLoadableWorldEvent>().ToList();
		foreach (WorldEventEntry worldEventEntry in saveFile.WorldEventEntries)
		{
			foreach (ISaveLoadableWorldEvent item4 in list2)
			{
				if (item4.GetWorldEventID() == worldEventEntry.WorldEventID)
				{
					item4.LoadFromSave(worldEventEntry.CustomDataJson);
				}
			}
		}
		Singleton<EconomyManager>.Instance.ShopPurchases = saveFile.ShopPurchases;
		Singleton<EconomyManager>.Instance.SetMoney(saveFile.Money);
		Singleton<ResearchManager>.Instance.SetResearchTickets(saveFile.ResearchTickets);
		Singleton<ResearchManager>.Instance.LoadFromSaveFile(saveFile.CompletedResearchItems);
		Singleton<OreLimitManager>.Instance.HasShownOreLimitPopup = saveFile.HasShownOreLimitPopup;
		Singleton<GamemodeManager>.Instance.GameModeType = saveFile.GameMode;
		Singleton<QuestManager>.Instance.LoadFromSaveFile(saveFile);
		if (saveFile.SaveVersion < 16)
		{
			Singleton<ResearchManager>.Instance.MigrateNewResearchPrices();
		}
		UnityEngine.Object.FindObjectOfType<PlayerInventory>().ClearInventory();
		UnityEngine.Object.FindObjectOfType<PlayerController>().TeleportPlayer(saveFile.PlayerPosition, saveFile.PlayerRotation);
		ResetSessionStartTime();
		_totalPlayTimeSeconds = saveFile.TotalPlayTimeSeconds;
		if (Singleton<UIManager>.Instance != null)
		{
			Singleton<UIManager>.Instance.PauseMenu.OnResumePressed();
		}
		LastSaveTime = Time.time;
		ActiveSaveFileName = Path.GetFileNameWithoutExtension(fullFilePath);
		IsCurrentlyLoadingGame = false;
		if (saveFile.SaveVersion < 15 && saveFile.LevelID == "BigCave")
		{
			AutoMiner[] array2 = UnityEngine.Object.FindObjectsOfType<AutoMiner>();
			foreach (AutoMiner autoMiner in array2)
			{
				if (autoMiner.enabled)
				{
					autoMiner.Toggle(on: false);
				}
			}
			Singleton<UIManager>.Instance.ShowInfoMessagePopup("New Update!", "The Big Cave has recieved an update! \nThere are now more explodable walls around the cave.\n \nAll Auto-Miners have been automatically turned off.\nIt's recommended to follow your conveyors explode any walls that may have been placed in their paths.");
		}
		if (saveFile.SaveVersion < 15 && saveFile.LevelID == "NewCave")
		{
			AutoMiner[] array2 = UnityEngine.Object.FindObjectsOfType<AutoMiner>();
			foreach (AutoMiner autoMiner2 in array2)
			{
				if (autoMiner2.enabled)
				{
					autoMiner2.Toggle(on: false);
				}
			}
			if (Singleton<UpdatedLevelNewObjectAdder>.Instance.AgeOfSteelObjectPrefab != null)
			{
				UnityEngine.Object.Instantiate(Singleton<UpdatedLevelNewObjectAdder>.Instance.AgeOfSteelObjectPrefab);
			}
			Singleton<UIManager>.Instance.ShowInfoMessagePopup("Age of Steel", "Welcome to the Age of Steel Update! \nThis is MineMogul's biggest update yet, featuring many new machines and quests.\n \nThe 'Classic Cave' has been updated. Many areas (including the starting area) have been expanded to provide more space to build. Some Auto-Miner spots have been moved around and upgraded to Heavy Auto-Miner spots. Please take a look around your factory.\n \nFull patch notes are available on Steam. Hope you enjoy!");
		}
		StartCoroutine(WaitThenFinishLoading(saveFile));
	}

	public IEnumerator WaitThenFinishLoading(SaveFile saveFile)
	{
		yield return null;
		yield return null;
		if (base.gameObject.activeInHierarchy)
		{
			UnityEngine.Object.FindObjectOfType<PlayerInventory>().SwitchTool(saveFile.LastActiveInventorySlot);
		}
	}
}
