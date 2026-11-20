import { useQuery } from '@tanstack/react-query';
import { departmentService } from '../services/departmentService';

export function useDepartments(companyId?: string) {
  return useQuery({
    queryKey: ['departments', companyId],
    queryFn: () => {
      console.log('Fetching departments for companyId:', companyId);
      return departmentService.getDepartments(companyId);
    },
    enabled: !!companyId,
  });
}