import { apiClient } from '../lib/api';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  employeeId?: string;
  employeeName?: string;
  employeeCode?: string;
  isActive: boolean;
  lastLoginAt?: string;
}

export const userService = {
  getAll: async (): Promise<User[]> => {
    const res = await apiClient.get('/users');
    return res.data;
  },
  get: async (id: string): Promise<User> => {
    const res = await apiClient.get(`/users/${id}`);
    return res.data;
  },
  create: async (data: {
    email: string;
    firstName: string;
    lastName: string;
    role: string;
    employeeId?: string;
    password?: string;
  }): Promise<User> => {
    const res = await apiClient.post('/users', data);
    return res.data;
  },
  update: async (data: User): Promise<User> => {
    const res = await apiClient.put(`/users/${data.id}`, data);
    return res.data;
  },
  resetPassword: async (id: string, newPassword: string): Promise<void> => {
  // ✅ Send both id and newPassword in the body
  await apiClient.post(`/users/${id}/reset-password`, { id, newPassword });
},
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/users/${id}`);
  },
};