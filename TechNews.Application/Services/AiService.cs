using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TechNews.Application.Interfaces;
using TechNews.Domain.Entities;
using TechNews.Domain.Interfaces;

namespace TechNews.Application.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IRepository<SystemSetting> _settingRepo;
        private string? _cachedApiKey;
        private string? _cachedProvider;
        private string? _cachedModel;
        private DateTime _cacheExpiry = DateTime.MinValue;

        public AiService(HttpClient httpClient, IRepository<SystemSetting> settingRepo)
        {
            _httpClient = httpClient;
            _settingRepo = settingRepo;
        }

        public bool IsConfigured
        {
            get
            {
                RefreshSettingsIfNeeded().GetAwaiter().GetResult();
                return !string.IsNullOrEmpty(_cachedApiKey);
            }
        }

        public async Task<string> GenerateContentAsync(string prompt, string? context = null)
        {
            var systemPrompt = "Bạn là trợ lý viết bài chuyên nghiệp. Viết nội dung bằng tiếng Việt, chất lượng cao, phù hợp SEO.";
            if (!string.IsNullOrEmpty(context))
                systemPrompt += $"\n\nNgữ cảnh bài viết:\n{context}";

            return await CallAiAsync(systemPrompt, prompt);
        }

        public async Task<string> SummarizeAsync(string content, int maxLength = 200)
        {
            var prompt = $"Tóm tắt nội dung sau thành đoạn ngắn gọn (tối đa {maxLength} ký tự), giữ nguyên ý chính:\n\n{content}";
            return await CallAiAsync("Bạn là trợ lý tóm tắt nội dung chuyên nghiệp.", prompt);
        }

        public async Task<List<string>> SuggestTagsAsync(string content)
        {
            var prompt = $"Phân tích nội dung sau và đề xuất 5-8 tags phù hợp. Trả về dạng JSON array, ví dụ: [\"tag1\", \"tag2\"]. Chỉ trả về JSON, không thêm text khác.\n\n{content}";
            var response = await CallAiAsync("Bạn là trợ lý phân tích nội dung.", prompt);

            try
            {
                // Try to parse JSON array from response
                var cleaned = response.Trim();
                if (cleaned.StartsWith("["))
                {
                    return JsonSerializer.Deserialize<List<string>>(cleaned) ?? new List<string>();
                }
                // If not JSON, split by comma
                return cleaned.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().Trim('"', '\''))
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();
            }
            catch
            {
                return new List<string> { response };
            }
        }

        public async Task<string> ImproveWritingAsync(string content)
        {
            var prompt = $"Cải thiện văn phong của đoạn văn sau, giữ nguyên ý nghĩa nhưng làm cho câu mạch lạc, chuyên nghiệp hơn:\n\n{content}";
            return await CallAiAsync("Bạn là biên tập viên chuyên nghiệp.", prompt);
        }

        public async Task<List<string>> GenerateTitlesAsync(string content)
        {
            var prompt = $"Đề xuất 5 tiêu đề hấp dẫn, SEO-friendly cho nội dung sau. Trả về dạng JSON array. Chỉ trả về JSON.\n\n{content}";
            var response = await CallAiAsync("Bạn là chuyên gia content marketing.", prompt);

            try
            {
                var cleaned = response.Trim();
                if (cleaned.StartsWith("["))
                {
                    return JsonSerializer.Deserialize<List<string>>(cleaned) ?? new List<string>();
                }
                return cleaned.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().TrimStart('-', '1', '2', '3', '4', '5', '.', ' '))
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();
            }
            catch
            {
                return new List<string> { response };
            }
        }

        private async Task<string> CallAiAsync(string systemPrompt, string userPrompt)
        {
            await RefreshSettingsIfNeeded();

            if (string.IsNullOrEmpty(_cachedApiKey))
                throw new InvalidOperationException("AI chưa được cấu hình. Vui lòng thêm API Key trong phần Cấu hình hệ thống.");

            var provider = _cachedProvider?.ToLower() ?? "openai";
            var model = _cachedModel ?? (provider == "gemini" ? "gemini-2.0-flash" : "gpt-4o-mini");

            if (provider == "gemini")
                return await CallGeminiAsync(systemPrompt, userPrompt, model);
            else
                return await CallOpenAiAsync(systemPrompt, userPrompt, model);
        }

        private async Task<string> CallOpenAiAsync(string systemPrompt, string userPrompt, string model)
        {
            var request = new
            {
                model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                max_tokens = 2000,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(request);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cachedApiKey);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenAI API error: {response.StatusCode} - {responseBody}");

            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }

        private async Task<string> CallGeminiAsync(string systemPrompt, string userPrompt, string model)
        {
            var request = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"{systemPrompt}\n\n{userPrompt}" }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(request);
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_cachedApiKey}";
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Gemini API error: {response.StatusCode} - {responseBody}");

            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;
        }

        private async Task RefreshSettingsIfNeeded()
        {
            if (DateTime.Now < _cacheExpiry) return;

            var settings = await _settingRepo.GetAllAsync();
            var settingList = settings.ToList();

            _cachedApiKey = settingList.FirstOrDefault(s => s.Key == "AiApiKey")?.Value;
            _cachedProvider = settingList.FirstOrDefault(s => s.Key == "AiProvider")?.Value ?? "openai";
            _cachedModel = settingList.FirstOrDefault(s => s.Key == "AiModel")?.Value;
            _cacheExpiry = DateTime.Now.AddMinutes(5); // Cache for 5 minutes
        }
    }
}
