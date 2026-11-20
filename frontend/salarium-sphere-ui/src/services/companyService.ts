import { apiClient } from '../lib/api';

export interface Company {
  id: string;
  name: string;
  parentCompanyId?: string;
  parentCompanyName?: string;
  isActive: boolean;
  departments: Department[];
}

export interface Department {
  id: string;
  name: string;
  companyId: string;
  isActive: boolean;
}

export const companyService = {
  getAll: async (): Promise<Company[]> => {
    const res = await apiClient.get('/companies');
    return res.data;
  },
  get: async (id: string): Promise<Company> => {
    const res = await apiClient.get(`/companies/${id}`);
    return res.data;
  },
  create: async (data: { name: string; parentCompanyId?: string }): Promise<Company> => {
    const res = await apiClient.post('/companies', data);
    return res.data;
  },
  update: async (data: Company): Promise<Company> => {
    const res = await apiClient.put(`/companies/${data.id}`, data);
    return res.data;
  },
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/companies/${id}`);
  },
};

export const departmentService = {
  create: async (data: { name: string; companyId: string }): Promise<Department> => {
    const res = await apiClient.post('/departments', data);
    return res.data;
  },
  update: async (data: Department): Promise<Department> => {
    const res = await apiClient.put(`/departments/${data.id}`, data);
    return res.data;
  },
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/departments/${id}`);
  },
};