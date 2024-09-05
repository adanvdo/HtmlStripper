using System;
using System.Diagnostics.Metrics;
using System.IO;
using HtmlStripper.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HtmlStripper
{
    internal class Program
    {
        static int maxInputAttempts = 3;
        static string directoryPath = string.Empty;
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
                if (arg.StartsWith("dir=")) directoryPath = arg.Replace("dir=", "");

                if (arg.StartsWith("json=")) jsonPath = arg.Replace("json=", "");
            }

            if (directoryPath.Length < 1)
            {
                Console.WriteLine("Directory Path Needed");
                getDirectory();
            }
            else if (htmlFiles.Count < 1)
            {
                Console.WriteLine("Html Files Needed");
                getHtmlFiles(new DirectoryInfo(directoryPath));
            }
            else if (jsonPath.Length < 1)
            {
                Console.WriteLine("Json Path Needed");
                getJsonFile();
            }
            else
            {
                Console.WriteLine("Deserialization Needed");
                parseJsonFile(new FileInfo(jsonPath));
            }
        }

        /// <summary>
        /// Prompts the user for the base path to the web files
        /// </summary>
        private static void getDirectory()
        {
            directoryPath = string.Empty;
            jsonPath = string.Empty;
            htmlFiles = new List<HtmlFile>();


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
            else
            {
                parseJsonFile(new FileInfo(jsonPath));
            }
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
                getJsonFile();
                return;
            }

            bool cont = promptForContinue($"{htmlFiles.Count} files will be scanned for the elements listed in the provided json file.");
            if (!cont)
            {
                getDirectory();
            }
            else
            {
                stripHtmlFiles();
            }

        }

        private static string progressTemplate = "[----------------------------------------]";

        /// <summary>
        /// Strips the Html file content of any elements specified in the json file.
        /// </summary>
        private static void stripHtmlFiles()
        {
            int processed = 0;
            DirectoryInfo tempDir = null;
            try
            {
                DirectoryInfo di = new DirectoryInfo(directoryPath);
                tempDir = Directory.CreateDirectory(Path.Combine(di.Parent == null ? di.FullName : di.Parent.FullName, $"{di.Name}_Stripped_{DateTime.Now.Ticks}"));

                decimal conversion = (decimal)40 / (decimal)htmlFiles.Count;

                int progress = 0;
                foreach (HtmlFile file in htmlFiles)
                {
                    file.Strip(elements, file.FilePath.Replace(directoryPath, tempDir.FullName));
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
            Console.WriteLine($"Files have been saved to: {tempDir.FullName}");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            Environment.Exit(0);
        }

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
            string input = Console.ReadLine();

            return input != null && input.ToLower() == "y";
        }

        /// <summary>
        /// Prompts user to input a value
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="validationMethod"></param>
        /// <returns></returns>
        private static string promptForInput(string prompt, Delegate validationMethod)
        {
            try
            {
                if (validationMethod == null)
                {
                    Console.WriteLine("Invalid Validation Method");
                    return null;
                }

                bool valid = false;
                Console.WriteLine(prompt);
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) { return null; }

                bool validate = (bool)validationMethod.DynamicInvoke(input);
                if (validate) return input;
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}