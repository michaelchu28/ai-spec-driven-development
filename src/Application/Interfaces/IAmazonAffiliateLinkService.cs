namespace Application.Interfaces;

/// <summary>
/// Represents the result of normalizing an Amazon affiliate link.
/// </summary>
public class AffiliateLinkResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the URL is eligible for affiliate tagging.
    /// </summary>
    public bool IsAffiliateEligible { get; set; }

    /// <summary>
    /// Gets or sets the normalized affiliate URL, or the original URL when it is not eligible.
    /// </summary>
    public string? NormalizedUrl { get; set; }

    /// <summary>
    /// Gets or sets the extracted product ASIN, when available.
    /// </summary>
    public string? Asin { get; set; }

    /// <summary>
    /// Gets or sets an explanation of the normalization result.
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Normalizes supported Amazon URLs with the appropriate affiliate tag.
/// </summary>
public interface IAmazonAffiliateLinkService
{
    /// <summary>
    /// Normalizes an Amazon URL when it represents an affiliate-eligible page.
    /// </summary>
    /// <param name="url">The URL to normalize.</param>
    /// <returns>The normalized URL and eligibility metadata.</returns>
    AffiliateLinkResult Normalize(string url);
}
