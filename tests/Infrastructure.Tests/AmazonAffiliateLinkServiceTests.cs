using Application.Interfaces;
using FluentAssertions;
using Infrastructure;

namespace Infrastructure.Tests;

public class AmazonAffiliateLinkServiceTests
{
    private readonly IAmazonAffiliateLinkService _service = new AmazonAffiliateLinkService();

    [Fact]
    public void Normalize_WhenProductUrlHasExtraParamsOnCom_ReturnsCanonicalAffiliateUrl()
    {
        var input = "https://www.amazon.com/dp/B0F9B3ZV61?tag=kclunicorn-20&linkCode=ogi&th=1&psc=1";

        var result = _service.Normalize(input);

        result.IsAffiliateEligible.Should().BeTrue();
        result.NormalizedUrl.Should().Be("https://www.amazon.com/dp/B0F9B3ZV61?tag=company00-20");
        result.Asin.Should().Be("B0F9B3ZV61");
    }

    [Fact]
    public void Normalize_WhenProductUrlHasExtraParamsOnCa_ReturnsCanonicalAffiliateUrl()
    {
        var input = "https://www.amazon.ca/dp/B0F9B3ZV61?tag=randomtag-20&psc=1&ref=abc";

        var result = _service.Normalize(input);

        result.IsAffiliateEligible.Should().BeTrue();
        result.NormalizedUrl.Should().Be("https://www.amazon.ca/dp/B0F9B3ZV61?tag=company-20");
        result.Asin.Should().Be("B0F9B3ZV61");
    }

    [Fact]
    public void Normalize_WhenProductUrlContainsQueryAsinAndOldTag_RemovesUnwantedParameters()
    {
        var input = "https://www.amazon.com/dp/B07FDJMC9Q?ie=UTF8&s=psc&ASIN=B07FDJMC9Q&th=1&tag=foodnetwor075-20&asc_campaign=SEO";

        var result = _service.Normalize(input);

        result.IsAffiliateEligible.Should().BeTrue();
        result.NormalizedUrl.Should().Be("https://www.amazon.com/dp/B07FDJMC9Q?tag=company00-20");
        result.Asin.Should().Be("B07FDJMC9Q");
    }

    [Fact]
    public void Normalize_WhenProductUrlHasLongPathOnCa_ReturnsCanonicalProductUrl()
    {
        var input = "https://www.amazon.ca/Innsky-Electric-Stainless-Touchscreen-Nonstick/dp/B07MR3KWSL/";

        var result = _service.Normalize(input);

        result.IsAffiliateEligible.Should().BeTrue();
        result.NormalizedUrl.Should().Be("https://www.amazon.ca/dp/B07MR3KWSL?tag=company-20");
        result.Asin.Should().Be("B07MR3KWSL");
    }

    [Fact]
    public void Normalize_WhenSearchUrlIsProvided_ReturnsCanonicalSearchUrl()
    {
        var result = _service.Normalize("https://www.amazon.com/s?k=wireless+headphones&tag=oldtag-20&ref=abc");

        result.IsAffiliateEligible.Should().BeTrue();
        result.NormalizedUrl.Should().Be("https://www.amazon.com/s?k=wireless%20headphones&tag=company00-20");
        result.Asin.Should().BeNull();
    }

    [Fact]
    public void Normalize_WhenCategoryUrlIsProvided_ReturnsCanonicalCategoryUrl()
    {
        var result = _service.Normalize("https://www.amazon.ca/b?node=123456&tag=oldtag-20&ref=abc");

        result.IsAffiliateEligible.Should().BeTrue();
        result.NormalizedUrl.Should().Be("https://www.amazon.ca/b?node=123456&tag=company-20");
    }

    [Fact]
    public void Normalize_WhenHomepageUrlIsProvided_ReturnsOriginalUrl()
    {
        var result = _service.Normalize("https://www.amazon.com");

        result.IsAffiliateEligible.Should().BeFalse();
        result.NormalizedUrl.Should().Be("https://www.amazon.com");
        result.Reason.Should().Contain("Homepage");

        var caResult = _service.Normalize("https://www.amazon.ca");
        caResult.IsAffiliateEligible.Should().BeFalse();
        caResult.NormalizedUrl.Should().Be("https://www.amazon.ca");
    }

    [Fact]
    public void Normalize_WhenImageUrlIsProvided_ReturnsOriginalUrl()
    {
        var input = "https://m.media-amazon.com/images/I/71Mu4+sWUfL._AC_SY240_QL70_.jpg";

        var result = _service.Normalize(input);

        result.IsAffiliateEligible.Should().BeFalse();
        result.NormalizedUrl.Should().Be(input);
        result.Reason.Should().Contain("ignored");
    }

    [Fact]
    public void Normalize_WhenUrlHasNoProtocol_HandlesRelativeAmazonUrl()
    {
        var result = _service.Normalize("amazon.com/gp/product/B07FDJMC9Q?ref=abc");

        result.IsAffiliateEligible.Should().BeTrue();
        result.NormalizedUrl.Should().Be("https://www.amazon.com/dp/B07FDJMC9Q?tag=company00-20");
        result.Asin.Should().Be("B07FDJMC9Q");
    }

    [Fact]
    public void Normalize_WhenAsinIsLowercase_UsesUppercaseInCanonicalUrl()
    {
        var result = _service.Normalize("https://www.amazon.ca/gp/product/b07fdjmc9q");

        result.IsAffiliateEligible.Should().BeTrue();
        result.NormalizedUrl.Should().Be("https://www.amazon.ca/dp/B07FDJMC9Q?tag=company-20");
        result.Asin.Should().Be("B07FDJMC9Q");
    }
}
