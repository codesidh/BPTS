# Mermaid Usage Guide for Work Intake System

## Installation Complete ✅

Mermaid has been successfully installed in your React application with the following packages:
- `mermaid@^11.12.0` - Core Mermaid library
- `@types/mermaid@^9.1.0` - TypeScript type definitions

## Components Created

### 1. MermaidDiagram Component
Location: `src/components/MermaidDiagram.tsx`

A reusable React component that renders Mermaid diagrams from text definitions.

**Props:**
- `chart: string` - The Mermaid diagram definition
- `id?: string` - Optional unique ID (auto-generated if not provided)
- `className?: string` - Optional CSS classes

### 2. Example Usage Component
Location: `src/components/ExampleMermaidUsage.tsx`

Demonstrates various Mermaid diagram types relevant to your Work Intake System.

## How to Use

### Basic Usage

```tsx
import MermaidDiagram from './components/MermaidDiagram';

const MyComponent = () => {
  const flowChart = `
    flowchart TD
      A[Start] --> B[Process]
      B --> C[End]
  `;

  return <MermaidDiagram chart={flowChart} />;
};
```

### Integration with Your App

To add the example component to your app, you can import and use it in any of your existing components:

```tsx
import ExampleMermaidUsage from './components/ExampleMermaidUsage';

// In your router or main component
<Route path="/diagrams" element={<ExampleMermaidUsage />} />
```

## Diagram Types Available

### 1. Flowcharts
Perfect for workflow visualization:
```
flowchart TD
  A[Work Request] --> B{Priority?}
  B -->|High| C[Fast Track]
  B -->|Low| D[Standard Queue]
```

### 2. System Architecture
Great for technical documentation:
```
graph TB
  Frontend --> API
  API --> Database
  API --> Cache
```

### 3. Gantt Charts
Ideal for project timelines:
```
gantt
  title Project Timeline
  dateFormat YYYY-MM-DD
  section Phase 1
    Task 1 :done, 2024-01-01, 2024-01-15
    Task 2 :active, 2024-01-16, 2024-01-30
```

### 4. Sequence Diagrams
Perfect for API interactions:
```
sequenceDiagram
  participant User
  participant API
  participant DB
  User->>API: Submit Request
  API->>DB: Save Request
  DB-->>API: Confirmation
  API-->>User: Success Response
```

### 5. Class Diagrams
Great for code architecture:
```
classDiagram
  class WorkRequest {
    +int id
    +string title
    +Priority priority
    +submit()
    +approve()
  }
```

### 6. State Diagrams
Perfect for workflow states:
```
stateDiagram-v2
  [*] --> Submitted
  Submitted --> InReview
  InReview --> Approved
  InReview --> Rejected
  Approved --> InProgress
  InProgress --> Completed
  Completed --> [*]
```

## Styling and Themes

Mermaid supports various themes. You can customize the theme in the MermaidDiagram component:

```tsx
mermaid.initialize({
  startOnLoad: false,
  theme: 'default', // 'default', 'dark', 'forest', 'neutral'
  securityLevel: 'loose',
  fontFamily: 'Arial, sans-serif'
});
```

## Advanced Features

### Custom CSS
You can add custom CSS classes to style your diagrams:

```tsx
<MermaidDiagram 
  chart={myChart} 
  className="my-custom-diagram-style" 
/>
```

### Dynamic Diagrams
Create diagrams based on your application data:

```tsx
const createWorkflowDiagram = (workRequest: WorkRequest) => {
  return `
    flowchart TD
      A[${workRequest.title}] --> B[Priority: ${workRequest.priority}]
      B --> C[Assigned to: ${workRequest.assignee}]
  `;
};
```

## Integration with Your Work Intake System

Consider using Mermaid for:

1. **Workflow Visualization**: Show the current state and next steps for work requests
2. **System Documentation**: Create architecture diagrams for your technical documentation
3. **Project Planning**: Use Gantt charts for project timelines
4. **Process Flow**: Document your approval and escalation processes
5. **User Journey**: Map out user interactions with your system

## Next Steps

1. Import the `MermaidDiagram` component in your existing components
2. Create diagrams specific to your business processes
3. Consider adding diagram editing capabilities for administrators
4. Integrate with your data to create dynamic diagrams

## Resources

- [Mermaid Documentation](https://mermaid.js.org/)
- [Mermaid Live Editor](https://mermaid.live/) - Test your diagrams online
- [Diagram Syntax Reference](https://mermaid.js.org/intro/)

## Troubleshooting

If you encounter any issues:
1. Ensure the chart syntax is valid using the Mermaid Live Editor
2. Check browser console for any JavaScript errors
3. Verify that the component is properly imported and used
4. Make sure the chart string doesn't contain any invalid characters

Happy diagramming! 🎨📊
