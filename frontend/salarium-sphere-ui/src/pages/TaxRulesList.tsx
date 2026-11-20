import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type{  TaxRule } from '../services/taxRuleService';
import{taxRuleService} from '../services/taxRuleService';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { toast } from 'sonner';
import { Plus, Edit, Trash2 } from 'lucide-react';
import { TaxRuleForm } from './TaxRuleForm';
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

export function TaxRulesList() {
  const [editingRule, setEditingRule] = useState<TaxRule | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const queryClient = useQueryClient();

  const { data: rules, isLoading } = useQuery({
    queryKey: ['taxRules'],
    queryFn: taxRuleService.getAll,
  });

  const deleteMutation = useMutation({
    mutationFn: taxRuleService.delete,
    onSuccess: () => {
      toast.success('Tax rule deleted');
      queryClient.invalidateQueries({ queryKey: ['taxRules'] });
    },
    onError: () => toast.error('Failed to delete tax rule'),
  });

  const handleDelete = (id: string) => deleteMutation.mutate(id);

  if (isLoading) return <div className="p-8">Loading tax rules...</div>;

  const getTypeLabel = (type: number) => {
    switch (type) {
      case 0: return 'Fixed';
      case 1: return 'Percentage';
      case 2: return 'Tiered';
      default: return 'Unknown';
    }
  };

  return (
    <div className="p-8 max-w-6xl mx-auto">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Government Tax Rules</h1>
        <Button onClick={() => { setEditingRule(null); setIsFormOpen(true); }}>
          <Plus className="mr-2 h-4 w-4" /> Add Tax Rule
        </Button>
      </div>

      <Card>
        <CardHeader><CardTitle>Tax Rules</CardTitle></CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead>Type</TableHead>
                <TableHead>Priority</TableHead>
                <TableHead>Mandatory</TableHead>
                <TableHead>Active</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {rules?.map((rule) => (
                <TableRow key={rule.id}>
                  <TableCell>{rule.name}</TableCell>
                  <TableCell>{getTypeLabel(rule.calculationType)}</TableCell>
                  <TableCell>{rule.priority}</TableCell>
                  <TableCell>{rule.isMandatory ? '✅' : '❌'}</TableCell>
                  <TableCell>{rule.isActive ? '✅' : '❌'}</TableCell>
                  <TableCell className="text-right">
                    <Button variant="ghost" size="sm" onClick={() => { setEditingRule(rule); setIsFormOpen(true); }}>
                      <Edit className="h-4 w-4" />
                    </Button>
                    <AlertDialog>
                      <AlertDialogTrigger asChild>
                        <Button variant="ghost" size="sm"><Trash2 className="h-4 w-4 text-red-500" /></Button>
                      </AlertDialogTrigger>
                      <AlertDialogContent>
                        <AlertDialogHeader>
                          <AlertDialogTitle>Delete Tax Rule</AlertDialogTitle>
                          <AlertDialogDescription>
                            Are you sure you want to delete "{rule.name}"? This action cannot be undone.
                          </AlertDialogDescription>
                        </AlertDialogHeader>
                        <AlertDialogFooter>
                          <AlertDialogCancel>Cancel</AlertDialogCancel>
                          <AlertDialogAction onClick={() => handleDelete(rule.id)}>Delete</AlertDialogAction>
                        </AlertDialogFooter>
                      </AlertDialogContent>
                    </AlertDialog>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <TaxRuleForm
        open={isFormOpen}
        onOpenChange={setIsFormOpen}
        initialData={editingRule}
        onSuccess={() => {
          queryClient.invalidateQueries({ queryKey: ['taxRules'] });
          setIsFormOpen(false);
          setEditingRule(null);
        }}
      />
    </div>
  );
}