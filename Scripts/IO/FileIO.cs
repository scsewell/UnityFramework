using System;
using System.IO;

using UnityEngine;

namespace Framework.IO
{
    /// <summary>
    /// Utilities for handling file system access.
    /// </summary>
    public static class FileIO
    {
        /// <summary>
        /// The folder config files are saved to in the project directory.
        /// </summary>
        private static string EDITOR_CONFIGS_FOLDER = "Configs/";

        /// <summary>
        /// Gets the files in the given directory.
        /// </summary>
        /// <param name="directory">The full directory name to search.</param>
        /// <param name="extention">The file extention of the files to search for.</param>
        /// <returns>A new array with the file information.</returns>
        public static FileInfo[] GetFiles(string directory, string extention)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(directory);
                return dir.GetFiles('*' + extention);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get files in directory \"{directory}\"! {e.ToString()}");
                return new FileInfo[0];
            }
        }

        /// <summary>
        /// Writes text to a file.
        /// </summary>
        /// <param name="path">The full name of the file to write.</param>
        /// <param name="content">The content to write/</param>
        /// <returns>True if the writing was successful.</returns>
        public static bool WriteFile(string path, string content)
        {
            try
            {
                FileInfo file = new FileInfo(path);

                if (!Directory.Exists(file.DirectoryName))
                {
                    Directory.CreateDirectory(file.DirectoryName);
                }

                File.WriteAllText(path, content);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to \"{path}\"! {e.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// Writes binary data to a file.
        /// </summary>
        /// <param name="path">The full name of the file to write.</param>
        /// <param name="content">The content to write/</param>
        /// <returns>True if the writing was successful.</returns>
        public static bool WriteFile(string path, byte[] content)
        {
            try
            {
                FileInfo file = new FileInfo(path);

                if (!Directory.Exists(file.DirectoryName))
                {
                    Directory.CreateDirectory(file.DirectoryName);
                }

                File.WriteAllBytes(path, content);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to \"{path}\"! {e.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// Reads from a file.
        /// </summary>
        /// <param name="path">The full name of the file to read.</param>
        /// <param name="content">The read contents.</param>
        /// <returns>True if the file was successfuly read.</returns>
        public static bool ReadFileText(string path, out string content)
        {
            try
            {
                FileInfo file = new FileInfo(path);

                if (!file.Exists)
                {
                    content = null;
                    return false;
                }

                content = File.ReadAllText(path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read from \"{path}\"! {e.ToString()}");

                content = null;
                return false;
            }
        }

        /// <summary>
        /// Reads from a file.
        /// </summary>
        /// <param name="path">The full name of the file to read.</param>
        /// <param name="content">The read contents.</param>
        /// <returns>True if the file was successfuly read.</returns>
        public static bool ReadFileBytes(string path, out byte[] content)
        {
            try
            {
                FileInfo file = new FileInfo(path);

                if (!file.Exists)
                {
                    content = null;
                    return false;
                }

                content = File.ReadAllBytes(path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read from \"{path}\"! {e.ToString()}");

                content = null;
                return false;
            }
        }

        /// <summary>
        /// Gets the directory where configuration file can be written.
        /// </summary>
        public static string GetConfigDirectory()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.WindowsEditor:
                    return EDITOR_CONFIGS_FOLDER;
                default:
                    return Path.Combine(Application.dataPath, "/../");
            }
        }
    }
}