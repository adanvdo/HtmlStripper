using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlStripper
{
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
            catch (Exception ex)
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

        public StripResults Strip(Elements elementsToStrip)
        {
            Read();

            if (Html == null) throw new ArgumentNullException("Html");
            if (elementsToStrip == null) throw new ArgumentNullException("elementsToStrip");

            StripResults res = new StripResults(elementsToStrip);
            try
            {

                var docNode = Html.DocumentNode;
                var childNodes = GetChildrenRecursively(docNode);

                foreach (var element in elementsToStrip.Class)
                {
                    var classes = childNodes.Where(n => n.NodeType == HtmlNodeType.Element
                            && n.HasAttributes && n.Attributes["class"] != null
                            && n.Attributes["class"].Value.ToLower() == element.Name.ToLower()).ToList();

                    if (classes != null && classes.Count > 0)
                    {
                        foreach(HtmlNode n in classes)
                        {
                            if (element.Remove == Remove.Element)
                                n.Remove();
                            else if (element.Remove == Remove.Contents)
                                n.RemoveAllChildren();
                            else
                                n.RemoveAll();
                        }
                    }
                    else
                    {
                        res.NotStripped.Class.Add(element);
                    }
                }

                foreach (var element in elementsToStrip.Tag)
                {
                    var tags = childNodes.Where(n => n.NodeType == HtmlNodeType.Element && n.Name == element.Name).ToList();
                    if (tags != null && tags.Count > 0)
                    {
                        foreach (HtmlNode n in tags)
                        {
                            if (element.Remove == Remove.Element)
                                n.Remove();
                            else if (element.Remove == Remove.Contents)
                                n.RemoveAllChildren();
                            else
                                n.RemoveAll();
                        }
                    }
                    else
                    {
                        res.NotStripped.Tag.Add(element);
                    }
                }

                foreach (var element in elementsToStrip.Id)
                {
                    var ids = childNodes.Where(n => n.Id == element.Name).ToList();
                    if (ids != null && ids.Count > 0)
                    {
                        foreach (HtmlNode n in ids)
                        {
                            if (element.Remove == Remove.Element)
                                n.Remove();
                            else if (element.Remove == Remove.Contents)
                                n.RemoveAllChildren();
                            else
                                n.RemoveAll();
                        }
                    }
                    else
                    {
                        res.NotStripped.Id.Add(element);
                    }
                }

                foreach (var element in elementsToStrip.Other)
                {
                    var otherNodes = childNodes.Where(n => n.NodeType.ToString().ToLower() == element.Name).ToList();
                    if (otherNodes != null && otherNodes.Count > 0)
                    {
                        foreach (HtmlNode n in otherNodes)
                        {
                            if (element.Remove == Remove.Element)
                                n.Remove();
                            else if (element.Remove == Remove.Contents)
                                n.RemoveAllChildren();
                            else
                                n.RemoveAll();
                        }
                    }
                    else
                    {
                        res.NotStripped.Other.Add(element);
                    }
                }

                var emptyNodes = childNodes.Where(n => n.Name == "#text" && !n.HasChildNodes && string.IsNullOrWhiteSpace(n.InnerText)).ToList();

                this.Stripped = true;
                string test = docNode.OuterHtml;
                res.ResultHtml = docNode.WriteContentTo();

                using (TextWriter writer = new StreamWriter(FilePath, false))
                {
                    docNode.WriteTo(writer);
                    writer.Flush();
                }
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
