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
        /// The directory where the application can safely write persistent files.
        /// </summary>
        public static string ConfigDirectory { get; private set; }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.WindowsEditor:
                {
                    var project = new DirectoryInfo(Application.dataPath).Parent;
                    ConfigDirectory = Path.Combine(project.FullName, "Configs/");
                    break;
                }
                default:
                {
                    ConfigDirectory = Application.persistentDataPath;
                    break;
                }
            }
        }


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
                Debug.LogError($"Failed to get files in directory \"{directory}\"! {e}");
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
                Debug.LogError($"Failed to write to \"{path}\"! {e}");
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
                Debug.LogError($"Failed to write to \"{path}\"! {e}");
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
                Debug.LogError($"Failed to read from \"{path}\"! {e}");

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
                Debug.LogError($"Failed to read from \"{path}\"! {e}");

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
                Debug.LogError($"Failed to open file stream at \"{path}\"! {e}");

                stream = null;
                return false;
            }
        }

        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="path">The full name of the file to delete.</param>
        /// <returns>True if the file was deleted or does not exist, false if an exception occurs.</returns>
        public static bool DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete file \"{path}\"! {e}");
                return false;
            }
        }
    }
}