import { apiClient } from '../lib/api';

export interface Employee {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  employeeCode: string;
  joinedDate?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankBranchCode?: string;
  companyId: string;
  companyName: string;
  departmentId: string;
  departmentName: string;
  primaryProfileId: string;
  profileName: string;
  isActive: boolean;
  salaryStructures: SalaryStructure[];
  customBenefits: CustomBenefit[];
}

export interface SalaryStructure {
  id: string;
  currency: number;
  amount: number;
  effectiveStartDate: string;
  effectiveEndDate?: string | null; // Allow null
}

export interface CustomBenefit {
  id: string;
  benefitName: string;
  amount: number;
  currency: number;
  isRecurring: boolean;
}

export interface CreateEmployeeRequest {
  firstName: string;
  lastName: string;
  email: string;
  employeeCode?: string | null;
  joinedDate?: string | null;
  bankName?: string | null;
  bankAccountNumber?: string | null;
  bankBranchCode?: string | null;
  companyId: string;
  departmentId: string;
  primaryProfileId: string;
  salaryStructures: Omit<SalaryStructure, 'id'>[];
  customBenefits: Omit<CustomBenefit, 'id'>[];
}

export interface UpdateEmployeeRequest extends CreateEmployeeRequest {
  id: string;
}

export const employeeService = {
  getEmployees: async (params?: {
    companyId?: string;
    departmentId?: string;
    profileId?: string;
    search?: string;
    includeInactive?: boolean;
  }): Promise<Employee[]> => {
    const response = await apiClient.get('/employees', { params });
    return response.data;
  },

  getEmployee: async (id: string): Promise<Employee> => {
    const response = await apiClient.get(`/employees/${id}`);
    return response.data;
  },

  createEmployee: async (data: CreateEmployeeRequest): Promise<Employee> => {
    const response = await apiClient.post('/employees', data);
    return response.data;
  },

  updateEmployee: async (data: UpdateEmployeeRequest): Promise<Employee> => {
    const response = await apiClient.put(`/employees/${data.id}`, data);
    return response.data;
  },

  deleteEmployee: async (id: string): Promise<void> => {
    await apiClient.delete(`/employees/${id}`);
  },
};