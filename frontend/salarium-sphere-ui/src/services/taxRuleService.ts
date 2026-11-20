import { apiClient } from '../lib/api';

export interface TaxRule {
  id: string;
  name: string;
  description?: string;
  countryCode?: string;
  isMandatory: boolean;
  calculationType: number; // 0=Fixed, 1=Percentage, 2=Tiered
  fixedAmount?: number;
  fixedAmountCurrency?: number;
  percentageValue?: number;
  tieredConfigJson?: string;
  capAmount?: number;
  capCurrency?: number;
  thresholdAmount?: number;
  thresholdCurrency?: number;
  priority: number;
  isActive: boolean;
}

export const taxRuleService = {
  getAll: async (): Promise<TaxRule[]> => {
    const res = await apiClient.get('/taxrules');
    return res.data;
  },
  get: async (id: string): Promise<TaxRule> => {
    const res = await apiClient.get(`/taxrules/${id}`);
    return res.data;
  },
  create: async (data: Omit<TaxRule, 'id'>): Promise<TaxRule> => {
    const res = await apiClient.post('/taxrules', data);
    return res.data;
  },
  update: async (data: TaxRule): Promise<TaxRule> => {
    const res = await apiClient.put(`/taxrules/${data.id}`, data);
    return res.data;
  },
  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/taxrules/${id}`);
  },
};