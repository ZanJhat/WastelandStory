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

        public ComponentPlayer m_componentPlayer;

        public Dictionary<Point3, WorldDoor> m_worldDoors = new Dictionary<Point3, WorldDoor>();

        public WorldType m_worldType;

        private List<Entity> m_creatureEntities = new List<Entity>();

        private string m_worldPath;

        private bool m_canGenerateDoor;

        private bool m_initialize;

        private float m_lastDtime = 0f;

        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public virtual void Update(float dt)
        {
            if (m_componentPlayer == null)
            {
                return;
            }

            if (m_canGenerateDoor)
            {
                m_lastDtime += dt;
                if (m_lastDtime > 1f)
                {
                    GenerateDoor(new Point3(m_componentPlayer.ComponentBody.Position) + new Point3(0, 1, 2));
                    m_canGenerateDoor = false;
                }
            }

            if (!m_initialize)
            {
                m_initialize = true;
                Point3 point = new Point3(m_componentPlayer.ComponentBody.Position);

                foreach (Entity creatureEntity in m_creatureEntities)
                {
                    Point3 point2 = new Point3(0, 0, 0);
                    for (int i = 0; i <= 5; i++)
                    {
                        int x = point.X + new Random().Int(-5, 5);
                        int y = point.Y + i;
                        int z = point.Z + new Random().Int(-5, 5);
                        if (m_subsystemTerrain.Terrain.GetCellContents(x, y, z) == 0)
                        {
                            point2 = new Point3(x, y, z);
                            break;
                        }
                    }

                    creatureEntity.FindComponent<ComponentFrame>(throwOnError: true).Position = new Vector3(point2.X, point2.Y, point2.Z);
                    creatureEntity.FindComponent<ComponentFrame>(throwOnError: true).Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, new Random().Float(0f, (float)Math.PI * 2f));
                    creatureEntity.FindComponent<ComponentSpawn>(throwOnError: true).SpawnDuration = 0f;
                    base.Project.AddEntity(creatureEntity);
                }
                m_creatureEntities.Clear();
            }

            foreach (WorldDoor value in m_worldDoors.Values)
            {
                Vector3 minPoint = value.MinPoint;
                Vector3 maxPoint = value.MaxPoint;
                Vector3 position = m_componentPlayer.ComponentBody.Position;
                if (position.X >= minPoint.X && position.Y >= minPoint.Y && position.Z >= minPoint.Z && position.X <= maxPoint.X && position.Y <= maxPoint.Y && position.Z <= maxPoint.Z)
                {
                    if (m_worldType == value.WorldType)
                    {
                        ChildToMajorWorld();
                    }
                    else
                    {
                        string worldName = GetWorldName(value.WorldType);
                        if (m_worldType == WorldType.Default)
                        {
                            MajorToChildWorld(worldName);
                        }
                        else
                        {
                            ChildToChildWorld(worldName);
                        }
                    }
                }

                DynamicArray<ComponentBody> dynamicArray = new DynamicArray<ComponentBody>();
                m_subsystemBodies.FindBodiesInArea(minPoint.XZ - new Vector2(8f), maxPoint.XY + new Vector2(8f), dynamicArray);

                foreach (ComponentBody item in dynamicArray)
                {
                    Vector3 position2 = item.Position;
                    if (!(position2.X >= minPoint.X) || !(position2.Y >= minPoint.Y) || !(position2.Z >= minPoint.Z) || !(position2.X <= maxPoint.X) || !(position2.Y <= maxPoint.Y) || !(position2.Z <= maxPoint.Z))
                    {
                        continue;
                    }

                    ComponentPlayer componentPlayer = item.Entity.FindComponent<ComponentPlayer>();
                    if (componentPlayer != null)
                    {
                        continue;
                    }

                    string name = item.Entity.ValuesDictionary.DatabaseObject.Name;
                    base.Project.RemoveEntity(item.Entity, disposeEntity: true);

                    if (m_worldType == value.WorldType)
                    {
                        ChildToMajorWorld(IsAnimal: true, name);
                        continue;
                    }

                    string worldName2 = GetWorldName(value.WorldType);

                    if (m_worldType == WorldType.Default)
                    {
                        MajorToChildWorld(worldName2, IsAnimal: true, name);
                    }
                    else
                    {
                        ChildToChildWorld(worldName2, IsAnimal: true, name);
                    }
                }
            }
        }

        public override void Load(ValuesDictionary valuesDictionary)
        {
            base.Load(valuesDictionary);
            m_subsystemTerrain = base.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
            m_subsystemBodies = base.Project.FindSubsystem<SubsystemBodies>(throwOnError: true);

            GameLoadingScreen gameLoadingScreen = ScreensManager.FindScreen<GameLoadingScreen>("GameLoading");
            m_worldPath = gameLoadingScreen.m_worldInfo.DirectoryName;
            m_worldType = GetWorldType(m_worldPath);
            valuesDictionary.SetValue("WorldPath", m_worldPath);

            m_canGenerateDoor = false;
            m_initialize = false;

            if (valuesDictionary.ContainsKey("Creatures"))
            {
                string value = valuesDictionary.GetValue<string>("Creatures");
                if (value != "Null")
                {
                    string[] array = value.Split(',');
                    string[] array2 = array;
                    foreach (string entityTemplateName in array2)
                    {
                        Entity item = DatabaseManager.CreateEntity(base.Project, entityTemplateName, throwIfNotFound: true);
                        m_creatureEntities.Add(item);
                    }
                    valuesDictionary.SetValue("Creatures", "Null");
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
            ComponentPlayer componentPlayer = entity.FindComponent<ComponentPlayer>();
            if (componentPlayer != null)
            {
                m_componentPlayer = componentPlayer;
                if (m_componentPlayer.PlayerData.SpawnPosition == new Vector3(0f, 0f, 0f))
                {
                    Vector3 vector = m_subsystemTerrain.TerrainContentsGenerator.FindCoarseSpawnPosition();
                    m_componentPlayer.PlayerData.SpawnPosition = vector;
                    m_componentPlayer.ComponentBody.Position = vector;
                    m_canGenerateDoor = true;
                }
            }
            base.OnEntityAdded(entity);
        }

        public override void OnEntityRemoved(Entity entity)
        {
            base.OnEntityRemoved(entity);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public void ChildToMajorWorld(bool IsAnimal = false, string AnimalName = null)
        {
            string directoryName = GameManager.WorldInfo.DirectoryName;
            string directoryName2 = Storage.GetDirectoryName(directoryName);
            if (!IsChildWorld(directoryName))
            {
                m_componentPlayer?.ComponentGui.DisplaySmallMessage("Note: The current world is not a sub-world", Color.Yellow, blinking: false, playNotificationSound: false);
            }
            else if (!IsAnimal)
            {
                ChangeWorld(directoryName, directoryName2);
            }
            else
            {
                SaveCreatures(directoryName2, AnimalName);
            }
        }

        public void MajorToChildWorld(string worldName, bool IsAnimal = false, string AnimalName = null)
        {
            string directoryName = GameManager.WorldInfo.DirectoryName;
            string text = Storage.CombinePaths(directoryName, worldName);

            if (IsChildWorld(directoryName))
            {
                m_componentPlayer?.ComponentGui.DisplaySmallMessage("Note: The current world is not the main world", Color.Yellow, blinking: false, playNotificationSound: false);
            }
            else if (!IsChildWorld(text))
            {
                m_componentPlayer?.ComponentGui.DisplaySmallMessage("Note: The teleportation destination is not a sub-world", Color.Yellow, blinking: false, playNotificationSound: false);
            }
            else if (!IsAnimal)
            {
                ChangeWorld(directoryName, text);
            }
            else
            {
                SaveCreatures(text, AnimalName);
            }
        }

        public void ChildToChildWorld(string worldName, bool IsAnimal = false, string AnimalName = null)
        {
            string directoryName = GameManager.WorldInfo.DirectoryName;
            string text = Storage.CombinePaths(Storage.GetDirectoryName(directoryName), worldName);
            if (!IsChildWorld(directoryName))
            {
                m_componentPlayer?.ComponentGui.DisplaySmallMessage("Note: The current world is not a child world", Color.Yellow, blinking: false, playNotificationSound: false);
            }
            else if (!IsChildWorld(text))
            {
                m_componentPlayer?.ComponentGui.DisplaySmallMessage("Note: The world you are teleported to is not a sub-world", Color.Yellow, blinking: false, playNotificationSound: false);
            }
            else if (!IsAnimal)
            {
                ChangeWorld(directoryName, text);
            }
            else
            {
                SaveCreatures(text, AnimalName);
            }
        }

        public void ChangeWorld(string path, string wpath, bool IsAnimal = false)
        {
            bool isNewWorld = false;
            bool isWorldUnpacked = false;

            if (!Storage.DirectoryExists(wpath))
            {
                Storage.CreateDirectory(wpath);

                Dictionary<string, Stream> scworldList = GetScworldList(ModsManager.ModsPath);
                string fileNameWithoutExtension = Storage.GetFileNameWithoutExtension(wpath);

                if (scworldList.ContainsKey(fileNameWithoutExtension))
                {
                    WorldsManager.UnpackWorld(wpath, scworldList[fileNameWithoutExtension], importEmbeddedExternalContent: true);
                    isWorldUnpacked = true;
                }

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

                        foreach (char c in worldSettings.Seed)
                        {
                            worldSeed += c * seedMultiplier;
                            seedMultiplier += 29;
                        }
                    }

                    ValuesDictionary valuesDictionary = new ValuesDictionary();
                    worldSettings.Save(valuesDictionary, liveModifiableParametersOnly: false);
                    valuesDictionary.SetValue("WorldDirectoryName", wpath);
                    valuesDictionary.SetValue("WorldSeed", worldSeed);

                    ValuesDictionary valuesDictionary2 = new ValuesDictionary();
                    valuesDictionary2.SetValue("Players", new ValuesDictionary());

                    DatabaseObject databaseObject = DatabaseManager.GameDatabase.Database.FindDatabaseObject("GameProject", DatabaseManager.GameDatabase.ProjectTemplateType, throwIfNotFound: true);

                    XElement xElement = new XElement("Project");
                    XmlUtils.SetAttributeValue(xElement, "Guid", databaseObject.Guid);
                    XmlUtils.SetAttributeValue(xElement, "Name", "GameProject");
                    XmlUtils.SetAttributeValue(xElement, "Version", VersionsManager.SerializationVersion);
                    XmlUtils.SetAttributeValue(xElement, "APIVersion", ModsManager.APIVersionString);

                    XElement xElement2 = new XElement("Subsystems");
                    xElement.Add(xElement2);

                    XElement xElement3 = new XElement("Values");
                    XmlUtils.SetAttributeValue(xElement3, "Name", "GameInfo");
                    valuesDictionary.Save(xElement3);
                    xElement2.Add(xElement3);

                    XElement xElement4 = new XElement("Values");
                    XmlUtils.SetAttributeValue(xElement4, "Name", "Players");
                    valuesDictionary2.Save(xElement4);
                    xElement2.Add(xElement4);

                    XElement xElement5 = new XElement("Values");
                    XmlUtils.SetAttributeValue(xElement5, "Name", "PlayerStats");
                    valuesDictionary2.Save(xElement5);
                    xElement2.Add(xElement5);

                    using Stream stream = Storage.OpenFile(Storage.CombinePaths(wpath, "Project.xml"), OpenFileMode.Create);
                    XmlUtils.SaveXmlToStream(xElement, stream, null, throwOnError: true);
                }
            }

            GameManager.SaveProject(waitForCompletion: true, showErrorDialog: true);
            GameManager.DisposeProject();

            try
            {
                XElement xElement6 = null;
                XElement xElement7 = null;
                using (Stream stream2 = Storage.OpenFile(Storage.CombinePaths(path, "Project.xml"), OpenFileMode.Read))
                {
                    xElement6 = XmlUtils.LoadXmlFromStream(stream2, null, throwOnError: true);
                }
                using (Stream stream3 = Storage.OpenFile(Storage.CombinePaths(wpath, "Project.xml"), OpenFileMode.Read))
                {
                    xElement7 = XmlUtils.LoadXmlFromStream(stream3, null, throwOnError: true);
                }

                // Hàm hỗ trợ tìm Node Subsystem theo Tên (Tránh dùng ElementAt dễ gây lỗi ở API mới)
                Func<XElement, string, XElement> getSubsystem = (root, name) =>
                {
                    foreach (XElement el in root.Element("Subsystems").Elements())
                        if (XmlUtils.GetAttributeValue<string>(el, "Name") == name) return el;
                    return null;
                };

                // Hàm hỗ trợ đồng bộ: Nếu không có Node đích thì tự động tạo mới, có thêm tham số ưu tiên (highPriority)
                Action<string, bool> syncSubsystem = (subsystemName, highPriority) =>
                {
                    XElement sourceNode = getSubsystem(xElement6, subsystemName);
                    if (sourceNode != null)
                    {
                        XElement targetNode = getSubsystem(xElement7, subsystemName);
                        if (targetNode != null)
                        {
                            // Nếu thế giới con đã có Node này, tiến hành thay thế thông số
                            ReplaceNodes(sourceNode, targetNode, null);
                        }
                        else
                        {
                            // Nếu thế giới con mới tạo, sao chép từ thế giới chính sang
                            if (highPriority)
                            {
                                // Đẩy lên ĐẦU danh sách Subsystems để load TRƯỚC Terrain
                                xElement7.Element("Subsystems").AddFirst(new XElement(sourceNode));
                            }
                            else
                            {
                                // Chèn vào cuối như bình thường
                                xElement7.Element("Subsystems").Add(new XElement(sourceNode));
                            }
                        }
                    }
                };

                // Đồng bộ các Subsystem quan trọng ngay từ lần đầu tạo
                // Cho BlocksManager và UsedMods cờ "true" để chúng được ưu tiên khởi tạo đầu tiên
                syncSubsystem("UsedMods", true);
                syncSubsystem("BlocksManager", true);
                syncSubsystem("PlayerStats", false);

                if (isNewWorld)
                {
                    XElement playersSource = getSubsystem(xElement6, "Players");
                    XElement playersTarget = getSubsystem(xElement7, "Players");

                    if (playersSource != null && playersTarget != null)
                    {
                        ReplaceNodes(playersSource, playersTarget, null);

                        // Tìm chính xác node SpawnPosition ở mọi độ sâu
                        foreach (XElement val in playersTarget.Descendants("Value"))
                        {
                            if (XmlUtils.GetAttributeValue<string>(val, "Name") == "SpawnPosition")
                            {
                                XmlUtils.SetAttributeValue(val, "Value", new Vector3(0, 0, 0));
                            }
                        }
                    }

                    XElement content = null;
                    foreach (XElement item4 in xElement6.Element("Entities").Elements("Entity"))
                    {
                        string entName = XmlUtils.GetAttributeValue<string>(item4, "Name");
                        // Hỗ trợ cả MalePlayer/FemalePlayer (API cũ) và Player (API mới)
                        if (entName == "MalePlayer" || entName == "FemalePlayer" || entName == "Player")
                        {
                            content = item4;
                            break;
                        }
                    }

                    if (content != null)
                    {
                        if (xElement7.Element("Entities") == null)
                        {
                            xElement7.Add(new XElement("Entities"));
                        }
                        xElement7.Element("Entities").Add(content);
                    }
                }
                else // Nếu thế giới con đã tồn tại
                {
                    XElement playersSource = getSubsystem(xElement6, "Players");
                    XElement playersTarget = getSubsystem(xElement7, "Players");
                    if (playersSource != null && playersTarget != null)
                    {
                        string[] reserveParameters = new string[1] { "SpawnPosition" };
                        ReplaceNodes(playersSource.Element("Values"), playersTarget.Element("Values"), reserveParameters);
                    }

                    XElement sourceElement = null;
                    foreach (XElement item7 in xElement7.Element("Entities").Elements("Entity"))
                    {
                        string entName = XmlUtils.GetAttributeValue<string>(item7, "Name");
                        if (entName == "MalePlayer" || entName == "FemalePlayer" || entName == "Player")
                        {
                            sourceElement = item7;
                            break;
                        }
                    }
                    foreach (XElement item8 in xElement6.Element("Entities").Elements("Entity"))
                    {
                        string entName = XmlUtils.GetAttributeValue<string>(item8, "Name");
                        if (entName == "MalePlayer" || entName == "FemalePlayer" || entName == "Player")
                        {
                            string[] reserveParameters2 = new string[1] { "Body" };
                            if (sourceElement != null)
                            {
                                ReplaceNodes(item8, sourceElement, reserveParameters2);
                            }
                            break;
                        }
                    }
                }

                using (Stream stream4 = Storage.OpenFile(Storage.CombinePaths(wpath, "Project.xml"), OpenFileMode.Create))
                {
                    XmlUtils.SaveXmlToStream(xElement7, stream4, null, throwOnError: true);
                }

                SynchronizeGameInfo(xElement6, xElement7, path, wpath, isNewWorld);
                m_worldPath = wpath;

                if (IsChildWorld(path))
                {
                    path = Storage.GetDirectoryName(path);
                }

                SavePathToMajorWorld(path, m_worldPath);
            }
            catch (Exception)
            {
            }
            finally
            {
                WorldInfo worldInfo = WorldsManager.GetWorldInfo(wpath);
                ScreensManager.SwitchScreen("GameLoading", worldInfo, null);
            }
        }

        public void GenerateDoor(Point3 position)
        {
            int worldDoorBlock = GetWorldDoorBlock(m_worldType);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    int value = 0;
                    if (i == 0 || i == 3)
                    {
                        value = worldDoorBlock;
                    }
                    if ((i == 1 || i == 2) && (j == 0 || j == 4))
                    {
                        value = worldDoorBlock;
                    }
                    m_subsystemTerrain.ChangeCell(position.X + i, position.Y + j, position.Z, value);
                }
            }
            SubsystemTransferBlockBehavior subsystemTransferBlockBehavior = base.Project.FindSubsystem<SubsystemTransferBlockBehavior>(throwOnError: true);
            subsystemTransferBlockBehavior.CreateEntrance(position + new Point3(1, 0, 0), position + new Point3(2, 0, 0), skipJudge: true, m_worldType);
        }

        public static bool IsChildWorld(string cpath)
        {
            string directoryName = Storage.GetDirectoryName(cpath);
            return directoryName != WorldsManager.WorldsDirectoryName && Storage.GetDirectoryName(directoryName) == WorldsManager.WorldsDirectoryName;
        }

        public static Dictionary<string, Stream> GetScworldList(string path)
        {
            Dictionary<string, Stream> dictionary = new Dictionary<string, Stream>();
            foreach (string item in Storage.ListFileNames(path))
            {
                string extension = Storage.GetExtension(item);
                string path2 = Storage.CombinePaths(path, item);
                Stream stream = Storage.OpenFile(path2, OpenFileMode.Read);
                try
                {
                    if (!(extension == ".scmod"))
                    {
                        continue;
                    }
                    ZipArchive zipArchive = ZipArchive.Open(stream, keepStreamOpen: true);
                    foreach (ZipArchiveEntry item2 in zipArchive.ReadCentralDir())
                    {
                        if (Storage.GetExtension(item2.FilenameInZip) == ".scworld")
                        {
                            MemoryStream memoryStream = new MemoryStream();
                            zipArchive.ExtractFile(item2, memoryStream);
                            memoryStream.Position = 0L;
                            string fileNameWithoutExtension = Storage.GetFileNameWithoutExtension(item2.FilenameInZip);
                            dictionary.Add(fileNameWithoutExtension, memoryStream);
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            return dictionary;
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

        public static void SavePathToMajorWorld(string path, string wpath)
        {
            XElement xElement = null;

            using (Stream stream = Storage.OpenFile(Storage.CombinePaths(path, "Project.xml"), OpenFileMode.Read))
            {
                xElement = XmlUtils.LoadXmlFromStream(stream, null, throwOnError: true);
            }

            foreach (XElement item in xElement.Element("Subsystems").Elements())
            {
                if (XmlUtils.GetAttributeValue<string>(item, "Name") == SubsystemName)
                {
                    XmlUtils.SetAttributeValue(item.Element("Value"), "Value", wpath);
                }
            }

            using Stream stream2 = Storage.OpenFile(Storage.CombinePaths(path, "Project.xml"), OpenFileMode.Create);
            XmlUtils.SaveXmlToStream(xElement, stream2, null, throwOnError: true);
        }

        public static void SaveCreatures(string path, string creatureName)
        {
            XElement xElement = null;

            if (!Storage.DirectoryExists(path))
                return;

            using (Stream stream = Storage.OpenFile(Storage.CombinePaths(path, "Project.xml"), OpenFileMode.Read))
            {
                xElement = XmlUtils.LoadXmlFromStream(stream, null, throwOnError: true);
            }

            foreach (XElement item in xElement.Element("Subsystems").Elements())
            {
                if (XmlUtils.GetAttributeValue<string>(item, "Name") == SubsystemName)
                {
                    if (item.Elements().Count() == 1)
                    {
                        XElement content = new XElement("Value");
                        item.Add(content);
                        XmlUtils.SetAttributeValue(item.Elements().ElementAt(1), "Name", "Creatures");
                        XmlUtils.SetAttributeValue(item.Elements().ElementAt(1), "Type", "string");
                        XmlUtils.SetAttributeValue(item.Elements().ElementAt(1), "Value", "Null");
                    }
                    string attributeValue = XmlUtils.GetAttributeValue<string>(item.Elements().ElementAt(1), "Value");
                    XmlUtils.SetAttributeValue(value: (!(attributeValue == "Null")) ? (attributeValue + "," + creatureName) : creatureName, node: item.Elements().ElementAt(1), attributeName: "Value");
                    break;
                }
            }

            using Stream stream2 = Storage.OpenFile(Storage.CombinePaths(path, "Project.xml"), OpenFileMode.Create);
            XmlUtils.SaveXmlToStream(xElement, stream2, null, throwOnError: true);
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

        public static int GetWorldDoorBlock(WorldType worldType)
        {
            int result = 0;
            string[][] collections = WorldParameter.Collections;
            foreach (string[] array in collections)
            {
                if (worldType.ToString() == array[0])
                {
                    result = int.Parse(array[2]);
                    break;
                }
            }
            return result;
        }

        public static Color GetWorldDoorColor(WorldType worldType)
        {
            Color result = Color.White;
            string[][] collections = WorldParameter.Collections;
            foreach (string[] array in collections)
            {
                if (worldType.ToString() == array[0])
                {
                    string[] array2 = array[3].Split(',');
                    int r = int.Parse(array2[0]);
                    int g = int.Parse(array2[1]);
                    int b = int.Parse(array2[2]);
                    int a = int.Parse(array2[3]);
                    result = new Color(r, g, b, a);
                    break;
                }
            }
            return result;
        }
    }
}
