using System.IO;
using System.Xml.Linq;
using Engine;
using XmlUtilities;

namespace Game
{
    public class LoadScreenLoader : ModLoader
    {
        public override void __ModInitialize()
        {
            ModsManager.RegisterHook("BeforeGameLoading", this);
        }

        public override object BeforeGameLoading(PlayScreen playScreen, object item)
        {
            string directoryName = ((WorldInfo)item).DirectoryName;
            string text = string.Empty;
            XElement xElement = null;
            XElement subXElement = null;

            using (Stream stream = Storage.OpenFile(Storage.CombinePaths(directoryName, "Project.xml"), OpenFileMode.Read))
            {
                xElement = XmlUtils.LoadXmlFromStream(stream, null, throwOnError: true);
            }

            foreach (XElement item2 in xElement.Element("Subsystems").Elements())
            {
                if (XmlUtils.GetAttributeValue<string>(item2, "Name") == SubsystemDimensions.SubsystemName)
                {
                    text = XmlUtils.GetAttributeValue<string>(item2.Element("Value"), "Value");
                }
            }

            if (text != string.Empty)
            {
                using (Stream stream2 = Storage.OpenFile(Storage.CombinePaths(text, "Project.xml"), OpenFileMode.Read))
                {
                    subXElement = XmlUtils.LoadXmlFromStream(stream2, null, throwOnError: true);
                }
                SubsystemDimensions.SynchronizeGameInfo(xElement, subXElement, directoryName, text, newWorld: false);
            }
            else
            {
                text = directoryName;
            }

            GameLoadingScreen gameLoadingScreen = ScreensManager.FindScreen<GameLoadingScreen>("GameLoading");
            gameLoadingScreen.m_worldInfo = WorldsManager.GetWorldInfo(text);
            return gameLoadingScreen.m_worldInfo;
        }
    }
}
