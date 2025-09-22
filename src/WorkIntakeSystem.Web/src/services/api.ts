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
  WorkflowStageConfiguration,
  WorkflowTransition,
  WorkflowValidationResult,
  WorkflowMetrics,
  WorkflowBottleneckAnalysis,
  SLAStatus,
  PriorityConfiguration,
  PriorityAlgorithmConfig,
  TimeDecayConfig,
  BusinessValueWeightConfig,
  CapacityAdjustmentConfig,
  PriorityPreviewResult,
  PriorityTrendAnalysis,
  PriorityEffectivenessMetric,
  PriorityRecommendation,
  PriorityConfigValidationResult,
  PriorityPrediction,
  ResourceForecast,
  CompletionPrediction,
  BusinessValueROI,
  RiskAssessment,
  PredictiveInsight,
  WorkloadPrediction,
  CapacityPrediction,
  PriorityTrend,
  CompletionTrend,
  BusinessValueTrend,
  RiskIndicator,
  ExecutiveDashboard,
  DepartmentDashboard,
  ProjectDashboard,
  PrioritySummary,
  CompletionSummary,
  RiskSummary,
  DashboardAnalytics,
  DepartmentAnalytics,
  WorkflowAnalytics,
  PriorityAnalytics,
  ResourceUtilization,
  SLACompliance,
  TrendData
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
    try {
      console.log('API Service: Making login request...', credentials);
      const response = await this.api.post<AuthResponse>('/auth/login', credentials, {
        headers: {
          'Cache-Control': 'no-cache, no-store, must-revalidate',
          'Pragma': 'no-cache',
          'Expires': '0'
        }
      });
      console.log('API Service: Login response received:', response.status, response.data);
      
      // Validate response data
      if (!response.data) {
        throw new Error('No data in response');
      }
      
      const authData = response.data;
      console.log('API Service: Auth data validated:', authData);
      
      // Check if token exists
      if (!authData.token) {
        throw new Error('No token in response');
      }
      
      console.log('API Service: Setting auth token...');
      this.setAuthToken(authData.token);
      console.log('API Service: Auth token set, returning data');
      return authData;
    } catch (error) {
      console.error('API Service: Login error:', error);
      console.error('API Service: Error details:', {
        message: error.message,
        stack: error.stack,
        response: error.response?.data
      });
      throw error;
    }
  }

  async register(userData: RegisterRequest): Promise<AuthResponse> {
    const response = await this.api.post<AuthResponse>('/auth/register', userData, {
      headers: {
        'Cache-Control': 'no-cache, no-store, must-revalidate',
        'Pragma': 'no-cache',
        'Expires': '0'
      }
    });
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

  // Dashboard Analytics Methods
  async getDashboardAnalytics(businessVerticalId?: number, fromDate?: Date, toDate?: Date): Promise<DashboardAnalytics> {
    const params = new URLSearchParams();
    if (businessVerticalId) params.append('businessVerticalId', businessVerticalId.toString());
    if (fromDate) params.append('fromDate', fromDate.toISOString());
    if (toDate) params.append('toDate', toDate.toISOString());
    
    const response = await this.api.get<DashboardAnalytics>(`/analytics/dashboard?${params.toString()}`);
    return response.data;
  }

  async getDepartmentAnalytics(departmentId: number, fromDate?: Date, toDate?: Date): Promise<DepartmentAnalytics> {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate.toISOString());
    if (toDate) params.append('toDate', toDate.toISOString());
    
    const response = await this.api.get<DepartmentAnalytics>(`/analytics/department/${departmentId}?${params.toString()}`);
    return response.data;
  }

  async getWorkflowAnalytics(fromDate?: Date, toDate?: Date): Promise<WorkflowAnalytics> {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate.toISOString());
    if (toDate) params.append('toDate', toDate.toISOString());
    
    const response = await this.api.get<WorkflowAnalytics>(`/analytics/workflow?${params.toString()}`);
    return response.data;
  }

  async getPriorityAnalytics(fromDate?: Date, toDate?: Date): Promise<PriorityAnalytics> {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate.toISOString());
    if (toDate) params.append('toDate', toDate.toISOString());
    
    const response = await this.api.get<PriorityAnalytics>(`/analytics/priority?${params.toString()}`);
    return response.data;
  }

  async getResourceUtilization(fromDate?: Date, toDate?: Date): Promise<ResourceUtilization> {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate.toISOString());
    if (toDate) params.append('toDate', toDate.toISOString());
    
    const response = await this.api.get<ResourceUtilization>(`/analytics/resource-utilization?${params.toString()}`);
    return response.data;
  }

  async getSLACompliance(fromDate?: Date, toDate?: Date): Promise<SLACompliance> {
    const params = new URLSearchParams();
    if (fromDate) params.append('fromDate', fromDate.toISOString());
    if (toDate) params.append('toDate', toDate.toISOString());
    
    const response = await this.api.get<SLACompliance>(`/analytics/sla-compliance?${params.toString()}`);
    return response.data;
  }

  async getTrendData(metric: string, fromDate: Date, toDate: Date, groupBy?: string): Promise<TrendData[]> {
    const params = new URLSearchParams();
    params.append('metric', metric);
    params.append('fromDate', fromDate.toISOString());
    params.append('toDate', toDate.toISOString());
    if (groupBy) params.append('groupBy', groupBy);
    
    const response = await this.api.get<TrendData[]>(`/analytics/trends?${params.toString()}`);
    return response.data;
  }

  setAuthToken(token: string) {
    try {
      console.log('API Service: Setting auth token:', token ? 'Token exists' : 'No token');
      this.token = token;
      localStorage.setItem('authToken', token);
      this.api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
      console.log('API Service: Auth token set successfully');
    } catch (error) {
      console.error('API Service: Error setting auth token:', error);
      throw error;
    }
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

  // Priority Configuration API
  async getPriorityConfigurations(): Promise<PriorityConfiguration[]> {
    const response = await this.api.get<PriorityConfiguration[]>('/priorityconfiguration');
    return response.data;
  }

  async getPriorityConfigurationsByBusinessVertical(businessVerticalId: number): Promise<PriorityConfiguration[]> {
    const response = await this.api.get<PriorityConfiguration[]>(`/priorityconfiguration/business-vertical/${businessVerticalId}`);
    return response.data;
  }

  async getPriorityConfiguration(configurationId: number): Promise<PriorityConfiguration> {
    const response = await this.api.get<PriorityConfiguration>(`/priorityconfiguration/${configurationId}`);
    return response.data;
  }

  async createPriorityConfiguration(configuration: Omit<PriorityConfiguration, 'id'>): Promise<PriorityConfiguration> {
    const response = await this.api.post<PriorityConfiguration>('/priorityconfiguration', configuration);
    return response.data;
  }

  async updatePriorityConfiguration(configurationId: number, configuration: PriorityConfiguration): Promise<PriorityConfiguration> {
    const response = await this.api.put<PriorityConfiguration>(`/priorityconfiguration/${configurationId}`, configuration);
    return response.data;
  }

  async deletePriorityConfiguration(configurationId: number): Promise<void> {
    await this.api.delete(`/priorityconfiguration/${configurationId}`);
  }

  async getPriorityAlgorithmConfig(businessVerticalId: number): Promise<PriorityAlgorithmConfig> {
    const response = await this.api.get<PriorityAlgorithmConfig>(`/priorityconfiguration/algorithm/${businessVerticalId}`);
    return response.data;
  }

  async setPriorityAlgorithmConfig(businessVerticalId: number, config: PriorityAlgorithmConfig): Promise<void> {
    await this.api.post(`/priorityconfiguration/algorithm/${businessVerticalId}`, config);
  }

  async testPriorityCalculation(businessVerticalId: number, testRequest: WorkRequest): Promise<{ priorityScore: number; testRequest: WorkRequest }> {
    const response = await this.api.post<{ priorityScore: number; testRequest: WorkRequest }>(`/priorityconfiguration/test-calculation/${businessVerticalId}`, testRequest);
    return response.data;
  }

  async previewPriorityChanges(businessVerticalId: number, newConfig: PriorityAlgorithmConfig): Promise<PriorityPreviewResult> {
    const response = await this.api.post<PriorityPreviewResult>(`/priorityconfiguration/preview/${businessVerticalId}`, newConfig);
    return response.data;
  }

  async getTimeDecayConfig(businessVerticalId: number): Promise<TimeDecayConfig> {
    const response = await this.api.get<TimeDecayConfig>(`/priorityconfiguration/time-decay/${businessVerticalId}`);
    return response.data;
  }

  async setTimeDecayConfig(businessVerticalId: number, config: TimeDecayConfig): Promise<void> {
    await this.api.post(`/priorityconfiguration/time-decay/${businessVerticalId}`, config);
  }

  async calculateTimeDecayFactor(businessVerticalId: number, createdDate: string): Promise<any> {
    const response = await this.api.post(`/priorityconfiguration/time-decay/${businessVerticalId}/calculate`, createdDate);
    return response.data;
  }

  async getBusinessValueWeights(businessVerticalId: number): Promise<BusinessValueWeightConfig> {
    const response = await this.api.get<BusinessValueWeightConfig>(`/priorityconfiguration/business-value-weights/${businessVerticalId}`);
    return response.data;
  }

  async setBusinessValueWeights(businessVerticalId: number, config: BusinessValueWeightConfig): Promise<void> {
    await this.api.post(`/priorityconfiguration/business-value-weights/${businessVerticalId}`, config);
  }

  async getCapacityAdjustmentConfig(businessVerticalId: number): Promise<CapacityAdjustmentConfig> {
    const response = await this.api.get<CapacityAdjustmentConfig>(`/priorityconfiguration/capacity-adjustment/${businessVerticalId}`);
    return response.data;
  }

  async setCapacityAdjustmentConfig(businessVerticalId: number, config: CapacityAdjustmentConfig): Promise<void> {
    await this.api.post(`/priorityconfiguration/capacity-adjustment/${businessVerticalId}`, config);
  }

  async getPriorityTrends(businessVerticalId: number, fromDate: string, toDate: string): Promise<PriorityTrendAnalysis> {
    const response = await this.api.get<PriorityTrendAnalysis>(`/priorityconfiguration/analytics/trends/${businessVerticalId}?fromDate=${fromDate}&toDate=${toDate}`);
    return response.data;
  }

  async getPriorityEffectivenessMetrics(businessVerticalId: number): Promise<PriorityEffectivenessMetric[]> {
    const response = await this.api.get<PriorityEffectivenessMetric[]>(`/priorityconfiguration/analytics/effectiveness/${businessVerticalId}`);
    return response.data;
  }

  async getPriorityRecommendations(businessVerticalId: number): Promise<PriorityRecommendation> {
    const response = await this.api.get<PriorityRecommendation>(`/priorityconfiguration/analytics/recommendations/${businessVerticalId}`);
    return response.data;
  }

  async validatePriorityConfiguration(configuration: PriorityConfiguration): Promise<PriorityConfigValidationResult> {
    const response = await this.api.post<PriorityConfigValidationResult>('/priorityconfiguration/validate', configuration);
    return response.data;
  }

  // Advanced Analytics API
  async predictPriority(workRequestId: number): Promise<PriorityPrediction> {
    const response = await this.api.get<PriorityPrediction>(`/advancedanalytics/predict/priority/${workRequestId}`);
    return response.data;
  }

  async predictPriorityTrends(departmentId: number, targetDate: string): Promise<PriorityTrend[]> {
    const response = await this.api.get<PriorityTrend[]>(`/advancedanalytics/predict/priority-trends/${departmentId}?targetDate=${targetDate}`);
    return response.data;
  }

  async forecastResourceNeeds(departmentId: number, targetDate: string): Promise<ResourceForecast> {
    const response = await this.api.get<ResourceForecast>(`/advancedanalytics/forecast/resources/${departmentId}?targetDate=${targetDate}`);
    return response.data;
  }

  async predictCapacityUtilization(departmentId: number, targetDate: string): Promise<CapacityPrediction> {
    const response = await this.api.get<CapacityPrediction>(`/advancedanalytics/predict/capacity/${departmentId}?targetDate=${targetDate}`);
    return response.data;
  }

  async predictCompletionTime(workRequestId: number): Promise<CompletionPrediction> {
    const response = await this.api.get<CompletionPrediction>(`/advancedanalytics/predict/completion/${workRequestId}`);
    return response.data;
  }

  async predictCompletionTrends(departmentId: number, targetDate: string): Promise<CompletionTrend[]> {
    const response = await this.api.get<CompletionTrend[]>(`/advancedanalytics/predict/completion-trends/${departmentId}?targetDate=${targetDate}`);
    return response.data;
  }

  async calculateROI(workRequestId: number): Promise<BusinessValueROI> {
    const response = await this.api.get<BusinessValueROI>(`/advancedanalytics/analyze/roi/${workRequestId}`);
    return response.data;
  }

  async analyzeBusinessValueTrends(businessVerticalId: number, fromDate: string, toDate: string): Promise<BusinessValueTrend[]> {
    const response = await this.api.get<BusinessValueTrend[]>(`/advancedanalytics/analyze/business-value-trends/${businessVerticalId}?fromDate=${fromDate}&toDate=${toDate}`);
    return response.data;
  }

  async assessProjectRisk(workRequestId: number): Promise<RiskAssessment> {
    const response = await this.api.get<RiskAssessment>(`/advancedanalytics/assess/risk/${workRequestId}`);
    return response.data;
  }

  async getRiskIndicators(departmentId: number): Promise<RiskIndicator[]> {
    const response = await this.api.get<RiskIndicator[]>(`/advancedanalytics/assess/risk-indicators/${departmentId}`);
    return response.data;
  }

  async getPredictiveInsights(businessVerticalId: number): Promise<PredictiveInsight[]> {
    const response = await this.api.get<PredictiveInsight[]>(`/advancedanalytics/insights/predictive/${businessVerticalId}`);
    return response.data;
  }

  async predictWorkload(departmentId: number, targetDate: string): Promise<WorkloadPrediction> {
    const response = await this.api.get<WorkloadPrediction>(`/advancedanalytics/predict/workload/${departmentId}?targetDate=${targetDate}`);
    return response.data;
  }

  async getExecutiveDashboard(startDate: string, endDate: string): Promise<ExecutiveDashboard> {
    const response = await this.api.get<ExecutiveDashboard>(`/advancedanalytics/dashboard/executive?startDate=${startDate}&endDate=${endDate}`);
    return response.data;
  }

  async getDepartmentDashboard(departmentId: number, startDate: string, endDate: string): Promise<DepartmentDashboard> {
    const response = await this.api.get<DepartmentDashboard>(`/advancedanalytics/dashboard/department/${departmentId}?startDate=${startDate}&endDate=${endDate}`);
    return response.data;
  }

  async getProjectDashboard(workRequestId: number, startDate: string, endDate: string): Promise<ProjectDashboard> {
    const response = await this.api.get<ProjectDashboard>(`/advancedanalytics/dashboard/project/${workRequestId}?startDate=${startDate}&endDate=${endDate}`);
    return response.data;
  }

  async getPrioritySummary(fromDate: string, toDate: string): Promise<PrioritySummary> {
    const response = await this.api.get<PrioritySummary>(`/advancedanalytics/summary/priority?fromDate=${fromDate}&toDate=${toDate}`);
    return response.data;
  }

  async getCompletionSummary(fromDate: string, toDate: string): Promise<CompletionSummary> {
    const response = await this.api.get<CompletionSummary>(`/advancedanalytics/summary/completion?fromDate=${fromDate}&toDate=${toDate}`);
    return response.data;
  }

  async getRiskSummary(fromDate: string, toDate: string): Promise<RiskSummary> {
    const response = await this.api.get<RiskSummary>(`/advancedanalytics/summary/risk?fromDate=${fromDate}&toDate=${toDate}`);
    return response.data;
  }
}

// Create and export a singleton instance
export const apiService = new ApiService();

// Export the class for testing purposes
export default ApiService;