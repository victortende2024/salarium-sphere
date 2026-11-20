import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useEmployee, useCreateEmployee, useUpdateEmployee } from '../hooks/useEmployees';
import { useCompanies } from '../hooks/useCompanies';
import { useDepartments } from '../hooks/useDepartments';
import { useProfiles } from '../hooks/useProfiles';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { toast } from 'sonner';
import { Plus, Trash2 } from 'lucide-react';

// --- Zod Schema ---
const salaryStructureSchema = z.object({
  currency: z.number().min(0).max(10),
  amount: z.number().positive('Amount must be positive'),
  effectiveStartDate: z.string().min(1, 'Start date is required'),
  effectiveEndDate: z.string().nullable().optional(),
});

const customBenefitSchema = z.object({
  benefitName: z.string().min(1, 'Benefit name is required'),
  amount: z.number().positive('Amount must be positive'),
  currency: z.number().min(0).max(10),
  isRecurring: z.boolean().default(true),
});

const employeeFormSchema = z.object({
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  email: z.string().email('Invalid email address'),
  employeeCode: z.string().optional(),   // <-- optional, not nullable
  joinedDate: z.string().optional(),
  bankName: z.string().optional(),
  bankAccountNumber: z.string().optional(),
  bankBranchCode: z.string().optional(),
  companyId: z.string().min(1, 'Company is required'),
  departmentId: z.string().min(1, 'Department is required'),
  primaryProfileId: z.string().min(1, 'Profile is required'),
  salaryStructures: z.array(salaryStructureSchema).default([]),
  customBenefits: z.array(customBenefitSchema).default([]),
});

type EmployeeFormData = z.infer<typeof employeeFormSchema>;

export function EmployeeForm() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = !!id;

  // --- Data fetching hooks ---
  const { data: employee, isLoading: employeeLoading } = useEmployee(id || '');
  const { data: companies } = useCompanies();
  const { data: profiles } = useProfiles();

  // --- Form setup ---
 const form = useForm<EmployeeFormData>({
  resolver: zodResolver(employeeFormSchema) as any, // <-- This resolves the type mismatch
  defaultValues: {
    firstName: '',
    lastName: '',
    email: '',
    employeeCode: '',
    joinedDate: '',
    bankName: '',
    bankAccountNumber: '',
    bankBranchCode: '',
    companyId: '',
    departmentId: '',
    primaryProfileId: '',
    salaryStructures: [{ currency: 0, amount: 0, effectiveStartDate: new Date().toISOString().split('T')[0] }],
    customBenefits: [],
  },
});



  const {
    register,
    control,
    handleSubmit,
    watch,
    setValue,
    formState: { errors, isSubmitting },
  } = form;

  useEffect(() => {
  // If companies are loaded and no company is selected, set the first one
  if (companies && companies.length > 0 && !watch('companyId')) {
    setValue('companyId', companies[0].id);
  }
}, [companies, watch('companyId'), setValue]);

  // --- Watch companyId for cascading departments ---
  const companyIdWatch = watch('companyId');
  const { data: departments } = useDepartments(companyIdWatch || undefined);
 console.log('Departments:', departments);

  // --- Field arrays ---
  const { fields: salaryFields, append: appendSalary, remove: removeSalary } = useFieldArray({
    control,
    name: 'salaryStructures',
  });

  const { fields: benefitFields, append: appendBenefit, remove: removeBenefit } = useFieldArray({
    control,
    name: 'customBenefits',
  });

  // --- Populate form on edit ---
  useEffect(() => {
  if (employee && isEdit) {
    setValue('firstName', employee.firstName);
    setValue('lastName', employee.lastName);
    setValue('email', employee.email);
    setValue('employeeCode', employee.employeeCode || '');
    setValue('joinedDate', employee.joinedDate ? employee.joinedDate.split('T')[0] : '');
    setValue('bankName', employee.bankName || '');
    setValue('bankAccountNumber', employee.bankAccountNumber || '');
    setValue('bankBranchCode', employee.bankBranchCode || '');
    setValue('companyId', employee.companyId);
    setValue('departmentId', employee.departmentId);
    setValue('primaryProfileId', employee.primaryProfileId);

    if (employee.salaryStructures && employee.salaryStructures.length > 0) {
      setValue('salaryStructures', employee.salaryStructures.map((s: any) => ({
        currency: s.currency,
        amount: s.amount,
        effectiveStartDate: s.effectiveStartDate.split('T')[0],
        effectiveEndDate: s.effectiveEndDate ? s.effectiveEndDate.split('T')[0] : undefined, // <-- Fixed
      })));
    }

    if (employee.customBenefits && employee.customBenefits.length > 0) {
      setValue('customBenefits', employee.customBenefits.map((b: any) => ({
        benefitName: b.benefitName,
        amount: b.amount,
        currency: b.currency,
        isRecurring: b.isRecurring,
      })));
    }
  }
}, [employee, isEdit, setValue]);

  // --- Mutations ---
  const createMutation = useCreateEmployee();
  const updateMutation = useUpdateEmployee();

 const onSubmit = async (data: EmployeeFormData) => {
  try {
    // Transform data: convert empty strings to null for optional fields
    const payload = {
      ...data,
      employeeCode: data.employeeCode?.trim() || null,
      joinedDate: data.joinedDate || null,
      bankName: data.bankName?.trim() || null,
      bankAccountNumber: data.bankAccountNumber?.trim() || null,
      bankBranchCode: data.bankBranchCode?.trim() || null,
      salaryStructures: data.salaryStructures.map((s) => ({
        ...s,
        effectiveEndDate: s.effectiveEndDate?.trim() || null,
      })),
      customBenefits: data.customBenefits.map((b) => ({
        ...b,
        benefitName: b.benefitName.trim(),
      })),
    };

    if (isEdit) {
      await updateMutation.mutateAsync({ id: id!, ...payload });
      toast.success('Employee updated successfully');
    } else {
      await createMutation.mutateAsync(payload);
      toast.success('Employee created successfully');
    }
    navigate('/employees');
  } catch (error) {
    toast.error('Failed to save employee: ' + (error as Error).message);
  }
};

  if (employeeLoading && isEdit) {
    return <div className="p-8">Loading employee...</div>;
  }

  // Helper to handle Select value change (string | null)
  const handleSelectChange = (field: keyof EmployeeFormData) => (value: string | null) => {
    if (value !== null) {
      setValue(field, value as any);
    }
  };

  return (
    <div className="p-8 max-w-4xl mx-auto">
      <h1 className="text-2xl font-bold mb-6">{isEdit ? 'Edit Employee' : 'Add New Employee'}</h1>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        {/* Basic Information */}
        <Card>
          <CardHeader>
            <CardTitle>Basic Information</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label>First Name *</Label>
              <Input {...register('firstName')} />
              {errors.firstName && <p className="text-red-500 text-sm">{errors.firstName.message}</p>}
            </div>
            <div>
              <Label>Last Name *</Label>
              <Input {...register('lastName')} />
              {errors.lastName && <p className="text-red-500 text-sm">{errors.lastName.message}</p>}
            </div>
            <div>
              <Label>Email *</Label>
              <Input type="email" {...register('email')} />
              {errors.email && <p className="text-red-500 text-sm">{errors.email.message}</p>}
            </div>
            <div>
              <Label>Employee Code</Label>
              <Input {...register('employeeCode')} placeholder="Auto-generated if empty" />
            </div>
            <div>
              <Label>Joined Date</Label>
              <Input type="date" {...register('joinedDate')} />
            </div>
          </CardContent>
        </Card>

        {/* Bank Details */}
        <Card>
          <CardHeader>
            <CardTitle>Bank Details</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <Label>Bank Name</Label>
              <Input {...register('bankName')} />
            </div>
            <div>
              <Label>Account Number</Label>
              <Input {...register('bankAccountNumber')} />
            </div>
            <div>
              <Label>Branch Code</Label>
              <Input {...register('bankBranchCode')} />
            </div>
          </CardContent>
        </Card>

     
        {/* Organizational Assignment */}
<Card>
  <CardHeader>
    <CardTitle>Organizational Assignment</CardTitle>
  </CardHeader>
  <CardContent>
    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
      <div>
        <Label>Company *</Label>
        <Select
          value={watch('companyId')}
          onValueChange={handleSelectChange('companyId')}
        >
          <SelectTrigger className="w-full">
            <SelectValue placeholder="Select Company" />
          </SelectTrigger>
          <SelectContent className="max-h-[200px]">
            {companies?.map((c) => (
              <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>
            ))}
          </SelectContent>
        </Select>
        {errors.companyId && <p className="text-red-500 text-sm mt-1">{errors.companyId.message}</p>}
      </div>
      <div>
        <Label>Department *</Label>
                    <Select
                value={watch('departmentId')}
                onValueChange={handleSelectChange('departmentId')}
                disabled={!watch('companyId')}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select Department" />
                </SelectTrigger>
                <SelectContent>
                  {departments?.map((d) => (
                    <SelectItem key={d.id} value={d.id}>{d.name}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
        {errors.departmentId && <p className="text-red-500 text-sm mt-1">{errors.departmentId.message}</p>}
      </div>
      <div>
        <Label>Payroll Profile *</Label>
        <Select
          value={watch('primaryProfileId')}
          onValueChange={handleSelectChange('primaryProfileId')}
        >
          <SelectTrigger className="w-full">
            <SelectValue placeholder="Select Profile" />
          </SelectTrigger>
          <SelectContent className="max-h-[200px]">
            {profiles?.map((p) => (
              <SelectItem key={p.id} value={p.id}>{p.name}</SelectItem>
            ))}
          </SelectContent>
        </Select>
        {errors.primaryProfileId && <p className="text-red-500 text-sm mt-1">{errors.primaryProfileId.message}</p>}
      </div>
    </div>
  </CardContent>
</Card>

        {/* Salary Structures */}
        <Card>
          <CardHeader className="flex flex-row justify-between items-center">
            <CardTitle>Salary Structures</CardTitle>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() =>
                appendSalary({
                  currency: 0,
                  amount: 0,
                  effectiveStartDate: new Date().toISOString().split('T')[0],
                })
              }
            >
              <Plus className="h-4 w-4 mr-1" /> Add Salary
            </Button>
          </CardHeader>
          <CardContent>
            {salaryFields.map((field, index) => (
              <div key={field.id} className="grid grid-cols-1 md:grid-cols-4 gap-4 border-b pb-4 mb-4">
                <div>
                  <Label>Currency</Label>
                  <Select
                    value={String(watch(`salaryStructures.${index}.currency`))}
                    onValueChange={(val) => {
                      if (val !== null) setValue(`salaryStructures.${index}.currency`, Number(val));
                    }}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Currency" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="0">USD</SelectItem>
                      <SelectItem value="1">ZIG</SelectItem>
                      <SelectItem value="2">EUR</SelectItem>
                      <SelectItem value="3">GBP</SelectItem>
                      <SelectItem value="4">ZAR</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div>
                  <Label>Amount</Label>
                  <Input
                    type="number"
                    step="0.01"
                    {...register(`salaryStructures.${index}.amount`, { valueAsNumber: true })}
                  />
                </div>
                <div>
                  <Label>Start Date</Label>
                  <Input type="date" {...register(`salaryStructures.${index}.effectiveStartDate`)} />
                </div>
                <div>
                  <Label>End Date (optional)</Label>
                  <Input type="date" {...register(`salaryStructures.${index}.effectiveEndDate`)} />
                </div>
                <div className="md:col-span-4 flex justify-end">
                  <Button type="button" variant="destructive" size="sm" onClick={() => removeSalary(index)}>
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>

        {/* Custom Benefits */}
        <Card>
          <CardHeader className="flex flex-row justify-between items-center">
            <CardTitle>Custom Benefits</CardTitle>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() =>
                appendBenefit({
                  benefitName: '',
                  amount: 0,
                  currency: 0,
                  isRecurring: true,
                })
              }
            >
              <Plus className="h-4 w-4 mr-1" /> Add Benefit
            </Button>
          </CardHeader>
          <CardContent>
            {benefitFields.map((field, index) => (
              <div key={field.id} className="grid grid-cols-1 md:grid-cols-5 gap-4 border-b pb-4 mb-4">
                <div className="md:col-span-2">
                  <Label>Benefit Name</Label>
                  <Input {...register(`customBenefits.${index}.benefitName`)} placeholder="e.g., Fuel Allowance" />
                </div>
                <div>
                  <Label>Amount</Label>
                  <Input
                    type="number"
                    step="0.01"
                    {...register(`customBenefits.${index}.amount`, { valueAsNumber: true })}
                  />
                </div>
                <div>
                  <Label>Currency</Label>
                  <Select
                    value={String(watch(`customBenefits.${index}.currency`))}
                    onValueChange={(val) => {
                      if (val !== null) setValue(`customBenefits.${index}.currency`, Number(val));
                    }}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Currency" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="0">USD</SelectItem>
                      <SelectItem value="1">ZIG</SelectItem>
                      <SelectItem value="2">EUR</SelectItem>
                      <SelectItem value="3">GBP</SelectItem>
                      <SelectItem value="4">ZAR</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="flex items-end">
                  <label className="flex items-center gap-2">
                    <input type="checkbox" {...register(`customBenefits.${index}.isRecurring`)} />
                    Recurring
                  </label>
                  <Button type="button" variant="destructive" size="sm" className="ml-2" onClick={() => removeBenefit(index)}>
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>

        <div className="flex gap-4">
          <Button type="submit" disabled={isSubmitting} className="bg-royal-blue hover:bg-[#3558c7]">
            {isSubmitting ? 'Saving...' : (isEdit ? 'Update Employee' : 'Create Employee')}
          </Button>
          <Button type="button" variant="outline" onClick={() => navigate('/employees')}>
            Cancel
          </Button>
        </div>
      </form>
    </div>
  );
}