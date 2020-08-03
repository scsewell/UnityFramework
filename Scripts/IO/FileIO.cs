using System;
using System.IO;
using System.Text;

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
                var dir = new DirectoryInfo(directory);
                return dir.Exists ? dir.GetFiles('*' + extention) : new FileInfo[0];
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get files in directory \"{directory}\"! {e.ToString()}");
                return new FileInfo[0];
            }
        }

        /// <summary>
        /// Writes text to a UTF-8 encoded file.
        /// </summary>
        /// <param name="path">The full name of the file to write.</param>
        /// <param name="contents">The content to write.</param>
        /// <returns>True if the writing was successful.</returns>
        public static bool WriteFile(string path, string contents)
        {
            try
            {
                var file = new FileInfo(path);

                if (!file.Directory.Exists)
                {
                    Directory.CreateDirectory(file.DirectoryName);
                }

                File.WriteAllText(path, contents, Encoding.UTF8);
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
        /// <param name="contents">The content to write.</param>
        /// <returns>True if the writing was successful.</returns>
        public static bool WriteFile(string path, byte[] contents)
        {
            try
            {
                var file = new FileInfo(path);

                if (!file.Directory.Exists)
                {
                    Directory.CreateDirectory(file.DirectoryName);
                }

                File.WriteAllBytes(path, contents);
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
        /// <param name="contents">The file contents.</param>
        /// <returns>True if the file was successfuly read.</returns>
        public static bool ReadFileText(string path, out string contents)
        {
            try
            {
                var file = new FileInfo(path);

                if (!file.Exists)
                {
                    contents = null;
                    return false;
                }

                contents = File.ReadAllText(path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read from \"{path}\"! {e.ToString()}");

                contents = null;
                return false;
            }
        }

        /// <summary>
        /// Reads from a file.
        /// </summary>
        /// <param name="path">The full name of the file to read.</param>
        /// <param name="contents">The file contents.</param>
        /// <returns>True if the file was successfuly read.</returns>
        public static bool ReadFileBytes(string path, out byte[] contents)
        {
            try
            {
                var file = new FileInfo(path);

                if (!file.Exists)
                {
                    contents = null;
                    return false;
                }

                contents = File.ReadAllBytes(path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read from \"{path}\"! {e.ToString()}");

                contents = null;
                return false;
            }
        }

        /// <summary>
        /// Opens a file stream.
        /// </summary>
        /// <param name="path">The full name of the file to open.</param>
        /// <param name="stream">The file steram.</param>
        /// <returns>True if the file was successfuly openned.</returns>
        public static bool OpenFileStream(string path, out FileStream stream)
        {
            try
            {
                var file = new FileInfo(path);

                if (!file.Exists)
                {
                    stream = null;
                    return false;
                }

                stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to open file stream at \"{path}\"! {e.ToString()}");

                stream = null;
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
                    return new DirectoryInfo(Application.dataPath).Parent.FullName;
            }
        }
    }
}