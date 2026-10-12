# AI Spec-Driven Development Demo

This repository is a small .NET solution that demonstrates how to build software with AI-assisted, specification-first development. Instead of asking an AI to "write the feature" in one shot, the project defines behavior in spec files and lets the AI generate code that follows those contracts.

## What this project is demonstrating

The project uses a layered architecture and treats specification documents as the source of truth for feature behavior.

- Domain logic stays focused on business rules.
- Application code orchestrates use cases.
- Infrastructure handles external integrations and technical concerns.
- Tests validate the expected behavior.
- AI is guided by architecture instructions and feature specs instead of vague prompts.

This creates a much more predictable workflow than freeform coding.

## Project structure

```text
.
├── .ai/
│   └── architecture.md
├── src/
│   ├── Application/
│   └── Infrastructure/
│       ├── AmazonAffiliateLinkService.spec.md
│       └── github/
│           └── infrastructure.instructions.md
├── tests/
│   └── Infrastructure.Tests/
├── project.sln
├── LICENSE
└── README.md
```

## Core idea: specs before implementation

The project intentionally separates:

- architecture guidance
- folder-level instructions
- feature specifications
- implementation
- tests

That structure is designed to help AI assistants behave consistently.

### 1. Architecture instructions

The file [.ai/architecture.md](.ai/architecture.md) defines the solution boundaries and expectations:

- layered architecture
- responsibilities per project
- testing conventions
- how specification files should be used

This acts like a system-level blueprint for the AI.

### 2. Folder-specific instructions

The Infrastructure project has its own instructions in [src/Infrastructure/github/infrastructure.instructions.md](src/Infrastructure/github/infrastructure.instructions.md). These tell the AI:

- what belongs in Infrastructure
- which specs to load
- how to generate tests
- how to respect project boundaries

This prevents the AI from drifting into the wrong layer.

### 3. Feature specs

The feature behavior is captured in specification files such as [src/Infrastructure/AmazonAffiliateLinkService.spec.md](src/Infrastructure/AmazonAffiliateLinkService.spec.md).

The spec describes:

- valid affiliate-eligible URL types
- non-eligible URLs
- normalization rules
- examples
- edge cases
- suggested service contract
- unit test scenarios

This is the real "contract" that the implementation follows.

## Example workflow

A typical AI spec-driven workflow for this repo looks like this:

1. Define the feature in a `.spec.md` file.
2. Add or update architecture and layer instructions.
3. Ask the AI to implement the feature by following the spec.
4. Let the AI generate tests from the scenarios in the spec.
5. Validate behavior with the test suite.
6. Refine the spec if edge cases or requirements change.

That means the implementation is not created from a vague prompt; it is driven by explicit requirements.

## Why this is useful

This approach helps with several common software problems:

- clearer requirements
- fewer ambiguous code generation results
- better separation of concerns
- testable features before implementation
- code that matches business rules more closely

Instead of generating code from intuition, the AI works from a spec and the architecture guardrails.

## Example feature in this repo

The Amazon Affiliate Link Normalization Service is a concrete example of spec-driven development in action.

The spec defines:

- product/search/category URL handling
- correct affiliate tag per domain
- homepage detection
- ignoring image URLs
- ASIN extraction rules
- edge case behavior

This gives the AI a precise target for implementation and validation.

## Recommended workflow for future features

When adding a new feature to this solution:

1. Create a new `.spec.md` file near the relevant project.
2. Document required behavior, edge cases, and expected outputs.
3. Add or update project instructions if needed.
4. Let the AI generate implementation code and tests.
5. Run the relevant tests.
6. Update the spec as the feature evolves.

## Summary

This project shows that AI can be much more reliable when it is constrained by:

- architecture rules
- project-specific instructions
- feature specs
- testing expectations

The result is a practical example of AI spec-driven development: requirements are written as specifications, implementation follows them, and validation stays anchored to the same behavior contract.
