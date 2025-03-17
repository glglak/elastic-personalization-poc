using System.Collections.Generic;
using System.Threading.Tasks;
using ElasticPersonalization.Core.Models;

namespace ElasticPersonalization.Core.Interfaces
{
    public interface IPersonalizationService
    {
        // Get personalized content feed for a user
        Task<List<ContentDto>> GetPersonalizedFeedAsync(int userId, int page = 1, int pageSize = 20);
        
        // Calculate personalization score for a content item for a specific user
        Task<double> CalculatePersonalizationScoreAsync(int userId, string contentId);
        
        // Get personalization factors for a user (for transparency)
        Task<PersonalizationFactors> GetPersonalizationFactorsAsync(int userId);
    }
}
