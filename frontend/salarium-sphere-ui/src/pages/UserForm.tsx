import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { userService} from '../services/userService';
import { useEmployees } from '../hooks/useEmployees';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { toast } from 'sonner';
import { Checkbox } from '@/components/ui/checkbox';

export function UserForm() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const isEdit = !!id;

  const { data: employees } = useEmployees();
  const { data: user, isLoading } = useQuery({
    queryKey: ['user', id],
    queryFn: () => userService.get(id!),
    enabled: isEdit,
  });

  const [email, setEmail] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [role, setRole] = useState('Employee');
  const [employeeId, setEmployeeId] = useState<string>('');
  const [password, setPassword] = useState('');
  const [isActive, setIsActive] = useState(true);

  const createMutation = useMutation({
    mutationFn: userService.create,
    onSuccess: () => {
      toast.success('User created');
      queryClient.invalidateQueries({ queryKey: ['users'] });
      navigate('/users');
    },
    onError: (error: any) => toast.error(error.response?.data || 'Failed to create user'),
  });

  const updateMutation = useMutation({
    mutationFn: userService.update,
    onSuccess: () => {
      toast.success('User updated');
      queryClient.invalidateQueries({ queryKey: ['users'] });
      navigate('/users');
    },
    onError: (error: any) => toast.error(error.response?.data || 'Failed to update user'),
  });

  useEffect(() => {
    if (user) {
      setEmail(user.email);
      setFirstName(user.firstName);
      setLastName(user.lastName);
      setRole(user.role);
      setEmployeeId(user.employeeId || '');
      setIsActive(user.isActive);
    }
  }, [user]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (isEdit) {
        await updateMutation.mutateAsync({
          id: id!,
          email,
          firstName,
          lastName,
          role,
          employeeId: employeeId || undefined,
          isActive,
        });
      } else {
        await createMutation.mutateAsync({
          email,
          firstName,
          lastName,
          role,
          employeeId: employeeId || undefined,
          password: password || undefined,
        });
      }
    } catch (err) {
      // Error handled by mutation
    }
  };

  if (isLoading) return <div className="p-8">Loading...</div>;

  return (
    <div className="p-8 max-w-2xl mx-auto">
      <h1 className="text-2xl font-bold mb-6">{isEdit ? 'Edit' : 'Add'} User</h1>
      <form onSubmit={handleSubmit}>
        <Card>
          <CardHeader><CardTitle>User Details</CardTitle></CardHeader>
          <CardContent className="space-y-4">
            <div>
              <Label>First Name *</Label>
              <Input value={firstName} onChange={(e) => setFirstName(e.target.value)} required />
            </div>
            <div>
              <Label>Last Name *</Label>
              <Input value={lastName} onChange={(e) => setLastName(e.target.value)} required />
            </div>
            <div>
              <Label>Email *</Label>
              <Input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
            </div>
            {!isEdit && (
              <div>
                <Label>Password</Label>
                <Input
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Leave blank to auto-generate"
                />
              </div>
            )}
           <div>
                    <Label>Role *</Label>
                    <Select value={role} onValueChange={(value) => setRole(value || 'Employee')}>
                        <SelectTrigger>
                        <SelectValue placeholder="Select Role" />
                        </SelectTrigger>
                        <SelectContent>
                        <SelectItem value="Admin">Admin</SelectItem>
                        <SelectItem value="PayrollManager">Payroll Manager</SelectItem>
                        <SelectItem value="Employee">Employee</SelectItem>
                        </SelectContent>
                    </Select>
                    </div>
           <div>
                <Label>Linked Employee</Label>
                <Select value={employeeId} onValueChange={(value) => setEmployeeId(value || '')}>
                    <SelectTrigger>
                    <SelectValue placeholder="Select Employee (optional)" />
                    </SelectTrigger>
                    <SelectContent>
                    <SelectItem value="">None</SelectItem>
                    {employees?.map((emp) => (
                        <SelectItem key={emp.id} value={emp.id}>
                        {emp.firstName} {emp.lastName} ({emp.employeeCode})
                        </SelectItem>
                    ))}
                    </SelectContent>
                </Select>
                </div>
            {isEdit && (
              <div className="flex items-center gap-2">
                <Checkbox checked={isActive} onCheckedChange={(v) => setIsActive(!!v)} />
                <Label>Active</Label>
              </div>
            )}
          </CardContent>
        </Card>
        <div className="flex gap-4 mt-6">
          <Button type="submit" disabled={createMutation.isPending || updateMutation.isPending}>
            {isEdit ? 'Update' : 'Create'} User
          </Button>
          <Button type="button" variant="outline" onClick={() => navigate('/users')}>
            Cancel
          </Button>
        </div>
      </form>
    </div>
  );
}