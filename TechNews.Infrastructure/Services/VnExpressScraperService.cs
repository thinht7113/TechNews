using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using TechNews.Application.Interfaces;

namespace TechNews.Infrastructure.Services
{
    public class VnExpressScraperService : IArticleScraperService
    {
        private readonly HttpClient _httpClient;

        public VnExpressScraperService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ScrapedArticleResult> ScrapeAsync(string url)
        {
            var result = new ScrapedArticleResult { SourceUrl = url };

            try
            {
                if (string.IsNullOrWhiteSpace(url) || !url.Contains("vnexpress.net"))
                {
                    result.ErrorMessage = "URL không hợp lệ. Chỉ hỗ trợ vnexpress.net.";
                    return result;
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", "vi-VN,vi;q=0.9,en;q=0.8");

                var html = await _httpClient.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // === TITLE ===
                var titleNode = doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'title-detail')]")
                    ?? doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'title_news_detail')]")
                    ?? doc.DocumentNode.SelectSingleNode("//h1");
                result.Title = titleNode != null ? WebUtility.HtmlDecode(titleNode.InnerText.Trim()) : "";

                // === SHORT DESCRIPTION ===
                var descNode = doc.DocumentNode.SelectSingleNode("//p[contains(@class,'description')]");
                if (descNode != null)
                {
                    var locationSpan = descNode.SelectSingleNode(".//span[contains(@class,'location-stamp')]");
                    locationSpan?.Remove();
                    result.ShortDescription = WebUtility.HtmlDecode(descNode.InnerText.Trim());
                }

                // === CONTENT (HTML) ===
                var contentNode = doc.DocumentNode.SelectSingleNode("//article[contains(@class,'fck_detail')]");
                if (contentNode != null)
                {
                    // Remove unwanted elements
                    RemoveNodes(contentNode, ".//div[contains(@class,'box-tinlienquanv2')]");
                    RemoveNodes(contentNode, ".//div[contains(@class,'author_mail')]");
                    RemoveNodes(contentNode, ".//p[contains(@class,'author_mail')]");
                    RemoveNodes(contentNode, ".//div[contains(@class,'box_brief_info')]");
                    RemoveNodes(contentNode, ".//div[contains(@class,'social')]");
                    RemoveNodes(contentNode, ".//div[contains(@class,'banner')]");
                    RemoveNodes(contentNode, ".//div[contains(@class,'box_comment')]");
                    RemoveNodes(contentNode, ".//div[contains(@class,'width_common')]");
                    RemoveNodes(contentNode, ".//script");
                    RemoveNodes(contentNode, ".//style");
                    RemoveNodes(contentNode, ".//noscript");
                    RemoveNodes(contentNode, ".//p[contains(@class,'Normal') and contains(@style,'text-align:right')]");

                    // Process all images: convert data-src to src and fix attributes
                    var imgNodes = contentNode.SelectNodes(".//img");
                    if (imgNodes != null)
                    {
                        foreach (var img in imgNodes.ToList())
                        {
                            var src = img.GetAttributeValue("data-src", "")
                                   .Replace(" ", "");
                            if (string.IsNullOrEmpty(src))
                                src = img.GetAttributeValue("src", "").Replace(" ", "");

                            if (string.IsNullOrEmpty(src) || src.StartsWith("data:"))
                            {
                                img.Remove();
                                continue;
                            }

                            // Clean up all unnecessary attributes
                            var attrsToRemove = img.Attributes
                                .Select(a => a.Name)
                                .Where(n => n != "src" && n != "alt")
                                .ToList();
                            foreach (var attr in attrsToRemove)
                                img.Attributes.Remove(attr);

                            img.SetAttributeValue("src", src);
                            img.SetAttributeValue("style", "max-width:100%;height:auto;display:block;margin:12px auto;");
                        }
                    }

                    // Simplify figure elements: extract just img + caption
                    var figures = contentNode.SelectNodes(".//figure");
                    if (figures != null)
                    {
                        foreach (var figure in figures.ToList())
                        {
                            var img = figure.SelectSingleNode(".//img");
                            var caption = figure.SelectSingleNode(".//figcaption")
                                       ?? figure.SelectSingleNode(".//p[contains(@class,'Image')]");

                            if (img != null)
                            {
                                var captionText = caption != null ? WebUtility.HtmlDecode(caption.InnerText.Trim()) : "";
                                var imgSrc = img.GetAttributeValue("src", "");
                                var imgAlt = img.GetAttributeValue("alt", captionText);

                                var replacement = $"<div><p><img src=\"{imgSrc}\" alt=\"{WebUtility.HtmlEncode(imgAlt)}\" style=\"max-width:100%;height:auto;display:block;margin:12px auto;\" /></p>";
                                if (!string.IsNullOrEmpty(captionText))
                                    replacement += $"<p style=\"text-align:center;font-style:italic;color:#666;font-size:14px;margin-top:4px;\">{WebUtility.HtmlEncode(captionText)}</p>";
                                replacement += "</div>";

                                var newNode = HtmlNode.CreateNode(replacement);
                                figure.ParentNode.ReplaceChild(newNode, figure);
                            }
                            else
                            {
                                figure.Remove();
                            }
                        }
                    }

                    // Remove table wrappers around images (VnExpress uses tables for image layout)
                    var tables = contentNode.SelectNodes(".//table");
                    if (tables != null)
                    {
                        foreach (var table in tables.ToList())
                        {
                            var tableImgs = table.SelectNodes(".//img");
                            if (tableImgs != null && tableImgs.Count > 0)
                            {
                                // Replace table with just the images
                                var container = doc.CreateElement("div");
                                foreach (var tImg in tableImgs)
                                {
                                    var p = doc.CreateElement("p");
                                    p.AppendChild(tImg.Clone());
                                    container.AppendChild(p);
                                }
                                table.ParentNode.ReplaceChild(container, table);
                            }
                        }
                    }

                    // Get the HTML content
                    var contentHtml = contentNode.InnerHtml.Trim();

                    // Clean up HTML
                    contentHtml = Regex.Replace(contentHtml, @"<p[^>]*>\s*</p>", ""); // Remove empty <p>
                    contentHtml = Regex.Replace(contentHtml, @"<p[^>]*>&nbsp;</p>", ""); // Remove &nbsp; <p>
                    contentHtml = Regex.Replace(contentHtml, @"<br\s*/?>(\s*<br\s*/?>)+", "<br>"); // Collapse <br>
                    contentHtml = Regex.Replace(contentHtml, @"\n\s*\n+", "\n"); // Collapse newlines
                    contentHtml = Regex.Replace(contentHtml, @"\s*data-[a-z-]+=""[^""]*""", ""); // Remove data- attributes
                    contentHtml = Regex.Replace(contentHtml, @"\s*class=""[^""]*""", ""); // Remove class attributes
                    contentHtml = Regex.Replace(contentHtml, @"\s*itemprop=""[^""]*""", ""); // Remove itemprop
                    contentHtml = Regex.Replace(contentHtml, @"\s*itemtype=""[^""]*""", ""); // Remove itemtype
                    contentHtml = Regex.Replace(contentHtml, @"\s*itemscope", ""); // Remove itemscope

                    result.Content = contentHtml;
                }

                // === THUMBNAIL ===
                var ogImage = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
                result.ThumbnailUrl = ogImage?.GetAttributeValue("content", "") ?? "";

                // === TAGS ===
                var keywords = doc.DocumentNode.SelectSingleNode("//meta[@name='keywords']");
                if (keywords != null)
                {
                    result.Tags = keywords.GetAttributeValue("content", "");
                }
                else
                {
                    var tagNodes = doc.DocumentNode.SelectNodes("//ul[contains(@class,'breadcrumb')]//a");
                    if (tagNodes != null)
                    {
                        var tags = tagNodes.Select(n => WebUtility.HtmlDecode(n.InnerText.Trim()))
                            .Where(t => !string.IsNullOrEmpty(t) && t != "Trang chủ")
                            .ToList();
                        result.Tags = string.Join(", ", tags);
                    }
                }

                result.Success = true;
            }
            catch (HttpRequestException ex)
            {
                result.ErrorMessage = $"Không thể kết nối đến VnExpress: {ex.Message}";
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Lỗi khi xử lý bài viết: {ex.Message}";
            }

            return result;
        }

        private void RemoveNodes(HtmlNode parent, string xpath)
        {
            var nodes = parent.SelectNodes(xpath);
            if (nodes != null)
            {
                foreach (var node in nodes.ToList())
                {
                    node.Remove();
                }
            }
        }
    }
}
