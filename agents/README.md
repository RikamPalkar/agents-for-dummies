# Schrodingers-AI

Learn to build AI agents from scratch with hands-on tutorials and code examples. This repository contains practical, step-by-step guides for creating autonomous AI systems that can make decisions and chain multiple tasks together.

## What We Are Building Here

This project walks through a beginner-friendly, notebook-first implementation of an AI agent in Python.

You will build a two-step agent chain where:

1. The model makes a decision (selects a meal from constraints).
2. That decision is automatically passed into a second model call (generates a complete recipe).

This repository is paired with the original article and an in-repo notes version:

- Original article: [Build Your AI Agent in Python From Scratch](https://www.c-sharpcorner.com/article/build-your-ai-agent-in-python-from-scratch/)
- Repo notes: [python/docs/build-your-ai-agent-in-python-from-scratch.md](python/docs/build-your-ai-agent-in-python-from-scratch.md)

  If you'd rather see this in action instead of reading through all the steps, I've got you covered. Check out the
  [![Watch video](https://img.youtube.com/vi/AjJvqsLg7ug/maxresdefault.jpg)](https://www.youtube.com/watch?v=AjJvqsLg7ug)

## What You'll Learn

- **Agent Fundamentals**: Understand what AI agents are and how they work
- **Hands-On Code**: Build real agents with OpenAI's API
- **Agent Patterns**: Learn the core patterns used in AutoGPT, research assistants, and coding tools
- **Practical Examples**: Start with simple meal planners, scale to complex multi-step agents

## Prerequisites

This series builds on foundational AI concepts. If you're new to AI, start here:

### Schrodinger AI Series

This repository is part of the Schrodinger's AI article series, currently 13 parts and growing.

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

👉 Read the complete Schrodinger's AI series: https://www.c-sharpcorner.com/members/rikam-palkar/articles/ai

Also published on Medium: https://rikampalkar.medium.com

## Repository Structure

```
├── README.md            # You are here
├── SETUP_GUIDE.md       # Detailed environment setup instructions
└── python/
  ├── notebooks/
  │   ├── agent.ipynb
  │   └── persona_chatbot_with_evaluation.ipynb
  ├── docs/
  │   └── build-your-ai-agent-in-python-from-scratch.md
  └── kb/
    ├── aboutme.txt
    └── links.txt
```

## Quick Start

### 1. Clone the Repository
```bash
git clone https://github.com/RikamPalkar/agents-for-dummies.git
cd agents-for-dummies
```

### 2. Set Up Environment
```bash
# Create virtual environment
python3.12 -m venv .venv

# Activate it
source .venv/bin/activate  # Windows: .venv\Scripts\Activate.ps1

# Install dependencies
pip install openai python-dotenv jupyter
```

### 3. Configure API Key
```bash
# Create .env file
echo "OPENAI_API_KEY=your-key-here" > .env
```

Use `../common/config/.env.example` as the template for shared configuration.

Get your API key at [platform.openai.com](https://platform.openai.com)

> **Note:** The persona chatbot uses `gpt-4o-mini` for chat and `gpt-4o` for quality evaluation—both powered by OpenAI. Only one API key is needed.

### 4. Start Building
```bash
# Open the notebook
jupyter notebook python/notebooks/agent.ipynb

# Or use VS Code
code .
```

## What Are AI Agents?

AI agents are systems where AI output becomes input for another AI call, creating autonomous decision-making loops:

```
Traditional:
You → AI → Response → You decide next step

Agent:
You → AI → Decision → AI → Execution → Result
```

This pattern powers:
- **AutoGPT** - Autonomous task completion
- **Research assistants** - Find, read, summarize
- **Code generators** - Plan, write, test
- **Problem solvers** - Break down, solve, synthesize

## Tutorial Walkthrough

The main notebook (`python/notebooks/agent.ipynb`) guides you through building a meal planning agent that:

1. Takes your constraints (budget, time, dietary needs)
2. Autonomously decides what meal to make
3. Generates a complete recipe with ingredients and instructions

**Cost**: ~$0.002 per run (less than a penny)

Each cell is explained step-by-step, teaching you:
- OpenAI API basics
- Message formatting
- Agent chaining patterns
- Error handling
- Best practices

## Requirements

- Python 3.12+
- OpenAI API key
- Basic Python knowledge
- Familiarity with Jupyter notebooks

## Cost

Running the tutorials costs approximately:
- Basic examples: $0.0001 - 0.001 per run
- Full agent workflows: $0.002 - 0.01 per run

Set a spending limit in your OpenAI account for safety.

## Coming Soon

- Multi-step agents with tool use
- Function calling and external APIs
- Memory and conversation state
- Agent frameworks (LangChain, AutoGPT)
- Production deployment patterns

## Contributing

Found an issue? Have an idea? Open an issue or submit a pull request!

## Resources

- [OpenAI Platform](https://platform.openai.com)
- [OpenAI API Documentation](https://platform.openai.com/docs)
- [Prompt Engineering Guide](https://platform.openai.com/docs/guides/prompt-engineering)

## About the Author

**Rikam Palkar**

AI enthusiast sharing practical, hands-on tutorials for building with AI.

- [C# Corner Articles](https://www.c-sharpcorner.com/members/rikam-palkar/articles/ai)
- [GitHub](https://github.com/RikamPalkar)

## License

MIT License - feel free to use this code for learning and building!

---

**Ready to build your first AI agent?** Open `python/notebooks/agent.ipynb` and let's start coding!
