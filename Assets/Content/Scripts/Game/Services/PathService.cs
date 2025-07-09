using System.IO;
using UnityEngine;

namespace Content.Scripts.Game.Services
{
    public static class PathService
    {
        public static string DataPath => Application.dataPath;
        public static string DataFolderPath => DataPath + @"\..\Data\";

        public static string MapsPath => DataFolderPath + @"\Maps\";
        public static string PlayerFolderPath => DataFolderPath + @"\Player\";
        public static string PlayerConfigPath => PlayerFolderPath + @"\config.json";

        public static void CreateFolders()
        {
            if (!Directory.Exists(DataFolderPath))
            {
                Directory.CreateDirectory(DataFolderPath);
            }

            if (!Directory.Exists(MapsPath))
            {
                Directory.CreateDirectory(MapsPath);
            }
            
            if (!Directory.Exists(PlayerFolderPath))
            {
                Directory.CreateDirectory(PlayerFolderPath);
            }
        }
    }
}
