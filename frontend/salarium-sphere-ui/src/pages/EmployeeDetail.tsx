import { useParams, useNavigate } from 'react-router-dom';
import { useEmployee, useDeleteEmployee } from '../hooks/useEmployees';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { ArrowLeft, Edit, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '@/components/ui/alert-dialog';
import { EmployeeDeductionList } from '../components/EmployeeDeductionList';
// Currency labels
const currencyLabels: Record<number, string> = {
  0: 'USD',
  1: 'ZIG',
  2: 'EUR',
  3: 'GBP',
  4: 'ZAR',
};

// Define types for the employee data
interface SalaryStructure {
  id: string;
  currency: number;
  amount: number;
  effectiveStartDate: string;
   effectiveEndDate?: string | null;
}

interface CustomBenefit {
  id: string;
  benefitName: string;
  amount: number;
  currency: number;
  isRecurring: boolean;
}

interface Employee {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  employeeCode: string;
  joinedDate?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankBranchCode?: string;
  companyName: string;
  departmentName: string;
  profileName: string;
  isActive: boolean;
  salaryStructures: SalaryStructure[];
  customBenefits: CustomBenefit[];
}

export function EmployeeDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: employee, isLoading, error } = useEmployee(id || '');
  const deleteMutation = useDeleteEmployee();

  if (isLoading) return <div className="p-8">Loading employee...</div>;
  if (error || !employee) return <div className="p-8 text-red-500">Employee not found</div>;

  const handleDelete = () => {
    deleteMutation.mutate(employee.id, {
      onSuccess: () => {
        toast.success(`Employee ${employee.firstName} ${employee.lastName} deleted successfully`);
        navigate('/employees');
      },
      onError: (err: Error) => toast.error('Failed to delete employee: ' + err.message),
    });
  };

  return (
    <div className="p-8 max-w-4xl mx-auto">
      <div className="flex justify-between items-center mb-6">
        <div className="flex items-center gap-4">
          <Button variant="outline" onClick={() => navigate('/employees')}>
            <ArrowLeft className="h-4 w-4 mr-2" /> Back
          </Button>
          <h1 className="text-2xl font-bold">{employee.firstName} {employee.lastName}</h1>
        </div>
        <div className="flex gap-2">
          <Button onClick={() => navigate(`/employees/${employee.id}/edit`)}>
            <Edit className="h-4 w-4 mr-2" /> Edit
          </Button>
          <AlertDialog>
            <AlertDialogTrigger asChild>
              <Button variant="destructive">
                <Trash2 className="h-4 w-4 mr-2" /> Delete
              </Button>
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>Delete Employee</AlertDialogTitle>
                <AlertDialogDescription>
                  Are you sure you want to delete {employee.firstName} {employee.lastName}? This action cannot be undone.
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Cancel</AlertDialogCancel>
                <AlertDialogAction onClick={handleDelete}>Delete</AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card>
          <CardHeader><CardTitle>Personal Information</CardTitle></CardHeader>
          <CardContent className="space-y-2">
            <p><strong>Employee Code:</strong> {employee.employeeCode}</p>
            <p><strong>Email:</strong> {employee.email}</p>
            <p><strong>Joined Date:</strong> {employee.joinedDate ? new Date(employee.joinedDate).toLocaleDateString() : 'N/A'}</p>
            <p><strong>Status:</strong> {employee.isActive ? 'Active' : 'Inactive'}</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><CardTitle>Organizational Details</CardTitle></CardHeader>
          <CardContent className="space-y-2">
            <p><strong>Company:</strong> {employee.companyName}</p>
            <p><strong>Department:</strong> {employee.departmentName}</p>
            <p><strong>Payroll Profile:</strong> {employee.profileName}</p>
          </CardContent>
        </Card>

        {employee.bankName && (
          <Card>
            <CardHeader><CardTitle>Bank Details</CardTitle></CardHeader>
            <CardContent className="space-y-2">
              <p><strong>Bank:</strong> {employee.bankName}</p>
              <p><strong>Account:</strong> {employee.bankAccountNumber}</p>
              <p><strong>Branch Code:</strong> {employee.bankBranchCode}</p>
            </CardContent>
          </Card>
        )}

        <Card>
          <CardHeader><CardTitle>Salary Structures</CardTitle></CardHeader>
          <CardContent>
            {employee.salaryStructures.length === 0 ? (
              <p className="text-muted-foreground">No salary structures defined</p>
            ) : (
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th>Currency</th>
                    <th className="text-right">Amount</th>
                    <th>Start</th>
                    <th>End</th>
                  </tr>
                </thead>
                <tbody>
                  {employee.salaryStructures.map((s: SalaryStructure) => (
                    <tr key={s.id}>
                      <td>{currencyLabels[s.currency] || s.currency}</td>
                      <td className="text-right">${s.amount.toFixed(2)}</td>
                      <td>{new Date(s.effectiveStartDate).toLocaleDateString()}</td>
                      <td>{s.effectiveEndDate ? new Date(s.effectiveEndDate).toLocaleDateString() : 'Current'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </CardContent>
        </Card>

        <Card className="md:col-span-2">
          <CardHeader><CardTitle>Custom Benefits</CardTitle></CardHeader>
          <CardContent>
            {employee.customBenefits.length === 0 ? (
              <p className="text-muted-foreground">No custom benefits assigned</p>
            ) : (
              <table className="w-full text-sm">
                <thead>
                  <tr>
                    <th>Benefit</th>
                    <th className="text-right">Amount</th>
                    <th>Currency</th>
                    <th>Recurring</th>
                  </tr>
                </thead>
                <tbody>
                  {employee.customBenefits.map((b: CustomBenefit) => (
                    <tr key={b.id}>
                      <td>{b.benefitName}</td>
                      <td className="text-right">{b.amount.toFixed(2)}</td>
                      <td>{currencyLabels[b.currency] || b.currency}</td>
                      <td>{b.isRecurring ? 'Yes' : 'No'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </CardContent>
        </Card>
        <EmployeeDeductionList employeeId={employee.id} />
      </div>
    </div>
  );
}