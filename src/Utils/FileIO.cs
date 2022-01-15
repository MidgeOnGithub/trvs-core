using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace TRVS.Core
{
    /// <summary>
    ///     Provides utility filesystem functionality.
    /// </summary>
    public static class FileIO
    {
        /// <summary>
        ///     Copies from <paramref name="sourceDir"/> to <paramref name="destDir"/>, overwriting if applicable.
        /// </summary>
        /// <param name="sourceDir">Source directory</param>
        /// <param name="destDir">Destination directory</param>
        /// <param name="recursive">Whether or not to copy subdirectories</param>
        /// <remarks>
        ///     See MSDN: https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        /// </remarks>
        public static void CopyDirectory(string sourceDir, string destDir, bool recursive)
        {
            // Ensure destination directory exists.
            _ = Directory.CreateDirectory(destDir);
            
            // Get the source directory's files and subdirectories.
            var dir = new DirectoryInfo(sourceDir);
            DirectoryInfo[] subDirs = dir.GetDirectories();
            FileInfo[] files = dir.GetFiles();

            // Copy the files in the directory.
            foreach (FileInfo file in files)
            {
                string destPath = Path.Combine(destDir, file.Name);
                file.CopyTo(destPath, overwrite: true);
            }

            // Recursively call this function to copy subdirectories.
            if (recursive)
            {
                foreach (DirectoryInfo subDir in subDirs)
                {
                    string destPath = Path.Combine(destDir, subDir.Name);
                    CopyDirectory(subDir.FullName, destPath, true);
                }
            }
        }

        /// <summary>
        ///     Computes <paramref name="file"/>'s MD5 hash.
        /// </summary>
        /// <param name="file"></param>
        /// <exception cref="FileNotFoundException">File that needs to be checked is missing.</exception>
        /// <returns>
        ///     <paramref name="file"/>'s MD5 hash
        /// </returns>
        public static string ComputeMd5Hash(string file)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(file, FileMode.Open);
                using var md5 = MD5.Create();
                byte[] hash = md5.ComputeHash(fs);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            catch (IOException e)
            {
                if (e is DirectoryNotFoundException || e is FileNotFoundException)
                    throw new FileNotFoundException($"File \"{file}\" not found!", e);
            }
            finally
            {
                fs?.Close();
            }

            return null;
        }

        /// <summary>
        ///     Deletes <paramref name="dirs"/>.
        /// </summary>
        /// <param name="dirs">paths of directories to delete</param>
        public static void DeleteDirectories(IEnumerable<string> dirs, bool recursive = false)
        {
            foreach (string dir in dirs)
	        {
                Directory.Delete(dir, recursive);
	        }
        }

        /// <summary>
        ///     Deletes <paramref name="files"/>.
        /// </summary>
        /// <param name="files">paths of files to delete</param>
        public static void DeleteFiles(IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }

        /// <summary>
        ///     Checks that all <paramref name="fileNames"/> exist in <paramref name="dir"/>.
        /// </summary>
        /// <param name="fileNames">File names to check for</param>
        /// <param name="dir">Directory to operate within</param>
        /// <returns>
        ///     The name of the first missing file or null if no files are missing
        /// </returns>
        public static string FindMissingFile(IEnumerable<string> fileNames, string dir)
        {
            return fileNames.Select(file => Path.Combine(dir, file)).FirstOrDefault(path => !File.Exists(path));
        }
    }
}
