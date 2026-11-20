import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm, useFieldArray } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useProfile, useCreateProfile, useUpdateProfile } from '../hooks/useProfiles';
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
const benefitSchema = z.object({
  benefitName: z.string().min(1, 'Benefit name is required'),
  amount: z.number().positive('Amount must be positive'),
  currency: z.number().min(0).max(10),
  isRecurring: z.boolean().default(true),
});

const profileFormSchema = z.object({
  name: z.string().min(1, 'Profile name is required'),
  description: z.string().optional(),
  baseSalaryAmount: z.number().positive('Base salary must be positive'),
  baseSalaryCurrency: z.number().min(0).max(10),
  defaultBenefits: z.array(benefitSchema).default([]),
});

type ProfileFormData = z.infer<typeof profileFormSchema>;

const currencyLabels: Record<number, string> = {
  0: 'USD',
  1: 'ZIG',
  2: 'EUR',
  3: 'GBP',
  4: 'ZAR',
};

export function ProfileForm() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = !!id;

  const { data: profile, isLoading } = useProfile(id || '');
  const createMutation = useCreateProfile();
  const updateMutation = useUpdateProfile();

  const form = useForm<ProfileFormData>({
    resolver: zodResolver(profileFormSchema) as any,
    defaultValues: {
      name: '',
      description: '',
      baseSalaryAmount: 0,
      baseSalaryCurrency: 0,
      defaultBenefits: [],
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

  const { fields, append, remove } = useFieldArray({
    control,
    name: 'defaultBenefits',
  });

  // Populate form when editing
  useEffect(() => {
    if (profile && isEdit) {
      setValue('name', profile.name);
      setValue('description', profile.description || '');
      setValue('baseSalaryAmount', profile.baseSalaryAmount);
      setValue('baseSalaryCurrency', profile.baseSalaryCurrency);
      if (profile.defaultBenefits && profile.defaultBenefits.length > 0) {
        setValue('defaultBenefits', profile.defaultBenefits.map((b) => ({
          benefitName: b.benefitName,
          amount: b.amount,
          currency: b.currency,
          isRecurring: b.isRecurring,
        })));
      }
    }
  }, [profile, isEdit, setValue]);

  const onSubmit = async (data: ProfileFormData) => {
    try {
      if (isEdit) {
        await updateMutation.mutateAsync({ id: id!, ...data });
        toast.success('Profile updated successfully');
      } else {
        await createMutation.mutateAsync(data);
        toast.success('Profile created successfully');
      }
      navigate('/profiles');
    } catch (error) {
      toast.error('Failed to save profile: ' + (error as Error).message);
    }
  };

  if (isLoading && isEdit) {
    return <div className="p-8">Loading profile...</div>;
  }

  return (
    <div className="p-8 max-w-4xl mx-auto">
      <h1 className="text-2xl font-bold mb-6">{isEdit ? 'Edit Profile' : 'Add New Profile'}</h1>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Basic Information</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <Label>Profile Name *</Label>
              <Input {...register('name')} />
              {errors.name && <p className="text-red-500 text-sm">{errors.name.message}</p>}
            </div>
            <div>
              <Label>Description</Label>
              <Input {...register('description')} />
            </div>
            <div>
              <Label>Base Salary Amount *</Label>
              <Input
                type="number"
                step="0.01"
                {...register('baseSalaryAmount', { valueAsNumber: true })}
              />
              {errors.baseSalaryAmount && <p className="text-red-500 text-sm">{errors.baseSalaryAmount.message}</p>}
            </div>
            <div>
              <Label>Base Salary Currency *</Label>
              <Select
                value={String(watch('baseSalaryCurrency'))}
                onValueChange={(val) => setValue('baseSalaryCurrency', Number(val))}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select Currency" />
                </SelectTrigger>
                <SelectContent>
                  {Object.entries(currencyLabels).map(([key, label]) => (
                    <SelectItem key={key} value={key}>{label}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {errors.baseSalaryCurrency && <p className="text-red-500 text-sm">{errors.baseSalaryCurrency.message}</p>}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row justify-between items-center">
            <CardTitle>Default Benefits</CardTitle>
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() =>
                append({
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
            {fields.length === 0 && (
              <p className="text-muted-foreground text-sm">No default benefits defined.</p>
            )}
            {fields.map((field, index) => (
              <div key={field.id} className="grid grid-cols-1 md:grid-cols-5 gap-4 border-b pb-4 mb-4">
                <div className="md:col-span-2">
                  <Label>Benefit Name</Label>
                  <Input {...register(`defaultBenefits.${index}.benefitName`)} placeholder="e.g., Fuel Allowance" />
                </div>
                <div>
                  <Label>Amount</Label>
                  <Input
                    type="number"
                    step="0.01"
                    {...register(`defaultBenefits.${index}.amount`, { valueAsNumber: true })}
                  />
                </div>
                <div>
                  <Label>Currency</Label>
                  <Select
                    value={String(watch(`defaultBenefits.${index}.currency`))}
                    onValueChange={(val) => setValue(`defaultBenefits.${index}.currency`, Number(val))}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Currency" />
                    </SelectTrigger>
                    <SelectContent>
                      {Object.entries(currencyLabels).map(([key, label]) => (
                        <SelectItem key={key} value={key}>{label}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="flex items-end">
                  <label className="flex items-center gap-2">
                    <input type="checkbox" {...register(`defaultBenefits.${index}.isRecurring`)} />
                    Recurring
                  </label>
                  <Button
                    type="button"
                    variant="destructive"
                    size="sm"
                    className="ml-2"
                    onClick={() => remove(index)}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>

        <div className="flex gap-4">
          <Button type="submit" disabled={isSubmitting} className="bg-royal-blue hover:bg-[#3558c7]">
            {isSubmitting ? 'Saving...' : (isEdit ? 'Update Profile' : 'Create Profile')}
          </Button>
          <Button type="button" variant="outline" onClick={() => navigate('/profiles')}>
            Cancel
          </Button>
        </div>
      </form>
    </div>
  );
}