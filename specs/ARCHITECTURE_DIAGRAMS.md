# Architecture Diagrams
## Business Prioritization Tracking System (BPTS)

This document contains detailed architecture diagrams for the BPTS system.

---

## 1. High-Level System Architecture

```mermaid
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
```

---

## 2. Detailed Application Architecture

```mermaid
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
```

---

## 3. Data Architecture & Entity Relationships

```mermaid
erDiagram
    BusinessVertical ||--o{ Department : contains
    BusinessVertical ||--o{ WorkRequest : belongs_to
    BusinessVertical ||--o{ User : assigned_to
    
    Department ||--o{ User : employs
    Department ||--o{ WorkRequest : submits
    Department ||--o{ Priority : votes_on
    
    User ||--o{ WorkRequest : submits
    User ||--o{ Priority : creates
    User ||--o{ AuditTrail : performs
    
    WorkRequest ||--o{ Priority : has
    WorkRequest ||--o{ AuditTrail : generates
    WorkRequest ||--o{ EventStore : creates_events
    WorkRequest ||--o{ WorkflowTransition : transitions_through
    
    Priority ||--|| Department : voted_by
    Priority ||--|| WorkRequest : belongs_to
    
    EventStore ||--|| WorkRequest : aggregates
    AuditTrail ||--|| WorkRequest : tracks
    AuditTrail ||--|| User : performed_by
    
    BusinessCapability ||--o{ WorkRequest : categorizes
    BusinessCapability ||--o{ CapabilityDepartmentMapping : maps_to
    
    SystemConfiguration ||--o{ ConfigurationChangeRequest : changes_through
    WorkflowStageConfiguration ||--o{ WorkflowTransition : defines
    
    BusinessVertical {
        int Id PK
        string Name
        string Description
        bool IsActive
        datetime CreatedDate
    }
    
    Department {
        int Id PK
        string Name
        string Description
        int BusinessVerticalId FK
        decimal VotingWeight
        int ResourceCapacity
        decimal CurrentUtilization
    }
    
    User {
        int Id PK
        string Email
        string Name
        string PasswordHash
        int DepartmentId FK
        int BusinessVerticalId FK
        string Role
        int Capacity
        decimal CurrentWorkload
    }
    
    WorkRequest {
        int Id PK
        string Title
        string Description
        string Category
        int BusinessVerticalId FK
        int DepartmentId FK
        int SubmitterId FK
        datetime TargetDate
        string CurrentStage
        string Status
        decimal Priority
        decimal BusinessValue
        decimal TimeDecayFactor
        decimal CapacityAdjustment
    }
    
    Priority {
        int Id PK
        int WorkRequestId FK
        int DepartmentId FK
        decimal Vote
        string Comments
        datetime VotedDate
        int VotedBy FK
    }
    
    EventStore {
        int Id PK
        string AggregateId
        string EventType
        string EventData
        int EventVersion
        datetime Timestamp
        string CorrelationId
    }
    
    AuditTrail {
        int Id PK
        int WorkRequestId FK
        string Action
        string OldValue
        string NewValue
        int ChangedBy FK
        datetime ChangedDate
    }
```

---

## 4. Workflow Engine Architecture

```mermaid
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
```

---

## 5. Priority Calculation Engine

```mermaid
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
```

---

## 6. Security Architecture

```mermaid
graph TB
    subgraph "Client Security"
        HTTPS[HTTPS/TLS Encryption]
        JWT_Client[JWT Token Storage]
        XSS[XSS Protection]
        CSRF[CSRF Protection]
    end
    
    subgraph "API Gateway Security"
        RateLimit[Rate Limiting]
        Auth_Validation[Authentication Validation]
        Authorization[Authorization Check]
        Input_Validation[Input Validation]
    end
    
    subgraph "Application Security"
        JWT_Service[JWT Token Service]
        Password_Hash[Password Hashing]
        RBAC[Role-Based Access Control]
        Data_Encryption[Data Encryption]
    end
    
    subgraph "Database Security"
        Connection_Encrypt[Connection Encryption]
        Data_Encrypt[Data at Rest Encryption]
        Audit_Log[Audit Logging]
        Backup_Encrypt[Encrypted Backups]
    end
    
    subgraph "Infrastructure Security"
        Firewall[Network Firewall]
        VPN[VPN Access]
        SSL_Cert[SSL Certificates]
        Security_Headers[Security Headers]
    end
    
    HTTPS --> RateLimit
    JWT_Client --> Auth_Validation
    XSS --> Input_Validation
    CSRF --> Authorization
    
    RateLimit --> JWT_Service
    Auth_Validation --> Password_Hash
    Authorization --> RBAC
    Input_Validation --> Data_Encryption
    
    JWT_Service --> Connection_Encrypt
    Password_Hash --> Data_Encrypt
    RBAC --> Audit_Log
    Data_Encryption --> Backup_Encrypt
    
    Connection_Encrypt --> Firewall
    Data_Encrypt --> VPN
    Audit_Log --> SSL_Cert
    Backup_Encrypt --> Security_Headers
```

---

## 7. Integration Architecture

```mermaid
graph TB
    subgraph "Internal Services"
        API[Work Intake API]
        Workflow[Workflow Engine]
        Priority[Priority Engine]
        Analytics[Analytics Engine]
    end
    
    subgraph "Microsoft 365 Integration"
        Teams[Microsoft Teams]
        SharePoint[SharePoint]
        PowerBI[Power BI]
        Graph[Microsoft Graph API]
    end
    
    subgraph "Development Tools"
        AzureDevOps[Azure DevOps]
        Jira[Jira]
        GitLab[GitLab]
    end
    
    subgraph "Enterprise Service Bus"
        MessageRouter[Message Router]
        ProtocolTrans[Protocol Translation]
        CircuitBreaker[Circuit Breaker]
        MessageTransform[Message Transformation]
    end
    
    subgraph "External Systems"
        HR_System[HR System]
        Finance_System[Finance System]
        Legacy_System[Legacy Systems]
    end
    
    API --> MessageRouter
    Workflow --> MessageRouter
    Priority --> MessageRouter
    Analytics --> MessageRouter
    
    MessageRouter --> ProtocolTrans
    ProtocolTrans --> CircuitBreaker
    CircuitBreaker --> MessageTransform
    
    MessageTransform --> Teams
    MessageTransform --> SharePoint
    MessageTransform --> PowerBI
    MessageTransform --> Graph
    
    MessageTransform --> AzureDevOps
    MessageTransform --> Jira
    MessageTransform --> GitLab
    
    MessageTransform --> HR_System
    MessageTransform --> Finance_System
    MessageTransform --> Legacy_System
```

---

## 8. Deployment Architecture

```mermaid
graph TB
    subgraph "Load Balancer Tier"
        LB[IIS ARR Load Balancer]
        SSL[SSL Termination]
        HealthCheck[Health Check Monitor]
    end
    
    subgraph "Web Server Tier"
        Web1[IIS Web Server 1]
        Web2[IIS Web Server 2]
        Web3[IIS Web Server 3]
    end
    
    subgraph "Application Tier"
        API1[API Instance 1]
        API2[API Instance 2]
        API3[API Instance 3]
    end
    
    subgraph "Database Tier"
        PrimaryDB[(Primary SQL Server)]
        SecondaryDB[(Secondary SQL Server)]
        ReadReplica[(Read Replica)]
    end
    
    subgraph "Cache Tier"
        Redis1[(Redis Node 1)]
        Redis2[(Redis Node 2)]
        Redis3[(Redis Node 3)]
    end
    
    subgraph "Storage Tier"
        FileServer[File Server]
        BackupStorage[Backup Storage]
        LogStorage[Log Storage]
    end
    
    LB --> SSL
    SSL --> HealthCheck
    HealthCheck --> Web1
    HealthCheck --> Web2
    HealthCheck --> Web3
    
    Web1 --> API1
    Web2 --> API2
    Web3 --> API3
    
    API1 --> PrimaryDB
    API2 --> PrimaryDB
    API3 --> PrimaryDB
    
    PrimaryDB --> SecondaryDB
    PrimaryDB --> ReadReplica
    
    API1 --> Redis1
    API2 --> Redis2
    API3 --> Redis3
    
    Redis1 --> Redis2
    Redis2 --> Redis3
    Redis3 --> Redis1
    
    API1 --> FileServer
    API2 --> FileServer
    API3 --> FileServer
    
    PrimaryDB --> BackupStorage
    FileServer --> BackupStorage
    
    API1 --> LogStorage
    API2 --> LogStorage
    API3 --> LogStorage
```

---

## 9. Monitoring & Observability Architecture

```mermaid
graph TB
    subgraph "Application Layer"
        API[API Endpoints]
        Services[Business Services]
        Database[Database Queries]
    end
    
    subgraph "Logging Layer"
        StructuredLog[Structured Logging]
        CorrelationID[Correlation ID Tracking]
        PerformanceLog[Performance Logging]
        ErrorLog[Error Logging]
    end
    
    subgraph "Metrics Collection"
        APMMetrics[Application Metrics]
        BusinessMetrics[Business Metrics]
        SystemMetrics[System Metrics]
        CustomMetrics[Custom Metrics]
    end
    
    subgraph "Monitoring Stack"
        LogAggregator[Log Aggregator]
        MetricsDB[Metrics Database]
        AlertManager[Alert Manager]
        Dashboard[Monitoring Dashboard]
    end
    
    subgraph "External Monitoring"
        ELK[ELK Stack]
        Prometheus[Prometheus]
        Grafana[Grafana]
        Seq[Seq Logging]
    end
    
    API --> StructuredLog
    Services --> CorrelationID
    Database --> PerformanceLog
    
    StructuredLog --> APMMetrics
    CorrelationID --> BusinessMetrics
    PerformanceLog --> SystemMetrics
    ErrorLog --> CustomMetrics
    
    APMMetrics --> LogAggregator
    BusinessMetrics --> MetricsDB
    SystemMetrics --> AlertManager
    CustomMetrics --> Dashboard
    
    LogAggregator --> ELK
    MetricsDB --> Prometheus
    AlertManager --> Grafana
    Dashboard --> Seq
```

---

## 10. Data Flow Architecture

```mermaid
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
```

---

These diagrams provide a comprehensive visual representation of the BPTS system architecture, covering all major components, data flows, and integration points. Each diagram focuses on a specific aspect of the system to provide clear understanding of the overall design and implementation approach.

