using System.Linq;
using System.IO;
using UnityEngine;

namespace Framework.IO
{
    public static class FileIO
    {
        private static string EDITOR_TEMP = "Configs/";

        public static FileInfo[] GetFiles(string directory, string extention)
        {
            DirectoryInfo dir = new DirectoryInfo(directory);
            return dir.GetFiles('*' + extention);
        }

        public static void WriteFile(string content, string path)
        {
            FileInfo file = new FileInfo(path);
            Directory.CreateDirectory(file.DirectoryName);
            File.WriteAllText(path, content);
        }

        public static void WriteFile(byte[] content, string path)
        {
            FileInfo file = new FileInfo(path);
            Directory.CreateDirectory(file.DirectoryName);
            File.WriteAllBytes(path, content);
        }

        public static string ReadFileText(string path)
        {
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                return File.ReadAllText(path);
            }
            return null;
        }

        public static byte[] ReadFileBytes(string path)
        {
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                return File.ReadAllBytes(path);
            }
            return null;
        }

        public static string GetInstallDirectory()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.WindowsEditor: return EDITOR_TEMP;
                default: return Application.dataPath + "/../";
            }
        }
    }
}