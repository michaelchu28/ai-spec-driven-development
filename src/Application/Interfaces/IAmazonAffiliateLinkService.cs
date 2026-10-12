namespace Application.Interfaces;

/// <summary>
/// Represents the normalized output of an Amazon affiliate link normalization request.
/// </summary>
public class AffiliateLinkResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the input URL is eligible for affiliate tagging.
    /// </summary>
    public bool IsAffiliateEligible { get; set; }

    /// <summary>
    /// Gets or sets the normalized canonical affiliate URL, or the original URL if the input is not eligible.
    /// </summary>
    public string? NormalizedUrl { get; set; }

    /// <summary>
    /// Gets or sets the extracted ASIN when one is present on a product page.
    /// </summary>
    public string? Asin { get; set; }

    /// <summary>
    /// Gets or sets the reason the URL was accepted or rejected.
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Normalizes Amazon URLs into the correct affiliate-tagged canonical form.
/// </summary>
public interface IAmazonAffiliateLinkService
{
    /// <summary>
    /// Normalizes an Amazon URL to the canonical affiliate-tagged form when eligible.
    /// </summary>
    /// <param name="url">The Amazon URL to normalize.</param>
    /// <returns>A normalized result containing the canonical URL, ASIN, and eligibility metadata.</returns>
    AffiliateLinkResult Normalize(string url);
}
