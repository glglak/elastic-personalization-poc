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
    public class PersonalizationController : ControllerBase
    {
        private readonly IPersonalizationService _personalizationService;
        private readonly ILogger<PersonalizationController> _logger;

        public PersonalizationController(IPersonalizationService personalizationService, ILogger<PersonalizationController> logger)
        {
            _personalizationService = personalizationService;
            _logger = logger;
        }

        [HttpGet("feed/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ContentDto>>> GetPersonalizedFeed(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var feed = await _personalizationService.GetPersonalizedFeedAsync(userId, page, pageSize);
                return Ok(feed);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving personalized feed for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the personalized feed");
            }
        }

        [HttpGet("score/{userId}/{contentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<double>> GetPersonalizationScore(int userId, string contentId)
        {
            try
            {
                var score = await _personalizationService.CalculatePersonalizationScoreAsync(userId, contentId);
                return Ok(new { userId, contentId, score });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating personalization score for user {UserId} and content {ContentId}", userId, contentId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while calculating the personalization score");
            }
        }

        [HttpGet("factors/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PersonalizationFactors>> GetPersonalizationFactors(int userId)
        {
            try
            {
                var factors = await _personalizationService.GetPersonalizationFactorsAsync(userId);
                return Ok(factors);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving personalization factors for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the personalization factors");
            }
        }
    }
}
