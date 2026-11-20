import { apiClient } from '../lib/api';

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  email: string;
  role: string;
  userId: string;
  employeeId?: string;
  tenantId: string;

}

export const authService = {
  login: async (credentials: LoginCredentials): Promise<LoginResponse> => {
    // Use the real login endpoint
    const response = await apiClient.post<LoginResponse>('/auth/login', credentials);
    return response.data;
  },
};