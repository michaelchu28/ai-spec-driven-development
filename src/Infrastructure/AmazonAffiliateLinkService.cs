using System.Text.RegularExpressions;
using Application.Interfaces;

namespace Infrastructure;

/// <summary>
/// Normalizes Amazon product, search, and category URLs into the canonical affiliate-tagged format.
/// </summary>
public sealed class AmazonAffiliateLinkService : IAmazonAffiliateLinkService
{
    private const string ComAffiliateTag = "company00-20";
    private const string CaAffiliateTag = "company-20";

    /// <summary>
    /// Normalizes an Amazon URL to the canonical affiliate-tagged form when eligible.
    /// </summary>
    /// <param name="url">The URL to normalize.</param>
    /// <returns>A structured affiliate result with the normalized URL and metadata.</returns>
    public AffiliateLinkResult Normalize(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return CreateResult(false, url, null, "Input URL is empty.");
        }

        var originalUrl = url.Trim();
        var candidate = originalUrl.Replace("&amp;", "&", StringComparison.OrdinalIgnoreCase);

        if (!TryCreateUri(candidate, out var uri))
        {
            return CreateResult(false, originalUrl, null, "Input URL is not a valid Amazon URL.");
        }

        var host = uri.Host.Trim();
        if (IsImageUrl(host, uri.AbsolutePath))
        {
            return CreateResult(false, originalUrl, null, "Image/CDN URLs are ignored.");
        }

        if (!IsAmazonHost(host))
        {
            return CreateResult(false, originalUrl, null, "URL is not an Amazon domain.");
        }

        if (IsHomepage(uri))
        {
            return CreateResult(false, originalUrl, null, "Homepage URLs are not affiliate eligible.");
        }

        var domain = GetDomain(host);
        var affiliateTag = domain == "com" ? ComAffiliateTag : CaAffiliateTag;

        if (TryGetQueryValue(uri.Query, "k", out var searchTerm) && !string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedUrl = $"https://www.amazon.{domain}/s?k={Uri.EscapeDataString(searchTerm)}&tag={affiliateTag}";
            return CreateResult(true, normalizedUrl, null, "Search URL normalized.");
        }

        if (TryGetQueryValue(uri.Query, "node", out var nodeId) && !string.IsNullOrWhiteSpace(nodeId))
        {
            var normalizedUrl = $"https://www.amazon.{domain}/b?node={Uri.EscapeDataString(nodeId)}&tag={affiliateTag}";
            return CreateResult(true, normalizedUrl, null, "Category URL normalized.");
        }

        var asin = ExtractAsin(uri);
        if (!string.IsNullOrWhiteSpace(asin))
        {
            var normalizedUrl = $"https://www.amazon.{domain}/dp/{asin.Trim()}?tag={affiliateTag}";
            return CreateResult(true, normalizedUrl, asin.Trim(), "Product URL normalized.");
        }

        return CreateResult(false, originalUrl, null, "URL does not contain affiliate-eligible product, search, or category context.");
    }

    private static AffiliateLinkResult CreateResult(bool isEligible, string? normalizedUrl, string? asin, string? reason)
    {
        return new AffiliateLinkResult
        {
            IsAffiliateEligible = isEligible,
            NormalizedUrl = normalizedUrl,
            Asin = asin,
            Reason = reason,
        };
    }

    private static bool IsImageUrl(string host, string path)
    {
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        return host.EndsWith("images-amazon.com", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith("media-amazon.com", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/images/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAmazonHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return false;
        }

        host = host.TrimEnd('.');
        return host.Equals("amazon.com", StringComparison.OrdinalIgnoreCase)
            || host.Equals("www.amazon.com", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".amazon.com", StringComparison.OrdinalIgnoreCase)
            || host.Equals("amazon.ca", StringComparison.OrdinalIgnoreCase)
            || host.Equals("www.amazon.ca", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".amazon.ca", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetDomain(string host)
    {
        if (host.EndsWith("amazon.com", StringComparison.OrdinalIgnoreCase))
        {
            return "com";
        }

        if (host.EndsWith("amazon.ca", StringComparison.OrdinalIgnoreCase))
        {
            return "ca";
        }

        throw new InvalidOperationException($"Unsupported Amazon domain: {host}");
    }

    private static bool IsHomepage(Uri uri)
    {
        var path = uri.AbsolutePath;
        return string.IsNullOrEmpty(path) || path == "/";
    }

    private static bool TryCreateUri(string candidate, out Uri uri)
    {
        uri = null!;

        var normalizedCandidate = candidate.Trim();
        if (string.IsNullOrWhiteSpace(normalizedCandidate))
        {
            return false;
        }

        if (!normalizedCandidate.Contains("://", StringComparison.Ordinal))
        {
            normalizedCandidate = $"https://{normalizedCandidate.TrimStart('/')}";
        }

        if (!Uri.TryCreate(normalizedCandidate, UriKind.Absolute, out var createdUri))
        {
            return false;
        }

        uri = createdUri;
        return true;
    }

    private static bool TryGetQueryValue(string queryString, string key, out string value)
    {
        value = string.Empty;
        if (string.IsNullOrWhiteSpace(queryString))
        {
            return false;
        }

        var pairs = queryString.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var index = pair.IndexOf('=');
            var parameterName = index >= 0 ? pair[..index] : pair;
            if (!string.Equals(parameterName, key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            value = index >= 0 ? pair[(index + 1)..] : string.Empty;
            value = Uri.UnescapeDataString(value.Replace('+', ' '));
            return true;
        }

        return false;
    }

    private static string? ExtractAsin(Uri uri)
    {
        var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        for (var index = 0; index < pathSegments.Length - 1; index++)
        {
            if (pathSegments[index].Equals("dp", StringComparison.OrdinalIgnoreCase) && IsValidAsin(pathSegments[index + 1]))
            {
                return pathSegments[index + 1].ToUpperInvariant();
            }

            if (pathSegments[index].Equals("gp", StringComparison.OrdinalIgnoreCase)
                && index + 2 < pathSegments.Length
                && pathSegments[index + 1].Equals("product", StringComparison.OrdinalIgnoreCase)
                && IsValidAsin(pathSegments[index + 2]))
            {
                return pathSegments[index + 2].ToUpperInvariant();
            }
        }

        var pathMatch = Regex.Match(uri.AbsolutePath, @"(?:/dp/|/gp/product/)([A-Z0-9]{10})(?:/|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        if (pathMatch.Success)
        {
            return pathMatch.Groups[1].Value.ToUpperInvariant();
        }

        if (TryGetQueryValue(uri.Query, "ASIN", out var queryAsin) && IsValidAsin(queryAsin))
        {
            return queryAsin.ToUpperInvariant();
        }

        return null;
    }

    private static bool IsValidAsin(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Length == 10
            && Regex.IsMatch(value, "^[A-Z0-9]{10}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}
