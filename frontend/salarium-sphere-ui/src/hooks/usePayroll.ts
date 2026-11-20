import { useMutation } from '@tanstack/react-query';
import { payrollService } from '../services/payrollService';
import type { PayrollRunRequest, PayrollRunSummary } from '../services/payrollService';

export function usePayroll() {
  const runPayrollMutation = useMutation({
    mutationFn: (request: PayrollRunRequest) => payrollService.runPayroll(request),
  });

  return {
    runPayroll: runPayrollMutation.mutate,
    isRunning: runPayrollMutation.isPending,
    result: runPayrollMutation.data,
    error: runPayrollMutation.error,
    reset: runPayrollMutation.reset,
  };
}