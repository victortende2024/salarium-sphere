import { apiClient } from '../lib/api';

export interface Department {
  id: string;
  name: string;
  companyId: string;
  isActive: boolean;
}

export const departmentService = {
  getDepartments: async (companyId?: string): Promise<Department[]> => {
    const params = companyId ? { companyId } : {};
    const response = await apiClient.get('/departments', { params });
    return response.data;
  },
};