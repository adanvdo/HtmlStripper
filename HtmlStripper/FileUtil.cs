using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace HtmlStripper.Utils
{
    public static class FileUtil
    {
        /// <summary>
        /// Checks if the path exists
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public static bool IsValidDirectory(string directoryPath, bool createOnInvalid = false)
        {
            try
            {
                var dir = new DirectoryInfo(directoryPath);
                if (!dir.Exists && createOnInvalid)
                {
                    dir.Create();
                }

                return dir.Exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Gets a list of all html files in the directory
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static List<HtmlFile> ListHtmlFiles(DirectoryInfo dir)
        {
            try
            {
                return Directory.EnumerateFiles(dir.FullName, "*.html", SearchOption.AllDirectories).Select(s => new HtmlFile(s)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Checks that the file actually exists
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool IsValidFile(string filePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Deserializes json string into Element object
        /// </summary>
        /// <param name="jsonFileInfo"></param>
        /// <returns></returns>
        public static Elements DeserializeJsonFileContent(FileInfo jsonFileInfo)
        {
            string json = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(jsonFileInfo.FullName))
                {
                    json = reader.ReadToEnd();
                    var ds = JsonSerializer.Deserialize<Elements>(json);
                    if (ds != null) return ds;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        private static string progressTemplate = "[----------------------------------------]";

        /// <summary>
        /// Recursive Copy of Folder Contents
        /// </summary>
        /// <param name="sourceFolderPath"></param>
        /// <param name="targetFolderPath"></param>
        public static bool CopyFilesAndFoldersRecursively(string sourceFolderPath, string targetFolderPath, Delegate progressMethod)
        {
            try
            {
                var folders = Directory.GetDirectories(sourceFolderPath, "*", SearchOption.AllDirectories);
                var files = Directory.GetFiles(sourceFolderPath, "*.*", SearchOption.AllDirectories);

                decimal conversion = (decimal)40 / (decimal)(folders.Length + files.Length);

                int processed = 0;
                int progress = 0;

                Console.Write(progressTemplate);

                //Now Create all of the directories
                foreach (string dirPath in folders)
                {
                    Directory.CreateDirectory(dirPath.Replace(sourceFolderPath, targetFolderPath));
                    processed++;
                    progress = (int)(processed * conversion);
                    progressMethod.DynamicInvoke(progress);
                }

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in files)
                {
                    File.Copy(newPath, newPath.Replace(sourceFolderPath, targetFolderPath), true);
                    processed++;
                    progress = (int)(processed * conversion);
                    progressMethod.DynamicInvoke(progress);
                }
                progress = (int)((processed + 1) * conversion);
                progressMethod.DynamicInvoke(progress);

                Console.WriteLine();
                Console.WriteLine();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public static bool DeleteFolderContents(string folderPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(folderPath);
                if (dir.Exists)
                {
                    foreach(DirectoryInfo di in dir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                    {
                        di.Delete(true);
                    }

                    foreach(FileInfo fi in dir.EnumerateFiles("*.*", SearchOption.AllDirectories))
                    {
                        fi.Delete();
                    }
                    return true;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }
    }    
}
