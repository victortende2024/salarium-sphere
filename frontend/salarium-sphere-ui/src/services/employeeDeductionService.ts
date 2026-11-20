import { apiClient } from '../lib/api';

export interface EmployeeDeduction {
  id: string;
  employeeId: string;
  deductionName: string;
  description?: string;
  calculationType: number; // 0=Fixed, 1=Percentage, 2=Tiered
  fixedAmount?: number;
  fixedAmountCurrency?: number;
  percentageValue?: number;
  tieredConfigJson?: string;
  capAmount?: number;
  capCurrency?: number;
  thresholdAmount?: number;
  thresholdCurrency?: number;
  isActive: boolean;
}

export const employeeDeductionService = {
  getByEmployee: async (employeeId: string): Promise<EmployeeDeduction[]> => {
    const res = await apiClient.get(`/employeedeductions/employee/${employeeId}`);
    return res.data;
  },
  get: async (id: string): Promise<EmployeeDeduction> => {
    const res = await apiClient.get(`/employeedeductions/${id}`);
    return res.data;
  },
  create: async (data: Omit<EmployeeDeduction, 'id'>): Promise<EmployeeDeduction> => {
    const res = await apiClient.post('/employeedeductions', data);
    return res.data;
  },
  update: async (data: EmployeeDeduction): Promise<EmployeeDeduction> => {
    const res = await apiClient.put(`/employeedeductions/${data.id}`, data);
    return res.data;
  },
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/employeedeductions/${id}`);
  },
};