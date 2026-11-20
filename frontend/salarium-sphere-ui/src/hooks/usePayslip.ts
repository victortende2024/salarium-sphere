import { useQuery } from '@tanstack/react-query';
import { payslipService } from '../services/payslipService';

export function useLatestPayslip(employeeId: string) {
  return useQuery({
    queryKey: ['latestPayslip', employeeId],
    queryFn: async () => {
      const payslips = await payslipService.getEmployeePayslips(employeeId, 1);
      return payslips.length > 0 ? payslips[0] : null;
    },
    enabled: !!employeeId,
  });
}