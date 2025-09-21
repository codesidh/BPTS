# Mermaid Diagram Rendering Setup Instructions

## Problem
Your Mermaid diagrams in `.md` files are not rendering as visual diagrams because your Markdown viewer doesn't support Mermaid syntax.

## Solutions

### Option 1: Install Mermaid Extension in Cursor/VS Code ⭐ RECOMMENDED

1. **Install the Mermaid Preview Extension:**
   - Open Cursor/VS Code
   - Go to Extensions (Ctrl+Shift+X)
   - Search for "Mermaid Preview" or "Mermaid Markdown Syntax Highlighting"
   - Install the extension by **bierner.markdown-mermaid**

2. **View Diagrams:**
   - Open your `specs/ARCHITECTURE_DIAGRAMS.md` file
   - Press `Ctrl+Shift+V` (or `Cmd+Shift+V` on Mac) to open Markdown Preview
   - Your Mermaid diagrams will now render as visual diagrams!

### Option 2: Use Mermaid Live Editor (Online)

1. Go to https://mermaid.live/
2. Copy your diagram code (without the ```mermaid wrapper)
3. Paste it into the editor
4. View and export the diagram

### Option 3: Create HTML Viewer (Local Solution)

I can create an HTML file that renders all your Mermaid diagrams locally.

### Option 4: Use the React Component in Your Web App

Since you now have Mermaid installed in your React app, you can create a documentation page that displays all your architecture diagrams.

## Current Status of Your Diagrams

Your `specs/ARCHITECTURE_DIAGRAMS.md` file contains 10 properly formatted Mermaid diagrams:

1. ✅ High-Level System Architecture
2. ✅ Detailed Application Architecture  
3. ✅ Data Architecture & Entity Relationships
4. ✅ Workflow Engine Architecture
5. ✅ Priority Calculation Engine
6. ✅ Security Architecture
7. ✅ Integration Architecture
8. ✅ Deployment Architecture
9. ✅ Monitoring & Observability Architecture
10. ✅ Data Flow Architecture

All diagrams are correctly formatted with:
```markdown
\`\`\`mermaid
graph TB
    subgraph "Client Layer"
        Web[React Web App]
        Mobile[Mobile Web]
        API_Client[API Clients]
    end
\`\`\`
```

## Recommended Action

**Install the Mermaid Preview extension** - this is the quickest and most effective solution for viewing your diagrams directly in your Markdown files.

Would you like me to create any of the alternative solutions?
