using System;
using System.Diagnostics.Metrics;
using System.IO;
using HtmlStripper.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace HtmlStripper
{
    internal class Program
    {
        static int maxInputAttempts = 3;
        static string directoryPath = string.Empty;
        static string copyToDirectoryPath = string.Empty;
        static List<HtmlFile> htmlFiles = new();
        static string jsonPath = string.Empty;
        static Elements elements = null;

        /// <summary>
        /// Main Entry Point for Application
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                string argValue = arg;
                if (arg.StartsWith("-")) argValue = arg.Remove(0, 1);

                if (argValue.StartsWith("dir=")) directoryPath = argValue.Replace("dir=", "");
                if (argValue.StartsWith("json=")) jsonPath = argValue.Replace("json=", "");
                if (argValue.StartsWith("copydir=")) copyToDirectoryPath = argValue.Replace("copydir=", "");
            }

            if (directoryPath.Length < 1)
            {
                Console.WriteLine("Directory Path Needed");
                Console.WriteLine();
                getDirectory();
                return;
            }
            else if (htmlFiles.Count < 1)
            {
                Console.WriteLine("Html Files Needed");
                Console.WriteLine();
                getHtmlFiles(new DirectoryInfo(directoryPath));
                return;
            }
            else if (jsonPath.Length < 1)
            {
                Console.WriteLine("Json Path Needed");
                Console.WriteLine();
                getJsonFile();
                return;
            }
            else
            {
                Console.WriteLine("Deserialization Needed");
                Console.WriteLine();
                parseJsonFile(new FileInfo(jsonPath));
                return;
            }
        }

        /// <summary>
        /// Prompts the user for the base path to the web files
        /// </summary>
        private static void getDirectory()
        {
            directoryPath = string.Empty;
            copyToDirectoryPath = string.Empty;
            jsonPath = string.Empty;
            htmlFiles = new List<HtmlFile>();
            elements = null;

            int inputAttempts = 0;
            bool ok = false;
            while (!ok && inputAttempts < 3)
            {
                directoryPath = promptForInput("Enter Base Site Path", FileUtil.IsValidDirectory);
                ok = directoryPath != null;
                if (!ok) inputAttempts++;
            }

            if (!ok)
            {
                Console.WriteLine("User Error. Terminating Program");
                Environment.Exit(0);
            }

            DirectoryInfo dir = new DirectoryInfo(directoryPath);
            
            getHtmlFiles(dir);
        }

        /// <summary>
        /// Gets a list of all html files in the targeted directory
        /// </summary>
        /// <param name="dir"></param>
        private static void getHtmlFiles(DirectoryInfo dir)
        {
            htmlFiles = FileUtil.ListHtmlFiles(dir);
            if (htmlFiles == null)
            {
                Console.WriteLine("Failed to read html files");
                getDirectory();
            }
            else if(jsonPath.Length < 1)
            {
                getJsonFile();
            }
            else if(elements == null)
            {
                parseJsonFile(new FileInfo(jsonPath));
            }
        }

        /// <summary>
        /// Gets a list of all html files in the targeted directory
        /// </summary>
        /// <param name="dir"></param>
        private static bool updateHtmlFiles(DirectoryInfo dir)
        {
            htmlFiles = FileUtil.ListHtmlFiles(dir);
            if (htmlFiles == null) return false;
            return true;
        }

        /// <summary>
        /// Loads the node json file
        /// </summary>
        private static void getJsonFile()
        {
            int inputAttempts = 0;
            bool ok = false;
            while (!ok && inputAttempts < 3)
            {
                jsonPath = promptForInput("Enter json File Path", FileUtil.IsValidFile);
                ok = jsonPath != null;
                if (!ok) inputAttempts++;
            }

            if (!ok)
            {
                Console.WriteLine("User Error. Terminating Program");
                Environment.Exit(0);
            }

            var file = new FileInfo(jsonPath);
            if (file.Exists)
            {
                parseJsonFile(file);
            }
        }

        /// <summary>
        /// Reads the node json file and deserializes it into an Element object
        /// </summary>
        /// <param name="jsonFileInfo"></param>
        private static void parseJsonFile(FileInfo jsonFileInfo)
        {
            string json = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(jsonFileInfo.FullName))
                {
                    json = reader.ReadToEnd();
                    var ds = JsonSerializer.Deserialize<Elements>(json);
                    if (ds != null) elements = ds;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            bool cont = promptForContinue($"{htmlFiles.Count} files will be scanned for the elements listed in the provided json file.");
            if (!cont)
            {
                getDirectory();
            }
            else
            {
                createCopies();
            }
        }

        private static string progressTemplate = "[----------------------------------------]";

        /// <summary>
        /// Updates the progress displayed in the console
        /// </summary>
        /// <param name="completed"></param>
        /// <exception cref="ArgumentException"></exception>
        private static void updateProgress(int completed)
        {
            if (completed > 40) throw new ArgumentException("value exceeds expected range");
            if (completed == 0) return;

            var progress = progressTemplate.Replace(progressTemplate.Substring(0, completed + 1), progressTemplate.Substring(0, completed + 1).Replace('-', '#'));
            Console.Write("\r{0}", progress);
        }

        private static void createCopies()
        {
            try
            {
                if (!string.IsNullOrEmpty(copyToDirectoryPath))
                {
                    DirectoryInfo copyDir = new DirectoryInfo(copyToDirectoryPath);
                    if (copyDir.Exists)
                    {
                        var files = copyDir.GetFiles("*.*", SearchOption.AllDirectories);
                        if(files.Length > 0)
                        {
                            bool clean = promptForContinue($"The destination folder {copyToDirectoryPath} exists and contains files. These files will be deleted if we proceed.");
                            if (clean)
                            {
                                Console.WriteLine();
                                Console.WriteLine("Deleting Folder Contents");
                                FileUtil.DeleteFolderContents(copyToDirectoryPath);
                            }
                            else
                            {
                                Console.WriteLine("The process was cancelled.");
                                Console.WriteLine();
                                getDirectory();
                            }
                        }                        
                    }
                }
                else
                {
                    bool ok = false;
                    copyToDirectoryPath = promptForInput("Enter destination folder path for updated files, or press enter to have one created for you", FileUtil.IsValidDirectory, true);
                    ok = copyToDirectoryPath != null;
                    if (!ok)
                    {
                        DirectoryInfo dir = new DirectoryInfo(directoryPath);
                        var copyDir = Directory.CreateDirectory(Path.Combine(dir.Parent != null ? dir.Parent.FullName : dir.FullName, $"{dir.Name}_Copy_{DateTime.Now.ToString("hhmmssms")}"));
                        copyToDirectoryPath = copyDir.FullName;
                    }
                    else if(copyToDirectoryPath == directoryPath)
                    {
                        Console.WriteLine();
                        Console.WriteLine("The destination folder cannot be the same as the source folder");
                        copyToDirectoryPath = string.Empty;
                        createCopies();
                        return;
                    }
                }

                Console.WriteLine("Creating File Copies..");
                Console.WriteLine();

                bool copyComplete = FileUtil.CopyFilesAndFoldersRecursively(directoryPath, copyToDirectoryPath, updateProgress);
                if (!copyComplete)
                {
                    Console.WriteLine("The process has failed or was interrupted.");
                    Console.WriteLine();
                    getDirectory();
                    return;
                }

                Console.WriteLine("Updating Metadata");
                Console.WriteLine();
                bool updateFiles = updateHtmlFiles(new DirectoryInfo(copyToDirectoryPath));

                Console.WriteLine("Stripping HTML..");
                Console.WriteLine();
                stripHtmlFiles(copyToDirectoryPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Strips the Html file content of any elements specified in the json file.
        /// </summary>
        private static void stripHtmlFiles(string baseDirectory)
        {
            int processed = 0;
            try
            {
                DirectoryInfo di = new DirectoryInfo(baseDirectory);

                decimal conversion = (decimal)40 / (decimal)htmlFiles.Count;

                int progress = 0;
                Console.Write(progressTemplate);

                foreach (HtmlFile file in htmlFiles)
                {
                    file.Strip(elements);
                    processed++;
                    progress = (int)(processed * conversion);
                    updateProgress(progress);
                }
                progress = (int)((processed + 1) * conversion);
                updateProgress(progress);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return;
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Stripped Elements from {processed} HTML files");
            Console.WriteLine($"Updated Files can be found in: {baseDirectory}");
            Console.WriteLine();
            Console.WriteLine("Press 'o' to open the directory. Press any other key to exit");
            var key = Console.ReadKey();
            if(key.Key == ConsoleKey.O)
            {
                string argument = "/select, \"" + baseDirectory + "\"";

                Process.Start("explorer.exe", argument);
                Thread.Sleep(500);
            }

            Environment.Exit(0);
        }

        /// <summary>
        /// Continue Prompt
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool promptForContinue(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Console.WriteLine(message);
            }
            Console.WriteLine("Continue? (y/n)");
            Console.Write("=> ");
            var input = Console.ReadKey();

            Console.WriteLine();
            Console.WriteLine();
            return input != null && input.Key == ConsoleKey.Y;
        }

        /// <summary>
        /// Prompts user to input a value
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="validationMethod"></param>
        /// <returns></returns>
        private static string promptForInput(string prompt, Delegate validationMethod, bool? methodCondition = null)
        {
            try
            {
                if (validationMethod == null)
                {
                    Console.WriteLine("Invalid Validation Method");
                    return null;
                }

                bool valid = false;
                Console.WriteLine();
                Console.WriteLine(prompt);
                Console.Write("=> ");
                string input = Console.ReadLine();

                Console.WriteLine();
                if (string.IsNullOrEmpty(input.Remove(0,3))) { return null; }

                bool validate = methodCondition != null ? (bool)validationMethod.DynamicInvoke(input, methodCondition) : (bool)validationMethod.DynamicInvoke(input);
                if (validate) return input;
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}