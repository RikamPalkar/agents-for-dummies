# Schrodingers AI Monorepo

This repository is organized as a language-aware monorepo for the Schrodingers AI article series.

## Schrodinger's AI Series

This repository is part of the Schrodinger's AI article series, which is currently 13 parts and growing.

- Full series on C# Corner: https://www.c-sharpcorner.com/members/rikam-palkar/articles/ai
- Also published on Medium: https://rikampalkar.medium.com

Current parts in the series:

1. Schrodinger's AI Part 13: Let's Build an MCP Server with .NET
2. Schrodinger's AI Part 12: Build Your AI Clone
3. Schrodinger's AI Part 11: Build Your AI Agent in Python From Scratch
4. Schrodinger's AI Part 10: Fine Tuning
5. Schrodinger's AI Part 9: Inside RAG
6. Schrodinger's AI Part 8: Inside the Model Context Protocol
7. Schrodinger's AI Part 7: What Is an AI Agent
8. Schrodinger's AI Part 6: Foundation Models: Everything, Everywhere, All at Once
9. Schrodinger's AI Part 5: Transformers in AI
10. Schrodinger's AI Part 4: The ABCs of Deep Learning
11. Schrodinger's AI Part 3: The ABCs of Machine Learning
12. Schrodinger's AI Part 2: Fascinating History of AI: From Turing to Today
13. Schrodinger's AI Part 1: Layers of Artificial Intelligence

## Structure

```text
.
├── agents/                    # Agent-focused projects and learning assets
│   ├── README.md
│   ├── SETUP_GUIDE.md
│   └── python/
│       ├── notebooks/         # Jupyter notebooks for Python agents
│       ├── docs/              # Article-aligned Python agent notes
│       └── kb/                # Knowledge base assets used by agent demos
├── mcp-servers/               # MCP server implementations
│   └── dotnet/
│       ├── code-review/       # MCP code-review server
│       ├── shell/             # MCP shell server
│       └── weather-mcp/       # MCP weather server
└── common/
    └── config/
        └── .env.example       # Shared environment template
```

## Why This Layout

- Keeps domain boundaries explicit: `agents` and `mcp-servers` are first-class areas.
- Separates by runtime language (`python`, `dotnet`) to reduce mixed-tooling friction.
- Moves reusable configuration into a shared location under `common/config`.
- Makes future growth easier (new languages/projects can be added without restructuring).

## Getting Started

### Python Agent Work

1. Go to `agents/`.
2. Follow `SETUP_GUIDE.md`.
3. Start with `python/notebooks/agent.ipynb`.

### .NET MCP Work

Open one of these solution files:

- `mcp-servers/dotnet/code-review/code-review.sln`
- `mcp-servers/dotnet/shell/ShellMcpServer.sln`
- `mcp-servers/dotnet/weather-mcp/mcp-server-dotnet.sln`

## Shared Configuration

Use `common/config/.env.example` as the base template for local environment variables.
