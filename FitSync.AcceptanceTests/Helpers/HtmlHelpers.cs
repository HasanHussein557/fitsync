using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace FitSync.AcceptanceTests.Helpers
{
    public static class HtmlHelpers
    {
        public static async Task<IHtmlDocument> GetDocumentAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(content);
            return document;
        }

        public static string GetAntiForgeryToken(this IHtmlDocument document)
        {
            var element = document.QuerySelector("input[name='__RequestVerificationToken']");
            return element?.GetAttribute("value") ?? string.Empty;
        }
    }
} 