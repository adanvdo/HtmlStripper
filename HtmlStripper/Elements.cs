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
        public List<string> Class { get; set; } = new();
        [JsonPropertyName("tag")]
        public List<string> Tag { get; set; } = new();
        [JsonPropertyName("other")]
        public List<string> Other { get; set; } = new();

        public Elements() { }
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
}
