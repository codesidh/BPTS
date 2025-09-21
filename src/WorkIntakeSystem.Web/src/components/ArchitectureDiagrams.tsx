import React from 'react';
import MermaidDiagram from './MermaidDiagram';
import { Box, Typography, Paper, Divider } from '@mui/material';

const ArchitectureDiagrams: React.FC = () => {
  // High-Level System Architecture
  const systemArchitecture = `
graph TB
    subgraph "Client Layer"
        Web[React Web App]
        Mobile[Mobile Web]
        API_Client[API Clients]
    end
    
    subgraph "Presentation Layer"
        CDN[Content Delivery Network]
        LB[Load Balancer - IIS ARR]
    end
    
    subgraph "Application Layer"
        API[.NET 8 Web API]
        Gateway[API Gateway]
        Auth[Authentication Service]
    end
    
    subgraph "Business Layer"
        Domain[Domain Services]
        Workflow[Workflow Engine]
        Priority[Priority Engine]
        Analytics[Analytics Engine]
    end
    
    subgraph "Integration Layer"
        M365[Microsoft 365]
        DevOps[Azure DevOps]
        Jira[Jira]
        ESB[Enterprise Service Bus]
    end
    
    subgraph "Data Layer"
        DB[(SQL Server)]
        Cache[(Redis Cache)]
        Files[File Storage]
    end
    
    subgraph "Infrastructure Layer"
        IIS[IIS Web Server]
        Monitoring[Monitoring & Logging]
        Backup[Backup & Recovery]
    end
    
    Web --> CDN
    Mobile --> CDN
    API_Client --> LB
    CDN --> LB
    LB --> API
    API --> Gateway
    Gateway --> Auth
    API --> Domain
    Domain --> Workflow
    Domain --> Priority
    Domain --> Analytics
    API --> M365
    API --> DevOps
    API --> Jira
    API --> ESB
    Domain --> DB
    Domain --> Cache
    API --> Files
    API --> IIS
    IIS --> Monitoring
    DB --> Backup
  `;

  // Application Architecture
  const applicationArchitecture = `
graph TB
    subgraph "Frontend - React Application"
        UI[User Interface Components]
        State[State Management - Redux]
        API_Client[API Client Layer]
        Router[React Router]
    end
    
    subgraph "API Gateway Layer"
        RateLimit[Rate Limiting]
        Auth_Gateway[Authentication Gateway]
        Versioning[API Versioning]
        Transform[Request/Response Transformation]
    end
    
    subgraph ".NET 8 Web API"
        Controllers[API Controllers]
        Middleware[Middleware Pipeline]
        Validation[Input Validation]
        Swagger[API Documentation]
    end
    
    subgraph "Business Logic Layer"
        Services[Domain Services]
        Workflow_Svc[Workflow Service]
        Priority_Svc[Priority Service]
        Analytics_Svc[Analytics Service]
        Config_Svc[Configuration Service]
    end
    
    subgraph "Data Access Layer"
        Repositories[Repository Pattern]
        UnitOfWork[Unit of Work]
        EF_Core[Entity Framework Core]
        Migrations[Database Migrations]
    end
    
    subgraph "Event Sourcing"
        EventStore[Event Store]
        EventHandlers[Event Handlers]
        Projections[Event Projections]
        Snapshots[Event Snapshots]
    end
    
    UI --> State
    State --> API_Client
    API_Client --> RateLimit
    RateLimit --> Auth_Gateway
    Auth_Gateway --> Versioning
    Versioning --> Transform
    Transform --> Controllers
    Controllers --> Middleware
    Middleware --> Validation
    Controllers --> Services
    Services --> Workflow_Svc
    Services --> Priority_Svc
    Services --> Analytics_Svc
    Services --> Config_Svc
    Services --> Repositories
    Repositories --> UnitOfWork
    UnitOfWork --> EF_Core
    Services --> EventStore
    EventStore --> EventHandlers
    EventHandlers --> Projections
  `;

  // Data Flow Sequence
  const dataFlow = `
sequenceDiagram
    participant U as User
    participant W as Web App
    participant G as API Gateway
    participant A as API Controller
    participant S as Service Layer
    participant R as Repository
    participant D as Database
    participant E as Event Store
    participant C as Cache
    
    U->>W: Submit Work Request
    W->>G: API Call with JWT Token
    G->>G: Validate Token & Rate Limit
    G->>A: Forward Request
    A->>S: Process Work Request
    S->>R: Save to Database
    R->>D: Insert Work Request
    D-->>R: Return Success
    R-->>S: Return Entity
    S->>E: Store Domain Event
    E-->>S: Event Stored
    S->>C: Update Cache
    C-->>S: Cache Updated
    S-->>A: Return Response
    A-->>G: API Response
    G-->>W: Forward Response
    W-->>U: Display Success
  `;

  // Workflow Engine
  const workflowEngine = `
graph TD
    subgraph "Workflow Configuration"
        WSC[Workflow Stage Configuration]
        WTC[Workflow Transition Configuration]
        Validation[Validation Rules]
        SLA[SLA Configuration]
    end
    
    subgraph "Workflow Engine Core"
        WSM[Workflow State Machine]
        WE[Workflow Engine Service]
        Validator[Transition Validator]
        Notifier[Notification Service]
    end
    
    subgraph "Event Processing"
        EventStore[Event Store]
        EventHandlers[Event Handlers]
        Projections[State Projections]
    end
    
    subgraph "External Integrations"
        Email[Email Service]
        Teams[Teams Integration]
        Calendar[Calendar Integration]
    end
    
    WSC --> WSM
    WTC --> WSM
    Validation --> Validator
    SLA --> WE
    
    WE --> WSM
    WE --> Validator
    WE --> Notifier
    
    WSM --> EventStore
    EventStore --> EventHandlers
    EventHandlers --> Projections
    
    Notifier --> Email
    Notifier --> Teams
    Notifier --> Calendar
    
    style WSM fill:#e1f5fe
    style WE fill:#f3e5f5
    style EventStore fill:#e8f5e8
  `;

  // Priority Calculation Engine
  const priorityEngine = `
graph TD
    subgraph "Input Data"
        Votes[Department Votes]
        Config[Priority Configuration]
        WorkRequest[Work Request Data]
        TimeFactors[Time Decay Factors]
        Capacity[Resource Capacity]
    end
    
    subgraph "Calculation Engine"
        BaseCalc[Base Score Calculator]
        TimeDecay[Time Decay Calculator]
        BusinessValue[Business Value Calculator]
        CapacityAdj[Capacity Adjustment Calculator]
        Normalizer[Score Normalizer]
    end
    
    subgraph "Output"
        PriorityScore[Final Priority Score]
        PriorityLevel[Priority Level Classification]
        Recommendations[Priority Recommendations]
    end
    
    Votes --> BaseCalc
    Config --> BaseCalc
    Config --> TimeDecay
    Config --> BusinessValue
    Config --> CapacityAdj
    
    WorkRequest --> TimeDecay
    WorkRequest --> BusinessValue
    TimeFactors --> TimeDecay
    Capacity --> CapacityAdj
    
    BaseCalc --> Normalizer
    TimeDecay --> Normalizer
    BusinessValue --> Normalizer
    CapacityAdj --> Normalizer
    
    Normalizer --> PriorityScore
    PriorityScore --> PriorityLevel
    PriorityScore --> Recommendations
    
    style BaseCalc fill:#e3f2fd
    style Normalizer fill:#f1f8e9
    style PriorityScore fill:#fff3e0
  `;

  return (
    <Box sx={{ padding: 3, maxWidth: 1200, margin: '0 auto' }}>
      <Typography variant="h3" component="h1" gutterBottom align="center" sx={{ mb: 4 }}>
        BPTS Architecture Diagrams
      </Typography>

      <Paper elevation={3} sx={{ p: 3, mb: 4 }}>
        <Typography variant="h4" component="h2" gutterBottom color="primary">
          1. High-Level System Architecture
        </Typography>
        <MermaidDiagram chart={systemArchitecture} />
      </Paper>

      <Divider sx={{ my: 4 }} />

      <Paper elevation={3} sx={{ p: 3, mb: 4 }}>
        <Typography variant="h4" component="h2" gutterBottom color="primary">
          2. Detailed Application Architecture
        </Typography>
        <MermaidDiagram chart={applicationArchitecture} />
      </Paper>

      <Divider sx={{ my: 4 }} />

      <Paper elevation={3} sx={{ p: 3, mb: 4 }}>
        <Typography variant="h4" component="h2" gutterBottom color="primary">
          3. Data Flow Architecture
        </Typography>
        <MermaidDiagram chart={dataFlow} />
      </Paper>

      <Divider sx={{ my: 4 }} />

      <Paper elevation={3} sx={{ p: 3, mb: 4 }}>
        <Typography variant="h4" component="h2" gutterBottom color="primary">
          4. Workflow Engine Architecture
        </Typography>
        <MermaidDiagram chart={workflowEngine} />
      </Paper>

      <Divider sx={{ my: 4 }} />

      <Paper elevation={3} sx={{ p: 3, mb: 4 }}>
        <Typography variant="h4" component="h2" gutterBottom color="primary">
          5. Priority Calculation Engine
        </Typography>
        <MermaidDiagram chart={priorityEngine} />
      </Paper>
    </Box>
  );
};

export default ArchitectureDiagrams;
