using Application.Interfaces;
using FluentAssertions;
using Infrastructure;

namespace Infrastructure.Tests;

public class AmazonAffiliateLinkServiceTests
{
    private readonly AmazonAffiliateLinkService _service = new();

    [Theory]
    [InlineData(
        "https://www.amazon.com/dp/B0F9B3ZV61?tag=old-20&linkCode=ogi&th=1&psc=1",
        "https://www.amazon.com/dp/B0F9B3ZV61?tag=company00-20",
        "B0F9B3ZV61")]
    [InlineData(
        "https://www.amazon.ca/dp/B0F9B3ZV61?tag=randomtag-20&psc=1&ref=abc",
        "https://www.amazon.ca/dp/B0F9B3ZV61?tag=company-20",
        "B0F9B3ZV61")]
    [InlineData(
        "https://www.amazon.ca/Innsky-Electric-Stainless-Touchscreen-Nonstick/dp/b07mr3kwsl/",
        "https://www.amazon.ca/dp/B07MR3KWSL?tag=company-20",
        "B07MR3KWSL")]
    [InlineData(
        "https://www.amazon.com/product/details/dp/B07FDJMC9Q/reviews",
        "https://www.amazon.com/dp/B07FDJMC9Q?tag=company00-20",
        "B07FDJMC9Q")]
    [InlineData(
        "https://www.amazon.com/gp/product/B07FDJMC9Q?ie=UTF8&s=psc&ASIN=B07FDJMC9Q&th=1&tag=old-20&tag=other-20",
        "https://www.amazon.com/dp/B07FDJMC9Q?tag=company00-20",
        "B07FDJMC9Q")]
    [InlineData(
        "amazon.com/dp/B07FDJMC9Q",
        "https://www.amazon.com/dp/B07FDJMC9Q?tag=company00-20",
        "B07FDJMC9Q")]
    public void Normalize_ProductUrl_ReturnsCanonicalAffiliateUrl(string input, string expectedUrl, string expectedAsin)
    {
        var result = _service.Normalize(input);

        result.IsAffiliateEligible.Should().BeTrue();
        result.NormalizedUrl.Should().Be(expectedUrl);
        result.Asin.Should().Be(expectedAsin);
    }

    [Theory]
    [InlineData(
        "https://www.amazon.com/s?k=coffee+maker&ref=nb_sb_noss&tag=old-20",
        "https://www.amazon.com/s?k=coffee%20maker&tag=company00-20")]
    [InlineData(
        "https://www.amazon.ca/s?k=coffee%20maker&amp;tag=old-20",
        "https://www.amazon.ca/s?k=coffee%20maker&tag=company-20")]
    public void Normalize_SearchUrl_ReturnsCanonicalAffiliateUrl(string input, string expectedUrl)
    {
        var result = _service.Normalize(input);

        result.IsAffiliateEligible.Should().BeTrue();
        result.NormalizedUrl.Should().Be(expectedUrl);
        result.Asin.Should().BeNull();
    }

    [Theory]
    [InlineData(
        "https://www.amazon.com/b?node=123456&ref=nav_cs_gb&tag=old-20",
        "https://www.amazon.com/b?node=123456&tag=company00-20")]
    [InlineData(
        "https://www.amazon.ca/b?node=987654&amp;tag=old-20",
        "https://www.amazon.ca/b?node=987654&tag=company-20")]
    public void Normalize_CategoryUrl_ReturnsCanonicalAffiliateUrl(string input, string expectedUrl)
    {
        var result = _service.Normalize(input);

        result.IsAffiliateEligible.Should().BeTrue();
        result.NormalizedUrl.Should().Be(expectedUrl);
    }

    [Theory]
    [InlineData("https://www.amazon.com")]
    [InlineData("https://www.amazon.ca")]
    [InlineData("https://amazon.com")]
    [InlineData("https://amazon.ca")]
    [InlineData("https://www.amazon.com/?tag=existing-20")]
    public void Normalize_Homepage_ReturnsOriginalUrlAsIneligible(string input)
    {
        var result = _service.Normalize(input);

        result.IsAffiliateEligible.Should().BeFalse();
        result.NormalizedUrl.Should().Be(input);
        result.Asin.Should().BeNull();
    }

    [Theory]
    [InlineData("https://g-ec2.images-amazon.com/images/example.jpg")]
    [InlineData("https://m.media-amazon.com/images/I/example.jpg")]
    public void Normalize_ImageCdnUrl_ReturnsOriginalUrlAsIneligible(string input)
    {
        var result = _service.Normalize(input);

        result.IsAffiliateEligible.Should().BeFalse();
        result.NormalizedUrl.Should().Be(input);
    }

    [Theory]
    [InlineData(
        "https://www.amazon.com/dp/B07FDJMC9Q?ASIN=B07FDJMC9Q&psc=1",
        "https://www.amazon.com/dp/B07FDJMC9Q?tag=company00-20")]
    [InlineData(
        "https://www.amazon.ca/product?ASIN=b07fdjmc9q",
        "https://www.amazon.ca/dp/B07FDJMC9Q?tag=company-20")]
    public void Normalize_ValidAsinQuery_ReturnsCanonicalProductUrl(string input, string expectedUrl)
    {
        var result = _service.Normalize(input);

        result.IsAffiliateEligible.Should().BeTrue();
        result.NormalizedUrl.Should().Be(expectedUrl);
        result.Asin.Should().Be("B07FDJMC9Q");
    }

    [Theory]
    [InlineData("https://amazon.co.uk/dp/B07FDJMC9Q")]
    [InlineData("https://amazon.com.example.org/dp/B07FDJMC9Q")]
    [InlineData("https://www.amazon.com/dp/TOO-SHORT")]
    [InlineData("https://example.com/s?k=coffee")]
    public void Normalize_UnsupportedOrIneligibleUrl_ReturnsOriginalUrl(string input)
    {
        var result = _service.Normalize(input);

        result.IsAffiliateEligible.Should().BeFalse();
        result.NormalizedUrl.Should().Be(input);
    }

    [Fact]
    public void Normalize_HtmlEncodedAmpersands_DecodesBeforeParsing()
    {
        const string input = "https://www.amazon.com/s?k=coffee&amp;tag=old-20&amp;ref=tracking";

        var result = _service.Normalize(input);

        result.IsAffiliateEligible.Should().BeTrue();
        result.NormalizedUrl.Should().Be("https://www.amazon.com/s?k=coffee&tag=company00-20");
    }
}
