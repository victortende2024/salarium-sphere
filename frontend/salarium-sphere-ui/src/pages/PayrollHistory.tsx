import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiClient } from '../lib/api';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { ArrowLeft, Search } from 'lucide-react';
import { toast } from 'sonner';

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

export function PayrollHistory() {
  const navigate = useNavigate();
  const [history, setHistory] = useState<PayrollHistoryItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');

  const companyId = '11111111-1111-1111-1111-111111111111';

 useEffect(() => {
  const fetchHistory = async () => {
    try {
      const response = await apiClient.get(`/payroll/history?companyId=${companyId}`);
      
      // ✅ Map backend response to frontend interface
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
      toast.error('Failed to load payroll history');
    } finally {
      setIsLoading(false);
    }
  };
  fetchHistory();
}, []);

  const filtered = history.filter(item =>
    item.payrollRunId.toLowerCase().includes(search.toLowerCase()) ||
    new Date(item.payPeriodStart).toLocaleDateString().includes(search) ||
    new Date(item.payPeriodEnd).toLocaleDateString().includes(search)
  );

  return (
    <div className="p-8 max-w-6xl mx-auto">
      <div className="flex items-center gap-4 mb-6">
        <Button variant="outline" onClick={() => navigate('/dashboard')}>
          <ArrowLeft className="h-4 w-4 mr-2" /> Back
        </Button>
        <h1 className="text-2xl font-bold">Payroll History</h1>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>All Payroll Runs</CardTitle>
          <CardDescription>View all payroll runs for your company</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-2 mb-4">
            <div className="relative flex-1">
              <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search by Run ID or period..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="pl-8"
              />
            </div>
          </div>

          {isLoading ? (
            <p>Loading history...</p>
          ) : filtered.length === 0 ? (
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
                  {filtered.map((item) => (
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
  );
}