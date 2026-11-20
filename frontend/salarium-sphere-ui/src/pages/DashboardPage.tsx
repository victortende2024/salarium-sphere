import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { usePayroll } from '../hooks/usePayroll';
import { useEmployees } from '../hooks/useEmployees';
import { useLatestPayslip } from '../hooks/usePayslip';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { apiClient } from '../lib/api';
import { Users, Briefcase, FileText, Plus, DollarSign, PieChart, TrendingUp, User } from 'lucide-react';

interface PayrollHistoryItem {
  payrollRunId: string;
  payPeriodStart: string;
  payPeriodEnd: string;
  totalGrossPay: number;
  totalNetPay: number;
  status: string;
  totalEmployees: number;
  completedAtUtc: string;
}

export function DashboardPage() {
  const navigate = useNavigate();
  const { userRole, employeeId, logout } = useAuth();
  const { runPayroll, isRunning, result, error, reset } = usePayroll();
  const [history, setHistory] = useState<PayrollHistoryItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const { data: employees, isLoading: employeesLoading } = useEmployees();
  const { data: latestPayslip } = useLatestPayslip(employeeId || '');

  const companyId = '11111111-1111-1111-1111-111111111111';
  const [payPeriodStart, setPayPeriodStart] = useState('2026-08-01');
  const [payPeriodEnd, setPayPeriodEnd] = useState('2026-08-31');

  // Fetch payroll history
  useEffect(() => {
    const fetchHistory = async () => {
      try {
        const response = await apiClient.get(`/payroll/history?companyId=${companyId}`);
        const mappedHistory = response.data.map((item: any) => ({
          payrollRunId: item.id || item.payrollRunId,
          payPeriodStart: item.payPeriodStart,
          payPeriodEnd: item.payPeriodEnd,
          totalGrossPay: item.totalGrossPay,
          totalNetPay: item.totalNetPay,
          status: item.status,
          totalEmployees: item.totalEmployees,
          completedAtUtc: item.completedAtUtc,
        }));
        setHistory(mappedHistory);
      } catch (error) {
        console.error('Failed to fetch payroll history:', error);
      } finally {
        setIsLoading(false);
      }
    };
    fetchHistory();
  }, [companyId]);

  // Compute latest payroll totals
  const latestRun = history.length > 0 ? history[0] : null;

  // Compute department breakdown (only for Admin/PayrollManager)
 const departmentBreakdown: Record<string, number> = employees
    ? employees.reduce<Record<string, number>>((acc, emp) => {
        const dept = emp.departmentName || 'Unassigned';
        acc[dept] = (acc[dept] || 0) + 1;
        return acc;
      }, {})
    : {};

  const handleRunPayroll = () => {
    runPayroll({
      companyId,
      payPeriodStart: new Date(payPeriodStart).toISOString(),
      payPeriodEnd: new Date(payPeriodEnd).toISOString(),
    });
  };

  return (
    <div className="p-8 max-w-6xl mx-auto">
      {/* Header */}
      <div className="flex justify-between items-center mb-8">
        <h1 className="text-3xl font-bold">Salarium Sphere Dashboard</h1>
        <Button variant="outline" onClick={logout}>Logout</Button>
      </div>

      {/* ======== ANALYTICS SECTION ======== */}
      <div className="mb-8">
        {/* Role‑based heading */}
        {userRole === 'Employee' ? (
          <h2 className="text-xl font-semibold mb-4">My Payroll Summary</h2>
        ) : (
          <h2 className="text-xl font-semibold mb-4">Company Analytics</h2>
        )}

        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {userRole === 'Employee' ? (
            // --- Employee Personal Summary ---
            latestPayslip ? (
              <>
                <Card className="bg-green-50 border-green-200">
                  <CardContent className="p-4">
                    <p className="text-sm text-muted-foreground">Latest Payslip Period</p>
                    <p className="font-semibold">{latestPayslip.payPeriod}</p>
                  </CardContent>
                </Card>
                <Card className="bg-blue-50 border-blue-200">
                  <CardContent className="p-4">
                    <p className="text-sm text-muted-foreground">Gross Pay</p>
                    <p className="font-semibold text-blue-700">${latestPayslip.grossPay.toFixed(2)}</p>
                  </CardContent>
                </Card>
                <Card className="bg-red-50 border-red-200">
                  <CardContent className="p-4">
                    <p className="text-sm text-muted-foreground">Taxes & Deductions</p>
                    <p className="font-semibold text-red-700">
                      ${(latestPayslip.totalTax + latestPayslip.totalDeductions).toFixed(2)}
                    </p>
                  </CardContent>
                </Card>
                <Card className="bg-green-50 border-green-200">
                  <CardContent className="p-4">
                    <p className="text-sm text-muted-foreground">Net Pay</p>
                    <p className="font-semibold text-green-700">${latestPayslip.netPay.toFixed(2)}</p>
                  </CardContent>
                </Card>
              </>
            ) : (
              <Card className="col-span-4 text-center p-4 text-muted-foreground">
                No payslip found for you yet. Please contact payroll.
              </Card>
            )
          ) : (
            // --- Admin / Payroll Manager Company Analytics ---
            <>
              <Card>
                <CardContent className="p-4 flex items-center justify-between">
                  <div>
                    <p className="text-sm text-muted-foreground">Total Employees</p>
                    <p className="text-2xl font-bold">{employees?.length || 0}</p>
                  </div>
                  <Users className="h-8 w-8 text-royal-blue" />
                </CardContent>
              </Card>
              <Card>
                <CardContent className="p-4 flex items-center justify-between">
                  <div>
                    <p className="text-sm text-muted-foreground">Latest Gross Pay</p>
                    <p className="text-2xl font-bold">
                      ${latestRun ? latestRun.totalGrossPay.toFixed(2) : '0.00'}
                    </p>
                  </div>
                  <DollarSign className="h-8 w-8 text-royal-blue" />
                </CardContent>
              </Card>
              <Card>
                <CardContent className="p-4 flex items-center justify-between">
                  <div>
                    <p className="text-sm text-muted-foreground">Latest Net Pay</p>
                    <p className="text-2xl font-bold">
                      ${latestRun ? latestRun.totalNetPay.toFixed(2) : '0.00'}
                    </p>
                  </div>
                  <TrendingUp className="h-8 w-8 text-royal-blue" />
                </CardContent>
              </Card>
              <Card>
                <CardContent className="p-4 flex items-center justify-between">
                  <div>
                    <p className="text-sm text-muted-foreground">Avg Net Pay / Employee</p>
                    <p className="text-2xl font-bold">
                      ${latestRun && latestRun.totalEmployees > 0
                        ? (latestRun.totalNetPay / latestRun.totalEmployees).toFixed(2)
                        : '0.00'}
                    </p>
                  </div>
                  <PieChart className="h-8 w-8 text-royal-blue" />
                </CardContent>
              </Card>
            </>
          )}
        </div>

        {/* Department Breakdown – visible only to Admin/PayrollManager */}
        {userRole !== 'Employee' && Object.keys(departmentBreakdown).length > 0 && (
          <Card className="mt-4">
            <CardHeader>
              <CardTitle>Department Headcount</CardTitle>
              <CardDescription>Number of employees per department</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                {Object.entries(departmentBreakdown).map(([dept, count]) => (
                  <div key={dept} className="flex items-center gap-2">
                    <span className="w-1/3 text-sm">{dept}</span>
                    <div className="flex-1 h-4 bg-gray-200 rounded-full overflow-hidden">
                      <div
                        className="h-full bg-royal-blue rounded-full"
                        style={{
                          width: `${((count as number) / (employees?.length || 1)) * 100}%`,
                        }}
                      />
                    </div>
                    <span className="text-sm font-medium">{count}</span>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        )}
      </div>

      {/* ======== NAVIGATION CARDS ======== */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
        {userRole !== 'Employee' && (
          <>
            <Card
              className="cursor-pointer hover:shadow-lg transition-shadow border-royal-blue/20 hover:border-royal-blue"
              onClick={() => navigate('/employees')}
            >
              <CardContent className="p-6 flex flex-col items-center text-center">
                <Users className="h-8 w-8 text-royal-blue mb-2" />
                <h3 className="font-semibold">Employees</h3>
                <p className="text-sm text-muted-foreground">Manage your workforce</p>
              </CardContent>
            </Card>
            <Card
              className="cursor-pointer hover:shadow-lg transition-shadow border-royal-blue/20 hover:border-royal-blue"
              onClick={() => navigate('/employees/new')}
            >
              <CardContent className="p-6 flex flex-col items-center text-center">
                <Plus className="h-8 w-8 text-royal-blue mb-2" />
                <h3 className="font-semibold">Add Employee</h3>
                <p className="text-sm text-muted-foreground">Create new employee record</p>
              </CardContent>
            </Card>
            <Card
              className="cursor-pointer hover:shadow-lg transition-shadow border-royal-blue/20 hover:border-royal-blue"
              onClick={() => navigate('/profiles')}
            >
              <CardContent className="p-6 flex flex-col items-center text-center">
                <Briefcase className="h-8 w-8 text-royal-blue mb-2" />
                <h3 className="font-semibold">Profiles</h3>
                <p className="text-sm text-muted-foreground">Manage payroll profiles</p>
              </CardContent>
            </Card>
          </>
        )}
        <Card
          className="cursor-pointer hover:shadow-lg transition-shadow border-royal-blue/20 hover:border-royal-blue"
          onClick={() => navigate('/payslips')}
        >
          <CardContent className="p-6 flex flex-col items-center text-center">
            <FileText className="h-8 w-8 text-royal-blue mb-2" />
            <h3 className="font-semibold">Payslips</h3>
            <p className="text-sm text-muted-foreground">View and generate payslips</p>
          </CardContent>
        </Card>
      </div>

      {/* ======== PAYROLL ACTIONS (Admin/Manager only) ======== */}
      {userRole !== 'Employee' && (
        <div className="grid gap-6">
          {/* Run Payroll Card */}
          <Card>
            <CardHeader>
              <CardTitle>Run Payroll</CardTitle>
              <CardDescription>
                Select pay period and run payroll for your company.
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label htmlFor="start">Period Start</Label>
                  <Input
                    id="start"
                    type="date"
                    value={payPeriodStart}
                    onChange={(e) => setPayPeriodStart(e.target.value)}
                  />
                </div>
                <div>
                  <Label htmlFor="end">Period End</Label>
                  <Input
                    id="end"
                    type="date"
                    value={payPeriodEnd}
                    onChange={(e) => setPayPeriodEnd(e.target.value)}
                  />
                </div>
              </div>
              <Button
                onClick={handleRunPayroll}
                disabled={isRunning}
                className="mt-4 bg-royal-blue hover:bg-[#3558c7]"
              >
                {isRunning ? 'Running Payroll...' : 'Run Payroll'}
              </Button>
            </CardContent>
          </Card>

          {/* Results Card */}
          {result && (
            <Card>
              <CardHeader>
                <CardTitle>Payroll Run Summary</CardTitle>
                <CardDescription>Run ID: {result.payrollRunId}</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  <div>
                    <p className="text-sm text-muted-foreground">Status</p>
                    <p className="font-semibold">{result.status}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Total Employees</p>
                    <p className="font-semibold">{result.totalEmployees}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Successful</p>
                    <p className="font-semibold text-green-600">{result.successfulEmployees}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Failed</p>
                    <p className="font-semibold text-red-600">{result.failedEmployees}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Total Gross Pay</p>
                    <p className="font-semibold">${result.totalGrossPay.toFixed(2)}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Total Net Pay</p>
                    <p className="font-semibold">${result.totalNetPay.toFixed(2)}</p>
                  </div>
                </div>
                {result.errors.length > 0 && (
                  <div className="mt-4 p-4 bg-red-50 border border-red-200 rounded">
                    <p className="font-semibold text-red-700">Errors:</p>
                    <ul className="list-disc list-inside text-sm text-red-600">
                      {result.errors.map((err, idx) => (
                        <li key={idx}>{err}</li>
                      ))}
                    </ul>
                  </div>
                )}
              </CardContent>
            </Card>
          )}

          {/* History Card */}
          <Card>
            <CardHeader className="flex flex-row justify-between items-center">
              <div>
                <CardTitle>Payroll History</CardTitle>
                <CardDescription>Recent payroll runs for your company</CardDescription>
              </div>
              <Button variant="outline" size="sm" onClick={() => navigate('/payroll/history')}>
                View All
              </Button>
            </CardHeader>
            <CardContent>
              {isLoading ? (
                <p>Loading history...</p>
              ) : history.length === 0 ? (
                <p className="text-muted-foreground">No payroll runs found.</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b">
                        <th className="text-left py-2 px-3">Run ID</th>
                        <th className="text-left py-2 px-3">Period</th>
                        <th className="text-right py-2 px-3">Employees</th>
                        <th className="text-right py-2 px-3">Gross Pay</th>
                        <th className="text-right py-2 px-3">Net Pay</th>
                        <th className="text-left py-2 px-3">Status</th>
                      </tr>
                    </thead>
                    <tbody>
                      {history.slice(0, 5).map((item) => (
                        <tr
                          key={item.payrollRunId}
                          className="border-b cursor-pointer hover:bg-gray-50"
                          onClick={() => navigate(`/payroll/runs/${item.payrollRunId}`)}
                        >
                          <td className="py-2 px-3 font-mono text-xs">
                            {item.payrollRunId.substring(0, 8)}...
                          </td>
                          <td className="py-2 px-3">
                            {new Date(item.payPeriodStart).toLocaleDateString()} - {new Date(item.payPeriodEnd).toLocaleDateString()}
                          </td>
                          <td className="text-right py-2 px-3">{item.totalEmployees}</td>
                          <td className="text-right py-2 px-3">${item.totalGrossPay.toFixed(2)}</td>
                          <td className="text-right py-2 px-3">${item.totalNetPay.toFixed(2)}</td>
                          <td className="py-2 px-3">
                            <span className={`px-2 py-1 rounded-full text-xs ${
                              item.status === 'Completed' ? 'bg-green-100 text-green-800' : 'bg-yellow-100 text-yellow-800'
                            }`}>
                              {item.status}
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      )}

      {error && (
        <Card className="border-red-500 mt-4">
          <CardHeader>
            <CardTitle className="text-red-600">Error</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-red-500">{(error as Error).message}</p>
            <Button variant="outline" className="mt-2" onClick={reset}>
              Dismiss
            </Button>
          </CardContent>
        </Card>
      )}
    </div>
  );
}