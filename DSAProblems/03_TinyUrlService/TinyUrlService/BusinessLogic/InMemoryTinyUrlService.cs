using System.Collections.Concurrent;
using TinyUrlService.Interfaces;

namespace TinyUrlService.BusinessLogic;

/// <summary>
/// In-memory implementation of a URL shortening service using Base62 encoding
/// Thread-safe implementation using ConcurrentDictionary and Interlocked operations
/// </summary>
public class InMemoryTinyUrlService : ITinyUrlService
{
    // Thread-safe storage: shortCode -> longUrl
    private readonly ConcurrentDictionary<string, string> _urlMappings = new();
    
    // Thread-safe storage: longUrl -> shortCode (for idempotency - same URL gets same short code)
    private readonly ConcurrentDictionary<string, string> _reverseMapping = new();
    
    // Counter for generating unique IDs (thread-safe via Interlocked)
    private long _counter = 0;
    
    // Base62 characters: a-z, A-Z, 0-9 (62 characters total)
    private const string Base62Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public string CreateShortUrl(string longUrl)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(longUrl))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(longUrl));
        }

        // Validate URL format
        if (!Uri.TryCreate(longUrl, UriKind.Absolute, out var uri) || 
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Invalid URL format. Must be a valid HTTP or HTTPS URL", nameof(longUrl));
        }

        // Check if this URL already has a short code (idempotency)
        if (_reverseMapping.TryGetValue(longUrl, out var existingShortCode))
        {
            return existingShortCode;
        }

        // Generate new unique ID and convert to Base62
        long id = Interlocked.Increment(ref _counter);
        string shortCode = EncodeToBase62(id);

        // Store both mappings atomically
        _urlMappings[shortCode] = longUrl;
        _reverseMapping[longUrl] = shortCode;

        return shortCode;
    }

    public string? GetLongUrl(string shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
        {
            return null;
        }

        return _urlMappings.TryGetValue(shortCode, out var longUrl) ? longUrl : null;
    }

    /// <summary>
    /// Converts a number to Base62 encoding
    /// Base62 uses 0-9, a-z, A-Z (62 characters)
    /// This creates short, URL-safe strings
    /// </summary>
    /// <param name="num">Number to encode</param>
    /// <returns>Base62 encoded string</returns>
    private static string EncodeToBase62(long num)
    {
        if (num == 0)
        {
            return Base62Chars[0].ToString();
        }

        var result = new Stack<char>();
        while (num > 0)
        {
            result.Push(Base62Chars[(int)(num % 62)]);
            num /= 62;
        }

        return new string(result.ToArray());
    }
}
