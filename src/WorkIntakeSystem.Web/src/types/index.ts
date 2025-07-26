export enum WorkCategory {
  WorkRequest = 1,
  Project = 2,
  BreakFix = 3
}

export enum WorkflowStage {
  Intake = 1,
  BusinessReview = 2,
  PriorityAssessment = 3,
  WorkRequestCreation = 4,
  ArchitectureAssessment = 5,
  Estimation = 6,
  Approval = 7,
  BudgetApproval = 8,
  Planning = 9,
  Requirements = 10,
  Development = 11,
  Testing = 12,
  UAT = 13,
  Deployment = 14,
  Closure = 15
}

export enum WorkStatus {
  Draft = 1,
  Prioritized = 2,
  Submitted = 3,
  UnderReview = 4,
  Approved = 5,
  InProgress = 6,
  Testing = 7,
  Deployed = 8,
  Closed = 9,
  Rejected = 10,
  OnHold = 11
}

export enum PriorityVote {
  Low = 1,
  Medium = 2,
  High = 3
}

export enum PriorityLevel {
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4
}

export enum UserRole {
  EndUser = 1,
  Lead = 2,
  DepartmentManager = 3,
  DepartmentHead = 4,
  BusinessExecutive = 5,
  SystemAdministrator = 6
}

export interface WorkRequest {
  id: number;
  title: string;
  description: string;
  category: WorkCategory;
  businessVerticalId: number;
  businessVerticalName: string;
  departmentId: number;
  departmentName: string;
  submitterId: number;
  submitterName: string;
  targetDate?: string;
  actualDate?: string;
  currentStage: WorkflowStage;
  status: WorkStatus;
  priority: number;
  priorityLevel: PriorityLevel;
  capabilityId?: number;
  capabilityName?: string;
  estimatedEffort: number;
  actualEffort: number;
  businessValue: number;
  timeDecayFactor: number;
  capacityAdjustment: number;
  createdDate: string;
  modifiedDate: string;
  createdBy: string;
  modifiedBy: string;
  priorityVotes: PriorityVoteDto[];
}

export interface CreateWorkRequest {
  title: string;
  description: string;
  category: WorkCategory;
  businessVerticalId: number;
  departmentId: number;
  targetDate?: string;
  capabilityId?: number;
  estimatedEffort: number;
  businessValue: number;
}

export interface UpdateWorkRequest {
  title: string;
  description: string;
  category: WorkCategory;
  targetDate?: string;
  currentStage: WorkflowStage;
  status: WorkStatus;
  capabilityId?: number;
  estimatedEffort: number;
  actualEffort: number;
  businessValue: number;
}

export interface PriorityVoteDto {
  id: number;
  departmentId: number;
  departmentName: string;
  vote: PriorityVote;
  weight: number;
  votedById: number;
  votedByName: string;
  votedDate: string;
  comments: string;
  businessValueScore: number;
  strategicAlignment: number;
  resourceImpactAssessment: string;
}

export interface CreatePriorityVote {
  workRequestId: number;
  departmentId: number;
  vote: PriorityVote;
  comments: string;
  businessValueScore: number;
  strategicAlignment: number;
  resourceImpactAssessment: string;
}

export interface Department {
  id: number;
  name: string;
  description: string;
  businessVerticalId: number;
  businessVerticalName: string;
  displayOrder: number;
  departmentCode: string;
  votingWeight: number;
  resourceCapacity: number;
  currentUtilization: number;
  skillMatrix: string;
  isActive: boolean;
}

export interface BusinessVertical {
  id: number;
  name: string;
  description: string;
  configuration: string;
  version: number;
  configurationHistory: string;
  isActive: boolean;
}

export interface User {
  id: number;
  email: string;
  name: string;
  departmentId: number;
  departmentName: string;
  businessVerticalId: number;
  businessVerticalName: string;
  role: UserRole;
  skillSet: string;
  capacity: number;
  currentWorkload: number;
  isActive: boolean;
}

export interface DashboardStats {
  totalActiveRequests: number;
  totalByCategory: Record<WorkCategory, number>;
  totalByPriority: Record<PriorityLevel, number>;
  totalByStatus: Record<WorkStatus, number>;
  averageCompletionTime: number;
  slaCompliance: number;
  resourceUtilization: number;
}

export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
}

export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}