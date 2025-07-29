# User Roles and Permissions Guide

## Overview

The Work Intake System implements a hierarchical role-based access control (RBAC) system designed to provide granular permissions while maintaining organizational hierarchy. Each role inherits permissions from lower-level roles and adds role-specific capabilities.

## Role Hierarchy

```
SystemAdministrator (Level 6)
    ↓
BusinessExecutive (Level 5)
    ↓
Director (Level 4)
    ↓
Manager (Level 3)
    ↓
Lead (Level 2)
    ↓
EndUser (Level 1)
```

## Detailed Role Descriptions

### 1. EndUser (Level 1)
**Base Role - Basic System User**

**Primary Responsibilities:**
- Create and submit work requests
- View and track own work requests
- Participate in priority voting
- Access basic system features

**Permissions:**
- ✅ Create work requests
- ✅ View own work requests and status
- ✅ Edit own work requests (before approval)
- ✅ Participate in priority voting
- ✅ View basic reports and dashboards
- ✅ Update personal profile
- ✅ Access help and documentation

**Restrictions:**
- ❌ Cannot approve work requests
- ❌ Cannot view other users' requests
- ❌ Cannot access administrative functions
- ❌ Cannot modify system configuration

---

### 2. Lead (Level 2)
**Team Leadership Role**

**Primary Responsibilities:**
- Manage team workload and priorities
- Approve team member work requests
- Coordinate team activities
- Provide team-level reporting

**Permissions:**
- ✅ All EndUser permissions
- ✅ Approve/reject team member work requests
- ✅ View team member work requests
- ✅ Manage team workload distribution
- ✅ Access team-level analytics and reports
- ✅ Assign work requests to team members
- ✅ Create team-specific reports
- ✅ Manage team priorities

**Restrictions:**
- ❌ Cannot approve cross-team requests
- ❌ Cannot access department-wide administrative functions
- ❌ Cannot modify system-wide configuration

---

### 3. Manager (Level 3)
**Department Management Role**

**Primary Responsibilities:**
- Manage department resources and priorities
- Approve department-wide work requests
- Coordinate cross-team initiatives
- Provide department-level reporting

**Permissions:**
- ✅ All Lead permissions
- ✅ Approve/reject department-wide work requests
- ✅ View all department work requests
- ✅ Manage department resource allocation
- ✅ Access department-level analytics and reports
- ✅ Coordinate cross-team initiatives
- ✅ Manage department priorities and budgets
- ✅ Create department-specific reports
- ✅ Manage department user accounts

**Restrictions:**
- ❌ Cannot approve cross-department requests
- ❌ Cannot access business unit administrative functions
- ❌ Cannot modify system-wide security settings

---

### 4. Director (Level 4)
**Business Unit Leadership Role**

**Primary Responsibilities:**
- Manage business unit strategy and priorities
- Approve cross-department initiatives
- Coordinate business unit resources
- Provide business unit reporting

**Permissions:**
- ✅ All Manager permissions
- ✅ Approve/reject cross-department work requests
- ✅ View all business unit work requests
- ✅ Manage business unit resource allocation
- ✅ Access business unit analytics and reports
- ✅ Coordinate cross-department initiatives
- ✅ Manage business unit priorities and budgets
- ✅ Create business unit reports
- ✅ Manage business unit user accounts
- ✅ Access strategic planning tools

**Restrictions:**
- ❌ Cannot approve enterprise-wide initiatives
- ❌ Cannot access system administration functions
- ❌ Cannot modify system security policies

---

### 5. BusinessExecutive (Level 5)
**Executive Leadership Role**

**Primary Responsibilities:**
- Define business strategy and priorities
- Approve enterprise-wide initiatives
- Manage enterprise resources
- Provide executive reporting

**Permissions:**
- ✅ All Director permissions
- ✅ Approve/reject enterprise-wide work requests
- ✅ View all enterprise work requests
- ✅ Manage enterprise resource allocation
- ✅ Access enterprise analytics and reports
- ✅ Coordinate enterprise-wide initiatives
- ✅ Manage enterprise priorities and budgets
- ✅ Create executive reports
- ✅ Access strategic planning and forecasting tools
- ✅ Manage enterprise user accounts
- ✅ Access business intelligence dashboards

**Restrictions:**
- ❌ Cannot access system administration functions
- ❌ Cannot modify system security policies
- ❌ Cannot access technical system configuration

---

### 6. SystemAdministrator (Level 6)
**System Administration Role**

**Primary Responsibilities:**
- Manage system configuration and security
- Administer user accounts and permissions
- Monitor system performance and health
- Maintain system compliance and audit trails

**Permissions:**
- ✅ All BusinessExecutive permissions
- ✅ Full system access and configuration
- ✅ Manage all user accounts and roles
- ✅ Configure system settings and policies
- ✅ Access system logs and audit trails
- ✅ Manage system security policies
- ✅ Monitor system performance and health
- ✅ Create and manage system backups
- ✅ Access system administration tools
- ✅ Manage system integrations
- ✅ Configure workflow rules and processes
- ✅ Access system diagnostic tools

**No Restrictions:**
- 🔓 Full system access and control

## Permission Matrix

| Feature | EndUser | Lead | Manager | Director | BusinessExecutive | SystemAdministrator |
|---------|---------|------|---------|----------|-------------------|-------------------|
| **Work Request Management** |
| Create work requests | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| View own requests | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| View team requests | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |
| View department requests | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ |
| View business unit requests | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ |
| View all requests | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ |
| Approve team requests | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Approve department requests | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ |
| Approve business unit requests | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ |
| Approve enterprise requests | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ |
| **User Management** |
| View own profile | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Update own profile | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| View team members | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |
| View department users | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ |
| View business unit users | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ |
| View all users | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ |
| Manage user accounts | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ |
| **Analytics & Reporting** |
| Basic reports | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Team reports | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Department reports | ❌ | ❌ | ✅ | ✅ | ✅ | ✅ |
| Business unit reports | ❌ | ❌ | ❌ | ✅ | ✅ | ✅ |
| Executive reports | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ |
| System reports | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **System Administration** |
| System configuration | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| Security policies | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| User role management | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| System monitoring | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| Audit logs | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |

## Implementation Details

### Role Assignment
- Roles are assigned during user registration or by system administrators
- Users can only be assigned one role at a time
- Role changes require appropriate permissions
- Role changes are logged in the audit trail

### Permission Inheritance
- Each role automatically inherits all permissions from lower-level roles
- Role-specific permissions are additive
- No role can have fewer permissions than a lower-level role

### Security Considerations
- Role-based access is enforced at both API and UI levels
- All role changes are audited and logged
- Role permissions are cached for performance
- JWT tokens include role information for authorization

### Audit Trail
- All role changes are logged with timestamp and user information
- Permission usage is tracked for compliance
- Failed permission attempts are logged for security monitoring

## Best Practices

### Role Assignment
1. **Principle of Least Privilege**: Assign the minimum role necessary for job function
2. **Regular Review**: Periodically review role assignments for appropriateness
3. **Documentation**: Maintain clear documentation of role responsibilities
4. **Training**: Ensure users understand their role permissions and limitations

### Security
1. **Regular Audits**: Conduct regular audits of role assignments and permissions
2. **Access Reviews**: Implement periodic access reviews for sensitive roles
3. **Monitoring**: Monitor for unusual permission usage patterns
4. **Incident Response**: Have procedures for responding to security incidents

### Compliance
1. **Documentation**: Maintain detailed records of role assignments and changes
2. **Reporting**: Generate regular reports on role usage and permissions
3. **Validation**: Validate role assignments against organizational policies
4. **Training**: Ensure compliance with role-based access control policies

## Technical Implementation

### Database Schema
```sql
-- User table with role assignment
CREATE TABLE Users (
    Id INT PRIMARY KEY,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    Role INT NOT NULL, -- References UserRole enum
    DepartmentId INT,
    BusinessVerticalId INT,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    ModifiedDate DATETIME2 DEFAULT GETDATE()
);
```

### JWT Token Claims
```json
{
  "sub": "user_id",
  "email": "user@company.com",
  "role": "Manager",
  "roleLevel": 3,
  "department": "IT",
  "permissions": ["create_request", "approve_team_requests", "view_department_reports"]
}
```

### Authorization Attributes
```csharp
[Authorize(Roles = "Manager,Director,BusinessExecutive,SystemAdministrator")]
[Authorize(RoleLevel = 3)] // Minimum role level required
[Authorize(Permission = "approve_department_requests")]
```

This comprehensive role-based access control system ensures secure, scalable, and maintainable user management while supporting organizational hierarchy and compliance requirements. 