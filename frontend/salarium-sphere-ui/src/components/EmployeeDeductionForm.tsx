import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import type{EmployeeDeduction } from '../services/employeeDeductionService';
import { employeeDeductionService } from '../services/employeeDeductionService';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { toast } from 'sonner';
import { Checkbox } from '@/components/ui/checkbox';

const schema = z.object({
  employeeId: z.string().uuid(),
  deductionName: z.string().min(1, 'Name is required'),
  description: z.string().optional(),
  calculationType: z.number().min(0).max(2),
  fixedAmount: z.number().nullable().optional(),
  fixedAmountCurrency: z.number().nullable().optional(),
  percentageValue: z.number().nullable().optional(),
  tieredConfigJson: z.string().nullable().optional(),
  capAmount: z.number().nullable().optional(),
  capCurrency: z.number().nullable().optional(),
  thresholdAmount: z.number().nullable().optional(),
  thresholdCurrency: z.number().nullable().optional(),
  isActive: z.boolean().default(true),
});

type FormData = z.infer<typeof schema>;

interface Props {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  employeeId: string;
  initialData?: EmployeeDeduction | null;
  onSuccess: () => void;
}

export function EmployeeDeductionForm({ open, onOpenChange, employeeId, initialData, onSuccess }: Props) {
  const { register, handleSubmit, setValue, watch, reset, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema) as any,
    defaultValues: {
      employeeId: employeeId,
      deductionName: '',
      description: '',
      calculationType: 0,
      fixedAmount: null,
      fixedAmountCurrency: 0,
      percentageValue: null,
      tieredConfigJson: null,
      capAmount: null,
      capCurrency: 0,
      thresholdAmount: null,
      thresholdCurrency: 0,
      isActive: true,
    },
  });

  useEffect(() => {
    if (initialData) {
      reset({
        employeeId: initialData.employeeId,
        deductionName: initialData.deductionName,
        description: initialData.description || '',
        calculationType: initialData.calculationType,
        fixedAmount: initialData.fixedAmount ?? null,
        fixedAmountCurrency: initialData.fixedAmountCurrency ?? 0,
        percentageValue: initialData.percentageValue ?? null,
        tieredConfigJson: initialData.tieredConfigJson ?? null,
        capAmount: initialData.capAmount ?? null,
        capCurrency: initialData.capCurrency ?? 0,
        thresholdAmount: initialData.thresholdAmount ?? null,
        thresholdCurrency: initialData.thresholdCurrency ?? 0,
        isActive: initialData.isActive,
      });
    } else {
      reset({ employeeId });
    }
  }, [initialData, reset, employeeId]);

  const onSubmit = async (data: FormData) => {
    try {
      const payload = {
        ...data,
        fixedAmount: data.fixedAmount ?? undefined,
        fixedAmountCurrency: data.fixedAmountCurrency ?? undefined,
        percentageValue: data.percentageValue ?? undefined,
        tieredConfigJson: data.tieredConfigJson || undefined,
        capAmount: data.capAmount ?? undefined,
        capCurrency: data.capCurrency ?? undefined,
        thresholdAmount: data.thresholdAmount ?? undefined,
        thresholdCurrency: data.thresholdCurrency ?? undefined,
      };
      if (initialData) {
        await employeeDeductionService.update({ ...payload, id: initialData.id });
        toast.success('Deduction updated');
      } else {
        await employeeDeductionService.create(payload);
        toast.success('Deduction created');
      }
      onSuccess();
    } catch (error) {
      toast.error('Failed to save deduction');
    }
  };

  const calcType = watch('calculationType');

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader><DialogTitle>{initialData ? 'Edit' : 'Add'} Employee Deduction</DialogTitle></DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label>Deduction Name *</Label>
              <Input {...register('deductionName')} />
              {errors.deductionName && <p className="text-red-500 text-sm">{errors.deductionName.message}</p>}
            </div>
            <div>
              <Label>Description</Label>
              <Input {...register('description')} />
            </div>
            <div>
              <Label>Calculation Type</Label>
              <Select value={String(calcType)} onValueChange={(v) => setValue('calculationType', Number(v))}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="0">Fixed</SelectItem>
                  <SelectItem value="1">Percentage</SelectItem>
                  <SelectItem value="2">Tiered</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="flex items-center gap-4">
              <div className="flex items-center gap-2">
                <Checkbox checked={watch('isActive')} onCheckedChange={(v) => setValue('isActive', !!v)} />
                <Label>Active</Label>
              </div>
            </div>
          </div>

          {calcType === 0 && (
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Fixed Amount</Label>
                <Input type="number" step="0.01" {...register('fixedAmount', { valueAsNumber: true })} />
              </div>
              <div>
                <Label>Currency</Label>
                <Select value={String(watch('fixedAmountCurrency') ?? 0)} onValueChange={(v) => setValue('fixedAmountCurrency', Number(v))}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="0">USD</SelectItem>
                    <SelectItem value="1">ZIG</SelectItem>
                    <SelectItem value="2">EUR</SelectItem>
                    <SelectItem value="3">GBP</SelectItem>
                    <SelectItem value="4">ZAR</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
          )}

          {calcType === 1 && (
            <div>
              <Label>Percentage Value (%)</Label>
              <Input type="number" step="0.01" {...register('percentageValue', { valueAsNumber: true })} />
            </div>
          )}

          {calcType === 2 && (
            <div>
              <Label>Tiered Config (JSON)</Label>
              <Input {...register('tieredConfigJson')} placeholder='[{"startBand":0,"endBand":1000,"rate":0},...]' />
            </div>
          )}

          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label>Cap Amount</Label>
              <Input type="number" step="0.01" {...register('capAmount', { valueAsNumber: true })} />
            </div>
            <div>
              <Label>Cap Currency</Label>
              <Select value={String(watch('capCurrency') ?? 0)} onValueChange={(v) => setValue('capCurrency', Number(v))}>
                <SelectTrigger><SelectValue /></SelectTrigger>
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
              <Label>Threshold Amount</Label>
              <Input type="number" step="0.01" {...register('thresholdAmount', { valueAsNumber: true })} />
            </div>
            <div>
              <Label>Threshold Currency</Label>
              <Select value={String(watch('thresholdCurrency') ?? 0)} onValueChange={(v) => setValue('thresholdCurrency', Number(v))}>
                <SelectTrigger><SelectValue /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="0">USD</SelectItem>
                  <SelectItem value="1">ZIG</SelectItem>
                  <SelectItem value="2">EUR</SelectItem>
                  <SelectItem value="3">GBP</SelectItem>
                  <SelectItem value="4">ZAR</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="flex justify-end gap-2 pt-4">
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>Cancel</Button>
            <Button type="submit" disabled={isSubmitting}>{isSubmitting ? 'Saving...' : 'Save'}</Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}