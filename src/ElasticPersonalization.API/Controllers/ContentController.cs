using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ElasticPersonalization.Core.Interfaces;
using ElasticPersonalization.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ElasticPersonalization.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContentController : ControllerBase
    {
        private readonly IContentService _contentService;
        private readonly ILogger<ContentController> _logger;

        public ContentController(IContentService contentService, ILogger<ContentController> logger)
        {
            _contentService = contentService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContentDto>> GetContent(string id)
        {
            try
            {
                var content = await _contentService.GetContentAsync(id);
                return Ok(content);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving content with ID {ContentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the content");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ContentDto>> CreateContent(CreateContentDto contentDto)
        {
            try
            {
                var createdContent = await _contentService.CreateContentAsync(contentDto);
                return CreatedAtAction(nameof(GetContent), new { id = createdContent.Id }, createdContent);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating content");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the content");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ContentDto>> UpdateContent(string id, UpdateContentDto contentDto)
        {
            try
            {
                var updatedContent = await _contentService.UpdateContentAsync(id, contentDto);
                return Ok(updatedContent);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating content with ID {ContentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the content");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteContent(string id)
        {
            try
            {
                await _contentService.DeleteContentAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting content with ID {ContentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the content");
            }
        }

        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ContentDto>>> SearchContent([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var content = await _contentService.SearchContentAsync(query, page, pageSize);
                return Ok(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching content with query {Query}", query);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching for content");
            }
        }

        [HttpGet("category/{category}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ContentDto>>> GetContentByCategory(string category, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var content = await _contentService.GetContentByCategoryAsync(category, page, pageSize);
                return Ok(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving content by category {Category}", category);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving content by category");
            }
        }

        [HttpGet("tag/{tag}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ContentDto>>> GetContentByTag(string tag, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var content = await _contentService.GetContentByTagAsync(tag, page, pageSize);
                return Ok(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving content by tag {Tag}", tag);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving content by tag");
            }
        }

        [HttpGet("creator/{creatorId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ContentDto>>> GetContentByCreator(int creatorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var content = await _contentService.GetContentByCreatorAsync(creatorId, page, pageSize);
                return Ok(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving content by creator {CreatorId}", creatorId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving content by creator");
            }
        }
    }
}
