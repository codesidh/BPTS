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

// Workflow Designer Types
export interface WorkflowStageConfiguration {
  id: number;
  name: string;
  order: number;
  description: string;
  businessVerticalId?: number;
  requiredRoles: UserRole[];
  approvalRequired: boolean;
  isActive: boolean;
  stageType: 'Standard' | 'Approval' | 'Notification' | 'Auto';
  slaHours?: number;
  notificationTemplate: NotificationTemplate;
  autoTransition: boolean;
  allowedTransitions: number[];
  validationRules: ValidationRules;
  version: number;
  effectiveDate?: string;
  changeHistory: ChangeHistoryEntry[];
  
  // Designer-specific properties
  x?: number;
  y?: number;
  width?: number;
  height?: number;
}

export interface WorkflowTransition {
  id: number;
  fromStageId: number;
  toStageId: number;
  businessVerticalId?: number;
  transitionName: string;
  requiredRole?: UserRole;
  isActive: boolean;
  conditionScript: ConditionalScript;
  notificationRequired: boolean;
  notificationTemplate: NotificationTemplate;
  eventSourceId?: string;
  correlationId?: string;
  autoTransitionDelayMinutes?: number;
  validationRules: TransitionValidationRules;
}

export interface NotificationTemplate {
  id?: string;
  subject: string;
  body: string;
  recipients: string[];
  ccRecipients?: string[];
  templateVariables: Record<string, string>;
  isActive: boolean;
}

export interface ValidationRules {
  id?: string;
  rules: ValidationRule[];
  isStrict: boolean;
  errorMessages: Record<string, string>;
}

export interface ValidationRule {
  field: string;
  operator: 'equals' | 'notEquals' | 'contains' | 'notEmpty' | 'greaterThan' | 'lessThan' | 'regex';
  value: any;
  message: string;
  isRequired: boolean;
}

export interface TransitionValidationRules extends ValidationRules {
  preConditions: ValidationRule[];
  postConditions: ValidationRule[];
}

export interface ConditionalScript {
  type: 'priority' | 'role' | 'timeElapsed' | 'businessVertical' | 'custom';
  conditions: ConditionExpression[];
  operator: 'and' | 'or';
  isActive: boolean;
}

export interface ConditionExpression {
  field: string;
  operator: 'equals' | 'notEquals' | 'greaterThan' | 'lessThan' | 'contains' | 'startsWith' | 'endsWith';
  value: any;
  type: 'string' | 'number' | 'boolean' | 'date' | 'enum';
}

export interface WorkflowDesignerState {
  stages: WorkflowStageConfiguration[];
  transitions: WorkflowTransition[];
  selectedStage?: WorkflowStageConfiguration;
  selectedTransition?: WorkflowTransition;
  isDragging: boolean;
  isConnecting: boolean;
  businessVerticalId?: number;
  validationResults: WorkflowValidationResult;
  hasUnsavedChanges: boolean;
  previewMode: boolean;
  gridSnap: boolean;
  zoomLevel: number;
}

export interface WorkflowValidationResult {
  isValid: boolean;
  validationErrors: string[];
  warnings: string[];
  validationDate: string;
}

export interface ChangeHistoryEntry {
  id: string;
  changeType: 'Create' | 'Update' | 'Delete';
  fieldName?: string;
  oldValue?: string;
  newValue?: string;
  changedBy: string;
  changedDate: string;
  reason?: string;
}

export interface StagePosition {
  stageId: number;
  x: number;
  y: number;
  width: number;
  height: number;
}

export interface TransitionPath {
  transitionId: number;
  fromStageId: number;
  toStageId: number;
  controlPoints: { x: number; y: number }[];
}

// Workflow Metrics Types
export interface WorkflowMetrics {
  totalWorkRequests: number;
  completedWorkRequests: number;
  averageCompletionDays: number;
  stageDistribution: Record<number, number>;
  averageStageTime: Record<number, number>;
  slaViolations: number;
  slaComplianceRate: number;
  fromDate: string;
  toDate: string;
}

export interface WorkflowBottleneckAnalysis {
  stage: WorkflowStage;
  pendingCount: number;
  averageWaitTime: number;
  bottleneckType: 'Resource' | 'Approval' | 'System';
  recommendation: string;
}

export interface SLAStatus {
  workRequestId: number;
  currentStage: WorkflowStage;
  stageEntryTime: string;
  slaHours?: number;
  slaDeadline?: string;
  timeRemaining: number;
  isViolated: boolean;
  isAtRisk: boolean;
  status: string;
}

export interface WorkflowState {
  workRequestId: number;
  stage: WorkflowStage;
  entryTime: string;
  exitTime?: string;
  userId: number;
  comments?: string;
  duration?: number;
  metadata: Record<string, any>;
}

export interface ApprovalResult {
  success: boolean;
  message: string;
  nextStage?: WorkflowStage;
  processedDate: string;
}