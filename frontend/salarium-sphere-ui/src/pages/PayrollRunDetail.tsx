import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { apiClient } from '../lib/api';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';

interface PayrollRun {
  id: string;
  payPeriodStart: string;
  payPeriodEnd: string;
  status: string;
  totalEmployees: number;
  totalGrossPay: number;
  totalNetPay: number;
  completedAtUtc: string;
}

interface PayrollRunDetail {
  employeeId: string;
  firstName: string;
  lastName: string;
  employeeCode: string;
  currency: number;
  grossPay: number;
  totalDeductions: number;
  netPay: number;
  lineItemsJson: string;
}

const currencyLabels: Record<number, string> = {
  0: 'USD',
  1: 'ZIG',
  2: 'EUR',
  3: 'GBP',
  4: 'ZAR',
};

export function PayrollRunDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [run, setRun] = useState<PayrollRun | null>(null);
  const [details, setDetails] = useState<PayrollRunDetail[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        // Fetch the payroll run header
        const runResponse = await apiClient.get(`/payroll/runs/${id}`);
        setRun(runResponse.data);

        // Fetch the details
        const detailsResponse = await apiClient.get(`/payroll/runs/${id}/details`);
        console.log('Raw details:', detailsResponse.data);
        setDetails(detailsResponse.data);
      } catch (error) {
        toast.error('Failed to load payroll run details');
        console.error(error);
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      fetchData();
    }
  }, [id]);

  if (loading) {
    return <div className="p-8">Loading payroll details...</div>;
  }

  if (!run) {
    return <div className="p-8 text-red-500">Payroll run not found</div>;
  }

  return (
    <div className="p-8 max-w-6xl mx-auto">
      <div className="flex items-center gap-4 mb-6">
        <Button variant="outline" onClick={() => navigate('/payroll/history')}>
          <ArrowLeft className="h-4 w-4 mr-2" /> Back
        </Button>
        <h1 className="text-2xl font-bold">Payroll Run Details</h1>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <Card>
          <CardContent className="p-4">
            <p className="text-sm text-muted-foreground">Period</p>
            <p className="font-semibold">
              {new Date(run.payPeriodStart).toLocaleDateString()} - {new Date(run.payPeriodEnd).toLocaleDateString()}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <p className="text-sm text-muted-foreground">Total Employees</p>
            <p className="font-semibold">{run.totalEmployees}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <p className="text-sm text-muted-foreground">Total Net Pay</p>
            <p className="font-semibold">${run.totalNetPay.toFixed(2)}</p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Employee Breakdown</CardTitle>
          <CardDescription>Detailed payroll calculation per employee</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b">
                  <th className="text-left py-2 px-3">Employee</th>
                  <th className="text-right py-2 px-3">Gross Pay</th>
                  <th className="text-right py-2 px-3">Taxes</th>
                  <th className="text-right py-2 px-3">Deductions</th>
                  <th className="text-right py-2 px-3">Net Pay</th>
                  <th className="text-left py-2 px-3">Currency</th>
                </tr>
              </thead>
              <tbody>
                {details.map((detail) => {
                  // Parse line items to show breakdown
                  let lineItems: any[] = [];
                  try {
                    if (detail.lineItemsJson) {
                      lineItems = JSON.parse(detail.lineItemsJson);
                    }
                  } catch (e) {
                    console.error('Failed to parse line items for', detail.employeeId, e);
                  }

                  // Helper to get value safely (case-insensitive)
                  const getValue = (item: any, key: string) => {
                    const lowerKey = key.toLowerCase();
                    for (const prop of Object.keys(item)) {
                      if (prop.toLowerCase() === lowerKey) {
                        return item[prop];
                      }
                    }
                    return undefined;
                  };

                  // Calculate taxes and deductions (case-insensitive)
                  const taxes = lineItems
                    .filter((i: any) => {
                      const type = getValue(i, 'type') || getValue(i, 'Type');
                      return type === 'Tax';
                    })
                    .reduce((sum: number, i: any) => sum + (getValue(i, 'amount') || getValue(i, 'Amount') || 0), 0);

                  const deductions = lineItems
                    .filter((i: any) => {
                      const type = getValue(i, 'type') || getValue(i, 'Type');
                      return type === 'Deduction';
                    })
                    .reduce((sum: number, i: any) => sum + (getValue(i, 'amount') || getValue(i, 'Amount') || 0), 0);

                  // Fallback: if still zero but totalDeductions > 0, use totalDeductions
                  const finalTaxes = taxes;
                  const finalDeductions = deductions > 0 ? deductions : (detail.totalDeductions || 0);

                  return (
                    <tr key={detail.employeeId} className="border-b">
                      <td className="py-2 px-3">
                        <div className="font-medium">{detail.firstName} {detail.lastName}</div>
                        <div className="text-xs text-muted-foreground">{detail.employeeCode}</div>
                      </td>
                      <td className="text-right py-2 px-3">${detail.grossPay.toFixed(2)}</td>
                      <td className="text-right py-2 px-3 text-red-600">${finalTaxes.toFixed(2)}</td>
                      <td className="text-right py-2 px-3 text-orange-600">${finalDeductions.toFixed(2)}</td>
                      <td className="text-right py-2 px-3 font-semibold text-green-700">${detail.netPay.toFixed(2)}</td>
                      <td className="py-2 px-3">{currencyLabels[detail.currency] || detail.currency}</td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}