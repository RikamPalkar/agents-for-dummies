# Build a Real MCP C# Code Review Server (No Fluff)

If you want to learn MCP by actually shipping something useful, this is a good project to study.

This repository is a .NET 8 MCP server that reviews raw C# code and returns structured JSON: findings, scoring, category-level coverage, and suggested fixes.

You can plug it into Cursor, VS Code MCP clients, Claude Code, or any MCP-compatible client.

---

## 1. What This Project Is

This is not a toy "hello world" MCP server.

It does three concrete things:

1. `review_csharp_code`: analyzes C# input against a rule set (security, async, performance, maintainability, design), then returns findings + scores.
2. `health_check`: returns server health and timestamp.
3. `get_rule_backlog`: reads markdown docs and returns pending rule ideas.

The important part: it returns not only issues, but also what was checked and how each category scored.

---

## 2. Start With the Project Setup

### Runtime + package setup

- Target framework: .NET 8
- Host model: `Microsoft.Extensions.Hosting`
- MCP SDK: `ModelContextProtocol`

See `McpCodeReviewServer.csproj`.

### Server bootstrap

The entire server startup is in `Program.cs`:

- Create generic host
- Register services and rules via DI
- Add MCP server
- Use stdio transport
- Register tool class
- Run host

In short: no HTTP server, no controllers, no extra plumbing. MCP over stdio keeps it simple and client-friendly.

---

## 3. Project Structure

High-level structure:

- `Tools/`: MCP tool endpoints
- `Services/`: business logic (analysis, scoring, backlog parsing, DI)
- `Rules/`: pluggable rule providers + rule abstractions
- `Models/`: response contracts
- `documentation/rule-catalog/`: human-editable rule backlog docs
- `app/`: demo source that intentionally breaks rules

If you build MCP systems, this separation is what you want: transport concerns in one place, domain logic in another.

---

## 4. Purpose of Each File

### Root files

- `code-review.sln`: solution container.
- `McpCodeReviewServer.csproj`: framework + package references.
- `Program.cs`: host + MCP server bootstrapping.
- `README.md`: usage and demo docs.
- `Article.md`: this teaching article.

### Tools

- `Tools/CodeReviewTool.cs`: MCP tool contract implementation.
  - Exposes `review_csharp_code`, `health_check`, `get_rule_backlog`.
  - Handles input validation, serialization, and error envelopes.
  - Logs invocation metadata for traceability.

### Services (interfaces + implementations)

- `Services/IReviewAnalyzer.cs`: analyzer contract.
- `Services/ReviewAnalyzer.cs`: runs all rules, tracks checked/matched counts by category, caps returned issues by `maxIssues`.
- `Services/IReviewScorer.cs`: scoring contract.
- `Services/ReviewScorer.cs`: computes overall score and per-category scores.
- `Services/IRuleBacklogService.cs`: backlog reader contract.
- `Services/MarkdownRuleBacklogService.cs`: parses `documentation/rule-catalog/*.md` and extracts "Add New Rules" entries.
- `Services/ServiceCollectionExtensions.cs`: DI registration for analyzer/scorer/backlog service + all rule providers.

### Models

- `Models/ReviewIssue.cs`: single finding shape.
- `Models/ReviewResult.cs`: full tool output payload.
- `Models/ReviewAnalysisResult.cs`: analyzer internal result (all issues + returned issues + category analysis).
- `Models/CategoryAnalysis.cs`: per-category checked/matched counters.
- `Models/CategoryReviewScore.cs`: per-category score + counts.
- `Models/SuggestedChange.cs`: normalized fix suggestion shape.

### Rule abstractions

- `Rules/Abstractions/ICodeRule.cs`: one rule = one optional finding.
- `Rules/Abstractions/IRuleGroupProvider.cs`: category-level rule provider contract.
- `Rules/Abstractions/RuleContext.cs`: input context (full code + normalized lines).
- `Rules/Abstractions/ContainsTokenRule.cs`: token matcher helper.
- `Rules/Abstractions/RegexRule.cs`: regex matcher helper.
- `Rules/Abstractions/DelegateRule.cs`: custom evaluator wrapper.

### Rule providers

- `Rules/Async/AsyncRulesProvider.cs`: async anti-pattern checks.
- `Rules/Security/SecurityRulesProvider.cs`: SQL/process/secrets checks.
- `Rules/Performance/PerformanceRulesProvider.cs`: allocation + LINQ pattern checks.
- `Rules/Maintainability/MaintainabilityRulesProvider.cs`: broad/empty catch checks.
- `Rules/Method/MethodRulesProvider.cs`: naming + parameter count checks.
- `Rules/TypeDesign/TypeDesignRulesProvider.cs`: interface naming + type design guidance.
- `Rules/FileAndFolder/FileAndFolderRulesProvider.cs`: namespace conventions, large file, multiple types/file checks.
- `Rules/CSharpModernization/CSharpModernizationRulesProvider.cs`: top-level statement modernization check.

### Documentation and demo

- `documentation/rules.md`: rule system docs.
- `documentation/rule-catalog/*.md`: category rule docs + backlog section parsed by tool.
- `app/RuleBreakerDemo.cs`: intentionally bad C# file to trigger rules.
- `app/README.md`: demo instructions.
- `app/screenshots/*`: Cursor MCP workflow screenshots.

---

## 5. Main Focus: How the MCP Server Is Built

This is the part most people overcomplicate.

### Step A: Register MCP + transport

In `Program.cs`:

- `AddMcpServer()` creates MCP server services.
- `WithStdioServerTransport()` selects stdio transport.
- `WithTools<CodeReviewTool>()` exposes all methods marked with MCP attributes in that class.

That is the entire MCP plumbing.

### Step B: Expose tools via attributes

In `Tools/CodeReviewTool.cs`:

- Class marked with `[McpServerToolType]`
- Methods marked with `[McpServerTool(Name = "...")]`
- Parameter descriptions via `[Description]`

Those attributes define your MCP contract surface.

### Step C: Keep tool methods thin

`CodeReviewTool` does orchestration only:

- Validate input
- Call analyzer/scorer services
- Shape output in stable JSON contract
- Log invocation metadata
- Return safe error response on exception

Rule logic does not belong in tool methods.

### Step D: Build an analyzer pipeline

`ReviewAnalyzer` executes all registered rules against a normalized line context.

Key behavior:

- Every rule increments `RulesChecked` for its category.
- Matched rules increment `RulesMatched`.
- All matches collected in `AllIssues`.
- Response list is bounded by `maxIssues`.

This gives you both diagnostics and coverage telemetry.

### Step E: Score results predictably

`ReviewScorer` maps severities to penalties:

- critical: -3
- warning: -2
- suggestion: -1

Scores are clamped to `0..10`.

Per-category scoring uses the same logic against category-specific issue slices.

### Step F: Return a contract clients can render easily

`ReviewResult` returns:

- summary
- overall score
- issues
- invocationId
- totalRulesChecked
- totalRulesMatched
- checkedCategories
- categoryScores
- suggestedChanges

This is what makes the output useful in real UI and workflow automation.

---

## 6. Data Flow (Request to Response)

When a client calls `review_csharp_code`:

1. MCP client sends method call over stdio.
2. MCP server dispatches to `CodeReviewTool.ReviewCSharpCode`.
3. Tool calls `IReviewAnalyzer`.
4. Analyzer executes all rules and computes coverage.
5. Tool calls `IReviewScorer` for overall + per-category scores.
6. Tool maps findings to `SuggestedChange` entries.
7. Tool serializes `ReviewResult` to JSON and returns response.
8. Invocation metadata is logged.

That is the full loop.

---

## 7. How to Build This Project Yourself

Use this order.

1. Create .NET console project.
2. Add `ModelContextProtocol` and host package references.
3. Wire MCP server in `Program.cs` with stdio transport.
4. Add one tool class with one method (`health_check`) first.
5. Add service interfaces and implementations.
6. Add rule abstractions (`ICodeRule`, providers).
7. Add rule categories incrementally.
8. Add models for stable JSON output.
9. Add scoring and category coverage.
10. Add markdown-backed backlog parser.
11. Add a deliberately bad demo file and validate end-to-end in a real MCP client.

Do not start by writing dozens of rules. Get one rule and one response contract working first.

---

## 8. Practical MCP Lessons From This Repo

1. Keep transport thin. Put logic in services.
2. Design stable output contracts early.
3. Include observability fields (`invocationId`, checked/matched counts).
4. Use DI for all rule providers so the system is easy to extend.
5. Keep rules composable and deterministic.
6. Make demo data intentionally noisy so coverage is obvious.

---

## 9. What to Improve Next

If you want to take this from solid to production-grade:

- Add automated tests per rule provider.
- Add duplicate-finding suppression where needed.
- Add file path support (today input is raw code text).
- Add optional JSON schema versioning in `ReviewResult`.
- Add configurable severity weights.
- Add benchmark tests for large files.

---

## Final Word

MCP is not complicated when you keep boundaries clean.

This project proves the pattern:

- one tool surface
- one analysis pipeline
- explicit contracts
- measurable output

That is how you build MCP servers people can trust in real developer workflows.
