using UnityEngine;
using System.IO;

namespace Framework.IO
{
    public static class FileIO
    {
        private static string EDITOR_CONFIGS = "Configs/";

        public static bool FileExists(string directory, string fileName)
        {
            string fullPath = directory + fileName;
            return Directory.Exists(directory) && File.Exists(fullPath);
        }

        public static void WriteFile(string content, string directory, string fileName)
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(directory + fileName, content);
        }

        public static string ReadFile(string directory, string fileName)
        {
            if (FileExists(directory, fileName))
            {
                string fullPath = directory + fileName;
                return File.ReadAllText(fullPath);
            }
            return null;
        }

        public static string GetInstallDirectory()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.WindowsEditor: return EDITOR_CONFIGS;
                default: return Application.dataPath + "/../";
            }
        }
    }
}