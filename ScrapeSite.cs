using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;

namespace JasonGaylord.Functions
{
    public sealed class ProductAvailability
    {
        public bool IsAvailable { get; set; }
        public string Url { get; set; }
    }

    public static class ScrapeSite
    {
        [FunctionName("ScrapeSite")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var products = new List<ProductAvailability>();
            products.Add(await Scraper.GetProductAvailability("https://www.walmart.com/ip/Xbox-Series-X/443574645"));
            products.Add(await Scraper.GetProductAvailability("https://www.walmart.com/ip/Microsoft-Xbox-One-S-1TB-All-Digital-Edition-Console-Disc-free-Gaming-White-NJP-00024/560014078"));

            return new OkObjectResult(products);
        }
    }

    public static class Scraper
    {
        public static async Task<ProductAvailability> GetProductAvailability(string uri)
        {
            var productAvailability = new ProductAvailability(){ Url = uri };

            var web = new HtmlWeb();
            var html = await web.LoadFromWebAsync(productAvailability.Url);
            var htmlString = html.DocumentNode;
            var buttons = htmlString.SelectNodes("//button").ToList();

            var outOfStock = buttons.Count(c => c.InnerHtml.Contains("Get in-stock alert"));
            productAvailability.IsAvailable = outOfStock == 0;

            return productAvailability;
        }
    }
}