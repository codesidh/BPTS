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
import { useMsal } from '@azure/msal-react';
import { PublicClientApplication } from '@azure/msal-browser';

class ApiService {
  private api: AxiosInstance;
  private msalInstance?: PublicClientApplication;

  constructor(msalInstance?: PublicClientApplication) {
    this.msalInstance = msalInstance;
    this.api = axios.create({
      baseURL: '/api',
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Request interceptor for authentication
    this.api.interceptors.request.use(
      async (config) => {
        if (this.msalInstance) {
          const accounts = this.msalInstance.getAllAccounts();
          if (accounts.length > 0) {
            const tokenResponse = await this.msalInstance.acquireTokenSilent({
              account: accounts[0],
              scopes: ['User.Read'],
            });
            config.headers.Authorization = `Bearer ${tokenResponse.accessToken}`;
          }
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
          window.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
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

  async getWorkRequestsByBusinessVertical(businessVerticalId: number): Promise<WorkRequest[]> {
    const response = await this.api.get<WorkRequest[]>(`/workrequests/business-vertical/${businessVerticalId}`);
    return response.data;
  }

  async getWorkRequestsByDepartment(departmentId: number): Promise<WorkRequest[]> {
    const response = await this.api.get<WorkRequest[]>(`/workrequests/department/${departmentId}`);
    return response.data;
  }

  async getWorkRequestsByPriorityLevel(priorityLevel: PriorityLevel): Promise<WorkRequest[]> {
    const response = await this.api.get<WorkRequest[]>(`/workrequests/priority/${priorityLevel}`);
    return response.data;
  }

  async getPendingPriorityVotes(departmentId: number): Promise<WorkRequest[]> {
    const response = await this.api.get<WorkRequest[]>(`/workrequests/pending-votes/${departmentId}`);
    return response.data;
  }

  async createWorkRequest(workRequest: CreateWorkRequest): Promise<WorkRequest> {
    const response = await this.api.post<WorkRequest>('/workrequests', workRequest);
    return response.data;
  }

  async updateWorkRequest(id: number, workRequest: UpdateWorkRequest): Promise<void> {
    await this.api.put(`/workrequests/${id}`, workRequest);
  }

  async deleteWorkRequest(id: number): Promise<void> {
    await this.api.delete(`/workrequests/${id}`);
  }

  async recalculatePriority(id: number): Promise<void> {
    await this.api.post(`/workrequests/${id}/recalculate-priority`);
  }

  async recalculateAllPriorities(): Promise<void> {
    await this.api.post('/workrequests/recalculate-all-priorities');
  }

  // Priority Votes API
  async createPriorityVote(vote: CreatePriorityVote): Promise<void> {
    await this.api.post('/priorities', vote);
  }

  async updatePriorityVote(id: number, vote: Partial<CreatePriorityVote>): Promise<void> {
    await this.api.put(`/priorities/${id}`, vote);
  }

  async deletePriorityVote(id: number): Promise<void> {
    await this.api.delete(`/priorities/${id}`);
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

  async getDepartmentsByBusinessVertical(businessVerticalId: number): Promise<Department[]> {
    const response = await this.api.get<Department[]>(`/departments/business-vertical/${businessVerticalId}`);
    return response.data;
  }

  async createDepartment(department: Partial<Department>): Promise<Department> {
    const response = await this.api.post<Department>('/departments', department);
    return response.data;
  }

  async updateDepartment(id: number, department: Partial<Department>): Promise<void> {
    await this.api.put(`/departments/${id}`, department);
  }

  async deleteDepartment(id: number): Promise<void> {
    await this.api.delete(`/departments/${id}`);
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

  async createBusinessVertical(businessVertical: Partial<BusinessVertical>): Promise<BusinessVertical> {
    const response = await this.api.post<BusinessVertical>('/businessverticals', businessVertical);
    return response.data;
  }

  async updateBusinessVertical(id: number, businessVertical: Partial<BusinessVertical>): Promise<void> {
    await this.api.put(`/businessverticals/${id}`, businessVertical);
  }

  async deleteBusinessVertical(id: number): Promise<void> {
    await this.api.delete(`/businessverticals/${id}`);
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

  async getCurrentUser(): Promise<User> {
    const response = await this.api.get<User>('/users/current');
    return response.data;
  }

  // Dashboard API
  async getDashboardStats(): Promise<DashboardStats> {
    const response = await this.api.get<DashboardStats>('/dashboard/stats');
    return response.data;
  }

  async getExecutiveDashboard(): Promise<any> {
    const response = await this.api.get('/dashboard/executive');
    return response.data;
  }

  async getDepartmentDashboard(departmentId: number): Promise<any> {
    const response = await this.api.get(`/dashboard/department/${departmentId}`);
    return response.data;
  }

  // Health Check
  async healthCheck(): Promise<{ status: string }> {
    const response = await this.api.get<{ status: string }>('/health');
    return response.data;
  }

  public getApi() {
    return this.api;
  }
}

export const createApiService = (msalInstance?: PublicClientApplication) => new ApiService(msalInstance);