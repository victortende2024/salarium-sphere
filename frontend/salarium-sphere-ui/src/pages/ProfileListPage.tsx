import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useProfiles, useDeleteProfile } from '../hooks/useProfiles';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
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
import { Plus, Search, Edit, Trash2, Eye } from 'lucide-react';
import { toast } from 'sonner';

const currencyLabels: Record<number, string> = {
  0: 'USD',
  1: 'ZIG',
  2: 'EUR',
  3: 'GBP',
  4: 'ZAR',
};

export function ProfileListPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const { data: profiles, isLoading, error } = useProfiles();
  const deleteMutation = useDeleteProfile();

  const handleDelete = (id: string, name: string) => {
    deleteMutation.mutate(id, {
      onSuccess: () => toast.success(`Profile "${name}" deleted`),
      onError: (err) => toast.error('Failed to delete profile: ' + (err as Error).message),
    });
  };

  const filtered = profiles?.filter((p) =>
    p.name.toLowerCase().includes(search.toLowerCase()) ||
    (p.description?.toLowerCase() || '').includes(search.toLowerCase())
  );

  if (isLoading) return <div className="p-8">Loading profiles...</div>;
  if (error) return <div className="p-8 text-red-500">Error loading profiles</div>;

  return (
    <div className="p-8 max-w-6xl mx-auto">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Payroll Profiles</h1>
        <Button onClick={() => navigate('/profiles/new')} className="bg-royal-blue hover:bg-[#3558c7]">
          <Plus className="mr-2 h-4 w-4" /> Add Profile
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Profile List</CardTitle>
          <CardDescription>Manage payroll profiles and their default benefits.</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-2 mb-4">
            <div className="relative flex-1">
              <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search profiles..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="pl-8"
              />
            </div>
          </div>
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Name</TableHead>
                  <TableHead>Base Salary</TableHead>
                  <TableHead>Currency</TableHead>
                  <TableHead>Benefits Count</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filtered?.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={6} className="text-center text-muted-foreground">
                      No profiles found
                    </TableCell>
                  </TableRow>
                ) : (
                  filtered?.map((p) => (
                    <TableRow key={p.id}>
                      <TableCell className="font-medium">{p.name}</TableCell>
                      <TableCell>${p.baseSalaryAmount.toFixed(2)}</TableCell>
                      <TableCell>{currencyLabels[p.baseSalaryCurrency] || p.baseSalaryCurrency}</TableCell>
                      <TableCell>{p.defaultBenefits.length}</TableCell>
                      <TableCell>
                        <span
                          className={`px-2 py-1 rounded-full text-xs ${
                            p.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                          }`}
                        >
                          {p.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </TableCell>
                      <TableCell className="text-right">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => navigate(`/profiles/${p.id}`)}
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => navigate(`/profiles/${p.id}/edit`)}
                        >
                          <Edit className="h-4 w-4" />
                        </Button>
                        <AlertDialog>
                          <AlertDialogTrigger asChild>
                            <Button variant="ghost" size="sm">
                              <Trash2 className="h-4 w-4 text-red-500" />
                            </Button>
                          </AlertDialogTrigger>
                          <AlertDialogContent>
                            <AlertDialogHeader>
                              <AlertDialogTitle>Delete Profile</AlertDialogTitle>
                              <AlertDialogDescription>
                                Are you sure you want to delete "{p.name}"? This action cannot be undone.
                              </AlertDialogDescription>
                            </AlertDialogHeader>
                            <AlertDialogFooter>
                              <AlertDialogCancel>Cancel</AlertDialogCancel>
                              <AlertDialogAction onClick={() => handleDelete(p.id, p.name)}>
                                Delete
                              </AlertDialogAction>
                            </AlertDialogFooter>
                          </AlertDialogContent>
                        </AlertDialog>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}