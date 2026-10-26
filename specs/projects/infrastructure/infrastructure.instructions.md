# Infrastructure Layer – Copilot Instructions

These instructions apply to all code generated inside the Infrastructure project.

---

## 1. Purpose of the Infrastructure Layer

The Infrastructure layer contains implementations of external integrations, utilities, and technical services.  
Copilot must ensure that code generated here:

- Implements interfaces defined in the Application layer.
- Does NOT contain business logic.
- Does NOT reference API controllers.
- May reference external libraries when appropriate.
- May perform I/O, HTTP calls, URL normalization, persistence, etc.

---

## 2. Specification Files Relevant to This Layer

Copilot must reference the following specification files when generating or modifying Infrastructure code:

- **Amazon Affiliate Link Normalization Service**  
  `/specs/features/AmazonAffiliateLinkService.spec.md`

Additional Infrastructure specs may be added later. Copilot should treat each `.spec.md` file as authoritative for behavior, rules, examples, and edge cases.

---

## 3. Code Generation Rules

When generating code in this layer:

- Follow all rules defined in the corresponding `.spec.md` file.
- Implement interfaces from the Application layer.
- Use dependency injection for all services.
- Avoid static classes unless the spec explicitly requires them.
- Include XML documentation summaries for all public members.
- Keep logic focused on external concerns (URL parsing, HTTP, persistence, etc.).

---

## 4. Unit Test Placement

Copilot must NOT generate tests inside the Infrastructure project.

All tests for Infrastructure services must be placed in:

/tests/Infrastructure.Tests

Test conventions and frameworks (xUnit, FluentAssertions, Moq) are defined in:

/.github/copilot-instructions.md


---

## 5. Folder-Specific Behavior

When generating code inside `/src/Infrastructure`, Copilot must:

- Load this instruction file.
- Load the global `copilot-instructions.md`.
- Load all referenced `.spec.md` files.
- Apply Infrastructure conventions.
- Respect architectural boundaries.

---

## 6. Behavior When Uncertain

If Copilot is unsure about placement or conventions:

- Prefer Infrastructure for external integrations.
- Ask for clarification rather than guessing silently.

