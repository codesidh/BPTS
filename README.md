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

## 🛠️ Technology Stack

### Backend
- **.NET 8**: Latest LTS version with performance improvements
- **Entity Framework Core**: Modern ORM with code-first approach
- **SQL Server**: Enterprise-grade database
- **Redis**: High-performance caching and session storage
- **Serilog**: Structured logging with multiple sinks
- **AutoMapper**: Object mapping and transformation
- **FluentValidation**: Input validation and business rules

### Frontend
- **React 18**: Modern UI library with hooks and concurrent features
- **TypeScript**: Type-safe JavaScript development
- **Material-UI**: Professional component library
- **React Query**: Server state management and caching
- **React Router**: Client-side routing
- **Recharts**: Data visualization and analytics charts

### DevOps & Infrastructure
- **Docker**: Containerization for consistent deployments
- **Docker Compose**: Multi-service orchestration
- **Azure AD**: Enterprise authentication and authorization
- **Health Checks**: Application monitoring and alerting
- **Rate Limiting**: API protection and throttling

## 📋 Prerequisites

- **.NET 8 SDK**: [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+**: [Download here](https://nodejs.org/)
- **SQL Server**: Local or cloud instance
- **Redis**: Local or cloud instance
- **Docker & Docker Compose**: For containerized deployment

## 🚀 Quick Start

### Option 1: Docker Compose (Recommended)

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd WorkIntakeSystem
   ```

2. **Configure environment variables**
   ```bash
   cp .env.example .env
   # Edit .env with your configuration
   ```

3. **Start the system**
   ```bash
   docker-compose up -d
   ```

4. **Access the application**
   - Frontend: http://localhost:3000
   - API: http://localhost:5000
   - Swagger: http://localhost:5000/swagger

### Option 2: Local Development

1. **Setup Database**
   ```bash
   # Update connection string in appsettings.Development.json
   # Run migrations
   dotnet ef database update --project src/WorkIntakeSystem.Infrastructure
   ```

2. **Start Backend**
   ```bash
   cd src/WorkIntakeSystem.API
   dotnet run
   ```

3. **Start Frontend**
   ```bash
   cd src/WorkIntakeSystem.Web
   npm install
   npm run dev
   ```

## 🔧 Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | - |
| `ConnectionStrings__Redis` | Redis connection string | `localhost:6379` |
| `AzureAd__TenantId` | Azure AD tenant ID | - |
| `AzureAd__ClientId` | Azure AD client ID | - |
| `AzureAd__Audience` | Azure AD audience | - |

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

## 🔄 Version History

- **v1.0.0** - Initial release with core functionality
- **v1.1.0** - Added priority voting system
- **v1.2.0** - Enhanced analytics and reporting
- **v1.3.0** - Production-ready with Docker support
