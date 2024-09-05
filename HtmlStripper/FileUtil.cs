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
        public static bool IsValidDirectory(string directoryPath)
        {
            try
            {
                var dir = new DirectoryInfo(directoryPath);
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
    }

    /// <summary>
    /// Html File
    /// </summary>
    public class HtmlFile
    {
        public FileInfo? FileInfo { get; set; } = null;
        public string FilePath 
        { 
            get
            {
                if (FileInfo == null) return null;
                return FileInfo.FullName;
            } 
        }

        public HtmlDocument Html { get; set; }
        public bool Stripped { get; set; } = false;
        public StripResults StripResults { get; set; } = null;

        public HtmlFile(string filePath)
        {
            if (!initFileInfo(filePath))
            {
                Console.WriteLine($"Failed to Populate File Info: {filePath}");
                return;
            }
        }

        private bool initFileInfo(string path)
        {
            try
            {
                FileInfo = new FileInfo(path);
                if (FileInfo.Exists)
                {                    
                    return true;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        public void Read()
        {
            try
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.Load2(this.FilePath);

                this.Html = htmlDoc;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        internal static IEnumerable<HtmlNode> GetChildrenRecursively(HtmlNode e) => e.ChildNodes.Union(e.ChildNodes.SelectMany(c => GetChildrenRecursively(c)));

        public StripResults Strip(Elements elementsToStrip, string savePath)
        {
            Read();

            if (Html == null) throw new ArgumentNullException("Html");
            if (elementsToStrip == null) throw new ArgumentNullException("elementsToStrip");

            StripResults res = new StripResults(elementsToStrip);
            try
            {

                var docNode = Html.DocumentNode;
                var test = GetChildrenRecursively(docNode);

                List<HtmlNode> classNodes = new();
                List<HtmlNode> tagNodes = new();
                List<HtmlNode> other = new();
                List<HtmlNode> allNodes = new();

                foreach (var element in elementsToStrip.Class)
                {
                    var classes = test.Where(n => n.HasAttributes && n.Attributes["class"] != null && n.Attributes["class"].Value == element).ToList();
                    if(classes != null && classes.Count > 0)
                    {
                        classNodes.AddRange(classes);
                    }
                    else
                    {
                        res.NotStripped.Class.Add(element);
                    }
                }

                foreach (var element in elementsToStrip.Tag)
                {
                    var tags = test.Where(n => n.Name == element).ToList();
                    if (tags != null && tags.Count > 0)
                    {
                        tagNodes.AddRange(tags);
                    }
                    else
                    {
                        res.NotStripped.Tag.Add(element);
                    }
                }

                foreach (var element in elementsToStrip.Other)
                {
                    var otherNodes = test.Where(n => n.NodeType.ToString().ToLower() == element).ToList();
                    if(otherNodes != null && otherNodes.Count > 0)
                    {
                        other.AddRange(otherNodes);
                    }
                    else
                    {
                        res.NotStripped.Other.Add(element);
                    }
                }

                allNodes.AddRange(classNodes);
                allNodes.AddRange(tagNodes);
                allNodes.AddRange(other);

                foreach (var node in allNodes)
                {
                    node.RemoveAll();
                }

                this.Stripped = true;

                res.ResultHtml = docNode.WriteContentTo();
                Html.Save(savePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            this.StripResults = res;

            return res;
        }
    }
}
