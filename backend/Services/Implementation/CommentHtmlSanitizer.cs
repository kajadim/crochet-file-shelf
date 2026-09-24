using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using Ganss.Xss;

namespace backend.Services.Implementation
{
    public static class CommentHtmlSanitizer
    {
        private static readonly string[] AllowedTags = ["p", "br", "strong", "em", "u", "ol", "ul", "li", "a"];
        private static readonly string[] AllowedSchemes = ["http", "https", "mailto"];
        private static readonly Regex BlockEnd = new(@"</(p|li)>|<br\s*/?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

        public static (string Html, string PlainText) Sanitize(string html)
        {
            var clean = Sanitizer.Sanitize(html).Trim();

            var spaced = BlockEnd.Replace(clean, " ");
            var text = new HtmlParser().ParseDocument(spaced).Body?.TextContent ?? string.Empty;
            var plain = Regex.Replace(text, @"\s+", " ").Trim();

            return (clean, plain);
        }

        private static HtmlSanitizer CreateSanitizer()
        {
            var sanitizer = new HtmlSanitizer { KeepChildNodes = true };

            sanitizer.AllowedTags.Clear();
            foreach (var tag in AllowedTags)
            {
                sanitizer.AllowedTags.Add(tag);
            }

            sanitizer.AllowedAttributes.Clear();
            sanitizer.AllowedAttributes.Add("href");

            sanitizer.AllowedSchemes.Clear();
            foreach (var scheme in AllowedSchemes)
            {
                sanitizer.AllowedSchemes.Add(scheme);
            }

            sanitizer.PostProcessNode += (_, e) =>
            {
                if (e.Node is AngleSharp.Html.Dom.IHtmlAnchorElement anchor)
                {
                    anchor.SetAttribute("target", "_blank");
                    anchor.SetAttribute("rel", "noopener noreferrer");
                }
            };

            return sanitizer;
        }
    }
}
