import { apiClient } from '../lib/api';

export interface PayslipLineItem {
  name: string;
  type: string;
  amount: number;
  source: string;
}

export interface Payslip {
   employeeId: string;
  payrollRunId: string;
 
  employeeName: string;
  employeeCode: string;
  email: string;
  companyName: string;
  departmentName: string;
  profileName: string;
  payPeriod: string;
  payPeriodStart: string;
  payPeriodEnd: string;
  currency: number;
  grossPay: number;
  totalTax: number;
  totalDeductions: number;
  netPay: number;
  lineItems: PayslipLineItem[];
  generatedAt: string;
  
}

export const payslipService = {
  getEmployeePayslips: async (employeeId: string, limit = 12): Promise<Payslip[]> => {
    const response = await apiClient.get(`/payslip/employee/${employeeId}`, { params: { limit } });
    return response.data;
  },

  getPayslip: async (employeeId: string, payrollRunId: string): Promise<Payslip> => {
    const response = await apiClient.get(`/payslip/${employeeId}/${payrollRunId}`);
    return response.data;
  },

  getCompanyPayslips: async (companyId: string, fromDate?: string, toDate?: string): Promise<Payslip[]> => {
    const params: any = {};
    if (fromDate) params.fromDate = fromDate;
    if (toDate) params.toDate = toDate;
    const response = await apiClient.get(`/payslip/company/${companyId}`, { params });
    return response.data;
  },

  downloadPayslipPdf: async (employeeId: string, payrollRunId: string): Promise<Blob> => {
    console.log('Downloading PDF with', employeeId, payrollRunId);
    const response = await apiClient.get(`/payslip/${employeeId}/${payrollRunId}/pdf`, {
      responseType: 'blob',
    });
    return response.data;
  },
};