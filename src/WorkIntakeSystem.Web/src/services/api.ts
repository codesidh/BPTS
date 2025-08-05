import axios, { AxiosInstance, AxiosResponse } from 'axios';
import { 
  WorkRequest, 
  CreateWorkRequest, 
  UpdateWorkRequest, 
  CreatePriorityVote,
  Department,
  BusinessVertical,
  User,
  DashboardStats,
  PriorityLevel,
  WorkflowStageConfiguration,
  WorkflowTransition,
  WorkflowValidationResult,
  WorkflowMetrics,
  WorkflowBottleneckAnalysis,
  SLAStatus
} from '../types';

interface LoginRequest {
  email: string;
  password: string;
}

interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
  departmentId: number;
  businessVerticalId: number;
}

interface AuthResponse {
  token: string;
  expiresAt: string;
  user: User;
}

interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

class ApiService {
  private api: AxiosInstance;
  private token: string | null = null;

  constructor() {
    this.api = axios.create({
      baseURL: '/api',
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Load token from localStorage on initialization
    this.token = localStorage.getItem('authToken');
    if (this.token) {
      this.setAuthToken(this.token);
    }

    // Request interceptor for authentication
    this.api.interceptors.request.use(
      (config) => {
        if (this.token) {
          config.headers.Authorization = `Bearer ${this.token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor for error handling
    this.api.interceptors.response.use(
      (response: AxiosResponse) => response,
      (error) => {
        if (error.response?.status === 401) {
          // Handle unauthorized access
          this.logout();
          window.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
  }

  // Authentication methods
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await this.api.post<AuthResponse>('/auth/login', credentials);
    const authData = response.data;
    this.setAuthToken(authData.token);
    return authData;
  }

  async register(userData: RegisterRequest): Promise<AuthResponse> {
    const response = await this.api.post<AuthResponse>('/auth/register', userData);
    const authData = response.data;
    this.setAuthToken(authData.token);
    return authData;
  }

  async changePassword(passwordData: ChangePasswordRequest): Promise<void> {
    await this.api.post('/auth/change-password', passwordData);
  }

  async resetPassword(email: string): Promise<void> {
    await this.api.post('/auth/reset-password', { email });
  }

  async confirmPasswordReset(token: string, newPassword: string): Promise<void> {
    await this.api.post('/auth/confirm-reset-password', { 
      token, 
      newPassword, 
      confirmNewPassword: newPassword 
    });
  }

  async validateToken(token: string): Promise<User> {
    const response = await this.api.post<User>('/auth/validate-token', { token });
    return response.data;
  }

  async getCurrentUser(): Promise<User> {
    const response = await this.api.get<User>('/auth/me');
    return response.data;
  }

  setAuthToken(token: string) {
    this.token = token;
    localStorage.setItem('authToken', token);
    this.api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
  }

  logout() {
    this.token = null;
    localStorage.removeItem('authToken');
    delete this.api.defaults.headers.common['Authorization'];
  }

  isAuthenticated(): boolean {
    return !!this.token;
  }

  getApi(): AxiosInstance {
    return this.api;
  }

  // Work Requests API
  async getWorkRequests(): Promise<WorkRequest[]> {
    const response = await this.api.get<WorkRequest[]>('/workrequests');
    return response.data;
  }

  async getWorkRequest(id: number): Promise<WorkRequest> {
    const response = await this.api.get<WorkRequest>(`/workrequests/${id}`);
    return response.data;
  }

  async createWorkRequest(workRequest: CreateWorkRequest): Promise<WorkRequest> {
    const response = await this.api.post<WorkRequest>('/workrequests', workRequest);
    return response.data;
  }

  async updateWorkRequest(id: number, workRequest: UpdateWorkRequest): Promise<WorkRequest> {
    const response = await this.api.put<WorkRequest>(`/workrequests/${id}`, workRequest);
    return response.data;
  }

  async deleteWorkRequest(id: number): Promise<void> {
    await this.api.delete(`/workrequests/${id}`);
  }

  async advanceWorkflow(id: number, stage: string): Promise<WorkRequest> {
    const response = await this.api.post<WorkRequest>(`/workrequests/${id}/advance-workflow`, { stage });
    return response.data;
  }

  // Priority Voting API
  async submitPriorityVote(workRequestId: number, vote: CreatePriorityVote): Promise<void> {
    await this.api.post(`/workrequests/${workRequestId}/priority-vote`, vote);
  }

  async getPriorityVotes(workRequestId: number): Promise<any[]> {
    const response = await this.api.get<any[]>(`/workrequests/${workRequestId}/priority-votes`);
    return response.data;
  }

  // Departments API
  async getDepartments(): Promise<Department[]> {
    const response = await this.api.get<Department[]>('/departments');
    return response.data;
  }

  async getDepartment(id: number): Promise<Department> {
    const response = await this.api.get<Department>(`/departments/${id}`);
    return response.data;
  }

  // Business Verticals API
  async getBusinessVerticals(): Promise<BusinessVertical[]> {
    const response = await this.api.get<BusinessVertical[]>('/businessverticals');
    return response.data;
  }

  async getBusinessVertical(id: number): Promise<BusinessVertical> {
    const response = await this.api.get<BusinessVertical>(`/businessverticals/${id}`);
    return response.data;
  }

  // Users API
  async getUsers(): Promise<User[]> {
    const response = await this.api.get<User[]>('/users');
    return response.data;
  }

  async getUser(id: number): Promise<User> {
    const response = await this.api.get<User>(`/users/${id}`);
    return response.data;
  }

  // Dashboard API
  async getDashboardStats(): Promise<DashboardStats> {
    const response = await this.api.get<DashboardStats>('/dashboard/stats');
    return response.data;
  }

  // Reports API
  async getReports(): Promise<any[]> {
    const response = await this.api.get<any[]>('/reports');
    return response.data;
  }

  async generateReport(reportType: string, filters: any): Promise<any> {
    const response = await this.api.post<any>(`/reports/${reportType}`, filters);
    return response.data;
  }

  // Workflow Designer API
  async getWorkflowStages(businessVerticalId?: number): Promise<WorkflowStageConfiguration[]> {
    const params = businessVerticalId ? `?businessVerticalId=${businessVerticalId}` : '';
    const response = await this.api.get<WorkflowStageConfiguration[]>(`/workflow/stages${params}`);
    return response.data;
  }

  async createWorkflowStage(stage: Omit<WorkflowStageConfiguration, 'id'>): Promise<WorkflowStageConfiguration> {
    const response = await this.api.post<WorkflowStageConfiguration>('/workflow/stages', stage);
    return response.data;
  }

  async updateWorkflowStage(stageId: number, stage: WorkflowStageConfiguration): Promise<WorkflowStageConfiguration> {
    const response = await this.api.put<WorkflowStageConfiguration>(`/workflow/stages/${stageId}`, stage);
    return response.data;
  }

  async deleteWorkflowStage(stageId: number): Promise<void> {
    await this.api.delete(`/workflow/stages/${stageId}`);
  }

  async getWorkflowTransitions(businessVerticalId?: number): Promise<WorkflowTransition[]> {
    const params = businessVerticalId ? `?businessVerticalId=${businessVerticalId}` : '';
    const response = await this.api.get<WorkflowTransition[]>(`/workflow/transitions${params}`);
    return response.data;
  }

  async createWorkflowTransition(transition: Omit<WorkflowTransition, 'id'>): Promise<WorkflowTransition> {
    const response = await this.api.post<WorkflowTransition>('/workflow/transitions', transition);
    return response.data;
  }

  async updateWorkflowTransition(transitionId: number, transition: WorkflowTransition): Promise<WorkflowTransition> {
    const response = await this.api.put<WorkflowTransition>(`/workflow/transitions/${transitionId}`, transition);
    return response.data;
  }

  async deleteWorkflowTransition(transitionId: number): Promise<void> {
    await this.api.delete(`/workflow/transitions/${transitionId}`);
  }

  async validateWorkflow(businessVerticalId?: number): Promise<WorkflowValidationResult> {
    const params = businessVerticalId ? `?businessVerticalId=${businessVerticalId}` : '';
    const response = await this.api.get<WorkflowValidationResult>(`/workflow/validate${params}`);
    return response.data;
  }

  async getWorkflowMetrics(fromDate: string, toDate: string, businessVerticalId?: number): Promise<WorkflowMetrics> {
    const params = new URLSearchParams({
      fromDate,
      toDate,
      ...(businessVerticalId && { businessVerticalId: businessVerticalId.toString() })
    });
    const response = await this.api.get<WorkflowMetrics>(`/workflow/metrics?${params}`);
    return response.data;
  }

  async getWorkflowBottlenecks(businessVerticalId?: number): Promise<WorkflowBottleneckAnalysis[]> {
    const params = businessVerticalId ? `?businessVerticalId=${businessVerticalId}` : '';
    const response = await this.api.get<WorkflowBottleneckAnalysis[]>(`/workflow/bottlenecks${params}`);
    return response.data;
  }

  async getSLAStatus(workRequestId: number): Promise<SLAStatus> {
    const response = await this.api.get<SLAStatus>(`/workflow/work-requests/${workRequestId}/sla-status`);
    return response.data;
  }

  async getAvailableTransitions(workRequestId: number): Promise<any[]> {
    const response = await this.api.get<any[]>(`/workflow/work-requests/${workRequestId}/transitions`);
    return response.data;
  }

  async advanceWorkflowStage(workRequestId: number, nextStage: string, comments?: string): Promise<void> {
    await this.api.post(`/workflow/work-requests/${workRequestId}/advance`, { nextStage, comments });
  }

  // Generic API methods for flexible usage
  async get<T = any>(url: string): Promise<{ data: T }> {
    const response = await this.api.get<T>(url);
    return { data: response.data };
  }

  async post<T = any>(url: string, data?: any): Promise<{ data: T }> {
    const response = await this.api.post<T>(url, data);
    return { data: response.data };
  }

  async put<T = any>(url: string, data?: any): Promise<{ data: T }> {
    const response = await this.api.put<T>(url, data);
    return { data: response.data };
  }

  async delete<T = any>(url: string): Promise<{ data?: T }> {
    const response = await this.api.delete<T>(url);
    return { data: response.data };
  }
}

// Create and export a singleton instance
export const apiService = new ApiService();

// Export the class for testing purposes
export default ApiService;