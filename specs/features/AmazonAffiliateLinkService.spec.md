# Amazon Affiliate Link Normalization Service
**Purpose:**  
A .NET service that receives any Amazon URL (amazon.com or amazon.ca) and returns a normalized, affiliate‑tag‑correct version using the correct tag:

- For **amazon.com** → `company00-20`
- For **amazon.ca** → `company-20`

---

## 1. Core Responsibilities
- Normalize Amazon URLs into a canonical product/category/search format.
- Strip all unnecessary query parameters.
- Replace any existing affiliate tag with the correct tag based on domain.
- Detect URLs where affiliate tags do not work (e.g., homepage) and return the original URL.
- Ignore non-product image URLs.
- Extract ASINs from product URLs even when embedded in long paths.
- Support both **amazon.com** and **amazon.ca**.

---

## 2. Valid Affiliate‑Eligible URL Types

### Product pages
Patterns:
- https://www.amazon.com/dp/{ASIN}
- https://www.amazon.com/gp/product/{ASIN}
- https://www.amazon.com/.../dp/{ASIN}/...
- https://www.amazon.ca/dp/{ASIN}
- https://www.amazon.ca/gp/product/{ASIN}
- https://www.amazon.ca/.../dp/{ASIN}/...

Output:
- For **amazon.com** → `https://www.amazon.com/dp/{ASIN}?tag=company00-20`
- For **amazon.ca** → `https://www.amazon.ca/dp/{ASIN}?tag=company-20`

---

### Search results
Pattern:
- https://www.amazon.com/s?k={query}
- https://www.amazon.ca/s?k={query}

Output:
- COM → `https://www.amazon.com/s?k={query}&tag=company00-20`
- CA → `https://www.amazon.ca/s?k={query}&tag=company-20`

---

### Category pages
Pattern:
- https://www.amazon.com/b?node={nodeId}
- https://www.amazon.ca/b?node={nodeId}

Output:
- COM → `https://www.amazon.com/b?node={nodeId}&tag=company00-20`
- CA → `https://www.amazon.ca/b?node={nodeId}&tag=company-20`

---

## 3. Non‑Affiliate‑Eligible URLs
Amazon ignores affiliate tags on:
- Homepage:  
  - https://www.amazon.com  
  - https://www.amazon.ca
- Any URL without product/search/category context.

Return: **original URL**

---

## 4. URLs to Ignore Entirely
Image/CDN URLs:
- http://g-ec2.images-amazon.com/...
- https://m.media-amazon.com/images/...

Return: **original URL**

---

## 5. Normalization Rules

### Rule A — Extract ASIN
ASIN is always a 10‑character alphanumeric string.

Extract from:
- /dp/{ASIN}
- /gp/product/{ASIN}
- /.../dp/{ASIN}/...
- Query parameters like ASIN=B07FDJMC9Q

---

### Rule B — Remove all query parameters except `tag`
Strip everything:
- ie=UTF8
- psc=1
- th=1
- asc_campaign
- asc_source
- asc_refurl
- duplicate tag parameters

---

### Rule C — Replace affiliate tag
Use domain‑specific tag:

- **amazon.com → company00-20**  
- **amazon.ca → company-20**

---

## 6. Examples

### Example 1 — Product with extra params (COM)
Input:
https://www.amazon.com/dp/B0F9B3ZV61?tag=kclunicorn-20&linkCode=ogi&th=1&psc=1

Output:
https://www.amazon.com/dp/B0F9B3ZV61?tag=company00-20

---

### Example 2 — Product with extra params (CA)
Input:
https://www.amazon.ca/dp/B0F9B3ZV61?tag=randomtag-20&psc=1&ref=abc

Output:
https://www.amazon.ca/dp/B0F9B3ZV61?tag=company-20

---

### Example 3 — Product with many params
Input:
https://www.amazon.com/dp/B07FDJMC9Q?ie=UTF8&s=psc&ASIN=B07FDJMC9Q&th=1&tag=foodnetwor075-20&asc_campaign=SEO

Output:
https://www.amazon.com/dp/B07FDJMC9Q?tag=company00-20

---

### Example 4 — Product with long path (CA)
Input:
https://www.amazon.ca/Innsky-Electric-Stainless-Touchscreen-Nonstick/dp/B07MR3KWSL/

Output:
https://www.amazon.ca/dp/B07MR3KWSL?tag=company-20

---

### Example 5 — Homepage
Input:
https://www.amazon.com

Output:
https://www.amazon.com

Input:
https://www.amazon.ca

Output:
https://www.amazon.ca

---

### Example 6 — Image URL
Input:
https://m.media-amazon.com/images/I/71Mu4+sWUfL._AC_SY240_QL70_.jpg

Output:
https://m.media-amazon.com/images/I/71Mu4+sWUfL._AC_SY240_QL70_.jpg

---

## 7. Service Output Model (Recommended)
public class AffiliateLinkResult
{
    public bool IsAffiliateEligible { get; set; }
    public string? NormalizedUrl { get; set; }
    public string? Asin { get; set; }
    public string? Reason { get; set; }
}

---

## 8. Suggested Service Responsibilities
public interface IAmazonAffiliateLinkService
{
    AffiliateLinkResult Normalize(string url);
}

Implementation tasks:
- Parse URL
- Detect domain (COM vs CA)
- Detect URL type
- Extract ASIN or node/query
- Normalize into canonical format
- Apply correct tag based on domain
- Remove unwanted parameters
- Return structured result

---

## 9. Edge Cases to Handle
- HTML entities (&amp;)
- Multiple tag parameters
- Missing protocol (amazon.com/dp/...)
- Uppercase/lowercase ASIN
- Tracking prefixes (ref=…)
- International Amazon domains (future enhancement)
- URLs with both COM and CA patterns (redirected links)

---

## 10. Unit Test Scenarios
- Product URL normalization (COM + CA)
- Search URL normalization (COM + CA)
- Category URL normalization (COM + CA)
- Homepage detection (COM + CA)
- Image URL detection
- ASIN extraction from multiple formats
- Query parameter stripping
- Tag replacement based on domain
