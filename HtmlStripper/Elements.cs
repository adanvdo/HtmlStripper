using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HtmlStripper
{
    public class Elements
    {
        [JsonPropertyName("class")]
        public List<Element> Class { get; set; } = new();
        [JsonPropertyName("tag")]
        public List<Element> Tag { get; set; } = new();
        [JsonPropertyName("id")]
        public List<Element> Id { get; set; } = new();
        [JsonPropertyName("other")]
        public List<Element> Other { get; set; } = new();

        public Elements() { }
    }

    public class Element
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("remove")]
        public Remove Remove { get; set; }

        public Element() { }

        public Element(string name, Remove remove)
        {
            Name = name;
            Remove = remove;
        }
    }

    public class StripResults
    {
        public Elements Strip { get; set; }
        public Elements NotStripped { get; set; }
        public string ResultHtml { get; set; } = null;

        public StripResults(Elements toStrip)
        {
            this.Strip = toStrip;
            this.NotStripped = new Elements();
        }
    }

    public enum Remove
    {
        Element = 0,
        Contents = 1,
        ContentsAndAttributes = 2
    }
}
