using TechNews.Application.DTOs;

namespace TechNews.Application.Interfaces
{
    public interface ISeoService
    {
        SeoAnalysisResult Analyze(SeoAnalysisRequest request);
    }
}
