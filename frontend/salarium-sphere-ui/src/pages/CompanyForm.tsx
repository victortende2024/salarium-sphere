import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { companyService, departmentService } from '../services/companyService';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { toast } from 'sonner';
import { Plus, Trash2, Edit } from 'lucide-react';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle, AlertDialogTrigger } from '@/components/ui/alert-dialog';

export function CompanyForm() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const isEdit = !!id;

  const { data: allCompanies } = useQuery({ queryKey: ['companies'], queryFn: companyService.getAll });
  const { data: company, isLoading } = useQuery({
    queryKey: ['company', id],
    queryFn: () => companyService.get(id!),
    enabled: isEdit,
  });

  const [name, setName] = useState('');
  const [parentCompanyId, setParentCompanyId] = useState<string>('');
  const [departments, setDepartments] = useState<{ id: string; name: string; isActive: boolean }[]>([]);
  const [editingDept, setEditingDept] = useState<{ id: string; name: string } | null>(null);

  const createCompany = useMutation({ mutationFn: companyService.create, onSuccess: () => queryClient.invalidateQueries({ queryKey: ['companies'] }) });
  const updateCompany = useMutation({ mutationFn: companyService.update, onSuccess: () => queryClient.invalidateQueries({ queryKey: ['companies'] }) });
  const deleteDept = useMutation({ mutationFn: departmentService.delete, onSuccess: () => queryClient.invalidateQueries({ queryKey: ['companies'] }) });
  const createDept = useMutation({ mutationFn: departmentService.create, onSuccess: () => queryClient.invalidateQueries({ queryKey: ['companies'] }) });
  const updateDept = useMutation({ mutationFn: departmentService.update, onSuccess: () => queryClient.invalidateQueries({ queryKey: ['companies'] }) });

  useEffect(() => {
    if (company) {
      setName(company.name);
      setParentCompanyId(company.parentCompanyId || '');
      setDepartments(company.departments || []);
    }
  }, [company]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (isEdit) {
        await updateCompany.mutateAsync({ ...company!, name, parentCompanyId: parentCompanyId || undefined });
        toast.success('Company updated');
      } else {
        await createCompany.mutateAsync({ name, parentCompanyId: parentCompanyId || undefined });
        toast.success('Company created');
      }
      navigate('/companies');
    } catch (err) {
      toast.error('Failed to save company');
    }
  };

  const handleAddDepartment = async () => {
    const deptName = prompt('Enter department name:');
    if (!deptName) return;
    try {
      if (editingDept) {
        await updateDept.mutateAsync({ id: editingDept.id, name: deptName, companyId: company!.id, isActive: true });
        setEditingDept(null);
      } else {
        await createDept.mutateAsync({ name: deptName, companyId: company!.id });
      }
      toast.success(editingDept ? 'Department updated' : 'Department added');
    } catch (err) {
      toast.error('Failed to save department');
    }
  };

  const handleDeleteDepartment = (deptId: string) => {
    deleteDept.mutate(deptId, { onSuccess: () => toast.success('Department deleted') });
  };

  if (isLoading) return <div className="p-8">Loading...</div>;

  return (
    <div className="p-8 max-w-4xl mx-auto">
      <h1 className="text-2xl font-bold mb-6">{isEdit ? 'Edit' : 'Add'} Company</h1>
      <form onSubmit={handleSubmit} className="space-y-6">
        <Card>
          <CardHeader><CardTitle>Company Details</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div>
              <Label>Name *</Label>
              <Input value={name} onChange={(e) => setName(e.target.value)} required />
            </div>
          <div>
            <Label>Parent Company</Label>
            <Select value={parentCompanyId} onValueChange={(value) => setParentCompanyId(value || '')}>
                <SelectTrigger>
                <SelectValue placeholder="Select parent company (optional)" />
                </SelectTrigger>
                <SelectContent>
                <SelectItem value="">None</SelectItem>
                {allCompanies
                    ?.filter((c, index, self) => 
                        index === self.findIndex((t) => t.id === c.id) && 
                        c.id !== id && 
                        c.isActive
                    )
                    .map(c => (
                        <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>
                    ))}
                </SelectContent>
            </Select>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row justify-between items-center">
            <CardTitle>Departments</CardTitle>
            {isEdit && (
              <Button type="button" onClick={handleAddDepartment}>
                <Plus className="mr-2 h-4 w-4" /> Add Department
              </Button>
            )}
          </CardHeader>
          <CardContent>
            {!isEdit ? (
              <p className="text-muted-foreground">Save the company first to add departments.</p>
            ) : departments.filter(d => d.isActive).length === 0 ? (
              <p className="text-muted-foreground">No departments.</p>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow><TableHead>Name</TableHead><TableHead className="text-right">Actions</TableHead></TableRow>
                </TableHeader>
                <TableBody>
                  {departments.filter(d => d.isActive).map(d => (
                    <TableRow key={d.id}>
                      <TableCell>{d.name}</TableCell>
                      <TableCell className="text-right">
                        <Button variant="ghost" size="sm" onClick={() => { setEditingDept({ id: d.id, name: d.name }); handleAddDepartment(); }}>
                          <Edit className="h-4 w-4" />
                        </Button>
                        <AlertDialog>
                          <AlertDialogTrigger asChild>
                            <Button variant="ghost" size="sm"><Trash2 className="h-4 w-4 text-red-500" /></Button>
                          </AlertDialogTrigger>
                          <AlertDialogContent>
                            <AlertDialogHeader>
                              <AlertDialogTitle>Delete Department</AlertDialogTitle>
                              <AlertDialogDescription>Are you sure you want to delete "{d.name}"?</AlertDialogDescription>
                            </AlertDialogHeader>
                            <AlertDialogFooter>
                              <AlertDialogCancel>Cancel</AlertDialogCancel>
                              <AlertDialogAction onClick={() => handleDeleteDepartment(d.id)}>Delete</AlertDialogAction>
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
        </Card>

        <div className="flex gap-4">
          <Button type="submit" disabled={createCompany.isPending || updateCompany.isPending}>
            {isEdit ? 'Update' : 'Create'} Company
          </Button>
          <Button type="button" variant="outline" onClick={() => navigate('/companies')}>Cancel</Button>
        </div>
      </form>
    </div>
  );
}