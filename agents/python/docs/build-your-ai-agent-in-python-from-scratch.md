# Build Your AI Agent in Python From Scratch (Study Notes)

This document is a practical, repo-specific guide inspired by the article:
[Build Your AI Agent in Python From Scratch](https://www.c-sharpcorner.com/article/build-your-ai-agent-in-python-from-scratch/).

Use this together with the notebook in this repo to understand the core agent pattern:

AI output -> AI input -> chained execution.

## Goal

Build a simple meal-planning AI agent that:

1. Takes high-level constraints from a prompt.
2. Lets the model choose a meal.
3. Uses that model output as input to generate a full recipe.

This is the same foundational pattern used in larger autonomous systems.

## Prerequisites

- Python 3.12+
- OpenAI API key
- Packages: `openai`, `python-dotenv`, `jupyter`
- Basic Python and Jupyter familiarity

## Project Flow (Mapped to `agent.ipynb`)

1. Load environment variables with `dotenv`.
2. Validate `OPENAI_API_KEY` before any API call.
3. Create the OpenAI client.
4. Run a basic test prompt (`What is 2+2?`) to confirm connectivity.
5. Ask the model to suggest one budget-friendly healthy meal.
6. Feed the suggested meal into a second prompt requesting a full recipe.
7. Render the final response as Markdown in the notebook.

## Core Agent Pattern

Traditional flow:

Human prompt -> model response -> human decides next action.

Agent flow:

Human prompt -> model decision -> code chains next prompt -> model executes next step.

In this notebook, the variable `meal_suggestion` is the bridge between step 1 and step 2.

## Why This Matters

The same chaining idea scales to:

- Research agents (discover -> summarize -> synthesize)
- Coding agents (plan -> implement -> test)
- Content agents (outline -> draft -> refine)
- Multi-step assistants with tools

## Common Issues

- `ModuleNotFoundError`: activate virtual environment and install required packages.
- API key not found: check `.env` filename and `OPENAI_API_KEY=...` format.
- Auth/rate-limit errors: verify account billing and API key validity.

## Cost Awareness

The example flow is low-cost, but each API call consumes tokens.
Use smaller models for learning and set spending limits in your OpenAI account.

## Suggested Experiments

1. Change constraints (budget, cuisine, vegetarian-only).
2. Add a third step (shopping list from recipe).
3. Keep conversation memory instead of resetting messages each step.
4. Add `try/except` blocks around API calls.
5. Tune `temperature` and `max_tokens` to compare behavior.

## References

- Source article: [Build Your AI Agent in Python From Scratch](https://www.c-sharpcorner.com/article/build-your-ai-agent-in-python-from-scratch/)
- Notebook in this repo: `agent.ipynb`
- Setup help: `SETUP_GUIDE.md`
