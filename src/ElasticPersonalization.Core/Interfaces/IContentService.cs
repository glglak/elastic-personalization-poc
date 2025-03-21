using System.Collections.Generic;
using System.Threading.Tasks;
using ElasticPersonalization.Core.Entities;
using ElasticPersonalization.Core.Models;

namespace ElasticPersonalization.Core.Interfaces
{
    public interface IContentService
    {
        // CRUD operations
        Task<ContentDto> GetContentAsync(int contentId);
        Task<ContentDto> CreateContentAsync(CreateContentDto contentDto);
        Task<ContentDto> UpdateContentAsync(int contentId, UpdateContentDto contentDto);
        Task DeleteContentAsync(int contentId);
        
        // Search operations
        Task<List<ContentDto>> SearchContentAsync(string query, int page = 1, int pageSize = 20);
        Task<List<ContentDto>> GetContentByCategoryAsync(string category, int page = 1, int pageSize = 20);
        Task<List<ContentDto>> GetContentByTagAsync(string tag, int page = 1, int pageSize = 20);
        Task<List<ContentDto>> GetContentByCreatorAsync(int creatorId, int page = 1, int pageSize = 20);
        
        // Synchronization with Elasticsearch
        Task SyncContentToElasticsearchAsync(Content content);
        Task RemoveContentFromElasticsearchAsync(int contentId);
    }
}