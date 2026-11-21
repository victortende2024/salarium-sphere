import { useEffect, useState } from 'react';
import { useAuth } from '../hooks/useAuth';
import { payslipService } from '../services/payslipService';
import type { Payslip } from '../services/payslipService';
import { useEmployees } from '../hooks/useEmployees';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { toast } from 'sonner';
import { Download, User } from 'lucide-react';

const currencyLabels: Record<number, string> = {
  0: 'USD',
  1: 'ZIG',
  2: 'EUR',
  3: 'GBP',
  4: 'ZAR',
};

export function PayslipViewer() {
  
  const { userRole, logout, employeeId } = useAuth();
  const [selectedEmployeeId, setSelectedEmployeeId] = useState<string>('');
  const [payslips, setPayslips] = useState<Payslip[]>([]);
  const [selectedPayslip, setSelectedPayslip] = useState<Payslip | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const { data: employees } = useEmployees();

  const fetchPayslips = async (empId: string) => {
    setIsLoading(true);
    try {
      const data = await payslipService.getEmployeePayslips(empId);
      setPayslips(data);
      if (data.length > 0) {
        setSelectedPayslip(data[0]);
      } else {
        setSelectedPayslip(null);
      }
    } catch (error) {
      toast.error('Failed to load payslips');
      console.error(error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleEmployeeChange = (value: string | null) => {
    if (!value) return;
    setSelectedEmployeeId(value);
    fetchPayslips(value);
  };

  const handlePayslipChange = (value: string | null) => {
    if (!value) return;
    const found = payslips.find((p) => p.payPeriod === value);
    setSelectedPayslip(found || null);
  };

  // Auto-load for Employee role; for Managers/Admins, load first employee from list
 useEffect(() => {
  if (userRole === 'Employee') {
    console.log('Employee ID from auth:', employeeId);
    if (employeeId) {
      setSelectedEmployeeId(employeeId);
      fetchPayslips(employeeId);
    }
  } else if (employees && employees.length > 0 && !selectedEmployeeId) {
    const firstEmployee = employees[0];
    setSelectedEmployeeId(firstEmployee.id);
    fetchPayslips(firstEmployee.id);
  }
}, [userRole, employeeId, employees, selectedEmployeeId]);

  const handleDownloadPdf = async () => {
    if (!selectedPayslip) return;
    try {
      const blob = await payslipService.downloadPayslipPdf(
        selectedPayslip.employeeId,
        selectedPayslip.payrollRunId
      );
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `payslip_${selectedPayslip.employeeCode}_${selectedPayslip.payPeriod.replace(/\s/g, '_')}.pdf`;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      toast.error('Failed to download PDF');
      console.error(error);
    }
  };

  return (
    <div className="p-8 max-w-5xl mx-auto">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Payslip Viewer</h1>
        <Button variant="outline" onClick={logout}>Logout</Button>
      </div>

      {/* Employee Selector – only for managers/admins */}
      {(userRole === 'PayrollManager' || userRole === 'Admin') && (
        <div className="mb-6">
          <label className="text-sm font-medium">Select Employee</label>
          <Select value={selectedEmployeeId} onValueChange={handleEmployeeChange}>
            <SelectTrigger className="w-[300px]">
              <SelectValue placeholder="Select Employee" />
            </SelectTrigger>
            <SelectContent>
              {employees?.map((emp) => (
                <SelectItem key={emp.id} value={emp.id}>
                  {emp.firstName} {emp.lastName} ({emp.employeeCode})
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      )}

      {/* Payslip Selector */}
      {payslips.length > 0 && (
        <div className="mb-6">
          <label className="text-sm font-medium">Select Payslip Period</label>
          <Select
            value={selectedPayslip?.payPeriod || ''}
            onValueChange={handlePayslipChange}
          >
            <SelectTrigger className="w-[300px]">
              <SelectValue placeholder="Select Payslip" />
            </SelectTrigger>
            <SelectContent>
              {payslips.map((p) => (
                <SelectItem key={p.payPeriod} value={p.payPeriod}>
                  {p.payPeriod}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      )}

      {isLoading && <p>Loading payslips...</p>}

      {selectedPayslip && (
        <Card>
          <CardHeader>
            <div className="flex justify-between items-center">
              <CardTitle>Payslip - {selectedPayslip.payPeriod}</CardTitle>
              <Button onClick={handleDownloadPdf}>
                <Download className="mr-2 h-4 w-4" /> Download PDF
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            {/* Employee Info */}
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6 p-4 bg-gray-50 rounded-lg">
              <div>
                <p className="text-sm text-muted-foreground">Employee</p>
                <p className="font-semibold">{selectedPayslip.employeeName}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Employee Code</p>
                <p className="font-semibold">{selectedPayslip.employeeCode}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Company</p>
                <p className="font-semibold">{selectedPayslip.companyName}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Department</p>
                <p className="font-semibold">{selectedPayslip.departmentName}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Profile</p>
                <p className="font-semibold">{selectedPayslip.profileName}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Currency</p>
                <p className="font-semibold">{currencyLabels[selectedPayslip.currency] || 'USD'}</p>
              </div>
            </div>

            {/* Earnings, Taxes, Deductions */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <h3 className="font-semibold text-lg mb-2">Earnings & Benefits</h3>
                <div className="space-y-2">
                  {selectedPayslip.lineItems
                    .filter((i) => i.type === 'Earning')
                    .map((item, idx) => (
                      <div key={idx} className="flex justify-between border-b py-1">
                        <span>{item.name}</span>
                        <span>${item.amount.toFixed(2)}</span>
                      </div>
                    ))}
                  <div className="flex justify-between font-bold text-lg pt-2 border-t-2">
                    <span>Gross Pay</span>
                    <span>${selectedPayslip.grossPay.toFixed(2)}</span>
                  </div>
                </div>
              </div>

              <div>
                <h3 className="font-semibold text-lg mb-2 text-red-600">Taxes & Deductions</h3>
                <div className="space-y-2">
                  {selectedPayslip.lineItems
                    .filter((i) => i.type === 'Tax' || i.type === 'Deduction')
                    .map((item, idx) => (
                      <div key={idx} className="flex justify-between border-b py-1">
                        <span className={item.type === 'Tax' ? 'text-red-600' : 'text-orange-600'}>
                          {item.name}
                        </span>
                        <span className={item.type === 'Tax' ? 'text-red-600' : 'text-orange-600'}>
                          -${item.amount.toFixed(2)}
                        </span>
                      </div>
                    ))}
                  <div className="flex justify-between font-semibold pt-2 border-t-2">
                    <span>Total Tax</span>
                    <span className="text-red-600">-${selectedPayslip.totalTax.toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between font-semibold">
                    <span>Total Deductions</span>
                    <span className="text-orange-600">-${selectedPayslip.totalDeductions.toFixed(2)}</span>
                  </div>
                </div>
              </div>
            </div>

            {/* Net Pay Summary */}
            <div className="mt-6 p-4 bg-green-50 border border-green-200 rounded-lg">
              <div className="flex justify-between items-center">
                <span className="text-lg font-bold text-green-800">Net Pay</span>
                <span className="text-2xl font-bold text-green-700">
                  ${selectedPayslip.netPay.toFixed(2)} {currencyLabels[selectedPayslip.currency] || 'USD'}
                </span>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {!isLoading && payslips.length === 0 && (
        <Card>
          <CardContent className="p-8 text-center text-muted-foreground">
            <User className="h-12 w-12 mx-auto mb-2 text-gray-300" />
            <p>No payslips found for this employee.</p>
          </CardContent>
        </Card>
      )}
    </div>
  );
}