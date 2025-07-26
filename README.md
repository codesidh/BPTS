# Work Intake System

A comprehensive enterprise web application designed to manage work intake, prioritization, and tracking across multiple healthcare business verticals with integrated Microsoft 365 authentication and role-based access control.

## 🏗️ Architecture Overview

### Technology Stack

**Backend:**
- .NET 8 Core Web API
- Entity Framework Core with SQL Server
- Windows Authentication + LDAP integration
- AutoMapper for object mapping
- Serilog for structured logging
- Redis for distributed caching
- FluentValidation for request validation

**Frontend:**
- React 18 with TypeScript
- Material-UI (MUI) for component library
- React Query for state management and caching
- React Router for navigation
- Recharts for data visualization
- Axios for API communication

**Database:**
- SQL Server 2019/2022
- Entity Framework Core Code-First approach
- Comprehensive audit trails
- Event sourcing implementation

### Key Features

- ✅ **Enhanced Priority System**: Advanced priority calculation with time decay, business value, and capacity adjustment
- ✅ **Multi-tier Workflow**: 15-stage configurable workflow from intake to closure
- ✅ **Role-based Access Control**: 6-level user hierarchy with granular permissions
- ✅ **Event Sourcing**: Complete audit trail with event replay capabilities
- ✅ **Real-time Dashboard**: Executive and department-specific KPI tracking
- ✅ **Priority Voting System**: Department-weighted voting with business value assessment
- ✅ **Configuration Management**: Version-controlled system configuration
- ✅ **Multi-tenant Support**: Business vertical isolation and configuration

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- SQL Server 2019+ or SQL Server LocalDB
- Redis (optional, for caching)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd work-intake-system
   ```

2. **Setup Backend**
   ```bash
   cd src/WorkIntakeSystem.API
   dotnet restore
   dotnet build
   ```

3. **Setup Database**
   ```bash
   # Update connection string in appsettings.json
   dotnet ef database update
   ```

4. **Setup Frontend**
   ```bash
   cd src/WorkIntakeSystem.Web
   npm install
   ```

### Running the Application

1. **Start the API**
   ```bash
   cd src/WorkIntakeSystem.API
   dotnet run
   ```
   API will be available at `https://localhost:7000`

2. **Start the Frontend**
   ```bash
   cd src/WorkIntakeSystem.Web
   npm run dev
   ```
   Frontend will be available at `http://localhost:3000`

## 📊 System Architecture

### Domain Models

#### Core Entities

- **WorkRequest**: Central entity managing work items through the workflow
- **Priority**: Enhanced voting system with weighted calculations
- **BusinessVertical**: Configurable business units (Medicaid, Medicare, Exchange, etc.)
- **Department**: Organizational units with voting weights and resource tracking
- **User**: Role-based user management with skill tracking
- **BusinessCapability**: Hierarchical capability management
- **SystemConfiguration**: Versioned configuration with change tracking
- **EventStore**: Event sourcing for complete audit trails
- **AuditTrail**: Enhanced audit logging with correlation IDs

#### Priority Calculation Algorithm

```
Enhanced Priority Score = Base_Score × Time_Decay_Factor × Business_Value_Weight × Capacity_Adjustment

Where:
- Base_Score = Σ(Department_Weight × Department_Vote) / Total_Departments
- Time_Decay_Factor = 1 + log(days_old + 1) / 100 (capped at 2.0)
- Business_Value_Weight = 1.0 + business_value (range: 1.0-2.0)
- Capacity_Adjustment = 1.5 - (utilization / 100) (range: 0.5-1.5)
```

### Workflow Stages

1. **Intake** - Initial submission
2. **Business Review** - Department evaluation
3. **Priority Assessment** - Cross-department voting
4. **Work Request Creation** - Create and assign to IS Organization
5. **Architecture Assessment** - Feasibility and estimation
6. **Estimation** - IS Department work estimation
7. **Approval** - Leadership and SLT sign-off
8. **Budget Approval** - Budget evaluation and approval
9. **Planning** - Resource allocation and scheduling
10. **Requirements** - Requirement analysis & documentation
11. **Development** - Implementation phase
12. **Testing** - Quality assurance
13. **UAT** - Business review and sign-off
14. **Deployment** - Production release
15. **Closure** - Final review and documentation

### User Roles

1. **System Administrator** - Full system access and configuration
2. **Business Executive** - Cross-department visibility and strategic reporting
3. **Department Head** - Department oversight and approval authority
4. **Department Manager** - Team management and priority voting
5. **Lead** - Technical assessment and development oversight
6. **End User** - Work request submission and status viewing

## 🔧 Configuration

### Database Configuration

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WorkIntakeSystemDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Authentication

The system supports Windows Authentication by default. Configure in `appsettings.json`:

```json
{
  "Authentication": {
    "WindowsAuthentication": {
      "Enabled": true
    }
  }
}
```

### Caching

Redis configuration for distributed caching:

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "Caching": {
    "DefaultExpirationMinutes": 30,
    "PriorityCalculationCacheMinutes": 15
  }
}
```

## 📈 Dashboard Features

### Executive Dashboard
- Total active requests tracking
- Average completion time metrics
- SLA compliance monitoring
- Resource utilization analytics
- Priority distribution charts
- Work category breakdowns
- Recent activity feeds

### Department Dashboard
- Department-specific work queues
- Voting status and deadlines
- Resource allocation views
- Team performance metrics

## 🔄 API Endpoints

### Work Requests
- `GET /api/workrequests` - Get all active work requests
- `GET /api/workrequests/{id}` - Get specific work request
- `POST /api/workrequests` - Create new work request
- `PUT /api/workrequests/{id}` - Update work request
- `DELETE /api/workrequests/{id}` - Soft delete work request
- `POST /api/workrequests/{id}/recalculate-priority` - Recalculate priority

### Priority Voting
- `POST /api/priorities` - Submit priority vote
- `PUT /api/priorities/{id}` - Update priority vote
- `GET /api/workrequests/pending-votes/{departmentId}` - Get pending votes

### Configuration
- `GET /api/departments` - Get all departments
- `GET /api/businessverticals` - Get all business verticals
- `GET /api/users` - Get all users

## 🔒 Security Features

- Windows Authentication integration
- Role-based authorization
- CORS configuration for frontend
- SQL injection prevention through Entity Framework
- Audit trails for all changes
- Session tracking and IP logging

## 📊 Performance Optimization

- Multi-tier caching strategy
- Database query optimization
- Connection pooling
- Lazy loading for navigation properties
- Pagination for large datasets
- Background processing for priority calculations

## 🧪 Testing

### Backend Testing
```bash
cd src/WorkIntakeSystem.Tests
dotnet test
```

### Frontend Testing
```bash
cd src/WorkIntakeSystem.Web
npm test
```

## 📦 Deployment

### Development
- IIS Express for API
- Vite dev server for frontend

### Production
- IIS 10+ on Windows Server
- SQL Server with Always On Availability Groups
- Redis cluster for caching
- Load balancing with IIS ARR

### Environment Variables

Set the following environment variables for production:

```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<production-db-connection>
ConnectionStrings__Redis=<redis-connection>
```

## 🔍 Monitoring and Logging

- Structured logging with Serilog
- Application Performance Monitoring (APM)
- Health check endpoints
- Custom performance counters
- Real-time dashboard metrics
- Error tracking and alerting

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:
- Create an issue in the GitHub repository
- Contact the development team
- Check the documentation wiki

## 🗺️ Roadmap

### Phase 1: Foundation (Completed)
- ✅ Core domain models and database schema
- ✅ Basic CRUD operations
- ✅ Authentication and authorization
- ✅ Executive dashboard

### Phase 2: Core Features (In Progress)
- 🔄 Enhanced priority voting system
- 🔄 Advanced workflow engine
- 🔄 Configuration management
- 🔄 Event sourcing implementation

### Phase 3: Advanced Features (Planned)
- 📋 Microsoft 365 integration
- 📋 Advanced analytics and reporting
- 📋 Mobile responsiveness
- 📋 External system integrations

### Phase 4: Enterprise Features (Future)
- 📋 Machine learning for priority prediction
- 📋 Advanced business intelligence
- 📋 API gateway implementation
- 📋 Microservices architecture

---

**Built with ❤️ for enterprise healthcare work management**
