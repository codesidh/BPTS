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
  PriorityLevel
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
}

// Create and export a singleton instance
export const apiService = new ApiService();

// Export the class for testing purposes
export default ApiService;