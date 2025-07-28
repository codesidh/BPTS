# Work Intake System

A comprehensive enterprise work intake management system designed for healthcare organizations, specifically built for Medicaid business verticals. The system provides advanced priority voting, workflow management, analytics, and external integrations.

## 🚀 Features

### Core Functionality
- **Work Request Management**: Create, track, and manage work requests with full lifecycle support
- **Priority Voting System**: Department-weighted voting with business value scoring and strategic alignment
- **Advanced Workflow Engine**: Configurable workflow stages with permissions and audit trails
- **Analytics Dashboard**: Real-time analytics, reporting, and trend analysis
- **Configuration Management**: Dynamic system configuration with versioning and approval workflows
- **Event Sourcing**: Complete audit trail and event replay capabilities

### Technical Features
- **Modern Architecture**: Clean Architecture with CQRS patterns
- **Real-time Updates**: WebSocket support for live updates
- **External Integrations**: Project management, calendar, and notification integrations
- **Security**: Azure AD authentication with role-based access control
- **Scalability**: Redis caching, database optimization, and horizontal scaling support
- **Monitoring**: Health checks, logging, and performance monitoring

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   React Frontend│    │   .NET 8 API    │    │   SQL Server    │
│                 │    │                 │    │                 │
│ - Priority      │◄──►│ - Controllers   │◄──►│ - WorkRequests  │
│   Voting        │    │ - Services      │    │ - Priorities    │
│ - Workflow      │    │ - Repositories  │    │ - Departments   │
│   Management    │    │ - Event Store   │    │ - Analytics     │
│ - Analytics     │    │ - Workflow      │    │ - Audit Trails  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                                ▼
                       ┌─────────────────┐
                       │   Redis Cache   │
                       │                 │
                       │ - Session Store │
                       │ - Analytics     │
                       │ - Rate Limiting │
                       └─────────────────┘
```

## 🛠️ Enterprise Technology Stack

### Core Technology Components

#### Frontend
- **React.js with TypeScript**: Modern UI development with type safety
- **Material-UI/Ant Design**: Professional enterprise component libraries
- **State Management**: Redux Toolkit with React Query for server state
- **Routing**: React Router for client-side navigation
- **Charts & Analytics**: Recharts for data visualization

#### Backend
- **.NET Core Web API**: Enterprise-grade API framework (IIS compatible)
- **Entity Framework Core**: Code-first ORM with SQL Server integration
- **AutoMapper**: Object-to-object mapping
- **FluentValidation**: Input validation and business rules
- **MediatR**: CQRS and mediator pattern implementation

#### Database & Storage
- **SQL Server 2019/2022**: Enterprise database with advanced features
- **Entity Framework Core**: Database access and migrations
- **SQL Server Service Broker**: Reliable messaging and queuing

#### Authentication & Security
- **Windows Authentication**: Integrated Windows security
- **LDAP Integration**: Active Directory integration for user management
- **ADFS Support**: Hybrid authentication for federated scenarios
- **Role-Based Access Control**: Granular permission management

#### Hosting & Infrastructure
- **IIS 10+**: Web server on Windows Server 2019/2022
- **Application Request Routing (ARR)**: Load balancing and reverse proxy
- **Windows Server 2019/2022**: Enterprise hosting platform
- **On-premises Deployment**: Full enterprise control and compliance

#### API Gateway & Management
- **On-premises API Management**: Centralized API governance
- **IIS ARR with Custom Modules**: Request routing and transformation
- **Rate Limiting & Throttling**: API protection and SLA enforcement
- **API Versioning**: Backward compatibility and evolution management
- **API Documentation**: Auto-generated Swagger/OpenAPI specifications

#### Caching Strategy (Multi-tier)
- **IIS Output Caching**: Static content and page-level caching with custom policies
- **Redis Distributed Cache**: Cross web farm session and data caching
- **Database Query Result Caching**: Entity Framework query optimization
- **Configuration Data Caching**: Dynamic settings with invalidation policies

#### Monitoring & Observability
- **ELK Stack**: Elasticsearch, Logstash, Kibana for log aggregation and analysis
- **Seq**: Structured logging alternative with rich querying
- **Application Performance Monitoring (APM)**: Performance tracking and alerting
- **Custom Logging**: Serilog with enterprise sinks and enrichers
- **Health Checks**: Comprehensive system health monitoring

#### CI/CD & DevOps
- **Jenkins**: Enterprise CI/CD automation platform
- **GitLab**: Source control and repository management
- **Automated Deployment**: Pipeline-driven deployments to Windows environments
- **Configuration Management**: Environment-specific settings and secrets

### Enhanced Architecture Components

#### API Gateway Layer
- **Centralized Authentication/Authorization**: Single security policy enforcement point
- **Rate Limiting and Throttling**: Backend service protection from overload
- **API Versioning**: Multiple API version support for backward compatibility
- **External System Integration Management**: Standardized integration patterns
- **Request/Response Transformation**: Data format adaptation for external systems

#### Event Sourcing Architecture
- **Event Store**: Immutable log of all system events with correlation tracking
- **Event Projections**: Read models for current state reconstruction
- **Workflow State Management**: Complete audit trail of workflow transitions
- **Replay Capabilities**: Event replay for debugging and state reconstruction
- **Snapshot Management**: Periodic snapshots for performance optimization

#### Enterprise Service Bus Pattern
- **Message Routing**: Intelligent message routing to appropriate services
- **Protocol Translation**: Multi-protocol communication support
- **Service Registry**: Dynamic service discovery and registration
- **Circuit Breaker**: Fault tolerance for external service calls
- **Message Transformation**: Data format conversion between systems

#### Reliability & Scalability
- **SQL Server Service Broker**: Reliable async messaging and queuing
- **Connection Pooling**: Optimized database connection management
- **Web Farm Support**: Horizontal scaling across multiple IIS servers
- **Session State Management**: Distributed session handling with Redis
- **Failover Support**: High availability and disaster recovery

## 📋 Enterprise Prerequisites

### Development Environment
- **.NET 8 SDK**: [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+**: [Download here](https://nodejs.org/)
- **Visual Studio 2022**: Enterprise or Professional edition recommended
- **SQL Server Management Studio (SSMS)**: Database management and development

### Enterprise Infrastructure
- **Windows Server 2019/2022**: Host server with appropriate licensing
- **IIS 10+**: Internet Information Services with ARR module
- **SQL Server 2019/2022**: Enterprise or Standard edition with Service Broker enabled
- **Active Directory**: Windows domain environment for authentication
- **Redis Server**: For distributed caching (Windows or Linux)

### Optional Enterprise Components
- **Jenkins**: CI/CD automation platform
- **GitLab**: Source control and repository management
- **ELK Stack**: Elasticsearch, Logstash, Kibana for logging (or Seq alternative)
- **Load Balancer**: Hardware or software load balancing solution
- **SSL Certificates**: For HTTPS and secure communications

## 🚀 Enterprise Deployment Guide

### Production IIS Deployment

1. **Prepare Windows Server Environment**
   ```powershell
   # Install IIS with ASP.NET Core Hosting Bundle
   Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole, IIS-WebServer, IIS-ApplicationDevelopment
   # Install ASP.NET Core Hosting Bundle
   # Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   ```

2. **Configure SQL Server**
   ```sql
   -- Enable Service Broker for message queuing
   ALTER DATABASE [WorkIntakeSystem] SET ENABLE_BROKER;
   
   -- Run Entity Framework migrations
   dotnet ef database update --project src/WorkIntakeSystem.Infrastructure
   ```

3. **Deploy Application**
   ```powershell
   # Publish the API
   dotnet publish src/WorkIntakeSystem.API -c Release -o "C:\inetpub\wwwroot\WorkIntakeAPI"
   
   # Build and deploy frontend
   cd src/WorkIntakeSystem.Web
   npm install
   npm run build
   # Copy dist folder to IIS web root
   ```

4. **Configure IIS**
   ```powershell
   # Create Application Pool
   New-WebAppPool -Name "WorkIntakeSystem" -Force
   Set-ItemProperty -Path "IIS:\AppPools\WorkIntakeSystem" -Name processModel.identityType -Value ApplicationPoolIdentity
   
   # Create Website
   New-Website -Name "WorkIntakeSystem" -Port 80 -PhysicalPath "C:\inetpub\wwwroot\WorkIntakeAPI" -ApplicationPool "WorkIntakeSystem"
   ```

### Development Environment Setup

1. **Local Development Database**
   ```bash
   # Update connection string in appsettings.Development.json
   # Enable Service Broker on local SQL Server
   dotnet ef database update --project src/WorkIntakeSystem.Infrastructure
   ```

2. **Start Development Services**
   ```bash
   # Start API (Backend)
   cd src/WorkIntakeSystem.API
   dotnet run --environment Development
   
   # Start React Frontend
   cd src/WorkIntakeSystem.Web
   npm install
   npm run dev
   ```

3. **Access Development Environment**
   - Frontend: http://localhost:3000
   - API: https://localhost:7000
   - Swagger: https://localhost:7000/swagger

## 🔧 Configuration

### Enterprise Configuration Settings

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string with Service Broker enabled | - |
| `ConnectionStrings__Redis` | Redis connection string for distributed caching | `localhost:6379` |
| `WindowsAuthentication__Enabled` | Enable Windows Authentication | `true` |
| `WindowsAuthentication__LdapServer` | LDAP server for user lookup | - |
| `WindowsAuthentication__Domain` | Windows domain name | - |
| `ADFS__Authority` | ADFS authority URL for hybrid authentication | - |
| `ADFS__ClientId` | ADFS client identifier | - |
| `ApiGateway__RateLimitPerMinute` | API rate limiting threshold | `1000` |
| `ApiGateway__EnableThrottling` | Enable request throttling | `true` |
| `Caching__IISOutputCacheEnabled` | Enable IIS output caching | `true` |
| `Caching__RedisDistributedEnabled` | Enable Redis distributed caching | `true` |
| `Caching__DatabaseQueryCacheMinutes` | Database query cache duration | `15` |
| `ServiceBroker__QueueName` | SQL Server Service Broker queue name | `WorkIntakeQueue` |
| `ServiceBroker__ServiceName` | Service Broker service name | `WorkIntakeService` |
| `Monitoring__ElkStackUrl` | ELK Stack endpoint for log shipping | - |
| `Monitoring__SeqUrl` | Seq server URL for structured logging | - |
| `Monitoring__APMEnabled` | Enable Application Performance Monitoring | `true` |

### Priority Calculation Settings

```json
{
  "PriorityCalculation": {
    "TimeDecayEnabled": true,
    "MaxTimeDecayMultiplier": 2.0,
    "BusinessValueWeight": 1.2,
    "CapacityAdjustmentEnabled": true
  }
}
```

### Workflow Configuration

```json
{
  "Workflow": {
    "AutoAdvanceEnabled": false,
    "NotificationEnabled": true,
    "SLAHours": 720
  }
}
```

## 🧪 Testing

### Run All Tests
```bash
# Backend tests
dotnet test src/WorkIntakeSystem.Tests

# Frontend tests
cd src/WorkIntakeSystem.Web
npm test
```

### Test Coverage
```bash
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 Monitoring & Health Checks

### Health Check Endpoints
- **Overall Health**: `GET /health`
- **Database**: `GET /health/db`
- **Redis**: `GET /health/redis`

### Logging
- **File Logs**: `logs/workintake-YYYY-MM-DD.txt`
- **Application Insights**: Configured for production
- **Structured Logging**: JSON format with correlation IDs

## 🔒 Security

### Authentication
- Azure AD integration with JWT tokens
- Role-based access control (RBAC)
- Multi-factor authentication support

### Authorization
- Department-level permissions
- Workflow stage permissions
- Configuration change approvals

### Data Protection
- HTTPS enforcement
- SQL injection prevention
- XSS protection
- CSRF protection
- Rate limiting

## 📈 Performance

### Caching Strategy
- **Redis**: Session storage, analytics cache, rate limiting
- **Application Cache**: In-memory caching for frequently accessed data
- **CDN**: Static asset caching for frontend

### Database Optimization
- Indexed queries for common operations
- Connection pooling
- Query optimization
- Read replicas for analytics

## 🚀 Production Deployment

### Azure Deployment

1. **Create Azure Resources**
   ```bash
   # Create resource group
   az group create --name workintake-rg --location eastus
   
   # Create App Service Plan
   az appservice plan create --name workintake-plan --resource-group workintake-rg --sku B1
   
   # Create Web App
   az webapp create --name workintake-api --resource-group workintake-rg --plan workintake-plan --runtime "DOTNETCORE|8.0"
   ```

2. **Configure Application Settings**
   ```bash
   az webapp config appsettings set --name workintake-api --resource-group workintake-rg --settings @appsettings.Production.json
   ```

3. **Deploy Application**
   ```bash
   az webapp deployment source config-zip --resource-group workintake-rg --name workintake-api --src ./publish.zip
   ```

### Kubernetes Deployment

1. **Create Kubernetes Cluster**
   ```bash
   # Using Azure AKS
   az aks create --resource-group workintake-rg --name workintake-cluster --node-count 3
   ```

2. **Deploy with Helm**
   ```bash
   helm install workintake ./helm-chart
   ```

## 🔧 Troubleshooting

### Common Issues

1. **Database Connection Issues**
   - Verify connection string
   - Check firewall rules
   - Ensure SQL Server is running

2. **Redis Connection Issues**
   - Verify Redis server is running
   - Check connection string format
   - Verify network connectivity

3. **Authentication Issues**
   - Verify Azure AD configuration
   - Check client ID and tenant ID
   - Ensure proper redirect URIs

### Logs Location
- **Application Logs**: `logs/workintake-YYYY-MM-DD.txt`
- **Docker Logs**: `docker-compose logs [service-name]`
- **Kubernetes Logs**: `kubectl logs [pod-name]`

## 📚 API Documentation

### Swagger UI
Access the interactive API documentation at `/swagger` when running the application.

### Key Endpoints

#### Work Requests
- `GET /api/workrequests` - Get all work requests
- `POST /api/workrequests` - Create new work request
- `PUT /api/workrequests/{id}` - Update work request
- `DELETE /api/workrequests/{id}` - Delete work request

#### Priority Voting
- `POST /api/priority/vote` - Submit priority vote
- `GET /api/priority/status/{id}` - Get voting status
- `GET /api/priority/pending/{departmentId}` - Get pending votes

#### Analytics
- `GET /api/analytics/dashboard` - Dashboard analytics
- `GET /api/analytics/department/{id}` - Department analytics
- `GET /api/analytics/workflow` - Workflow analytics

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the documentation

## 🎯 Architecture Alignment Status
Component	Specification	Implementation	Status
Frontend	React.js + TypeScript + Material-UI	✅ Aligned	Complete
Backend	.NET Core Web API (IIS compatible)	✅ .NET 8 API	Complete
Database	SQL Server 2019/2022 + EF Core	✅ With Service Broker	Complete
Authentication	Windows Auth + LDAP	✅ Full Implementation	Complete
Hosting	IIS 10+ on Windows Server	✅ Complete Config	Complete
API Gateway	On-premises API Management	✅ Custom Implementation	Complete
Caching	Multi-tier strategy	✅ Memory + Redis + IIS	Complete
Message Queue	SQL Server Service Broker	✅ Full Implementation	Complete
Event Store	Event sourcing for audit	✅ Existing + Enhanced	Complete


## 🗺️ Roadmap

### Phase 1: Foundation (Completed)
- ✅ Core domain models and database schema
- ✅ Basic CRUD operations for work requests, departments, and users
- ✅ Authentication and authorization (Azure AD integration)
- ✅ Executive dashboard with basic analytics
- ✅ Docker containerization and deployment setup

### Phase 2: Core Features (In Progress)
- ✅ **Enhanced Priority Voting System**
  - ✅ Department-weighted voting with business value scoring
  - ✅ Strategic alignment and resource impact assessment
  - ✅ Real-time priority calculation with time decay and capacity adjustment
  - ✅ Priority voting API endpoints and frontend integration
  - ✅ Priority recalculation service with configurable algorithms

- ✅ **Advanced Workflow Engine**
  - ✅ Configurable workflow stages (15 stages from Intake to Closure)
  - ✅ Permission-based stage transitions with role validation
  - ✅ Audit trail logging for all workflow changes
  - ✅ Event sourcing implementation for workflow state changes
  - ✅ Workflow advancement API with validation

- ✅ **Configuration Management System**
  - ✅ Dynamic system configuration with versioning
  - ✅ Effective/expiration dates and change tracking
  - ✅ Business vertical-specific configuration support
  - ✅ Configuration API with CRUD operations
  - ✅ Configuration service with fallback to appsettings

- ✅ **Event Sourcing & Audit Trail**
  - ✅ Event store implementation for all major actions
  - ✅ Comprehensive audit trail with security context
  - ✅ Event replay capabilities for aggregate reconstruction
  - ✅ Correlation IDs and causation tracking
  - ✅ Event store API for viewing and replaying events

### Phase 3: Advanced Features (In Progress)
- ✅ **Analytics & Reporting**
  - ✅ Real-time dashboard analytics
  - ✅ Department-specific analytics and workload tracking
  - ✅ Workflow analytics with bottleneck identification
  - ✅ Priority analytics and voting patterns
  - ✅ Team utilization and performance metrics

- ✅ **External Integrations**
  - ✅ Project management system integration framework
  - ✅ Calendar integration (Microsoft Graph)
  - ✅ Notification system integration
  - ✅ External system status monitoring
  - ✅ Integration logging and error handling

- ✅ **Frontend Enhancements**
  - ✅ Priority voting UI with real-time updates
  - ✅ Workflow management interface
  - ✅ Analytics dashboard with charts and metrics
  - ✅ Configuration management UI with versioning and approval workflows
  - ✅ Event/audit trail viewer with timeline and replay capabilities

### Phase 4: Enterprise Features (Completed)
- ✅ **Advanced Integrations**
  - ✅ Microsoft 365 deep integration (Teams, SharePoint, Power BI)
  - ✅ Azure DevOps/Jira project management sync
  - ✅ Advanced notification system with templates
  - ✅ Calendar scheduling and resource allocation

- ✅ **Enhanced Analytics & BI**
  - ✅ Advanced business intelligence dashboards
  - ✅ Predictive analytics for priority and workload
  - ✅ Custom report builder
  - ✅ Data export and integration capabilities

- ✅ **Mobile & Accessibility**
  - ✅ Mobile-responsive design optimization
  - ✅ Progressive Web App (PWA) features
  - ✅ Accessibility compliance (WCAG 2.1)
  - ✅ Offline capability for critical functions

### Phase 5: Enterprise Intelligence & Compliance
- 📋 **Machine Learning & AI** (High Priority)
  - 📋 Priority prediction using ML.NET and SQL Server ML Services
  - 📋 Workload optimization with existing analytics data
  - 📋 Automated workflow suggestions based on historical patterns
  - 📋 Anomaly detection integrated with Serilog and Service Broker alerting

- 📋 **Enterprise Architecture Enhancement** (Medium Priority)
  - 📋 Modular monolith design with clear service boundaries
  - 📋 Enhanced Service Broker patterns for inter-module communication
  - 📋 Circuit breaker patterns in API Gateway for fault tolerance
  - 📋 Advanced error handling and resilience patterns

- 📋 **Advanced Security & Compliance** (Critical Priority)
  - 📋 Enhanced RBAC with granular Active Directory group permissions
  - 📋 SQL Server Transparent Data Encryption (TDE) implementation
  - 📋 HIPAA/SOX compliance reporting dashboard
  - 📋 Advanced audit trail with real-time compliance monitoring
  - 📋 Data Loss Prevention (DLP) policies and monitoring

- 📋 **Enterprise Monitoring & Observability** (High Priority)
  - 📋 ELK Stack integration (Elasticsearch, Logstash, Kibana)
  - 📋 Application Performance Monitoring (APM) with Seq
  - 📋 Windows Performance Counters integration
  - 📋 Advanced health checks and alerting systems

### Phase 6: Enterprise Optimization & Scale
- 📋 **Performance & Enterprise Scalability** (High Priority)
  - 📋 Multi-server IIS deployment with ARR load balancing optimization
  - 📋 SQL Server Always On Availability Groups for high availability
  - 📋 Advanced query optimization and database performance tuning
  - 📋 Enterprise CDN or edge caching solutions for on-premises deployment

- 📋 **CI/CD & DevOps** (High Priority)
  - 📋 Jenkins CI/CD pipeline with GitLab integration
  - 📋 Automated IIS deployment and configuration management
  - 📋 Environment-specific configuration management
  - 📋 Automated testing and quality gates

- 📋 **Advanced Enterprise Features** (Medium Priority)
  - 📋 Real-time collaboration with SignalR and enterprise messaging
  - 📋 Advanced workflow automation and business rules engine
  - 📋 Enhanced business intelligence with healthcare-specific analytics
  - 📋 Advanced reporting with scheduled delivery and distribution

- 📋 **Enterprise Integration & Interoperability** (Medium Priority)
  - 📋 HL7 FHIR integration for healthcare data exchange
  - 📋 Enterprise Service Bus patterns with Service Broker
  - 📋 Legacy system integration adapters
  - 📋 Advanced API management and governance


## 🔄 Version History

- **v1.0.0** - Initial release with core functionality
- **v1.1.0** - Added priority voting system
- **v1.2.0** - Enhanced analytics and reporting
- **v1.3.0** - Production-ready with Docker support
- **v2.0.0** - Enterprise architecture transformation (Windows Server/IIS deployment)
- **v2.1.0** - Phase 5 & 6 roadmap aligned with enterprise requirements
