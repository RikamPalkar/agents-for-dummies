# AI Agent Development Setup Guide

Quick guide to the setup commands for building AI agents with Python. These prerequisites get you ready to start coding AI agents quickly.

---

## The Setup Commands Explained

### 1. Check Python Version
```bash
python3 --version
```
Checks if Python 3 is installed. You need Python 3.12+ for modern AI libraries.

### 2. Install Package Manager & Python

**macOS:**
```bash
# Install Homebrew (package manager)
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Install Python 3.12
brew install python@3.12
```
Homebrew is like an app store for developer tools on macOS. Makes installing Python and other tools simple.

**Windows:**
```powershell
# Option 1: Download from python.org
# Visit https://www.python.org/downloads/
# Download Python 3.12 installer
# Run installer and CHECK "Add Python to PATH"

# Option 2: Use Windows Package Manager (winget)
winget install Python.Python.3.12

# Option 3: Use Chocolatey
choco install python312
```
**Recommended for Windows:** Download directly from [python.org](https://www.python.org/downloads/) and ensure you check "Add Python to PATH" during installation.

### 3. Create Virtual Environment

**macOS/Linux:**
```bash
python3.12 -m venv .venv
```

**Windows:**
```powershell
python -m venv .venv
```

**What's a virtual environment?** An isolated Python workspace for your project. Each project gets its own packages without conflicts.

Think of it like this: Project A needs library version 1.0, Project B needs version 2.0. Virtual environments keep them separate.

### 4. Activate Virtual Environment

**macOS/Linux:**
```bash
source .venv/bin/activate
```

**Windows (PowerShell):**
```powershell
.venv\Scripts\Activate.ps1
```

**Windows (Command Prompt):**
```cmd
.venv\Scripts\activate.bat
```

Activates your isolated environment. You'll see `(.venv)` in your terminal prompt. **Run this every time you open a new terminal.**

To deactivate later: `deactivate`

### 5. Install AI Packages
```bash
pip install openai python-dotenv jupyter
```
- **openai**: Official library to interact with GPT models and build AI agents
- **python-dotenv**: Manages API keys securely (stores them in `.env` file)
- **jupyter**: Creates interactive notebooks for experimenting with AI code

### 6. Open in VS Code
```bash
code .
```
Opens VS Code in your project folder with all your tools ready.

---

## Getting Your OpenAI API Key

Before coding AI agents, you need an OpenAI API key:

### Step 1: Create OpenAI Account
1. Go to [platform.openai.com](https://platform.openai.com)
2. Sign up or log in
3. You may need to add billing information (API usage is pay-as-you-go)

### Step 2: Generate API Key
1. Click on your profile (top right)
2. Select **"API keys"** from the menu
3. Click **"Create new secret key"**
4. Give it a name (e.g., "AI Agent Project")
5. **Copy the key immediately** - you won't see it again!
6. Store it safely

---

## What is a `.env` File?

Your `.env` file stores sensitive data like API keys:

```env
OPENAI_API_KEY=sk-proj-xxxxxxxxxxxxx
```

**Why use it?**
- Keeps secrets out of code
- Easy to change keys without editing code
- Industry standard for configuration

**In your code:**
```python
from dotenv import load_dotenv
import os

load_dotenv()
api_key = os.getenv("OPENAI_API_KEY")
```

**Important:** Add `.env` to `.gitignore` so you don't commit secrets!

---

## What is a `.ipynb` (Jupyter Notebook)?

`.ipynb` = Interactive Python Notebook

**Why notebooks for AI agents?**
- **Test incrementally**: Run code in cells, see results immediately
- **Experiment fast**: Try different prompts and models without rerunning everything
- **Mix code + docs**: Explain your agent's logic alongside the code
- **Debug easily**: Check variables at each step

**Traditional `.py` file:**
```python
# Runs all at once, top to bottom
import openai
# ... 100 lines ...
print(result)  # Have to run everything to see this
```

**Notebook `.ipynb`:**
```python
# Cell 1: Import and setup (run once)
import openai

# Cell 2: Define agent (edit and rerun this cell)
def my_agent(prompt):
    return openai.chat.completions.create(...)

# Cell 3: Test (tweak prompt, rerun just this)
my_agent("Write a story")
```

**Creating a notebook in VS Code:**
- `Cmd + Shift + P` → "Create: New Jupyter Notebook"
- Or create a file: `agent.ipynb`

**Key shortcuts:**
- `Shift + Enter`: Run cell and move to next
- `Ctrl/Cmd + Enter`: Run cell and stay

---

## Essential Project Files

### `.gitignore`
```gitignore
.venv/
.env
__pycache__/
.ipynb_checkpoints/
```

### `requirements.txt`
```txt
openai
python-dotenv
jupyter
```

Save your packages: `pip freeze > requirements.txt`
Others can install: `pip install -r requirements.txt`

---

## Quick Commands Reference

**macOS/Linux:**
```bash
# Activate environment (do this first!)
source .venv/bin/activate

# Install packages
pip install <package-name>

# Save package list
pip freeze > requirements.txt

# Deactivate environment
deactivate
```

**Windows (PowerShell):**
```powershell
# Activate environment
.venv\Scripts\Activate.ps1

# Install packages
pip install <package-name>

# Save package list
pip freeze > requirements.txt

# Deactivate environment
deactivate
```

---

## Ready to Build AI Agents!

Your setup is complete. Now you can:

1. Create `agent.ipynb` notebook
2. Add your OpenAI API key to `.env`
3. Start building AI agents that can:
   - Have conversations
   - Use tools and functions
   - Make decisions
   - Chain multiple steps
   - Access external data

The interactive notebook lets you experiment, iterate, and refine your AI agent quickly.

**Next:** Start coding your first agent in `agent.ipynb`!
