using System;
using System.Threading.Tasks;
using ElasticPersonalization.Core.Entities;
using ElasticPersonalization.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ElasticPersonalization.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserInteractionController : ControllerBase
    {
        private readonly IUserInteractionService _userInteractionService;
        private readonly ILogger<UserInteractionController> _logger;

        public UserInteractionController(IUserInteractionService userInteractionService, ILogger<UserInteractionController> logger)
        {
            _userInteractionService = userInteractionService;
            _logger = logger;
        }

        // Share Content
        [HttpPost("share")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserShare>> ShareContent([FromQuery] int userId, [FromQuery] int contentId)
        {
            try
            {
                var share = await _userInteractionService.ShareContentAsync(userId, contentId);
                return Ok(share);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sharing content {ContentId} by user {UserId}", contentId, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while sharing the content");
            }
        }

        // Remove Share
        [HttpDelete("share")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveShare([FromQuery] int userId, [FromQuery] int contentId)
        {
            try
            {
                await _userInteractionService.RemoveShareAsync(userId, contentId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing share of content {ContentId} by user {UserId}", contentId, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing the share");
            }
        }

        // Like Content
        [HttpPost("like")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserLike>> LikeContent([FromQuery] int userId, [FromQuery] int contentId)
        {
            try
            {
                var like = await _userInteractionService.LikeContentAsync(userId, contentId);
                return Ok(like);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking content {ContentId} by user {UserId}", contentId, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while liking the content");
            }
        }

        // Remove Like
        [HttpDelete("like")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveLike([FromQuery] int userId, [FromQuery] int contentId)
        {
            try
            {
                await _userInteractionService.RemoveLikeAsync(userId, contentId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing like of content {ContentId} by user {UserId}", contentId, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing the like");
            }
        }

        // Comment on Content
        [HttpPost("comment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserComment>> CommentOnContent([FromQuery] int userId, [FromQuery] int contentId, [FromBody] CommentRequest request)
        {
            try
            {
                var comment = await _userInteractionService.CommentOnContentAsync(userId, contentId, request.CommentText);
                return Ok(comment);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error commenting on content {ContentId} by user {UserId}", contentId, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding the comment");
            }
        }

        // Remove Comment
        [HttpDelete("comment/{commentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveComment(int commentId)
        {
            try
            {
                await _userInteractionService.RemoveCommentAsync(commentId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing comment {CommentId}", commentId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing the comment");
            }
        }

        // Follow User
        [HttpPost("follow")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserFollow>> FollowUser([FromQuery] int userId, [FromQuery] int followedUserId)
        {
            try
            {
                var follow = await _userInteractionService.FollowUserAsync(userId, followedUserId);
                return Ok(follow);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error following user {FollowedUserId} by user {UserId}", followedUserId, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while following the user");
            }
        }

        // Unfollow User
        [HttpDelete("follow")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnfollowUser([FromQuery] int userId, [FromQuery] int followedUserId)
        {
            try
            {
                await _userInteractionService.UnfollowUserAsync(userId, followedUserId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unfollowing user {FollowedUserId} by user {UserId}", followedUserId, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while unfollowing the user");
            }
        }

        // Add User Preference
        [HttpPost("preference")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> AddUserPreference([FromQuery] int userId, [FromBody] PreferenceRequest request)
        {
            try
            {
                var user = await _userInteractionService.AddUserPreferenceAsync(userId, request.Preference);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding preference {Preference} to user {UserId}", request.Preference, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding the preference");
            }
        }

        // Remove User Preference
        [HttpDelete("preference")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> RemoveUserPreference([FromQuery] int userId, [FromQuery] string preference)
        {
            try
            {
                var user = await _userInteractionService.RemoveUserPreferenceAsync(userId, preference);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing preference {Preference} from user {UserId}", preference, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing the preference");
            }
        }

        // Add User Interest
        [HttpPost("interest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> AddUserInterest([FromQuery] int userId, [FromBody] InterestRequest request)
        {
            try
            {
                var user = await _userInteractionService.AddUserInterestAsync(userId, request.Interest);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding interest {Interest} to user {UserId}", request.Interest, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while adding the interest");
            }
        }

        // Remove User Interest
        [HttpDelete("interest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> RemoveUserInterest([FromQuery] int userId, [FromQuery] string interest)
        {
            try
            {
                var user = await _userInteractionService.RemoveUserInterestAsync(userId, interest);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing interest {Interest} from user {UserId}", interest, userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing the interest");
            }
        }
    }

    // Request models for binding
    public class CommentRequest
    {
        public string CommentText { get; set; } = string.Empty;
    }

    public class PreferenceRequest
    {
        public string Preference { get; set; } = string.Empty;
    }

    public class InterestRequest
    {
        public string Interest { get; set; } = string.Empty;
    }
}
