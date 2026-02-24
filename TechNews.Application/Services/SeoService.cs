using System.Text.RegularExpressions;
using TechNews.Application.DTOs;
using TechNews.Application.Interfaces;

namespace TechNews.Application.Services
{
    public class SeoService : ISeoService
    {
        public SeoAnalysisResult Analyze(SeoAnalysisRequest request)
        {
            var result = new SeoAnalysisResult();
            var suggestions = new List<string>();

            // 1. Title Analysis
            result.TitleAnalysis = AnalyzeTitle(request.Title, request.MetaTitle, request.FocusKeyword, suggestions);

            // 2. Meta Description Analysis
            result.MetaDescriptionAnalysis = AnalyzeMetaDescription(request.MetaDescription, request.FocusKeyword, suggestions);

            // 3. Keyword Analysis
            result.KeywordAnalysis = AnalyzeKeyword(request.Content, request.FocusKeyword, suggestions);

            // 4. Readability Analysis
            result.ReadabilityAnalysis = AnalyzeReadability(request.Content, suggestions);

            // 5. Heading Analysis
            result.HeadingAnalysis = AnalyzeHeadings(request.Content, request.FocusKeyword, suggestions);

            // 6. Image Analysis
            result.ImageAnalysis = AnalyzeImages(request.Content, suggestions);

            // Calculate overall score
            var checks = new[] { result.TitleAnalysis, result.MetaDescriptionAnalysis, result.KeywordAnalysis, result.ReadabilityAnalysis, result.HeadingAnalysis, result.ImageAnalysis };
            result.OverallScore = (int)checks.Average(c => c.Score);
            result.Suggestions = suggestions;

            return result;
        }

        private SeoCheck AnalyzeTitle(string title, string? metaTitle, string? focusKeyword, List<string> suggestions)
        {
            var effectiveTitle = !string.IsNullOrEmpty(metaTitle) ? metaTitle : title;
            var check = new SeoCheck();

            if (string.IsNullOrEmpty(effectiveTitle))
            {
                check.Status = "bad";
                check.Score = 0;
                check.Message = "Chưa có tiêu đề.";
                suggestions.Add("Thêm tiêu đề cho bài viết.");
                return check;
            }

            int len = effectiveTitle.Length;
            if (len >= 50 && len <= 60)
            {
                check.Score = 100;
                check.Status = "good";
                check.Message = $"Tiêu đề có {len} ký tự — độ dài lý tưởng (50-60).";
            }
            else if (len >= 30 && len < 50)
            {
                check.Score = 70;
                check.Status = "warning";
                check.Message = $"Tiêu đề có {len} ký tự — hơi ngắn, nên từ 50-60 ký tự.";
                suggestions.Add("Tăng độ dài tiêu đề lên 50-60 ký tự để tối ưu SEO.");
            }
            else if (len > 60 && len <= 70)
            {
                check.Score = 60;
                check.Status = "warning";
                check.Message = $"Tiêu đề có {len} ký tự — hơi dài, có thể bị cắt trên Google.";
                suggestions.Add("Rút gọn tiêu đề xuống dưới 60 ký tự.");
            }
            else
            {
                check.Score = 30;
                check.Status = "bad";
                check.Message = $"Tiêu đề có {len} ký tự — {(len < 30 ? "quá ngắn" : "quá dài")}.";
                suggestions.Add(len < 30 ? "Tiêu đề quá ngắn, nên từ 50-60 ký tự." : "Tiêu đề quá dài, rút gọn xuống 60 ký tự.");
            }

            // Check for focus keyword in title
            if (!string.IsNullOrEmpty(focusKeyword) && !effectiveTitle.Contains(focusKeyword, StringComparison.OrdinalIgnoreCase))
            {
                check.Score = Math.Max(check.Score - 20, 0);
                check.Status = check.Score >= 70 ? "warning" : "bad";
                suggestions.Add($"Thêm từ khóa \"{focusKeyword}\" vào tiêu đề.");
            }

            return check;
        }

        private SeoCheck AnalyzeMetaDescription(string? metaDescription, string? focusKeyword, List<string> suggestions)
        {
            var check = new SeoCheck();

            if (string.IsNullOrEmpty(metaDescription))
            {
                check.Status = "bad";
                check.Score = 0;
                check.Message = "Chưa có meta description.";
                suggestions.Add("Thêm meta description (120-160 ký tự) cho bài viết.");
                return check;
            }

            int len = metaDescription.Length;
            if (len >= 120 && len <= 160)
            {
                check.Score = 100;
                check.Status = "good";
                check.Message = $"Meta description có {len} ký tự — độ dài lý tưởng (120-160).";
            }
            else if (len >= 80 && len < 120)
            {
                check.Score = 60;
                check.Status = "warning";
                check.Message = $"Meta description có {len} ký tự — hơi ngắn, nên từ 120-160 ký tự.";
                suggestions.Add("Tăng độ dài meta description lên 120-160 ký tự.");
            }
            else if (len > 160)
            {
                check.Score = 50;
                check.Status = "warning";
                check.Message = $"Meta description có {len} ký tự — sẽ bị cắt trên Google (tối đa 160).";
                suggestions.Add("Rút gọn meta description xuống dưới 160 ký tự.");
            }
            else
            {
                check.Score = 30;
                check.Status = "bad";
                check.Message = $"Meta description quá ngắn ({len} ký tự).";
                suggestions.Add("Meta description quá ngắn, nên từ 120-160 ký tự.");
            }

            if (!string.IsNullOrEmpty(focusKeyword) && !metaDescription.Contains(focusKeyword, StringComparison.OrdinalIgnoreCase))
            {
                check.Score = Math.Max(check.Score - 15, 0);
                suggestions.Add($"Thêm từ khóa \"{focusKeyword}\" vào meta description.");
            }

            return check;
        }

        private SeoCheck AnalyzeKeyword(string content, string? focusKeyword, List<string> suggestions)
        {
            var check = new SeoCheck();

            if (string.IsNullOrEmpty(focusKeyword))
            {
                check.Status = "warning";
                check.Score = 50;
                check.Message = "Chưa đặt từ khóa chính (focus keyword).";
                suggestions.Add("Đặt từ khóa chính để phân tích mật độ từ khóa.");
                return check;
            }

            var plainText = StripHtml(content);
            if (string.IsNullOrEmpty(plainText))
            {
                check.Status = "bad";
                check.Score = 0;
                check.Message = "Nội dung trống.";
                return check;
            }

            var wordCount = CountWords(plainText);
            var keywordCount = Regex.Matches(plainText, Regex.Escape(focusKeyword), RegexOptions.IgnoreCase).Count;
            var density = wordCount > 0 ? (double)keywordCount / wordCount * 100 : 0;

            if (density >= 1.0 && density <= 3.0)
            {
                check.Score = 100;
                check.Status = "good";
                check.Message = $"Mật độ từ khóa: {density:F1}% ({keywordCount} lần / {wordCount} từ) — tối ưu.";
            }
            else if (density > 0 && density < 1.0)
            {
                check.Score = 60;
                check.Status = "warning";
                check.Message = $"Mật độ từ khóa: {density:F1}% — hơi thấp, nên 1-3%.";
                suggestions.Add($"Tăng tần suất từ khóa \"{focusKeyword}\" (hiện {keywordCount} lần trong {wordCount} từ).");
            }
            else if (density > 3.0)
            {
                check.Score = 40;
                check.Status = "warning";
                check.Message = $"Mật độ từ khóa: {density:F1}% — quá cao, có thể bị Google phạt.";
                suggestions.Add($"Giảm tần suất từ khóa \"{focusKeyword}\" xuống 1-3%.");
            }
            else
            {
                check.Score = 10;
                check.Status = "bad";
                check.Message = $"Từ khóa \"{focusKeyword}\" chưa xuất hiện trong nội dung.";
                suggestions.Add($"Thêm từ khóa \"{focusKeyword}\" vào nội dung bài viết.");
            }

            return check;
        }

        private SeoCheck AnalyzeReadability(string content, List<string> suggestions)
        {
            var check = new SeoCheck();
            var plainText = StripHtml(content);

            if (string.IsNullOrEmpty(plainText))
            {
                check.Status = "bad";
                check.Score = 0;
                check.Message = "Nội dung trống.";
                return check;
            }

            var wordCount = CountWords(plainText);
            var sentences = Regex.Split(plainText, @"[.!?]+").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            var sentenceCount = sentences.Length;
            var avgWordsPerSentence = sentenceCount > 0 ? (double)wordCount / sentenceCount : 0;

            // Simple readability: average words per sentence
            if (avgWordsPerSentence <= 20)
            {
                check.Score = 100;
                check.Status = "good";
                check.Message = $"Dễ đọc — trung bình {avgWordsPerSentence:F0} từ/câu ({wordCount} từ, {sentenceCount} câu).";
            }
            else if (avgWordsPerSentence <= 25)
            {
                check.Score = 70;
                check.Status = "warning";
                check.Message = $"Khá dài — trung bình {avgWordsPerSentence:F0} từ/câu. Nên dưới 20 từ/câu.";
                suggestions.Add("Chia các câu dài thành câu ngắn hơn (dưới 20 từ/câu).");
            }
            else
            {
                check.Score = 40;
                check.Status = "bad";
                check.Message = $"Khó đọc — trung bình {avgWordsPerSentence:F0} từ/câu. Cần chia nhỏ câu.";
                suggestions.Add("Câu quá dài, chia nhỏ thành các đoạn ngắn hơn.");
            }

            // Content length check
            if (wordCount < 300)
            {
                check.Score = Math.Max(check.Score - 30, 0);
                suggestions.Add($"Nội dung quá ngắn ({wordCount} từ). Nên tối thiểu 300 từ, lý tưởng 1000+.");
            }

            return check;
        }

        private SeoCheck AnalyzeHeadings(string content, string? focusKeyword, List<string> suggestions)
        {
            var check = new SeoCheck();

            var h2Matches = Regex.Matches(content, @"<h2[^>]*>(.*?)</h2>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var h3Matches = Regex.Matches(content, @"<h3[^>]*>(.*?)</h3>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            int h2Count = h2Matches.Count;
            int h3Count = h3Matches.Count;

            if (h2Count >= 2)
            {
                check.Score = 100;
                check.Status = "good";
                check.Message = $"Cấu trúc heading tốt: {h2Count} H2, {h3Count} H3.";
            }
            else if (h2Count == 1)
            {
                check.Score = 70;
                check.Status = "warning";
                check.Message = $"Chỉ có {h2Count} H2. Nên có ít nhất 2 heading H2.";
                suggestions.Add("Thêm heading H2 để cấu trúc bài viết rõ ràng hơn.");
            }
            else
            {
                check.Score = 30;
                check.Status = "bad";
                check.Message = "Không có heading H2 nào. Cần thêm heading cho SEO.";
                suggestions.Add("Thêm heading H2 để Google hiểu cấu trúc nội dung.");
            }

            // Focus keyword in headings
            if (!string.IsNullOrEmpty(focusKeyword) && h2Count > 0)
            {
                bool keywordInH2 = h2Matches.Cast<Match>().Any(m =>
                    m.Groups[1].Value.Contains(focusKeyword, StringComparison.OrdinalIgnoreCase));
                if (!keywordInH2)
                {
                    check.Score = Math.Max(check.Score - 15, 0);
                    suggestions.Add($"Thêm từ khóa \"{focusKeyword}\" vào ít nhất 1 heading H2.");
                }
            }

            return check;
        }

        private SeoCheck AnalyzeImages(string content, List<string> suggestions)
        {
            var check = new SeoCheck();

            var imgMatches = Regex.Matches(content, @"<img[^>]*>", RegexOptions.IgnoreCase);
            int imgCount = imgMatches.Count;

            if (imgCount == 0)
            {
                check.Score = 40;
                check.Status = "warning";
                check.Message = "Không có hình ảnh. Nên thêm ảnh minh họa.";
                suggestions.Add("Thêm ít nhất 1 hình ảnh vào bài viết.");
                return check;
            }

            // Check alt text
            int withAlt = imgMatches.Cast<Match>().Count(m =>
                Regex.IsMatch(m.Value, @"alt\s*=\s*""[^""]+""", RegexOptions.IgnoreCase));
            int missingAlt = imgCount - withAlt;

            if (missingAlt == 0)
            {
                check.Score = 100;
                check.Status = "good";
                check.Message = $"{imgCount} hình ảnh, tất cả có alt text — tốt cho SEO.";
            }
            else
            {
                check.Score = 60;
                check.Status = "warning";
                check.Message = $"{imgCount} hình ảnh, {missingAlt} thiếu alt text.";
                suggestions.Add($"Thêm alt text cho {missingAlt} hình ảnh thiếu.");
            }

            return check;
        }

        // Utility methods
        private static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            return Regex.Replace(html, "<[^>]+>", " ").Trim();
        }

        private static int CountWords(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            return Regex.Split(text.Trim(), @"\s+").Where(w => !string.IsNullOrWhiteSpace(w)).Count();
        }
    }
}
