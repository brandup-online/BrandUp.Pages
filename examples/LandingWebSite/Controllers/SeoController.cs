using BrandUp.Pages.Interfaces;
using BrandUp.Pages.Url;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace LandingWebSite.Controllers
{
    public class SeoController : ControllerBase
    {
        [HttpGet("sitemap.xml")]
        [Produces("application/xml")]
        public async Task<IActionResult> SitemapXmlAsync([FromServices]IPageService pageService, [FromServices]IPageLinkGenerator pageLinkGenerator)
        {
            var request = Request;

            var dateNow = DateTime.Now.ToString("s");
            var model = new SitemapModel { Urls = new List<SitemapUrl>() };

            var pages = await pageService.GetPublishedPagesAsync(HttpContext.RequestAborted);

            foreach (var page in pages)
            {
                var url = await pageLinkGenerator.GetUriAsync(page);

                model.Urls.Add(new SitemapUrl
                {
                    Location = url,
                    LastMod = dateNow,
                    ChangeFreq = "daily",
                    Priority = "1"
                });
            }

            model.Urls = model.Urls.OrderBy(it => it.Location).ToList();
            model.Urls[0].ChangeFreq = "daily";
            model.Urls[0].Priority = "1";

            return new XmlResult(model);
        }

        [XmlRoot("urlset")]
        public class SitemapModel
        {
            [XmlElement("url")]
            public List<SitemapUrl> Urls { get; set; }
        }

        public class SitemapUrl
        {
            [XmlElement("loc")]
            public string Location { get; set; }
            [XmlElement("lastmod")]
            public string LastMod { get; set; }
            [XmlElement("changefreq")]
            public string ChangeFreq { get; set; }
            [XmlElement("priority")]
            public string Priority { get; set; }
        }

        class XmlResult : ActionResult
        {
            readonly SitemapModel model;

            public XmlResult(SitemapModel model)
            {
                this.model = model;
            }

            public override void ExecuteResult(ActionContext context)
            {
                context.HttpContext.Response.ContentType = "text/xml";

                using (var textWriter = new StreamWriter(context.HttpContext.Response.Body, new UTF8Encoding(false)))
                {
                    var writer = new XmlTextWriter(textWriter);
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartDocument();
                    writer.WriteStartElement("urlset");
                    writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");

                    foreach (var p in model.Urls)
                    {
                        writer.WriteStartElement("url");

                        writer.WriteStartElement("loc");
                        writer.WriteRaw(p.Location);
                        writer.WriteEndElement();

                        writer.WriteStartElement("lastmod");
                        writer.WriteRaw(p.LastMod);
                        writer.WriteEndElement();

                        writer.WriteStartElement("changefreq");
                        writer.WriteRaw(p.ChangeFreq);
                        writer.WriteEndElement();

                        writer.WriteStartElement("priority");
                        writer.WriteRaw(p.Priority.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }
            }
        }
    }
}