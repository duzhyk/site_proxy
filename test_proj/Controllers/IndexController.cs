using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;

namespace test_proj.Controllers
{
    public class IndexController : Controller
    {
        private const string rootUrl = @"https://www.bbc.com"; //tested and developed for BBC web site

        private readonly ILogger<IndexController> _logger;

        public IndexController(ILogger<IndexController> logger)
        {
            _logger = logger;
        }

        [Route("{**catchall}")]
        public ContentResult Index()
        {
            var route = Request.Path.Value;
            var web = new HtmlWeb(); // use HtmlAgilityPack to work with html
            var htmlDoc = web.Load($"{rootUrl}{route}");   
            var body = htmlDoc.DocumentNode.SelectSingleNode("//body");

            ModifyLinks(body); // need to find links with absolute routes and modify them
            ModifyNodeInnerText(body); // main logic

            // for modifying sites that load content on front-end side need to inject some JS code
            // example below:
            //
            // var jsScript = @"<script>// some js script that catch loaded content and modify it</script>";
            // var scriptNode = HtmlNode.CreateNode(jsScript);
            // node.InsertBefore(scriptNode, node.ChildNodes[0]);

            return new ContentResult
            {
                Content = htmlDoc.DocumentNode.WriteContentTo(),
                ContentType = "text/html"
            };
        }

        private void ModifyNodeInnerText(HtmlNode node)
        {
            if(node is null) return;

            if(node.ChildNodes.Any())
            {
                foreach(var n in node.ChildNodes)
                {
                    ModifyNodeInnerText(n); // recursive call for finding and modifying nodes without childs
                }

                return;
            }

            if(node.ParentNode.Name == "script" || node.ParentNode.Name == "style" || string.IsNullOrWhiteSpace(node.InnerText)) return; //we need only text fields

            node.InnerHtml = Regex.Replace(node.InnerHtml, @"\b\w{6}\b", @"$0&trade;");
        }

        private void ModifyLinks(HtmlNode body)
        {
            if(body is null) return;

            var links = body.SelectNodes("//a/@href");

            if(links is null) return;

            foreach(var link in links)
            {
                var attributeValue = link.Attributes["href"].Value;
                link.Attributes["href"].Value = attributeValue.Replace(rootUrl, string.Empty);
            }
        }
    }
}