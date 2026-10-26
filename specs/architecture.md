# Global Copilot Instructions for This Solution

These instructions apply to the entire solution. Copilot should load this file for all projects and use it as the authoritative source for architecture, conventions, and cross‑project behavior.

---

# 1. Solution Architecture Overview

This solution follows a layered architecture

- Domain — core business logic, entities, value objects, domain services.
- Application — use cases, service orchestration, DTOs, interfaces.
- Infrastructure — external integrations, persistence, API clients, utility services.
- API — controllers, requestresponse models, HTTP endpoints.
- Tests — unit tests for each layer.

Copilot must respect these boundaries when generating code.

---

# 2. Specification Files (Feature-Level Instructions)

Copilot should reference the following `.spec.md` files when generating or modifying code related to their features

- Amazon Affiliate Link Normalization Service  
  `srcInfrastructureAmazonAffiliateLinkService.spec.md`

Additional `.spec.md` files may be added later. Copilot should treat each `.spec.md` as the authoritative definition of behavior, rules, edge cases, and test scenarios for that feature.

---

# 3. Layer Responsibilities (Copilot Guidance)

### Domain Layer
- Pure business logic only.
- No external dependencies.
- No HTTP, no file IO, no Amazon URL parsing.

### Application Layer
- Coordinates domain logic.
- Defines interfaces for infrastructure services.
- Contains use cases and DTOs.

### Infrastructure Layer
- Implements external integrations.
- Contains utility services such as
  - AmazonAffiliateLinkService
  - URL normalization
  - Persistence implementations
- May reference external libraries.

### API Layer
- Controllers only.
- No business logic.
- Calls Application layer services.

Copilot must generate code that respects these boundaries.

---

# 4. Unit Test Generation Rules

Copilot must generate unit tests in the correct test project based on the layer being tested

- Infrastructure services → `testsInfrastructure.Tests`
- Application services → `testsApplication.Tests`
- Domain logic → `testsDomain.Tests`
- API controllers → `testsApi.Tests`

Test conventions

- Test file naming `{ClassName}Tests.cs`
- Use xUnit unless otherwise specified.
- Use FluentAssertions for assertions.
- Mock external dependencies using Moq (or the project's chosen mocking library).

When a `.spec.md` includes test scenarios, Copilot must generate the tests in the appropriate test project.

---

# 5. How Copilot Should Use Specification Files

When generating code for a feature

1. Load the relevant `.spec.md` file.
2. Follow all rules, examples, and edge cases defined in the spec.
3. Generate code in the correct project (Infrastructure, Application, etc.).
4. Generate unit tests in the correct test project.
5. Maintain consistency with the architecture rules in this global file.

If multiple `.spec.md` files apply, Copilot should merge their guidance.

---

# 6. Folder-Specific Instructions

Copilot should automatically load folder-specific instructions when present

- `src.githubinstructions.instructions.md`
- `tests.githubinstructions.instructions.md`

These files override or extend global instructions for their respective layer.

---

# 7. Coding Conventions

- Use C# latest stable version.
- Use asyncawait for all IO operations.
- Use dependency injection for all services.
- Avoid static classes for business logic.
- Follow SOLID principles.
- Prefer interfaces for abstractions.
- Use meaningful naming aligned with the domain.

---

# 8. Documentation Conventions

- All generated services must include XML documentation summaries.
- Public methods must include parameter and return documentation.
- Copilot should reference `.spec.md` files when generating documentation.

---

# 9. Behavior When Uncertain

If Copilot is unsure which project a file belongs to

- Prefer Infrastructure for external integrations.
- Prefer Application for orchestration logic.
- Prefer Domain for pure business rules.
- Prefer Tests for anything test-related.

Copilot should never guess silently — it should ask for clarification.

---

# 10. Future Expansion

This solution may add more `.spec.md` files. Copilot should treat all `.spec.md` files as authoritative behavior definitions and integrate them into code generation automatically.

