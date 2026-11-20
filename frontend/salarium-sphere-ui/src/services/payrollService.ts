import { apiClient } from '../lib/api';

export interface PayrollRunRequest {
  companyId: string;
  payPeriodStart: string; // ISO date
  payPeriodEnd: string;   // ISO date
}

export interface PayrollRunSummary {
  payrollRunId: string;
  totalEmployees: number;
  successfulEmployees: number;
  failedEmployees: number;
  totalGrossPay: number;
  totalNetPay: number;
  status: string;
  errors: string[];
}

export const payrollService = {
  runPayroll: async (request: PayrollRunRequest): Promise<PayrollRunSummary> => {
    const response = await apiClient.post<PayrollRunSummary>('/payroll/run', request);
    return response.data;
  },
};