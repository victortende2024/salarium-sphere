import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type{ EmployeeDeduction } from '../services/employeeDeductionService';
import { employeeDeductionService } from '../services/employeeDeductionService';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { toast } from 'sonner';
import { Plus, Edit, Trash2 } from 'lucide-react';
import { EmployeeDeductionForm } from './EmployeeDeductionForm';
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

interface Props {
  employeeId: string;
}

export function EmployeeDeductionList({ employeeId }: Props) {
  const [editingDeduction, setEditingDeduction] = useState<EmployeeDeduction | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const queryClient = useQueryClient();

  const { data: deductions, isLoading } = useQuery({
    queryKey: ['employeeDeductions', employeeId],
    queryFn: () => employeeDeductionService.getByEmployee(employeeId),
    enabled: !!employeeId,
  });

  const deleteMutation = useMutation({
    mutationFn: employeeDeductionService.delete,
    onSuccess: () => {
      toast.success('Deduction deleted');
      queryClient.invalidateQueries({ queryKey: ['employeeDeductions', employeeId] });
    },
    onError: () => toast.error('Failed to delete deduction'),
  });

  const handleDelete = (id: string) => deleteMutation.mutate(id);

  if (isLoading) return <div className="text-sm text-muted-foreground">Loading deductions...</div>;

  const getTypeLabel = (type: number) => {
    switch (type) {
      case 0: return 'Fixed';
      case 1: return 'Percentage';
      case 2: return 'Tiered';
      default: return 'Unknown';
    }
  };

  return (
    <Card className="mt-6">
      <CardHeader className="flex flex-row justify-between items-center">
        <CardTitle>Employee Deductions</CardTitle>
        <Button onClick={() => { setEditingDeduction(null); setIsFormOpen(true); }}>
          <Plus className="mr-2 h-4 w-4" /> Add Deduction
        </Button>
      </CardHeader>
      <CardContent>
        {deductions?.length === 0 ? (
          <p className="text-muted-foreground">No deductions for this employee.</p>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead>Type</TableHead>
                <TableHead>Amount/Percentage</TableHead>
                <TableHead>Active</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {deductions?.map((d) => (
                <TableRow key={d.id}>
                  <TableCell>{d.deductionName}</TableCell>
                  <TableCell>{getTypeLabel(d.calculationType)}</TableCell>
                  <TableCell>
                    {d.calculationType === 0 ? `$${d.fixedAmount}` :
                     d.calculationType === 1 ? `${d.percentageValue}%` :
                     'Tiered'}
                  </TableCell>
                  <TableCell>{d.isActive ? '✅' : '❌'}</TableCell>
                  <TableCell className="text-right">
                    <Button variant="ghost" size="sm" onClick={() => { setEditingDeduction(d); setIsFormOpen(true); }}>
                      <Edit className="h-4 w-4" />
                    </Button>
                    <AlertDialog>
                      <AlertDialogTrigger asChild>
                        <Button variant="ghost" size="sm"><Trash2 className="h-4 w-4 text-red-500" /></Button>
                      </AlertDialogTrigger>
                      <AlertDialogContent>
                        <AlertDialogHeader>
                          <AlertDialogTitle>Delete Deduction</AlertDialogTitle>
                          <AlertDialogDescription>
                            Are you sure you want to delete "{d.deductionName}"? This action cannot be undone.
                          </AlertDialogDescription>
                        </AlertDialogHeader>
                        <AlertDialogFooter>
                          <AlertDialogCancel>Cancel</AlertDialogCancel>
                          <AlertDialogAction onClick={() => handleDelete(d.id)}>Delete</AlertDialogAction>
                        </AlertDialogFooter>
                      </AlertDialogContent>
                    </AlertDialog>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </CardContent>
      <EmployeeDeductionForm
        open={isFormOpen}
        onOpenChange={setIsFormOpen}
        employeeId={employeeId}
        initialData={editingDeduction}
        onSuccess={() => {
          queryClient.invalidateQueries({ queryKey: ['employeeDeductions', employeeId] });
          setIsFormOpen(false);
          setEditingDeduction(null);
        }}
      />
    </Card>
  );
}