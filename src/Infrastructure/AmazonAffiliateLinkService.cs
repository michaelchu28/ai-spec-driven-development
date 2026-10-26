using System.Net;
using System.Text.RegularExpressions;
using Application.Interfaces;

namespace Infrastructure;

/// <summary>
/// Normalizes Amazon product, search, and category URLs for affiliate linking.
/// </summary>
public sealed class AmazonAffiliateLinkService : IAmazonAffiliateLinkService
{
    private const string ComAffiliateTag = "company00-20";
    private const string CaAffiliateTag = "company-20";

    private static readonly Regex ProductPathPattern = new(
        @"(?:^|/)(?:dp|gp/product)/(?<asin>[A-Za-z0-9]{10})(?=/|$)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <inheritdoc />
    public AffiliateLinkResult Normalize(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        var decodedUrl = WebUtility.HtmlDecode(url);
        if (string.IsNullOrWhiteSpace(decodedUrl))
        {
            return Ineligible(url, "The URL is empty.");
        }

        var candidate = decodedUrl.Trim();
        if (candidate.StartsWith("//", StringComparison.Ordinal))
        {
            candidate = $"https:{candidate}";
        }
        else if (!candidate.Contains("://", StringComparison.Ordinal))
        {
            candidate = $"https://{candidate}";
        }

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            return Ineligible(url, "The URL is invalid.");
        }

        var host = uri.IdnHost;
        if (IsImageHost(host))
        {
            return Ineligible(url, "Image and CDN URLs are not affiliate-eligible.");
        }

        if (uri.Scheme is not ("http" or "https"))
        {
            return Ineligible(url, "Only HTTP and HTTPS URLs are supported.");
        }

        if (!TryGetAmazonMarketplace(host, out var marketplace, out var affiliateTag))
        {
            return Ineligible(url, "The URL is not for a supported Amazon marketplace.");
        }

        if (uri.AbsolutePath is "" or "/")
        {
            return Ineligible(url, "Amazon homepages are not affiliate-eligible.");
        }

        var asinMatch = ProductPathPattern.Match(uri.AbsolutePath);
        if (asinMatch.Success)
        {
            var asin = asinMatch.Groups["asin"].Value.ToUpperInvariant();
            return Eligible(
                $"https://www.{marketplace}/dp/{asin}?tag={affiliateTag}",
                asin,
                "Product URL normalized.");
        }

        var queryAsin = GetFirstNonEmptyQueryValue(uri, "ASIN");
        if (IsValidAsin(queryAsin))
        {
            var asin = queryAsin!.ToUpperInvariant();
            return Eligible(
                $"https://www.{marketplace}/dp/{asin}?tag={affiliateTag}",
                asin,
                "Product URL normalized from its ASIN query parameter.");
        }

        if (string.Equals(uri.AbsolutePath, "/s", StringComparison.OrdinalIgnoreCase))
        {
            var searchQuery = GetFirstNonEmptyQueryValue(uri, "k");
            if (searchQuery is not null)
            {
                return Eligible(
                    $"https://www.{marketplace}/s?k={Uri.EscapeDataString(searchQuery)}&tag={affiliateTag}",
                    null,
                    "Search URL normalized.");
            }
        }

        if (string.Equals(uri.AbsolutePath, "/b", StringComparison.OrdinalIgnoreCase))
        {
            var node = GetFirstNonEmptyQueryValue(uri, "node");
            if (node is not null)
            {
                return Eligible(
                    $"https://www.{marketplace}/b?node={Uri.EscapeDataString(node)}&tag={affiliateTag}",
                    null,
                    "Category URL normalized.");
            }
        }

        return Ineligible(url, "The URL does not contain an eligible product, search, or category page.");
    }

    private static AffiliateLinkResult Eligible(string normalizedUrl, string? asin, string reason)
    {
        return new AffiliateLinkResult
        {
            IsAffiliateEligible = true,
            NormalizedUrl = normalizedUrl,
            Asin = asin,
            Reason = reason
        };
    }

    private static AffiliateLinkResult Ineligible(string originalUrl, string reason)
    {
        return new AffiliateLinkResult
        {
            IsAffiliateEligible = false,
            NormalizedUrl = originalUrl,
            Reason = reason
        };
    }

    private static string? GetFirstNonEmptyQueryValue(Uri uri, string key)
    {
        var query = uri.Query;
        if (query.Length <= 1)
        {
            return null;
        }

        foreach (var pair in query[1..].Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separatorIndex = pair.IndexOf('=');
            var encodedKey = separatorIndex >= 0 ? pair[..separatorIndex] : pair;
            if (!string.Equals(WebUtility.UrlDecode(encodedKey), key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var encodedValue = separatorIndex >= 0 ? pair[(separatorIndex + 1)..] : string.Empty;
            var value = WebUtility.UrlDecode(encodedValue);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static bool IsValidAsin(string? value)
    {
        return value is { Length: 10 } && value.All(char.IsAsciiLetterOrDigit);
    }

    private static bool IsImageHost(string host)
    {
        return string.Equals(host, "g-ec2.images-amazon.com", StringComparison.OrdinalIgnoreCase)
            || string.Equals(host, "m.media-amazon.com", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetAmazonMarketplace(string host, out string marketplace, out string affiliateTag)
    {
        if (string.Equals(host, "amazon.com", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".amazon.com", StringComparison.OrdinalIgnoreCase))
        {
            marketplace = "amazon.com";
            affiliateTag = ComAffiliateTag;
            return true;
        }

        if (string.Equals(host, "amazon.ca", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".amazon.ca", StringComparison.OrdinalIgnoreCase))
        {
            marketplace = "amazon.ca";
            affiliateTag = CaAffiliateTag;
            return true;
        }

        marketplace = string.Empty;
        affiliateTag = string.Empty;
        return false;
    }
}
