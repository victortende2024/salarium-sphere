import { useQuery } from '@tanstack/react-query';
import { companyService } from '../services/companyService';

export function useCompanies() {
  return useQuery({
    queryKey: ['companies'],
    queryFn: () => companyService.getAll(),   // ← arrow wrapper, not bare method
    staleTime: 5 * 60 * 1000,
  });
}