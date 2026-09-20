using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Engine;
using GameEntitySystem;
using TemplatesDatabase;
using XmlUtilities;

namespace Game
{
    public class SubsystemDimensions : Subsystem, IUpdateable
    {
        public const string SubsystemName = "Dimensions";

        public SubsystemTerrain m_subsystemTerrain;
        public SubsystemBodies m_subsystemBodies;
        public SubsystemPlayers m_subsystemPlayers;

        protected string m_worldPath;
        protected WorldType m_worldType;

        private List<Entity> m_creatureEntities = new List<Entity>();

        public string WorldPath => m_worldPath;
        public WorldType WorldType => m_worldType;

        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public virtual void Update(float dt)
        {
        }

        public override void Load(ValuesDictionary valuesDictionary)
        {
            base.Load(valuesDictionary);
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
            m_subsystemBodies = Project.FindSubsystem<SubsystemBodies>(true);
            m_subsystemPlayers = Project.FindSubsystem<SubsystemPlayers>(true);

            GameLoadingScreen gameLoadingScreen = ScreensManager.FindScreen<GameLoadingScreen>("GameLoading");
            m_worldPath = gameLoadingScreen.m_worldInfo.DirectoryName;
            m_worldType = GetWorldType(m_worldPath);
            valuesDictionary.SetValue("WorldPath", m_worldPath);

            if (valuesDictionary.ContainsKey("Creatures"))
            {
                string creatureNames = valuesDictionary.GetValue<string>("Creatures");

                if (!string.IsNullOrEmpty(creatureNames))
                {
                    string[] entityNames = creatureNames.Split(',');

                    foreach (string entityName in entityNames)
                    {
                        Entity entity = DatabaseManager.CreateEntity(Project, entityName, throwIfNotFound: true);
                        m_creatureEntities.Add(entity);
                    }

                    valuesDictionary.SetValue("Creatures", "");
                }
            }
        }

        public override void Save(ValuesDictionary valuesDictionary)
        {
            base.Save(valuesDictionary);
            valuesDictionary.SetValue("WorldPath", m_worldPath);
        }

        public override void OnEntityAdded(Entity entity)
        {
            base.OnEntityAdded(entity);

            /*ComponentPlayer componentPlayer = entity.FindComponent<ComponentPlayer>();
            if (componentPlayer != null)
                return;

            PlayerData mainPlayerData = componentPlayer.PlayerData;

            if (mainPlayerData.SpawnPosition == Vector3.Zero)
            {
                if (m_subsystemPlayers.GlobalSpawnPosition == Vector3.Zero)
                {
                    PlayerData firstPlayerData = m_subsystemPlayers.PlayersData.FirstOrDefault(pd => pd.SpawnPosition != Vector3.Zero);
                    if (firstPlayerData != null)
                    {
                        if (firstPlayerData.ComponentPlayer != null)
                        {
                            mainPlayerData.SpawnPosition = firstPlayerData.ComponentPlayer.ComponentBody.Position;
                        }
                        else
                        {
                            mainPlayerData.SpawnPosition = firstPlayerData.SpawnPosition;
                        }
                    }
                    else
                    {
                        mainPlayerData.SpawnPosition = m_subsystemTerrain.TerrainContentsGenerator.FindCoarseSpawnPosition();
                    }
                    m_subsystemPlayers.GlobalSpawnPosition = mainPlayerData.SpawnPosition;
                }
                else
                {
                    mainPlayerData.SpawnPosition = m_subsystemPlayers.GlobalSpawnPosition;
                }
            }
            
            m_subsystemTerrain.TerrainUpdater.SetUpdateLocation(mainPlayerData.PlayerIndex, mainPlayerData.SpawnPosition.XZ, 0f, 64f);
            mainPlayerData.m_terrainWaitStartTime = Time.FrameStartTime;*/
        }

        public override void OnEntityRemoved(Entity entity)
        {
            base.OnEntityRemoved(entity);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public void MainToChildWorld(DimensionPortalEntityInfo info)
        {
            string currentWorldPath = GameManager.WorldInfo.DirectoryName;
            string targetWorldPath = Storage.CombinePaths(currentWorldPath, info.TargetWorldType.ToString());

            if (IsChildWorld(currentWorldPath))
            {
                DisplayPortalMessage(info, "Warning: The current world is not the main world");

                return;
            }

            if (!IsChildWorld(targetWorldPath))
            {
                DisplayPortalMessage(info, "Warning: Destination is not a child world");

                return;
            }

            if (info.EntityType == DimensionPortalEntityType.Player)
            {
                ChangeWorld(currentWorldPath, targetWorldPath);
                return;
            }

            SaveCreature(targetWorldPath, info);
        }

        public void ChildToMainWorld(DimensionPortalEntityInfo info)
        {
            string currentWorldPath = GameManager.WorldInfo.DirectoryName;
            string targetWorldPath = Storage.GetDirectoryName(currentWorldPath);

            if (!IsChildWorld(currentWorldPath))
            {
                DisplayPortalMessage(info, "Warning: The current world is not a child world");
                return;
            }

            if (info.EntityType == DimensionPortalEntityType.Player)
            {
                ChangeWorld(currentWorldPath, targetWorldPath);
                return;
            }

            SaveCreature(targetWorldPath, info);
        }

        public void ChildToChildWorld(DimensionPortalEntityInfo info)
        {
            string currentWorldPath = GameManager.WorldInfo.DirectoryName;
            string targetWorldPath = Storage.CombinePaths(Storage.GetDirectoryName(currentWorldPath), info.TargetWorldType.ToString());

            if (!IsChildWorld(currentWorldPath))
            {
                DisplayPortalMessage(info, "Warning: The current world is not a child world");
                return;
            }

            if (!IsChildWorld(targetWorldPath))
            {
                DisplayPortalMessage(info, "Warning: Destination is not a child world");
                return;
            }

            if (info.EntityType == DimensionPortalEntityType.Player)
            {
                ChangeWorld(currentWorldPath, targetWorldPath);
                return;
            }

            SaveCreature(targetWorldPath, info);
        }

        private void DisplayPortalMessage(DimensionPortalEntityInfo info, string message)
        {
            if (info.EntityType != DimensionPortalEntityType.Player)
                return;

            info.ComponentPlayer?.ComponentGui.DisplaySmallMessage(message, Color.Yellow, blinking: false, playNotificationSound: true);
        }

        public static bool IsChildWorld(string worldPath)
        {
            string parentPath = Storage.GetDirectoryName(worldPath);
            return parentPath != WorldsManager.WorldsDirectoryName && Storage.GetDirectoryName(parentPath) == WorldsManager.WorldsDirectoryName;
        }

        public static void SaveCreature(string worldPath, DimensionPortalEntityInfo info)
        {
            string entityName = info.Entity.ValuesDictionary.DatabaseObject.Name;

            if (!Storage.DirectoryExists(worldPath))
                return;

            string projectPath = Storage.CombinePaths(worldPath, "Project.xml");
            XElement projectElement;

            using (Stream stream = Storage.OpenFile(projectPath, OpenFileMode.Read))
            {
                projectElement = XmlUtils.LoadXmlFromStream(stream, null, throwOnError: true);
            }

            XElement subsystemElement = projectElement.Element("Subsystems").Elements().FirstOrDefault(element => XmlUtils.GetAttributeValue<string>(element, "Name") == SubsystemName);

            if (subsystemElement == null)
                return;

            XElement creaturesElement = subsystemElement.Elements().FirstOrDefault(element => XmlUtils.GetAttributeValue<string>(element, "Name") == "Creatures");

            if (creaturesElement == null)
            {
                creaturesElement = new XElement("Value");

                XmlUtils.SetAttributeValue(creaturesElement, "Name", "Creatures");
                XmlUtils.SetAttributeValue(creaturesElement, "Type", "string");
                XmlUtils.SetAttributeValue(creaturesElement, "Value", entityName);

                subsystemElement.Add(creaturesElement);
            }
            else
            {
                string creatureNames = XmlUtils.GetAttributeValue<string>(creaturesElement, "Value");
                if (string.IsNullOrEmpty(creatureNames))
                {
                    XmlUtils.SetAttributeValue(creaturesElement, "Value", entityName);
                }
                else
                {
                    XmlUtils.SetAttributeValue(creaturesElement, "Value", creatureNames + "," + entityName);
                }
            }

            using (Stream stream = Storage.OpenFile(projectPath, OpenFileMode.Create))
            {
                XmlUtils.SaveXmlToStream(projectElement, stream, null, throwOnError: true);
            }
        }

        public void ChangeWorld(string currentWorldPath, string targetWorldPath)
        {
            bool isNewWorld = false;
            bool isWorldUnpacked = false;

            // ============================================================
            // 1. Tạo world đích nếu world chưa tồn tại
            // ============================================================

            if (!Storage.DirectoryExists(targetWorldPath))
            {
                Storage.CreateDirectory(targetWorldPath);

                Dictionary<string, Stream> scworldList = GetScworldList(ModsManager.ModsPath);
                string worldName = Storage.GetFileNameWithoutExtension(targetWorldPath);

                // Nếu có world mẫu trong Mods thì giải nén world mẫu.
                if (scworldList.ContainsKey(worldName))
                {
                    WorldsManager.UnpackWorld(targetWorldPath, scworldList[worldName], importEmbeddedExternalContent: true);
                    isWorldUnpacked = true;
                }

                // Nếu không có world mẫu thì tạo world mới.
                if (!isWorldUnpacked)
                {
                    isNewWorld = true;

                    WorldSettings worldSettings = GameManager.WorldInfo.WorldSettings;
                    int worldSeed;

                    if (string.IsNullOrEmpty(worldSettings.Seed))
                    {
                        worldSeed = (int)(long)(Time.RealTime * 1000.0);
                    }
                    else if (worldSettings.Seed == "0")
                    {
                        worldSeed = 0;
                    }
                    else
                    {
                        worldSeed = 0;
                        int seedMultiplier = 1;

                        foreach (char character in worldSettings.Seed)
                        {
                            worldSeed += character * seedMultiplier;
                            seedMultiplier += 29;
                        }
                    }

                    // Lưu thông tin GameInfo cho world mới.
                    ValuesDictionary gameInfoValues = new ValuesDictionary();
                    worldSettings.Save(gameInfoValues, liveModifiableParametersOnly: false);
                    gameInfoValues.SetValue("WorldDirectoryName", targetWorldPath);
                    gameInfoValues.SetValue("WorldSeed", worldSeed);

                    // Tạo dữ liệu Players và PlayerStats mặc định.
                    ValuesDictionary playersValues = new ValuesDictionary();
                    playersValues.SetValue("Players", new ValuesDictionary());

                    DatabaseObject gameProjectDatabaseObject = DatabaseManager.GameDatabase.Database.FindDatabaseObject("GameProject", DatabaseManager.GameDatabase.ProjectTemplateType, throwIfNotFound: true);

                    XElement projectElement = new XElement("Project");
                    XmlUtils.SetAttributeValue(projectElement, "Guid", gameProjectDatabaseObject.Guid);
                    XmlUtils.SetAttributeValue(projectElement, "Name", "GameProject");
                    XmlUtils.SetAttributeValue(projectElement, "Version", VersionsManager.SerializationVersion);
                    XmlUtils.SetAttributeValue(projectElement, "APIVersion", ModsManager.APIVersionString);

                    XElement subsystemsElement = new XElement("Subsystems");
                    projectElement.Add(subsystemsElement);

                    XElement gameInfoElement = new XElement("Values");
                    XmlUtils.SetAttributeValue(gameInfoElement, "Name", "GameInfo");
                    gameInfoValues.Save(gameInfoElement);
                    subsystemsElement.Add(gameInfoElement);

                    XElement playersElement = new XElement("Values");
                    XmlUtils.SetAttributeValue(playersElement, "Name", "Players");
                    playersValues.Save(playersElement);
                    subsystemsElement.Add(playersElement);

                    XElement playerStatsElement = new XElement("Values");
                    XmlUtils.SetAttributeValue(playerStatsElement, "Name", "PlayerStats");
                    playersValues.Save(playerStatsElement);
                    subsystemsElement.Add(playerStatsElement);

                    using (Stream stream = Storage.OpenFile(Storage.CombinePaths(targetWorldPath, "Project.xml"), OpenFileMode.Create))
                    {
                        XmlUtils.SaveXmlToStream(projectElement, stream, null, throwOnError: true);
                    }
                }
            }

            // ============================================================
            // 2. Lưu và giải phóng world hiện tại
            // ============================================================

            GameManager.SaveProject(waitForCompletion: true, showErrorDialog: true);
            GameManager.DisposeProject();

            try
            {
                // ========================================================
                // 3. Đọc Project.xml của world hiện tại và world đích
                // ========================================================

                string currentProjectPath = Storage.CombinePaths(currentWorldPath, "Project.xml");
                string targetProjectPath = Storage.CombinePaths(targetWorldPath, "Project.xml");

                XElement currentProjectElement;
                XElement targetProjectElement;

                using (Stream stream = Storage.OpenFile(currentProjectPath, OpenFileMode.Read))
                {
                    currentProjectElement = XmlUtils.LoadXmlFromStream(stream, null, throwOnError: true);
                }

                using (Stream stream = Storage.OpenFile(targetProjectPath, OpenFileMode.Read))
                {
                    targetProjectElement = XmlUtils.LoadXmlFromStream(stream, null, throwOnError: true);
                }

                // ========================================================
                // 4. Hàm tìm Subsystem theo tên
                // ========================================================

                Func<XElement, string, XElement> getSubsystem = (projectElement, subsystemName) =>
                {
                    foreach (XElement subsystemElement in projectElement.Element("Subsystems").Elements())
                    {
                        if (XmlUtils.GetAttributeValue<string>(subsystemElement, "Name") == subsystemName)
                            return subsystemElement;
                    }

                    return null;
                };

                // ========================================================
                // 5. Đồng bộ Subsystem từ world hiện tại sang world đích
                // ========================================================

                Action<string, bool> synchronizeSubsystem = (subsystemName, addFirst) =>
                {
                    XElement sourceSubsystem = getSubsystem(currentProjectElement, subsystemName);

                    if (sourceSubsystem == null)
                        return;

                    XElement targetSubsystem = getSubsystem(targetProjectElement, subsystemName);

                    if (targetSubsystem != null)
                    {
                        ReplaceNodes(sourceSubsystem, targetSubsystem, null);
                    }
                    else
                    {
                        XElement newSubsystem = new XElement(sourceSubsystem);

                        if (addFirst)
                            targetProjectElement.Element("Subsystems").AddFirst(newSubsystem);
                        else
                            targetProjectElement.Element("Subsystems").Add(newSubsystem);
                    }
                };

                // Đồng bộ các Subsystem cần thiết cho world đích.
                synchronizeSubsystem("UsedMods", true);
                synchronizeSubsystem("BlocksManager", true);
                synchronizeSubsystem("PlayerStats", false);

                // ========================================================
                // 6. Xử lý dữ liệu Player
                // ========================================================

                XElement currentPlayersSubsystem = getSubsystem(currentProjectElement, "Players");
                XElement targetPlayersSubsystem = getSubsystem(targetProjectElement, "Players");

                if (isNewWorld)
                {
                    // World mới lấy dữ liệu Player từ world hiện tại.
                    if (currentPlayersSubsystem != null && targetPlayersSubsystem != null)
                    {
                        ReplaceNodes(currentPlayersSubsystem, targetPlayersSubsystem, null);

                        // World mới không sử dụng SpawnPosition của world hiện tại.
                        foreach (XElement valueElement in targetPlayersSubsystem.Descendants("Value"))
                        {
                            if (XmlUtils.GetAttributeValue<string>(valueElement, "Name") == "SpawnPosition")
                                XmlUtils.SetAttributeValue(valueElement, "Value", new Vector3(0, 0, 0));
                        }
                    }

                    // Sao chép Entity Player sang world mới.
                    XElement playerEntity = null;

                    foreach (XElement entityElement in currentProjectElement.Element("Entities").Elements("Entity"))
                    {
                        string entityName = XmlUtils.GetAttributeValue<string>(entityElement, "Name");

                        if (entityName == "MalePlayer" || entityName == "FemalePlayer" || entityName == "Player")
                        {
                            playerEntity = entityElement;
                            break;
                        }
                    }

                    if (playerEntity != null)
                    {
                        if (targetProjectElement.Element("Entities") == null)
                            targetProjectElement.Add(new XElement("Entities"));

                        targetProjectElement.Element("Entities").Add(playerEntity);
                    }
                }
                else
                {
                    // World đã tồn tại nên giữ lại SpawnPosition của world đích.
                    if (currentPlayersSubsystem != null && targetPlayersSubsystem != null)
                    {
                        string[] reservedPlayerParameters = new string[1] { "SpawnPosition" };
                        ReplaceNodes(currentPlayersSubsystem.Element("Values"), targetPlayersSubsystem.Element("Values"), reservedPlayerParameters);
                    }

                    // Tìm Player Entity của world đích.
                    XElement targetPlayerEntity = null;

                    foreach (XElement entityElement in targetProjectElement.Element("Entities").Elements("Entity"))
                    {
                        string entityName = XmlUtils.GetAttributeValue<string>(entityElement, "Name");

                        if (entityName == "MalePlayer" || entityName == "FemalePlayer" || entityName == "Player")
                        {
                            targetPlayerEntity = entityElement;
                            break;
                        }
                    }

                    // Cập nhật Player Entity của world đích từ world hiện tại.
                    foreach (XElement entityElement in currentProjectElement.Element("Entities").Elements("Entity"))
                    {
                        string entityName = XmlUtils.GetAttributeValue<string>(entityElement, "Name");

                        if (entityName == "MalePlayer" || entityName == "FemalePlayer" || entityName == "Player")
                        {
                            string[] reservedPlayerParameters = new string[1] { "Body" };

                            if (targetPlayerEntity != null)
                                ReplaceNodes(entityElement, targetPlayerEntity, reservedPlayerParameters);

                            break;
                        }
                    }
                }

                // ========================================================
                // 7. Lưu Project.xml của world đích
                // ========================================================

                using (Stream stream = Storage.OpenFile(targetProjectPath, OpenFileMode.Create))
                {
                    XmlUtils.SaveXmlToStream(targetProjectElement, stream, null, throwOnError: true);
                }

                // ========================================================
                // 8. Đồng bộ GameInfo và cập nhật đường dẫn world
                // ========================================================

                SynchronizeGameInfo(currentProjectElement, targetProjectElement, currentWorldPath, targetWorldPath, isNewWorld);

                m_worldPath = targetWorldPath;

                // Nếu world hiện tại là Child World thì lấy Main World.
                if (IsChildWorld(currentWorldPath))
                    currentWorldPath = Storage.GetDirectoryName(currentWorldPath);

                // Lưu quan hệ giữa Main World và Child World.
                SavePathToMainWorld(currentWorldPath, m_worldPath);
            }
            catch (Exception)
            {
            }
            finally
            {
                // ========================================================
                // 9. Chuyển sang màn hình loading của world đích
                // ========================================================

                WorldInfo worldInfo = WorldsManager.GetWorldInfo(targetWorldPath);
                ScreensManager.SwitchScreen("GameLoading", worldInfo, null);
            }
        }

        public static Dictionary<string, Stream> GetScworldList(string path)
        {
            Dictionary<string, Stream> scworldStreams = new Dictionary<string, Stream>();

            foreach (string fileName in Storage.ListFileNames(path))
            {
                string fileExtension = Storage.GetExtension(fileName);
                string modFilePath = Storage.CombinePaths(path, fileName);
                Stream modStream = Storage.OpenFile(modFilePath, OpenFileMode.Read);

                try
                {
                    if (fileExtension != ".scmod")
                        continue;

                    ZipArchive modArchive = ZipArchive.Open(modStream, keepStreamOpen: true);

                    foreach (ZipArchiveEntry archiveEntry in modArchive.ReadCentralDir())
                    {
                        if (Storage.GetExtension(archiveEntry.FilenameInZip) != ".scworld")
                            continue;

                        MemoryStream worldStream = new MemoryStream();
                        modArchive.ExtractFile(archiveEntry, worldStream);
                        worldStream.Position = 0L;

                        string worldName = Storage.GetFileNameWithoutExtension(archiveEntry.FilenameInZip);
                        scworldStreams.Add(worldName, worldStream);
                    }
                }
                catch (Exception)
                {
                }
            }

            return scworldStreams;
        }

        public static void ReplaceNodes(XElement replaceElement, XElement sourceElement, string[] reserveParameters)
        {
            Dictionary<string, XElement> dictionary = new Dictionary<string, XElement>();

            if (reserveParameters != null)
            {
                foreach (XElement item in sourceElement.Elements())
                {
                    string attributeValue = XmlUtils.GetAttributeValue<string>(item, "Name");
                    foreach (string text in reserveParameters)
                    {
                        if (attributeValue == text)
                        {
                            dictionary.Add(attributeValue, item);
                            break;
                        }
                    }
                }
            }
            sourceElement.RemoveNodes();

            if (reserveParameters != null)
            {
                foreach (XElement item2 in replaceElement.Elements())
                {
                    string attributeValue2 = XmlUtils.GetAttributeValue<string>(item2, "Name");
                    if (dictionary.ContainsKey(attributeValue2))
                    {
                        sourceElement.Add(dictionary[attributeValue2]);
                    }
                    else
                    {
                        sourceElement.Add(item2);
                    }
                }
                return;
            }

            foreach (XElement item3 in replaceElement.Elements())
            {
                sourceElement.Add(item3);
            }
        }

        public static void SynchronizeGameInfo(XElement rxElement, XElement subXElement, string path, string wpath, bool newWorld)
        {
            XElement replaceElement = null;

            if (newWorld)
            {
                if (IsChildWorld(path))
                {
                    path = Storage.GetDirectoryName(path);
                }

                using Stream stream = Storage.OpenFile(Storage.CombinePaths(path, "Project.xml"), OpenFileMode.Read);
                rxElement = XmlUtils.LoadXmlFromStream(stream, null, throwOnError: true);
            }

            foreach (XElement item in rxElement.Element("Subsystems").Elements())
            {
                if (XmlUtils.GetAttributeValue<string>(item, "Name") == "GameInfo")
                {
                    replaceElement = item;
                    break;
                }
            }

            foreach (XElement item2 in subXElement.Element("Subsystems").Elements())
            {
                if (XmlUtils.GetAttributeValue<string>(item2, "Name") == "GameInfo")
                {
                    string[] reserveParameters = new string[20]
                    {
                        "WorldName", "OriginalSerializationVersion", "EnvironmentBehaviorMode", "TimeOfDayMode", "AreWeatherEffectsEnabled", "TerrainGenerationMode", "IslandSize", "TerrainLevel", "ShoreRoughness", "TerrainBlockIndex",
                        "TerrainOceanBlockIndex", "TemperatureOffset", "HumidityOffset", "SeaLevelOffset", "BiomeSize", "StartingPositionMode", "BlockTextureName", "Palette", "WorldSeed", "TotalElapsedGameTime"
                    };

                    if (newWorld)
                    {
                        reserveParameters = new string[1] { "WorldSeed" };
                    }

                    ReplaceNodes(replaceElement, item2, reserveParameters);
                    break;
                }
            }

            using Stream stream2 = Storage.OpenFile(Storage.CombinePaths(wpath, "Project.xml"), OpenFileMode.Create);
            XmlUtils.SaveXmlToStream(subXElement, stream2, null, throwOnError: true);
        }

        public static void SavePathToMainWorld(string mainWorldPath, string worldPath)
        {
            XElement projectElement = null;

            using (Stream projectStream = Storage.OpenFile(Storage.CombinePaths(mainWorldPath, "Project.xml"), OpenFileMode.Read))
            {
                projectElement = XmlUtils.LoadXmlFromStream(projectStream, null, throwOnError: true);
            }

            foreach (XElement subsystemElement in projectElement.Element("Subsystems").Elements())
            {
                if (XmlUtils.GetAttributeValue<string>(subsystemElement, "Name") == SubsystemName)
                {
                    XmlUtils.SetAttributeValue(subsystemElement.Element("Value"), "Value", worldPath);
                }
            }

            using Stream outputStream = Storage.OpenFile(Storage.CombinePaths(mainWorldPath, "Project.xml"), OpenFileMode.Create);
            XmlUtils.SaveXmlToStream(projectElement, outputStream, null, throwOnError: true);
        }

        public static WorldType GetWorldType(string pathOrName)
        {
            WorldType result = WorldType.Default;
            pathOrName = pathOrName.Replace("\\", "/");
            string[] array = pathOrName.Split('/');
            string text = array[array.Length - 1];
            foreach (WorldType value in Enum.GetValues(typeof(WorldType)))
            {
                if (value.ToString() == text)
                {
                    result = value;
                    break;
                }
            }
            return result;
        }

        public static string GetWorldName(WorldType worldType)
        {
            return worldType.ToString();
        }
    }
}
