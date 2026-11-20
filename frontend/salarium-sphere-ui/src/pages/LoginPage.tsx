import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useAuth } from '../hooks/useAuth';
// ✅ Correct import for named export
import { Button } from "@/components/ui/button";
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

const loginSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(6, 'Password must be at least 6 characters'),
});

type LoginFormData = z.infer<typeof loginSchema>;

export function LoginPage() {
  const { login, isLoggingIn, loginError } = useAuth();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

 const onSubmit = (data: LoginFormData) => {
  console.log('Form submitted with:', data); // <-- Add this
  login(data);
};

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50">
      <Card className="w-[400px]">
       <CardHeader className="bg-gradient-to-r from-royal-blue to-sky-blue text-white rounded-t-xl p-6">
          <CardTitle className="text-2xl font-bold">Salarium Sphere</CardTitle>
          <CardDescription className="text-white/80">
            Sign in to your payroll dashboard
          </CardDescription>
        </CardHeader>
        <CardContent style={{ padding: '2rem' }}>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div>
              <Input
                  {...register('email')}
                  type="email"
                  placeholder="Email"
                  className={errors.email ? 'border-red-500' : ''}
                />

              {errors.email && (
                <p className="text-sm text-red-500 mt-1">{errors.email.message}</p>
              )}
            </div>
            <div>
              <Input
                  {...register('password')}
                  type="password"
                  placeholder="Password"
                  className={errors.password ? 'border-red-500' : ''}
                />
              {errors.password && (
                <p className="text-sm text-red-500 mt-1">{errors.password.message}</p>
              )}
            </div>
            {loginError && (
              <p className="text-sm text-red-500">Invalid credentials</p>
            )}
            <Button type="submit" className="w-full" style={{ backgroundColor: 'var(--color-royal-blue)', color: 'var(--color-white)' }} disabled={isLoggingIn}>
              {isLoggingIn ? 'Signing in...' : 'Sign In'}
            </Button>
            
          </form>
          
        </CardContent>
      </Card>
    </div>
  );
}