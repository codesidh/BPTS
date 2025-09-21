import React from 'react';
import MermaidDiagram from './MermaidDiagram';

const ExampleMermaidUsage: React.FC = () => {
  // Example workflow diagram for your Work Intake System
  const workflowChart = `
    flowchart TD
      A[Work Request Submitted] --> B{Priority Assessment}
      B -->|High| C[Immediate Review]
      B -->|Medium| D[Standard Queue]
      B -->|Low| E[Backlog]
      C --> F[Resource Allocation]
      D --> F
      E --> F
      F --> G[Work Assignment]
      G --> H[In Progress]
      H --> I{Quality Check}
      I -->|Pass| J[Completed]
      I -->|Fail| K[Rework Required]
      K --> H
      J --> L[Closed]
  `;

  // Example system architecture diagram
  const architectureChart = `
    graph TB
      subgraph "Frontend"
        UI[React Web App]
        PWA[PWA Features]
      end
      
      subgraph "Backend Services"
        API[ASP.NET Core API]
        AUTH[Authentication Service]
        WF[Workflow Engine]
        PRIORITY[Priority Calculation]
      end
      
      subgraph "Data Layer"
        DB[(SQL Server Database)]
        CACHE[(Redis Cache)]
      end
      
      subgraph "External Systems"
        AD[Active Directory]
        TEAMS[Microsoft Teams]
        PBI[Power BI]
      end
      
      UI --> API
      PWA --> API
      API --> AUTH
      API --> WF
      API --> PRIORITY
      API --> DB
      API --> CACHE
      AUTH --> AD
      API --> TEAMS
      API --> PBI
  `;

  // Example Gantt chart for project timeline
  const ganttChart = `
    gantt
      title Work Intake System Development
      dateFormat YYYY-MM-DD
      section Phase 1
        Requirements Analysis    :done, phase1-req, 2024-01-01, 2024-01-15
        System Design          :done, phase1-design, 2024-01-16, 2024-01-30
      section Phase 2
        Core Development       :done, phase2-dev, 2024-02-01, 2024-03-15
        Testing               :done, phase2-test, 2024-03-01, 2024-03-20
      section Phase 3
        Integration           :active, phase3-int, 2024-03-21, 2024-04-10
        Deployment            :phase3-deploy, 2024-04-11, 2024-04-20
      section Phase 4
        User Training         :phase4-train, 2024-04-21, 2024-05-05
        Go Live              :milestone, go-live, 2024-05-06, 0d
  `;

  return (
    <div style={{ padding: '20px' }}>
      <h1>Mermaid Diagrams Examples</h1>
      
      <div style={{ marginBottom: '40px' }}>
        <h2>Work Request Workflow</h2>
        <MermaidDiagram chart={workflowChart} />
      </div>

      <div style={{ marginBottom: '40px' }}>
        <h2>System Architecture</h2>
        <MermaidDiagram chart={architectureChart} />
      </div>

      <div style={{ marginBottom: '40px' }}>
        <h2>Project Timeline</h2>
        <MermaidDiagram chart={ganttChart} />
      </div>
    </div>
  );
};

export default ExampleMermaidUsage;
