using Microsoft.AspNetCore.Mvc;
using TinyUrlService.Interfaces;

namespace TinyUrlService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TinyUrlController : ControllerBase
{
    private readonly ITinyUrlService _tinyUrlService;
    private readonly ILogger<TinyUrlController> _logger;

    public TinyUrlController(ITinyUrlService tinyUrlService, ILogger<TinyUrlController> logger)
    {
        _tinyUrlService = tinyUrlService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a shortened URL from a long URL
    /// </summary>
    /// <param name="request">Request containing the long URL to shorten</param>
    /// <returns>Response with the short code and full short URL</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateShortUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateShortUrl([FromBody] CreateShortUrlRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.LongUrl))
            {
                return BadRequest(new { error = "LongUrl is required" });
            }

            string shortCode = _tinyUrlService.CreateShortUrl(request.LongUrl);
            
            // Build the full short URL using the current host
            string shortUrl = $"{Request.Scheme}://{Request.Host}/r/{shortCode}";

            _logger.LogInformation("Created short URL: {ShortCode} for {LongUrl}", shortCode, request.LongUrl);

            return Ok(new CreateShortUrlResponse
            {
                ShortCode = shortCode,
                ShortUrl = shortUrl,
                LongUrl = request.LongUrl
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid URL provided: {LongUrl}", request.LongUrl);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating short URL for: {LongUrl}", request.LongUrl);
            return StatusCode(500, new { error = "An error occurred while creating the short URL" });
        }
    }

    /// <summary>
    /// Retrieves the original long URL and redirects to it
    /// This endpoint is accessed at: /r/{shortCode}
    /// </summary>
    /// <param name="shortCode">The short code to look up</param>
    /// <returns>Redirect to the original URL or NotFound</returns>
    [HttpGet("~/r/{shortCode}")]
    [ProducesResponseType(StatusCodes.Status301MovedPermanently)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult RedirectToLongUrl(string shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
        {
            return NotFound(new { error = "Short code is required" });
        }

        string? longUrl = _tinyUrlService.GetLongUrl(shortCode);

        if (longUrl == null)
        {
            _logger.LogWarning("Short code not found: {ShortCode}", shortCode);
            return NotFound(new { error = "Short URL not found" });
        }

        _logger.LogInformation("Redirecting {ShortCode} to {LongUrl}", shortCode, longUrl);
        
        // 301 Moved Permanently - indicates the redirect is permanent
        // This is the standard for URL shorteners as it can be cached by browsers
        // Trade-off: Better performance but can't track repeat visits
        return RedirectPermanent(longUrl);
    }

    /// <summary>
    /// Optional: Get URL information without redirecting (for debugging/testing)
    /// </summary>
    /// <param name="shortCode">The short code to look up</param>
    /// <returns>URL information</returns>
    [HttpGet("info/{shortCode}")]
    [ProducesResponseType(typeof(UrlInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetUrlInfo(string shortCode)
    {
        string? longUrl = _tinyUrlService.GetLongUrl(shortCode);

        if (longUrl == null)
        {
            return NotFound(new { error = "Short URL not found" });
        }

        return Ok(new UrlInfoResponse
        {
            ShortCode = shortCode,
            LongUrl = longUrl,
            ShortUrl = $"{Request.Scheme}://{Request.Host}/r/{shortCode}"
        });
    }
}

// Request/Response DTOs
public record CreateShortUrlRequest
{
    public string LongUrl { get; init; } = string.Empty;
}

public record CreateShortUrlResponse
{
    public string ShortCode { get; init; } = string.Empty;
    public string ShortUrl { get; init; } = string.Empty;
    public string LongUrl { get; init; } = string.Empty;
}

public record UrlInfoResponse
{
    public string ShortCode { get; init; } = string.Empty;
    public string ShortUrl { get; init; } = string.Empty;
    public string LongUrl { get; init; } = string.Empty;
}
